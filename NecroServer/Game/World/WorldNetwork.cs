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
        public void RemovePlayer(int networkId)
        {
            if (_players.ContainsKey(networkId))
            {
                var player = _players[networkId];
                if (player.IsAlive)
                {
                    Logger.Log($"GAME remove player '{player.Name}'");
                    for (int i = player.Units.Count - 1; i >= 0; i--)
                        player.Units[i].TakeDamage(null, float.MaxValue);
                    player.PlayerStatus.Place = _config.MaxPlayers;
                    OnPlayerDead?.Invoke(player.UserId, player.PlayerStatus);
                }
                _players.Remove(networkId);
            }
        }

        public void SetInput(int networkId, ClientInput input)
        {
            if (_players == null) return;
            if (_players.ContainsKey(networkId))
                _players[networkId].SetInput(input);
        }

        public int GetAvaliablePlaces() =>
            _units.Where((u) => !u.IsAlive).Count() / (1 + _config.AdditionalUnitCount);

        public (ServerFrame ServerFrame, List<UnitFrame> UnitFrame) GetServerData(Player player)
        {
            float viewRange = Math.Max(player.ViewZone.ViewRange + 2f, _config.MaxViewRange);
            bool FilterRect(Vector2 pos) =>
                (pos.X > player.ViewZone.MinX - 1f && pos.X < player.ViewZone.MaxX + 1f) && (pos.Y > player.ViewZone.MinY - 1f && pos.Y < player.ViewZone.MaxY + 1f);

            var visiblePlayers = _players.Where((p) => p.Value == player ||
                    (p.Value.IsAlive && (p.Value.UnitsEffect?.StatsChange.UnitVisible ?? true) &&
                    (p.Value.AvgPosition - player.AvgPosition).SqrLength() < viewRange * viewRange) &&
                    FilterRect(p.Value.AvgPosition))
                .Select((p) => p.Value.GetPlayerCameraInfo()).ToArray();

            var visibleUnits = OverlapUnits(player.AvgPosition, viewRange)
                .Where((u) => (u.CurrentStats.UnitVisible || u.Owner == player) && FilterRect(u.Position));
            var visibleUnitsData = visibleUnits.Select((u) => u.GetUnitInfo(this, player)).ToList();

            int[] visibleUnitsMap = new int[_units.Length / 32 + 1];
            foreach (var unit in visibleUnitsData)
                visibleUnitsMap.SetBit(unit.UnitId);

            var visibleRunes = _runesTree.Overlap<Rune>(player.AvgPosition, viewRange)
                .Select((r) => r.GetRuneInfo()).ToArray();

            var sFrame = new ServerFrame()
            {
                State = _worldState,
                PlayTime = _timeToEnd,
                ZoneSize = ZoneRadius,
                AlivePlayers = _alivePlayers,
                Cooldown = player.GetCooldown(),
                VisualEffect = player.UnitsEffect?.VisualEffect ?? VisualEffect.None,
                VisualEffectTime = (float)((player.UnitsEffect?.EndTime ?? DateTime.Now) - DateTime.Now).TotalSeconds,
                PlayerCameras = visiblePlayers,
                EnabledUnits = visibleUnitsMap,
                UnitKill = player.PlayerStatus.UnitKill,
                PlayerUnitCount = player.Units.Count,
                Runes = visibleRunes
            };
            var unitFrames = SplitList.Split(visibleUnitsData, visibleUnitsData.Count < 45 ? 12 : 24)
                .Select((lst) => new UnitFrame() { Units = lst.ToArray(), PacketId = UnitFrame.UnitsPacketId++ }).ToList();
            return (sFrame, unitFrames);
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
                UnitKill = player.PlayerStatus.UnitKill,
                MoneyEarn = player.PlayerStatus.MoneyEarn,
            };

        public ServerMap GetServerMap() =>
            new ServerMap()
            {
                GameMode = _gameMode, //TODO: Game mode selection
                Scale = WorldScale,
                Obstacles = _obstacles.Select((o) => o.GetObstacleInfo()).ToArray(),
                MaxPlayers = _config.MaxPlayers,
                MaxUnits = _config.MaxUnitCount,
                MapType = _mapType,
                RiseRadius = _config.RiseRadius,
                Color1 = 0x303D30,//0x303D30
                Color2 = 0x232D2A,
                Runes = _runes.Select((r) => r.GetRuneInfo()).ToArray(),
                Units = _units.Select((u) => u.GetUnitInfo(this, null)).ToArray(),
            };
    }
}
