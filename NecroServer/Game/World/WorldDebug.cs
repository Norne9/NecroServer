using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public void DebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DEBUG world info");
            sb.AppendLine($"WorldState: {WorldState}");
            sb.AppendLine($"Zone: {ZoneRadius}/{WorldScale}");
            sb.AppendLine($"Alive units: {Units.Where((u)=>u.IsAlive).Count()}/{Config.UnitCount}");
            sb.AppendLine($"Runes: {Runes.Count}/{Config.RuneCount}");
            if (Players != null)
            {
                sb.AppendLine($"Alive players: {AlivePlayers}/{Config.MaxPlayers}");
                foreach (var (_, player) in Players)
                    sb.AppendLine($"\t{player.Name}\t\t{(player.IsAlive ? $"{player.Units.Count}/{Config.MaxUnitCount}" : "DEAD")}\t{player.AvgPosition.X}\t{player.AvgPosition.Y}");
            }
            Logger.Log(sb.ToString());
        }
    }
}
