using Deathville.Component;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject
{
    public class Enemy : KinematicBody2D
    {
        private Vector2 _playerPos;
        private ResourcePreloader _resourcePreloader;

        public override void _Ready()
        {
            _resourcePreloader = GetNode<ResourcePreloader>("ResourcePreloader");
            this.GetFirstNodeOfType<DamageReceiverComponent>()?.Connect(nameof(DamageReceiverComponent.DamageReceived), this, nameof(OnDamageReceived));
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerPositionUpdated), this, nameof(OnPlayerPositionUpdated));
            GetNode<Timer>("Timer").Connect("timeout", this, nameof(OnTimerTimeout));
        }

        private void OnPlayerPositionUpdated(Vector2 pos)
        {
            _playerPos = pos;
        }

        private void OnTimerTimeout()
        {
            var bullet = _resourcePreloader.InstanceScene<Projectile>();
            Zone.Current.EffectsLayer.AddChild(bullet);
            bullet.Start(GlobalPosition + Vector2.Up * 10f, _playerPos);
            bullet.SetEnemy();
        }

        private void OnDamageReceived(float damage)
        {
            GameEventDispatcher.DispatchEnemyStruck();
        }
    }
}