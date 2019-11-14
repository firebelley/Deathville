using System.Collections.Generic;
using System.Linq;
using Deathville.Enum;
using Deathville.Util;
using Godot;
using GodotApiTools.Logic;

namespace Deathville.Component
{
    public class AIMovementComponent : Node
    {
        private const float MAX_AHEAD = 8f;

        [Export]
        private float _maxSpeed = 100f;
        [Export]
        private float _jumpSpeed = 400f;

        private Vector2 _playerPos;
        private Vector2 _velocity;

        private KinematicBody2D _owner;

        private Line2D _line2d;
        private StateMachine<MoveState> _stateMachine = new StateMachine<MoveState>();
        private List<Pathfinder.PathfindCell> _pathfindCells = new List<Pathfinder.PathfindCell>();
        private int _targetCellId;

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
            if (_targetCellId < _pathfindCells.Count)
            {
                var targetPos = GetTargetPosition();
                if (_owner.IsOnFloor() && _owner.GlobalPosition.DistanceSquaredTo(targetPos) <= MAX_AHEAD * MAX_AHEAD)
                {
                    _targetCellId++;
                }
            }
        }

        // TODO: use accel/decel for movement instead of instantaneous
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

            if (GetTargetPosition() != Vector2.Zero && ShouldJump(GetTargetPathCell()))
            {
                _velocity.y = -GetJumpStep(GetTargetPathCell());
                _stateMachine.ChangeState(StateAirborne);
            }
            else if (!_owner.IsOnFloor())
            {
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
            if (_targetCellId < _pathfindCells.Count)
            {
                return (GetTargetPosition() - _owner.GlobalPosition).Normalized();
            }
            return Vector2.Zero;
        }

        private Vector2 GetTargetPosition()
        {
            if (_targetCellId < _pathfindCells.Count)
            {
                return _pathfindCells[_targetCellId].GlobalPosition;
            }
            return Vector2.Zero;
        }

        private Pathfinder.PathfindCell GetTargetPathCell()
        {
            return _pathfindCells[_targetCellId];
        }

        private float GetJumpStep(Pathfinder.PathfindCell targetCell)
        {
            var heightDiff = Mathf.Abs(_owner.GlobalPosition.y - targetCell.GlobalPosition.y);
            var speed = Mathf.Clamp(Mathf.Sqrt(2 * 800f * heightDiff) * 1.25f, 0f, _jumpSpeed);
            return speed;
        }

        private bool ShouldJump(Pathfinder.PathfindCell targetCell)
        {
            var isBelowTarget = _owner.GlobalPosition.y - targetCell.GlobalPosition.y >= 16f;
            //TODO: this doesn't work because it can pathfind to the middle of a platform
            // in other words, the pathfinding for jumping is not only corner to corner
            var isFarHorizontal = Mathf.Abs(targetCell.GlobalPosition.x - _owner.GlobalPosition.x) > 16f * 6f;
            return (!isBelowTarget && isFarHorizontal) || isBelowTarget;
        }

        private void OnPlayerPositionUpdated(Vector2 pos)
        {
            _playerPos = pos;
        }

        private void OnTimerTimeout()
        {
            if (!_owner.IsOnFloor()) return;

            _targetCellId = 0;
            _pathfindCells = Zone.Current.Pathfinder.GetGlobalPath((Owner as Node2D).GlobalPosition, _playerPos).ToList();

            if (IsInstanceValid(_line2d))
            {
                _line2d.QueueFree();
            }
            _line2d = new Line2D();
            _line2d.Width = 2f;
            foreach (var cell in _pathfindCells)
            {
                _line2d.AddPoint(cell.GlobalPosition);
            }
            Zone.Current.EffectsLayer.AddChild(_line2d);
        }
    }
}