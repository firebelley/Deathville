using Deathville.Component;
using Deathville.Enum;
using Deathville.GameObject.Combat;
using Deathville.Singleton;
using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Logic;

namespace Deathville.GameObject
{
    public class Player : KinematicBody2D
    {
        [Signal]
        public delegate void AttackStart(int socket);
        [Signal]
        public delegate void AttackEnd(int socket);

        public const string GROUP = "player";

        private const string ANIM_IDLE = "idle";
        private const string ANIM_RUN = "run";
        private const string ANIM_JUMP = "jump";
        private const string ANIM_FLIP = "flip";
        private const string ANIM_JUMP_IDLE = "jump_idle";

        private const string INPUT_MOVE_LEFT = "move_left";
        private const string INPUT_MOVE_RIGHT = "move_right";
        private const string INPUT_JUMP = "jump";
        private const string INPUT_ATTACK = "attack";
        private const string INPUT_DASH = "dash";

        private const float GRAVITY = 800f;
        private const float JUMP_SPEED = 350f;
        private const float GRAVITY_ACCELERATOR = 4f;
        private const float INITIAL_COYOTE_TIME = .2f;
        private const float TIME_SCALE = .15f;
        private const float DEFAULT_TIME_SCALE = 1f;
        private const float DASH_TIME = .2f;
        private const float SLIDE_TIME = .5f;
        private const int MAX_JUMPS = 2;
        private const int MAX_DASHES = 1;

        private AnimatedSprite _animatedSprite;
        private Sprite _flipSprite;
        private Tween _flipTween;
        private AnimationPlayer _animationPlayer;
        private VelocityComponent _velocityComponent;
        private HealthComponent _healthComponent;

        private float _coyoteTime;
        private float _dashTime;
        private float _slideTime;
        private int _jumpCount;
        private int _dashCount;

        private StateMachine<MoveState> _moveStateMachine = new StateMachine<MoveState>();

        public override void _Ready()
        {
            AddToGroup(GROUP);
            _moveStateMachine.AddState(MoveState.GROUNDED, MoveStateGrounded);
            _moveStateMachine.AddState(MoveState.AIRBORNE, MoveStateAirborne);
            _moveStateMachine.AddState(MoveState.DASH, MoveStateDash);
            _moveStateMachine.AddState(MoveState.SLIDE, MoveStateSlide);
            _moveStateMachine.SetInitialState(MoveState.GROUNDED);
            _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            _velocityComponent = this.GetFirstNodeOfType<VelocityComponent>();
            _healthComponent = this.GetFirstNodeOfType<HealthComponent>();
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _flipSprite = GetNode<Sprite>("FlipSprite");
            _flipTween = GetNode<Tween>("FlipTween");

            var weaponSocket = this.GetFirstNodeOfType<WeaponSocketComponent>();
            weaponSocket.EquipWeapon((GD.Load("res://scenes/GameObject/Combat/GrenadeLauncher.tscn") as PackedScene).Instance() as Weapon);
            weaponSocket = GetNode<WeaponSocketComponent>("WeaponSocketComponent2");
            weaponSocket.EquipWeapon((GD.Load("res://scenes/GameObject/Combat/PlayerWeapon.tscn") as PackedScene).Instance() as Weapon);

            _flipTween.Connect("tween_all_completed", this, nameof(OnFlipTweenCompleted));
            _healthComponent?.Connect(nameof(HealthComponent.HealthChanged), this, nameof(OnHealthChanged));
        }

        public override void _Process(float delta)
        {
            _moveStateMachine.Update();

            var scaleLerpTo = _moveStateMachine.GetCurrentState() != MoveState.GROUNDED ? TIME_SCALE : DEFAULT_TIME_SCALE;
            Engine.TimeScale = Mathf.Lerp(Engine.TimeScale, scaleLerpTo, 15f * delta / Engine.TimeScale);

            if (_moveStateMachine.GetCurrentState() != MoveState.DASH && _moveStateMachine.GetCurrentState() != MoveState.SLIDE)
            {
                _velocityComponent.SpeedPadding = Mathf.Lerp(_velocityComponent.SpeedPadding, 0f, 5f * delta / Engine.TimeScale);
            }
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsAction(INPUT_ATTACK))
            {
                var socket = (evt as InputEventMouseButton).ButtonIndex == (int) ButtonList.Left ? 0 : 1;
                if (evt.IsActionPressed(INPUT_ATTACK))
                {
                    EmitSignal(nameof(AttackStart), socket);
                }
                else
                {
                    EmitSignal(nameof(AttackEnd), socket);
                }
                GetTree().SetInputAsHandled();
            }
        }

        private void MoveStateGrounded()
        {
            if (_moveStateMachine.IsStateNew())
            {
                _jumpCount = 0;
                _dashCount = 0;
            }

            var moveVec = GetMovementVector();
            if (moveVec.x != 0f)
            {
                _velocityComponent.Accelerate(moveVec);
            }
            else
            {
                _velocityComponent.Decelerate();
            }

            _velocityComponent.MoveWithSnap();

            if (Input.IsActionJustPressed(INPUT_DASH))
            {
                _moveStateMachine.ChangeState(MoveStateSlide);
            }
            else if (moveVec.y < 0)
            {
                Jump();
            }

            if (!IsOnFloor() || moveVec.y < 0)
            {
                _coyoteTime = moveVec.y < 0 ? 0f : INITIAL_COYOTE_TIME;
                _moveStateMachine.ChangeState(MoveStateAirborne);
            }
            UpdateAnimations();
        }

