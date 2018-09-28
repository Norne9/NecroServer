using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using System.Linq;

namespace Game
{
    public static class AI
    {
        private static long AiUserId = -1;
        public static Player GetAiPlayer(Config config)
        {
            var id = AiUserId--;
            return new Player(id, id, "AI" + id, false, config);
        }

        public static void MakeStep(Config config, Player player, World world)
        {
            var nearUnits = world.OverlapUnits(player.AvgPosition, config.ViewRange);
            var enemyUnits = nearUnits.Where((u) => u.Owner != null && u.Owner != player);
            var neutralUnits = nearUnits.Where((u) => u.Owner == null);

            bool fight = player.Units.Count > enemyUnits.Count() && enemyUnits.Count() > 0;
            bool rise = neutralUnits.Count() > 0;
            bool goCenter = player.AvgPosition.SqrLength() * 1.1f > world.ZoneRadius * world.ZoneRadius;

            Vector2 inputDir = Vector2.Empty;
            if (goCenter)
                inputDir = Vector2.Empty - player.AvgPosition;
            else if (fight)
                inputDir = Vector2.Empty;
            else if (rise)
                inputDir = neutralUnits.First().Position - player.AvgPosition;

            player.SetInput(inputDir, rise);
        }
    }
}
