using Deathville.Component;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.GameObject
{
    public class Enemy : KinematicBody2D
    {
        private Vector2 _playerPos;

        public override void _Ready()
        {
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
            // var bullet = _resourcePreloader.InstanceScene<Projectile>();
            // Zone.Current.EffectsLayer.AddChild(bullet);
            // bullet.SetEnemy();
            // bullet.Start(GlobalPosition + Vector2.Up * 10f, GlobalPosition + Vector2.Up * 10f, _playerPos);
        }

        private void OnDamageReceived(float damage)
        {
            GameEventDispatcher.DispatchEnemyStruck();
        }
    }
}