using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using GameMath;
using NecroServer;

namespace Game
{
    public partial class World
    {
        public void StartGame(Dictionary<int, Player> players)
        {
            Players = players;
            Logger.Log($"GAME starting with {Players.Count} players");
            //Add ai players
            while (Players.Count < Config.MaxPlayers)
            {
                var aiPlayer = AI.GetAiPlayer(Config);
                Players.Add(aiPlayer.NetworkId, aiPlayer);
            }

            //Rise units for players
            foreach (var player in Players.Values)
            {
                var unit = RandomUnit();
                unit.Rise(player);
                if (player.DoubleUnits)
                {
                    for (int i = 0; i < Config.AdditionalUnitCount; i++)
                        NearUnit(unit).Rise(player);

                    var poses = UnitPosition.GetPositions(player.Units.Count);
                    var avgPosition = new Vector2(0, 0);
                    foreach (var unit1 in player.Units)
                        avgPosition += unit1.Position;
                    avgPosition /= player.Units.Count;

                    for (int i = 0; i < player.Units.Count; i++)
                        player.Units[i].Position = avgPosition + poses[i];
                }
            }

            StartTime = DateTime.Now;
            WorldState = WorldState.Static;
            Logger.Log($"GAME started");
        }
        private Unit RandomUnit()
        {
            var freeUnits = Units.Where((u) => u.Owner == null);
            return freeUnits.ElementAt(GameMath.MathF.RandomInt(0, freeUnits.Count()));
        }
        private Unit NearUnit(Unit unit) =>
            Units.Where((u) => u.Owner == null).OrderBy((u) => (u.Position - unit.Position).SqrLength()).First();

        public void Update()
        {
            //Calculate delta time
            DeltaTime = (float)DtTimer.Elapsed.TotalSeconds;
            DtTimer.Restart();

            //Rebuild unit tree
            UnitsTree = new OcTree(WorldZone, Units, true);

            //Get alive players count
            AlivePlayers = Players.Values.Where((p) => p.IsAlive).Count();

            //Kill all units if we have winner
            if (AlivePlayers == 1)
            {
                foreach (var unit in Units)
                    unit.TakeDamage(null, unit.MaxHealth * 2f);
            }

            //Update players
            foreach (var player in Players.Values)
            {
                player.Update(this);
                if (!player.IsAlive && player.PlayerStatus.Place == 0)
                {
                    player.PlayerStatus.Place = AlivePlayers > 0 ? AlivePlayers : 1;
                    if (!player.IsAI) OnPlayerDead?.Invoke(player.UserId, player.PlayerStatus);
                    Logger.Log($"GAME player dead {player.PlayerStatus.Place}/{Config.MaxPlayers}");
                }
            }

            //No more players - end game
            if (AlivePlayers == 0)
                OnGameEnd?.Invoke();

            //Zone processing
            switch (WorldState)
            {
                case WorldState.Static:
                    if ((DateTime.Now - StartTime).TotalSeconds > Config.StaticTime)
                    {
                        StartTime = DateTime.Now;
                        WorldState = WorldState.Resize;
                        BeginZoneRadius = ZoneRadius;
                        Logger.Log($"GAME zone begin");
                    }
                    TimeToEnd = Config.StaticTime - (float)(DateTime.Now - StartTime).TotalSeconds;
                    break;
                case WorldState.Resize:
                    float percent = (Config.ResizeTime - (float)(DateTime.Now - StartTime).TotalSeconds) / Config.ResizeTime;
                    if (percent < 0f) {
                        percent = 0; TargetZoneRadius = 0f;
                        StartTime = DateTime.Now; WorldState = WorldState.Static;
                    }
                    else
                    {
                        ZoneRadius = Lerp(BeginZoneRadius, TargetZoneRadius, 1f - percent);
                        TimeToEnd = Config.ResizeTime - (float)(DateTime.Now - StartTime).TotalSeconds;
                    }
                    break;
            }
            if (TimeToEnd < 0) TimeToEnd = 0;
        }

        private float Lerp(float from, float to, float percent) =>
            from * (1f - percent) + to * percent;
    }
}
