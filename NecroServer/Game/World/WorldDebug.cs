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
            Logger.Log($"DEBUG drawing {nameof(_obstaclesTree)}");
            _obstaclesTree.DrawTree(drawRes, $"{nameof(_obstaclesTree)}.png");
            Logger.Log($"DEBUG drawing {nameof(_unitsTree)}");
            _unitsTree.DrawTree(drawRes, $"{nameof(_unitsTree)}.png");
            Logger.Log($"DEBUG drawing {nameof(_runesTree)}");
            _runesTree.DrawTree(drawRes, $"{nameof(_runesTree)}.png");
            Logger.Log("DEBUG drawing finished");
        }

        public void DebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DEBUG world info");
            sb.AppendLine($"WorldState: {_worldState}");
            sb.AppendLine($"Zone: {ZoneRadius}/{WorldScale}");
            sb.AppendLine($"Alive units: {_units.Where((u)=>u.IsAlive).Count()}/{_config.UnitCount}");
            sb.AppendLine($"Runes: {_runes.Count}/{_config.RuneCount}");
            if (_players != null)
            {
                sb.AppendLine($"Alive players: {_alivePlayers}/{_config.MaxPlayers}");
                foreach (var (_, player) in _players)
                    sb.AppendLine($"\t{player.Name}\t\t{(player.IsAlive ? $"{player.Units.Count}/{_config.MaxUnitCount}" : "DEAD")}\t{player.AvgPosition.X}\t{player.AvgPosition.Y}");
            }
            Logger.Log(sb.ToString());
        }
    }
}
