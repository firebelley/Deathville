using Deathville.GameObject.Combat;
using Deathville.Singleton;
using Godot;
using GodotApiTools.Extension;

namespace Deathville.UI
{
    public class WeaponSwapUI : Control
    {
        private WeaponUI[] _weaponUIs = new WeaponUI[2];
        private AudioStreamPlayer _swapSound;

        public override void _Ready()
        {
            _weaponUIs[0] = GetNode<WeaponUI>("WeaponUI");
            _weaponUIs[1] = GetNode<WeaponUI>("WeaponUI2");
            _swapSound = GetNode<AudioStreamPlayer>("SwapSound");

            foreach (var ui in _weaponUIs)
            {
                ui.Hide();
            }
            _weaponUIs[0].PlaySwapAnimation();
            _weaponUIs[1].PlayStowAnimation();

            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerWeaponEquipped), this, nameof(OnPlayerWeaponEquipped));
            GameEventDispatcher.Instance.Connect(nameof(GameEventDispatcher.PlayerWeaponSwapped), this, nameof(OnPlayerWeaponSwapped));
        }

        private void OnPlayerWeaponEquipped(Weapon weapon)
        {
            foreach (var ui in _weaponUIs)
            {
                if (ui.CurrentWeapon == null)
                {
                    ui.ConnectWeapon(weapon);
                    ui.Show();
                    break;
                }
            }
        }

        private void OnPlayerWeaponSwapped(Weapon weapon)
        {
            foreach (var ui in _weaponUIs)
            {
                if (ui.CurrentWeapon == weapon)
                {
                    ui.PlaySwapAnimation();
                }
                else
                {
                    ui.PlayStowAnimation();
                }
            }
            _swapSound.PlayWithPitchRange(.9f, 1.1f);
        }
    }
}