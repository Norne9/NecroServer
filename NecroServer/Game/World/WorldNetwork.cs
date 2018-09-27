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
        public void SetInput(long NetworkId, ClientInput input)
        {
            if (Players.ContainsKey(NetworkId))
                Players[NetworkId].SetInput(new Vector2(input.MoveX, input.MoveY), input.Rise);
            else
                Logger.Log($"SERVER unknown network id {NetworkId}");
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
    }
}
