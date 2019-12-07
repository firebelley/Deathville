using Deathville.Environment;
using Deathville.GameObject.Combat;
using Godot;

namespace Deathville.Component
{
    public class ProjectileSpawnerComponent : Position2D
    {
        [Signal]
        public delegate void ProjectileSpawned();

        [Export]
        private PackedScene _scene;
        [Export]
        private NodePath _chamberPositionPath;
        [Export]
        private float _range = 250f;
        [Export]
        private float _speed = 250f;
        [Export]
        private float _force = 10f;

        private Node2D _chamberPosition;

        public override void _Ready()
        {
            _chamberPosition = GetNodeOrNull<Node2D>(_chamberPositionPath ?? string.Empty);
        }

        public Projectile Spawn(bool isPlayer, Vector2 toPos)
        {
            if (_scene == null) return null;

            var projectile = _scene.Instance() as Projectile;
            Zone.Current.EffectsLayer.AddChild(projectile);

            projectile.Range = _range;
            projectile.Speed = _speed;
            projectile.Force = _force;
            if (isPlayer)
            {
                projectile.SetPlayer();
            }
            else
            {
                projectile.SetEnemy();
            }
            projectile.Start(_chamberPosition.GlobalPosition, GlobalPosition, toPos);
            EmitSignal(nameof(ProjectileSpawned));
            return projectile;
        }
    }
}