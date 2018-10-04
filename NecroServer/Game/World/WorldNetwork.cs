using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using GameMath;
using NecroServer;
using Packets;

namespace Game
{
    public partial class World
    {
        public void RemovePlayer(long networkId)
        {
            if (Players.ContainsKey(networkId))
            {
                var player = Players[networkId];
                if (player.IsAlive)
                {
                    Logger.Log($"GAME remove player '{player.Name}'");
                    for (int i = player.Units.Count - 1; i >= 0; i--)
                        player.Units[i].TakeDamage(null, player.Units[i].MaxHealth * 2f);
                }
                Players.Remove(networkId);
            }
        }

        public bool SetInput(long networkId, ClientInput input)
        {
            if (Players == null) return true;
            if (Players.ContainsKey(networkId))
            {
                Players[networkId].SetInput(new Vector2(input.MoveX, input.MoveY), input.Rise);
                return true;
            }
            else
            {
                Logger.Log($"SERVER unknown network id {networkId}");
                return false;
            }
        }

        public ServerFrame GetServerFrame(Player player)
        {
            var visiblePlayers = Players.Where((p) => p.Value == player ||
                    (p.Value.UnitsRune != RuneType.Stealth && (p.Value.AvgPosition - player.AvgPosition).SqrLength() < Config.ViewRange))
                .Select((p) => p.Value.GetPlayerCameraInfo()).ToArray();

            var visibleUnits = OverlapUnits(player.AvgPosition, Config.ViewRange)
                .Where((u) => u.Owner?.UnitsRune != RuneType.Stealth || u.Owner == player)
                .Select((u) => u.GetUnitInfo(this, player)).ToArray();

            var visibleRunes = RunesTree.Overlap<Rune>(player.AvgPosition, Config.ViewRange)
                .Select((r) => r.GetRuneInfo()).ToArray();

            return new ServerFrame()
            {
                State = WorldState,
                PlayTime = TimeToEnd,
                ZoneSize = ZoneRadius,
                AlivePlayers = AlivePlayers,
                Cooldown = player.GetCooldown(),
                PlayerCameras = visiblePlayers,
                Units = visibleUnits,
                Runes = visibleRunes
            };
        }

        public ServerEnd GetServerEnd(Player player) =>
            new ServerEnd()
            {
                AliveTime = player.PlayerStatus.AliveTime,
                Place = player.PlayerStatus.Place,
                Rating = player.PlayerStatus.Rating,
                DamageDeal = player.PlayerStatus.DamageDeal,
                DamageReceive = player.PlayerStatus.DamageReceive,
                UnitRise = player.PlayerStatus.UnitRise,
                UnitKill = player.PlayerStatus.UnitKill
            };

        public ServerMap GetServerMap() =>
            new ServerMap()
            {
                Scale = WorldScale,
                Obstacles = Obstacles.Select((o) => o.GetObstacleInfo()).ToArray(),
                MaxPlayers = Config.MaxPlayers,
                MapType = MapType
            };
    }
}
