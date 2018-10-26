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
            var enemyUnits = world.OverlapUnits(player.AvgPosition, player.Units.FirstOrDefault()?.CurrentStats.ViewRadius ?? 1f)
                .Where((u) => u.Owner != null && u.Owner != player);
            var enemyCount = enemyUnits.Select((u) => u.Owner?.Units?.Count ?? 0).DefaultIfEmpty(0).Max();
            var neutralUnits = world.OverlapUnits(player.AvgPosition, config.RiseRadius).Where((u) => u.Owner == null);

            bool fight = false;
            if (config.GameMode != (int)GameMode.Royale)
                fight = ((player.Units.Count > enemyCount) && enemyCount > 0) || enemyUnits.Where((u) => !u.Owner.IsAI).Any();
            else
                fight = ((player.Units.Count >= enemyCount - 2) && enemyCount > 0) || enemyUnits.Where((u) => !u.Owner.IsAI).Any();

            bool rise = neutralUnits.Count() > 0;
            bool goCenter = !fight && player.AvgPosition.SqrLength() * 1.3f > world.ZoneRadius * world.ZoneRadius;

            var rndDir = RandomPosition.GetRandomPosition(byte.MaxValue + player.NetworkId);
            Vector2 inputDir = rndDir;
            var nearUnit = nearUnits.Where((u) => u.Owner == null).FirstOrDefault();
            if (nearUnit != null && config.GameMode == (int)GameMode.Royale)
                inputDir = nearUnit.Position - player.AvgPosition;
            if (player.GetCooldown() > 0f) inputDir = rndDir;
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

            player.SetInput(new Packets.ClientInput()
            {
                MoveX = inputDir.X,
                MoveY = inputDir.Y,
                Rise = rise
            });
        }
    }
}