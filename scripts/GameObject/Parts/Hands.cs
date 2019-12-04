using Godot;

namespace Deathville.GameObject.Parts
{
    public class Hands : Node2D
    {
        private Node2D _scale;

        public override void _Ready()
        {
            _scale = GetNode<Node2D>("Scale");
        }

        public void AlignWithWeapon(Weapon weapon)
        {
            Scale = weapon.Scale;
            Rotation = weapon.Rotation;
            _scale.Scale = weapon.Sprite.Scale;
        }

        public void ResetTransforms()
        {
            Scale = Vector2.One;
            Rotation = 0f;
            _scale.Scale = Vector2.One;
        }
    }
}