        private void MoveStateAirborne()
        {
            _coyoteTime = Mathf.Clamp(_coyoteTime - GetProcessDeltaTime() / Engine.TimeScale, 0f, INITIAL_COYOTE_TIME);

            var moveVec = GetMovementVector();
            if (moveVec.x != 0f)
            {
                _velocityComponent.Accelerate(moveVec);
            }
            else
            {
                _velocityComponent.Decelerate();
            }

            if (Input.IsActionJustPressed(INPUT_DASH))
            {
                Dash();
            }
            else if (moveVec.y < 0)
            {
                Jump();
            }

            // jump decelerator
            if (!Input.IsActionPressed(INPUT_JUMP) && _velocityComponent.Velocity.y < 0)
            {
                _velocityComponent.ApplyGravity(GRAVITY_ACCELERATOR);
            }
            else
            {
                _velocityComponent.ApplyGravity();
            }

            _velocityComponent.Move();

            if (IsOnFloor())
            {
                _moveStateMachine.ChangeState(MoveStateGrounded);
            }

            UpdateAnimations();
        }

        private void MoveStateDash()
        {
            if (_moveStateMachine.IsStateNew())
            {
                _dashTime = 0f;
                _velocityComponent.SpeedPadding = 250f;
                _velocityComponent.YVelocity = 0f;
                _velocityComponent.ApplyForce(ShouldGoLeft() ? Vector2.Left : Vector2.Right, 250f);
            }

            _dashTime += GetProcessDeltaTime() / Engine.TimeScale;

            _velocityComponent.ApplyGravity(.25f);
            _velocityComponent.Move();

            if (IsOnFloor())
            {
                _moveStateMachine.ChangeState(MoveStateGrounded);
            }
            else if (_dashTime > DASH_TIME)
            {
                _moveStateMachine.ChangeState(MoveStateAirborne);
            }

            UpdateAnimations();
        }

        private void MoveStateSlide()
        {
            if (_moveStateMachine.IsStateNew())
            {
                _velocityComponent.SpeedPadding = 100f;
                _slideTime = 0f;
                _velocityComponent.ApplyForce(ShouldGoLeft() ? Vector2.Left : Vector2.Right, _velocityComponent.MaxSpeed + _velocityComponent.SpeedPadding);
            }
            var moveVec = GetMovementVector();
            _slideTime += GetProcessDeltaTime() / Engine.TimeScale;
            _velocityComponent.MoveWithSnap();

            if (!IsOnFloor())
            {
                _coyoteTime = moveVec.y < 0 ? 0f : INITIAL_COYOTE_TIME;
                _moveStateMachine.ChangeState(MoveStateAirborne);
            }
            else if (moveVec.y < 0f)
            {
                Jump();
            }
            else if (_slideTime > SLIDE_TIME)
            {
                _moveStateMachine.ChangeState(MoveStateGrounded);
            }
        }

        private void UpdateAnimations()
        {
            _flipTween.PlaybackSpeed = 1f / Engine.TimeScale;
            _animationPlayer.PlaybackSpeed = 1f / Engine.TimeScale;
            _animatedSprite.SpeedScale = 1f / Engine.TimeScale;

            var moveVec = GetMovementVector();
            _animatedSprite.FlipH = GlobalPosition.x > GetGlobalMousePosition().x;
            _flipSprite.FlipH = _animatedSprite.FlipH;
            _flipSprite.RotationDegrees = Mathf.Abs(_flipSprite.RotationDegrees) * (_flipSprite.FlipH ? -1f : 1f);
            if (!IsOnFloor())
            {
                _animatedSprite.Play(ANIM_JUMP_IDLE);
            }
            else if (moveVec.x != 0)
            {
                _animatedSprite.Play(ANIM_RUN);
            }
            else
            {
                _animatedSprite.Play(ANIM_IDLE);
            }
        }

        private Vector2 GetMovementVector()
        {
            var moveVec = Vector2.Zero;
            moveVec.x = Input.GetActionStrength(INPUT_MOVE_RIGHT) - Input.GetActionStrength(INPUT_MOVE_LEFT);
            moveVec.y = Input.IsActionJustPressed(INPUT_JUMP) ? -1 : 0;
            return moveVec;
        }

        private bool ShouldGoLeft()
        {
            var moveVec = GetMovementVector();
            var left = false;
            if (moveVec.x != 0)
            {
                left = moveVec.x < 0;
            }
            else
            {
                left = GetGlobalMousePosition().x < GlobalPosition.x;
            }
            return left;
        }

        private void Jump()
        {
            // if airborne and the player has not used coyote time jump
            // then take away a jump
            if (_moveStateMachine.GetCurrentState() == MoveState.AIRBORNE && _coyoteTime == 0f && _jumpCount == 0)
            {
                _jumpCount++;
            }

            if (_jumpCount < MAX_JUMPS)
            {
                StartFlip();
                _velocityComponent.Jump(JUMP_SPEED);
                _jumpCount++;
            }
        }

        private void Dash()
        {
            if (_dashCount < MAX_DASHES)
            {
                _dashCount++;
                _moveStateMachine.ChangeState(MoveStateDash);
            }
        }

        private void StartFlip()
        {
            _flipTween.ResetAll();
            _flipTween.InterpolateProperty(
                _flipSprite,
                "rotation_degrees",
                0f,
                360f,
                .5f,
                Tween.TransitionType.Sine,
                Tween.EaseType.Out
            );
            _animatedSprite.Visible = false;
            _flipSprite.Visible = true;
            _flipTween.Start();
        }

        private void OnFlipTweenCompleted()
        {
            _animatedSprite.Visible = true;
            _flipSprite.Visible = false;
        }

        private void OnHealthChanged()
        {
            GameEventDispatcher.DispatchPlayerHealthChanged(_healthComponent.CurrentHealth);
        }
    }
}