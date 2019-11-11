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

        private const float MAX_SPEED = 200f;
        private const float GRAVITY = 800f;
        private const float JUMP_SPEED = -400f;
        private const float GRAVITY_ACCELERATOR = 4f;
        private const float INITIAL_COYOTE_TIME = .2f;
        private const float TIME_SCALE = .15f;

        private AnimatedSprite _animatedSprite;
        private Vector2 _velocity;
        private float _coyoteTime;

        private StateMachine<MoveState> _moveStateMachine = new StateMachine<MoveState>();

        public enum MoveState
        {
            GROUNDED,
            AIRBORNE
        }

        public override void _Ready()
        {
            _moveStateMachine.AddState(MoveState.GROUNDED, MoveStateGrounded);
            _moveStateMachine.AddState(MoveState.AIRBORNE, MoveStateAirborne);
            _moveStateMachine.SetInitialState(MoveState.GROUNDED);
            _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
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
            _velocity.x = moveVec.x * MAX_SPEED;
            _velocity = MoveAndSlideWithSnap(_velocity / Engine.TimeScale, Vector2.Down, Vector2.Up) * Engine.TimeScale;

            if (moveVec.y < 0)
            {
                _velocity.y = JUMP_SPEED;
            }

            if (!IsOnFloor() || moveVec.y < 0)
            {
                _coyoteTime = INITIAL_COYOTE_TIME;
                _moveStateMachine.ChangeState(MoveStateAirborne);
            }
            UpdateAnimations();
        }

        private void MoveStateAirborne()
        {
            _coyoteTime = Mathf.Clamp(_coyoteTime - GetProcessDeltaTime() / Engine.TimeScale, 0f, INITIAL_COYOTE_TIME);

            var moveVec = GetMovementVector();
            _velocity.x = moveVec.x * MAX_SPEED;

            if (moveVec.y < 0 && _coyoteTime > 0f)
            {
                _velocity.y = JUMP_SPEED;
            }

            var deltaGrav = GRAVITY * GetProcessDeltaTime() / Engine.TimeScale;
            if (!Input.IsActionPressed(INPUT_JUMP) && _velocity.y < 0)
            {
                deltaGrav *= GRAVITY_ACCELERATOR;
            }
            _velocity.y += deltaGrav;
            _velocity = MoveAndSlide(_velocity / Engine.TimeScale, Vector2.Up) * Engine.TimeScale;

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
                _animatedSprite.FlipH = moveVec.x < 0;
            }
            else
            {
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