using Godot;

namespace Deathville.GameObject
{
    public class Ragdoll : Node2D
    {
        [Export]
        private NodePath _headSpritePath;
        [Export]
        private NodePath _bodySpritePath;

        [Export]
        private NodePath _headRigidBodyPath;
        [Export]
        private NodePath _bodyRigidBodyPath;

        private Sprite _headSprite;
        private Sprite _bodySprite;
        private RigidBody2D _headRigidBody;
        private RigidBody2D _bodyRigidBody;

        public override void _Ready()
        {
            _headSprite = GetNodeOrNull<Sprite>(_headSpritePath ?? string.Empty);
            _bodySprite = GetNodeOrNull<Sprite>(_bodySpritePath ?? string.Empty);
            _bodyRigidBody = GetNodeOrNull<RigidBody2D>(_bodyRigidBodyPath ?? string.Empty);
            _headRigidBody = GetNodeOrNull<RigidBody2D>(_headRigidBodyPath ?? string.Empty);

            _headRigidBody.AngularVelocity = Main.RNG.RandfRange(0, 10f);
            _bodyRigidBody.AngularVelocity = Main.RNG.RandfRange(0, 10f);
        }

        public void Flip()
        {
            _headSprite.FlipH = true;
            _bodySprite.FlipH = true;
        }

        public void ApplyVelocity(Vector2 velocity)
        {
            _bodyRigidBody.LinearVelocity = velocity;
            _headRigidBody.LinearVelocity = velocity;
        }
    }
}