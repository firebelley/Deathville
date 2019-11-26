using System.Collections.Generic;
using Deathville.Environment;
using Godot;
using GodotApiTools.Extension;
using GodotApiTools.Util;

namespace Deathville.Singleton
{
    public class MetadataLoader : Node
    {
        public static Dictionary<int, List<string>> LevelPieceToPath = new Dictionary<int, List<string>>();

        public override void _Ready()
        {
            LoadLevelPieces();
        }

        private void LoadLevelPieces()
        {
            LevelPieceToPath[LevelPiece.N] = new List<string>();
            LevelPieceToPath[LevelPiece.E] = new List<string>();
            LevelPieceToPath[LevelPiece.S] = new List<string>();
            LevelPieceToPath[LevelPiece.W] = new List<string>();

            var pieces = FileSystem.InstanceScenesInPath<LevelPiece>("res://scenes/Environment/LevelPieces/");
            foreach (var piece in pieces)
            {
                foreach (var flag in LevelPieceToPath.Keys)
                {
                    if ((piece.ConnectsVia & flag) > 0)
                    {
                        LevelPieceToPath[flag].Add(piece.Filename);
                    }
                }
                piece.QueueFree();
            }
        }
    }
}