using Godot;

namespace Deathville
{
    public class Main : Node
    {
        public static RandomNumberGenerator RNG = new RandomNumberGenerator();

        public override void _Ready()
        {
            RNG.Randomize();
        }
    }
}