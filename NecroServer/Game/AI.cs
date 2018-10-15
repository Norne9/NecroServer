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
        private static readonly string[] AiNames = new string[]
        {
            "Bluebeard", "Wasabeii", "Faustox",
            "Diviper", "Jhalmar", "Hironnoa",
            "Phipps", "Michel", "Forsart",
            "Tempus", "ReneCake", "Vatkuli",
            "Rastapro", "exturnel", "Mafubaa",
            "SombreReve", "oSumairu", "1zom",
            "McChimney", "JoeColandru", "BruceMc",
            "DaxBabiix", "kashmarko", "County",
            "marsu29", "MayDie", "Gerro",
            "Nouryoku", "Rakav", "Volton",
            "Sunyio", "Periwinkled", "skellor",
            "masu1", "Sasha27", "Remali",
            "sidefr", "zaques", "Dubbeli",
            "Nedeze", "Wladik32rus", "Mpoublack",
            "DoLuX"
        };
        private static readonly int NameOffset = GameMath.MathF.RandomInt(0, 1000);
        private static int AiUserId = -1;
        public static Player GetAiPlayer(Config config)
        {
            var id = AiUserId--;
            var name = AiNames[(Math.Abs(id) + NameOffset) % AiNames.Length];
            return new Player(id, id, name, false, config);
        }

        public static void MakeStep(Config config, Player player, World world)
        {
            var nearUnits = world.OverlapUnits(player.AvgPosition, config.ViewRange);
            var enemyUnits = world.OverlapUnits(player.AvgPosition, player.Units.FirstOrDefault()?.CurrentStats.ViewRadius ?? 1f)
                .Where((u) => u.Owner != null && u.Owner != player);
            var neutralUnits = world.OverlapUnits(player.AvgPosition, config.RiseRadius).Where((u) => u.Owner == null);

            bool fight = ((player.Units.Count >= enemyUnits.Count() - 2) && enemyUnits.Any()) || enemyUnits.Where((u)=> !u.Owner.IsAI).Any();
            bool rise = neutralUnits.Count() > 0;
            bool goCenter = !fight && player.AvgPosition.SqrLength() * 1.3f > world.ZoneRadius * world.ZoneRadius;

            Vector2 inputDir = (nearUnits.Where((u) => u.Owner == null).FirstOrDefault()?.Position ?? Vector2.Empty) - player.AvgPosition;
            if (enemyUnits.Any())
            {
                inputDir = Vector2.Empty;
                foreach (var u in enemyUnits)
                    inputDir += u.Position;
                inputDir /= enemyUnits.Count();
                inputDir = player.AvgPosition - inputDir;
            }
            if (goCenter)
                inputDir = Vector2.Empty - player.AvgPosition;
            else if (fight)
                inputDir = Vector2.Empty;

            player.SetInput(inputDir, rise);
        }
    }
}
