using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public partial class World
    {
        public void DebugMap()
        {
            const int drawRes = 2048;

            Logger.Log("DEBUG drawing trees");
            Logger.Log($"DEBUG drawing {nameof(ObstaclesTree)}");
            ObstaclesTree.DrawTree(drawRes, $"{nameof(ObstaclesTree)}.png");
            Logger.Log($"DEBUG drawing {nameof(UnitsTree)}");
            UnitsTree.DrawTree(drawRes, $"{nameof(UnitsTree)}.png");
            Logger.Log($"DEBUG drawing {nameof(RunesTree)}");
            RunesTree.DrawTree(drawRes, $"{nameof(RunesTree)}.png");
            Logger.Log("DEBUG drawing finished");
        }
    }
}
