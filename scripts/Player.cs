using Deathville.Component;
using Deathville.Enum;
using Godot;
using GodotApiTools.Logic;

namespace Deathville.GameObject
{
    public class Player : KinematicBody2D
    {
        [Signal]
        public delegate void AttackStart();
        [Signal]
        public delegate void AttackEnd();

        private const string ANIM_IDLE = "idle";
        private const string ANIM_RUN = "run";
        private const string INPUT_MOVE_LEFT = "move_left";
        private const string INPUT_MOVE_RIGHT = "move_right";
        private const string INPUT_JUMP = "jump";
        private const string INPUT_ATTACK = "attack";

        private const float GRAVITY = 800f;
        private const float JUMP_SPEED = 420f;
        private const float GRAVITY_ACCELERATOR = 6f;
        private const float INITIAL_COYOTE_TIME = .2f;
        private const float TIME_SCALE = .15f;

        private AnimatedSprite _animatedSprite;
        private float _coyoteTime;
        private VelocityComponent _velocityComponent;

        private StateMachine<MoveState> _moveStateMachine = new StateMachine<MoveState>();

        public override void _Ready()
        {
            _moveStateMachine.AddState(MoveState.GROUNDED, MoveStateGrounded);
            _moveStateMachine.AddState(MoveState.AIRBORNE, MoveStateAirborne);
            _moveStateMachine.SetInitialState(MoveState.GROUNDED);
            _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            _velocityComponent = GetNode<VelocityComponent>("VelocityComponent");
        }

        public override void _Process(float delta)
        {
            _moveStateMachine.Update();
            GameEventDispatcher.DispatchPlayerPositionUpdated(GlobalPosition);

            var scaleLerpTo = _moveStateMachine.GetCurrentState() == MoveState.AIRBORNE ? TIME_SCALE : 1f;
            Engine.TimeScale = Mathf.Lerp(Engine.TimeScale, scaleLerpTo, 15f * delta / Engine.TimeScale);
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            if (evt.IsAction(INPUT_ATTACK))
            {
                if (evt.IsActionPressed(INPUT_ATTACK))
                {
                    EmitSignal(nameof(AttackStart));
                }
                else
                {
                    EmitSignal(nameof(AttackEnd));
                }
                GetTree().SetInputAsHandled();
            }
        }

        private void MoveStateGrounded()
        {
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

            if (moveVec.y < 0)
            {
                _velocityComponent.Jump(JUMP_SPEED);
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

            if (moveVec.y < 0 && _coyoteTime > 0f)
            {
                _velocityComponent.Jump(JUMP_SPEED);
            }

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

        private void UpdateAnimations()
        {
            var moveVec = GetMovementVector();
            if (moveVec.x != 0)
            {
                _animatedSprite.Play(ANIM_RUN);
                var shouldFlip = (moveVec.x < 0 && GlobalPosition.x > GetGlobalMousePosition().x) || (moveVec.x > 0 && GlobalPosition.x > GetGlobalMousePosition().x);
                _animatedSprite.FlipH = shouldFlip;
            }
            else
            {
                var shouldFlip = GlobalPosition.x > GetGlobalMousePosition().x;
                _animatedSprite.FlipH = shouldFlip;
                _animatedSprite.Play(ANIM_IDLE);
            }
            _animatedSprite.SpeedScale = 1f / Engine.TimeScale;
        }

        private Vector2 GetMovementVector()
        {
            var moveVec = Vector2.Zero;
            moveVec.x = Input.GetActionStrength(INPUT_MOVE_RIGHT) - Input.GetActionStrength(INPUT_MOVE_LEFT);
            moveVec.y = Input.IsActionJustPressed(INPUT_JUMP) ? -1 : 0;
            return moveVec;
        }
    }
}