using Deathville.Enum;
using Godot;
using GodotApiTools.Logic;

namespace Deathville.Component
{
    public class AIMovementComponent : Node
    {
        private const float MAX_AHEAD = 32f;

        [Export]
        private float _maxSpeed = 100f;
        [Export]
        private float _jumpSpeed = 400f;

        private Curve2D _pathCurve = new Curve2D();
        private float _currentT = 0f;
        private Vector2 _playerPos;
        private Vector2 _velocity;

        private KinematicBody2D _owner;

        private StateMachine<MoveState> _stateMachine = new StateMachine<MoveState>();

        public override void _Ready()
        {
            _stateMachine.AddState(MoveState.GROUNDED, StateGrounded);
            _stateMachine.AddState(MoveState.AIRBORNE, StateAirborne);
            _stateMachine.SetInitialState(MoveState.GROUNDED);

            _owner = Owner as KinematicBody2D;
            GetNode<Timer>("Timer").Connect("timeout", this, nameof(OnTimerTimeout));
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerPositionUpdated), this, nameof(OnPlayerPositionUpdated));
        }

        public override void _Process(float delta)
        {
            _stateMachine.Update();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (_pathCurve.GetPointCount() > 0)
            {
                var curvePos = _pathCurve.InterpolateBaked(_currentT);
                if (_owner.GlobalPosition.DistanceSquaredTo(curvePos) <= MAX_AHEAD * MAX_AHEAD)
                {
                    _currentT = Mathf.Clamp(_currentT + MAX_AHEAD, 0f, _pathCurve.GetBakedLength());
                }
            }
        }

        // TODO: change pathfinding to return information about the weight and the IDS
        // keep track of the path ids as well as the global positions
        // use the weight to determine jump strength
        // use accel/decel for movement instead of instantaneous
        private void StateGrounded()
        {
            if (_stateMachine.IsStateNew())
            {
                OnTimerTimeout();
            }

            var dir = GetTargetDirection();
            if (dir.x != 0f)
            {
                _velocity.x = Mathf.Sign(dir.x) * _maxSpeed;
            }
            _velocity = _owner.MoveAndSlideWithSnap(_velocity, Vector2.Down, Vector2.Up);

            if (GetTargetPosition() != Vector2.Zero && (GetTargetPosition().y - _owner.GlobalPosition.y) >= 16f)
            {
                _velocity.y = -_jumpSpeed;
                _stateMachine.ChangeState(StateAirborne);
            }
        }

        private void StateAirborne()
        {
            var dir = GetTargetDirection();
            if (dir.x != 0f)
            {
                _velocity.x = Mathf.Sign(dir.x) * _maxSpeed;
            }

            _velocity.y += 800f * GetProcessDeltaTime();
            _velocity = _owner.MoveAndSlide(_velocity, Vector2.Up);

            if (_owner.IsOnFloor())
            {
                _stateMachine.ChangeState(StateGrounded);
            }
        }

        private Vector2 GetTargetDirection()
        {
            if (_pathCurve.GetPointCount() > 0)
            {
                return (GetTargetPosition() - _owner.GlobalPosition).Normalized();
            }
            return Vector2.Zero;
        }

        private Vector2 GetTargetPosition()
        {
            if (_pathCurve.GetPointCount() > 0)
            {
                return _pathCurve.InterpolateBaked(_currentT);
            }
            return Vector2.Zero;
        }

        private void OnPlayerPositionUpdated(Vector2 pos)
        {
            _playerPos = pos;
        }

        private void OnTimerTimeout()
        {
            if (!_owner.IsOnFloor()) return;

            _pathCurve.ClearPoints();
            _currentT = 0f;
            var path = Zone.Current.Pathfinder.GetGlobalPath((Owner as Node2D).GlobalPosition, _playerPos);
            foreach (var point in path)
            {
                _pathCurve.AddPoint(point);
            }
        }
    }
}