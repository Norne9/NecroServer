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

        public bool SetInput(int networkId, ClientInput input)
        {
            if (_players == null) return true;
            if (_players.ContainsKey(networkId))
            {
                _players[networkId].SetInput(new Vector2(input.MoveX, input.MoveY), input.Rise);
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
            var visiblePlayers = _players.Where((p) => p.Value == player ||
                    (p.Value.IsAlive && (p.Value.UnitsEffect?.StatsChange.UnitVisible ?? true) &&
                    (p.Value.AvgPosition - player.AvgPosition).SqrLength() < _config.ViewRange * _config.ViewRange))
                .Select((p) => p.Value.GetPlayerCameraInfo()).ToArray();

            var visibleUnits = OverlapUnits(player.AvgPosition, _config.ViewRange)
                .Where((u) => u.CurrentStats.UnitVisible || u.Owner == player);
            if (visibleUnits.Count() > _config.MaxUnitsPacket)
                visibleUnits = visibleUnits.OrderBy((u) => (u.Position - player.AvgPosition).SqrLength()).Take(_config.MaxUnitsPacket);
            var visibleUnitsData = visibleUnits.Select((u) => u.GetUnitInfo(this, player)).ToArray();

            var visibleRunes = _runesTree.Overlap<Rune>(player.AvgPosition, _config.ViewRange)
                .Select((r) => r.GetRuneInfo()).ToArray();

            return new ServerFrame()
            {
                State = _worldState,
                PlayTime = _timeToEnd,
                ZoneSize = ZoneRadius,
                AlivePlayers = _alivePlayers,
                Cooldown = player.GetCooldown(),
                VisualEffect = player.UnitsEffect?.VisualEffect ?? VisualEffect.None,
                VisualEffectTime = (float)((player.UnitsEffect?.EndTime ?? DateTime.Now) - DateTime.Now).TotalSeconds,
                PlayerCameras = visiblePlayers,
                Units = visibleUnitsData,
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
                UnitKill = player.PlayerStatus.UnitKill,
                MoneyEarn = player.PlayerStatus.MoneyEarn,
            };

        public ServerMap GetServerMap() =>
            new ServerMap()
            {
                GameMode = GameMode.Royale, //TODO: Game mode selection
                Scale = WorldScale,
                Obstacles = _obstacles.Select((o) => o.GetObstacleInfo()).ToArray(),
                MaxPlayers = _config.MaxPlayers,
                MaxUnits = _config.MaxUnitCount,
                MapType = _mapType,
                RiseRadius = _config.RiseRadius,
                Runes = _runes.Select((r) => r.GetRuneInfo()).ToArray(),
                Units = _units.Select((u) => u.GetUnitInfo(this, null)).ToArray(),
            };
    }
}
