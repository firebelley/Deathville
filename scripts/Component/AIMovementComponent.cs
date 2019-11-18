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
        private NodePath _velocityComponentPath;
        [Export]
        private NodePath _entityAnimationComponentPath;
        [Export]
        private NodePath _weaponSocketComponentPath;

        public Vector2 TargetPosition;

        private KinematicBody2D _owner;
        private VelocityComponent _velocityComponent;
        private EntityAnimationComponent _entityAnimationComponent;
        private WeaponSocketComponent _weaponSocketComponent;

        private Line2D _line2d;
        private StateMachine<MoveState> _stateMachine = new StateMachine<MoveState>();
        private List<Pathfinder.PathfindCell> _pathfindCells = new List<Pathfinder.PathfindCell>();
        private int _targetCellId;
        private bool _shouldGeneratePath = true;

        public override void _Ready()
        {
            _stateMachine.AddState(MoveState.GROUNDED, StateGrounded);
            _stateMachine.AddState(MoveState.AIRBORNE, StateAirborne);
            _stateMachine.SetInitialState(MoveState.GROUNDED);

            _owner = Owner as KinematicBody2D;
            _velocityComponent = GetNode<VelocityComponent>(_velocityComponentPath);
            _entityAnimationComponent = GetNode<EntityAnimationComponent>(_entityAnimationComponentPath);
            _weaponSocketComponent = GetNode<WeaponSocketComponent>(_weaponSocketComponentPath);

            GetNode<Timer>("Timer").Connect("timeout", this, nameof(OnTimerTimeout));
        }

        public override void _Process(float delta)
        {
            _stateMachine.Update();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (_shouldGeneratePath && _owner.IsOnFloor())
            {
                GetNewPath();
            }
            else if (_targetCellId < _pathfindCells.Count)
            {
                var targetPos = GetTargetPosition();
                if (_owner.IsOnFloor() && _owner.GlobalPosition.DistanceSquaredTo(targetPos) <= MAX_AHEAD * MAX_AHEAD)
                {
                    _targetCellId++;
                }
            }
        }

        private void StateGrounded()
        {
            var dir = GetTargetDirection();
            if (dir.x != 0f)
            {
                _velocityComponent.Accelerate(dir);
            }
            else
            {
                _velocityComponent.Decelerate();
            }

            _velocityComponent.MoveWithSnap();

            if (GetTargetPosition() != Vector2.Zero && ShouldJump(GetTargetPathCell()))
            {
                var jumpSpeed = GetJumpSpeed(GetTargetPathCell());
                _velocityComponent.Jump(jumpSpeed);
                _stateMachine.ChangeState(StateAirborne);
            }
            else if (!_owner.IsOnFloor())
            {
                _stateMachine.ChangeState(StateAirborne);
            }

            UpdateAnimations();
        }

        private void StateAirborne()
        {
            _velocityComponent.Accelerate(GetTargetDirection());
            _velocityComponent.ApplyGravity();
            _velocityComponent.Move();

            if (_owner.IsOnFloor())
            {
                _stateMachine.ChangeState(StateGrounded);
            }

            UpdateAnimations();
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

        private float GetJumpSpeed(Pathfinder.PathfindCell targetCell)
        {
            var dir = (targetCell.GlobalPosition - _owner.GlobalPosition);
            var angle = dir.Angle();
            var percent = 1f;

            if (angle > 0)
            {
                var halfPi = Mathf.Pi * .5f;
                percent = Mathf.Abs((1f - (angle / halfPi))) * .75f;
            }

            var length = (targetCell.GlobalPosition - _owner.GlobalPosition).Length();
            var maxJumpHeight = (length) * percent;
            var jumpVel = Mathf.Sqrt(2f * 800f * maxJumpHeight);
            return jumpVel;
        }

        private bool ShouldJump(Pathfinder.PathfindCell targetCell)
        {
            var isBelowTarget = _owner.GlobalPosition.y - targetCell.GlobalPosition.y >= 16f;
            // this doesn't work because it can pathfind to the middle of a platform
            // in other words, the pathfinding for jumping is not only corner to corner
            var isFarHorizontal = Mathf.Abs(targetCell.GlobalPosition.x - _owner.GlobalPosition.x) > 16f * 6f;
            return (!isBelowTarget && isFarHorizontal) || isBelowTarget;
        }

        private void GetNewPath()
        {
            if (TargetPosition == Vector2.Zero) return;

            _shouldGeneratePath = false;
            _targetCellId = 0;
            var offset = Mathf.Sign(_velocityComponent.Velocity.x) * MAX_AHEAD;
            var offsetv = new Vector2(offset, 0f);
            _pathfindCells = Zone.Current.Pathfinder.GetGlobalPath((Owner as Node2D).GlobalPosition + offsetv, TargetPosition).ToList();

            // if (IsInstanceValid(_line2d))
            // {
            //     _line2d.QueueFree();
            // }
            // _line2d = new Line2D();
            // _line2d.Width = 2f;
            // foreach (var cell in _pathfindCells)
            // {
            //     _line2d.AddPoint(cell.GlobalPosition);
            // }

            // Zone.Current.EffectsLayer.AddChild(_line2d);
        }

        private void UpdateAnimations()
        {
            var dir = GetTargetDirection();
            if (dir.x != 0f)
            {
                _entityAnimationComponent.Play(EntityAnimationComponent.ANIM_RUN);
                var shouldFlip = false;
                if (_weaponSocketComponent != null)
                {
                    shouldFlip = _weaponSocketComponent.FacingLeft;
                }
                else
                {
                    shouldFlip = dir.x < 0f;
                }
                _entityAnimationComponent.Flip(shouldFlip);
            }
            else
            {
                _entityAnimationComponent.Play(EntityAnimationComponent.ANIM_IDLE);
            }
        }

        private void OnTimerTimeout()
        {
            _shouldGeneratePath = true;
        }
    }
}