using NecroServer;
using NecroServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using System.Linq;

namespace Game
{
    public static class AI
    {
        private static readonly string[] _aiNames = new string[]
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
        private static readonly int _nameOffset = GameMath.MathF.RandomInt(0, 1000);
        private static int _aiUserId = -1;

        public static Player GetAiPlayer(Config config)
        {
            var id = _aiUserId--;
            var name = _aiNames[(Math.Abs(id) + _nameOffset) % _aiNames.Length];
            return new Player(id, id, name, true, config);
        }

        public static Player GetNeutrallPlayer(Config config)
        {
            var id = -2000 + _aiUserId--;
            var name = "Neutrall";
            return new Player(id, id, name, false, config);
        }

        public static void MakeStep(Config config, Player player, World world)
        {
            if (player.IsNeutrall) return;

            var nearUnits = world.OverlapUnits(player.AvgPosition, config.MaxViewRange / 2f);
            var allEnemy = nearUnits.Where((u) => u.Owner != null && u.Owner != player && u.UnitStats.UnitVisible);
            var enemyCount = allEnemy.Count();
            var enemyUnits = world.OverlapUnits(player.AvgPosition, player.Units.FirstOrDefault()?.CurrentStats.ViewRadius ?? 1f)
                .Where((u) => u.Owner != null && u.Owner != player && u.UnitStats.UnitVisible);
            var neutralUnits = world.OverlapUnits(player.AvgPosition, config.RiseRadius).Where((u) => u.Owner == null);

            bool fight = enemyUnits.Any();

            bool rise = neutralUnits.Count() > 0;
            bool goCenter = !fight && player.AvgPosition.SqrLength() * 1.3f > world.ZoneRadius * world.ZoneRadius;

            Vector2 inputDir = RandomPosition.GetRandomPosition(byte.MaxValue + player.NetworkId);

            var nearUnit = nearUnits.Where((u) => u.Owner == null).FirstOrDefault();
            if (nearUnit != null) inputDir = nearUnit.Position - player.AvgPosition;

            if (fight)
                inputDir = Vector2.Empty;
            else if (goCenter)
                inputDir = Vector2.Empty - player.AvgPosition;
            else if (player.Units.Count < enemyCount)
            {
                inputDir = Vector2.Empty;
                foreach (var u in allEnemy)
                    inputDir += u.Position;
                inputDir /= allEnemy.Count();
                inputDir = player.AvgPosition - inputDir;
            }
            else if (enemyCount >= 1)
                inputDir = allEnemy.First().Position - player.AvgPosition;

            player.SetInput(new Packets.ClientInput()
            {
                MoveX = inputDir.X,
                MoveY = inputDir.Y,
                Rise = rise
            });
        }
    }
}