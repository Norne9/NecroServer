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

        private static Vector2 GetPos(int i, float zone, float time)
        {
            return RandomPosition.GetRandomPosition(i, time) * zone * RandomPosition.GetRandomFloat(i + 5, time);
        }

        public static void MakeStep(Config config, Player player, World world)
        {
            if (player.IsNeutrall) return;

            var enemyUnits = world.OverlapUnits(player.AvgPosition, (player.Units.FirstOrDefault()?.CurrentStats.ViewRadius ?? 1f) + 0.5f)
                .Where((u) => u.Owner != null && u.Owner != player && u.CurrentStats.UnitVisible);
            var enemyCount = enemyUnits.GroupBy((u) => u.Owner.UserId).Select((o) => o.First().Owner)
                .Select((o) => o.IsAI ? (o.IsNeutrall ? 6 : o.Units.Count) : 1).DefaultIfEmpty(0).Sum();

            Vector2 inputDir = RandomPosition.GetRandomPosition(byte.MaxValue + player.NetworkId * 17, 1f);
            float t = Math.Clamp((world.ZoneRadius * world.ZoneRadius * 0.8f) - player.AvgPosition.SqrLength(), 0f, 10f) / 10f;
            inputDir = inputDir * t + (player.AvgPosition * -1f) * (1f - t);

            bool rise = false;
            if (player.GetCooldown() < 0.3f)
            {
                var neutralUnits = world.OverlapUnits(player.AvgPosition, config.RiseRadius).Where((u) => u.Owner == null);
                rise = neutralUnits.Count() > 0;
                if (!rise)
                {
                    var nearUnits = world.OverlapUnits(player.AvgPosition, config.MaxViewRange);
                    var nearUnit = nearUnits.Where((u) => u.Owner == null).FirstOrDefault();
                    if (nearUnit != null) inputDir = nearUnit.Position - player.AvgPosition;
                }
            }
            else if (enemyCount > 0 && (player.Units.Count * 2 >= enemyCount || world.ZoneRadius < 3f || player.Units.Count == config.MaxUnitCount))
            {
                inputDir = Vector2.Empty;
            }

            player.SetInput(new Packets.ClientInput()
            {
                MoveX = inputDir.X,
                MoveY = inputDir.Y,
                Rise = rise
            });
        }
    }
}