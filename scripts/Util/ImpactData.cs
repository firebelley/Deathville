using Deathville.GameObject.Combat;
using Godot;
using GodotApiTools.Util;

namespace Deathville.Util
{
    public class ImpactData : Reference
    {
        public float Damage { get; private set; }
        public float Force { get; private set; }
        public Vector2 SourcePosition { get; private set; }
        public Vector2 Direction { get; private set; }

        public static ImpactData FromRaycastProjectile(Projectile projectile, RaycastResult raycastResult)
        {
            var data = FromProjectile(projectile);
            data.Direction = (raycastResult.ToPosition - raycastResult.FromPosition).Normalized();
            data.SourcePosition = raycastResult.Position;
            return data;
        }

        public static ImpactData FromAreaOfEffectProjectile(Projectile projectile, Vector2 sourcePosition)
        {
            var data = FromProjectile(projectile);
            data.SourcePosition = sourcePosition;
            return data;
        }

        public static ImpactData FromProjectile(Projectile projectile)
        {
            var data = new ImpactData();
            data.Damage = projectile.Damage;
            data.Force = projectile.Force;
            data.SourcePosition = projectile.GlobalPosition;
            return data;
        }

        private ImpactData() { }
    }
}