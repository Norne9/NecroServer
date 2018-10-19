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
            this._players = players;
            Logger.Log($"GAME starting with {this._players.Count} players");
            //Add ai players
            while (this._players.Count < _config.MaxPlayers)
            {
                var aiPlayer = AI.GetAiPlayer(_config);
                this._players.Add(aiPlayer.NetworkId, aiPlayer);
            }

            AddNeutrallPlayer();

            //Rise units for players
            foreach (var player in this._players.Values)
            {
                var unit = RandomUnit();
                unit.Rise(player);
                if (player.DoubleUnits)
                {
                    for (int i = 0; i < _config.AdditionalUnitCount; i++)
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

            _startTime = DateTime.Now;
            _worldState = WorldState.Static;
            Logger.Log($"GAME started");
        }
        private Unit RandomUnit()
        {
            var freeUnits = _units.Where((u) => u.Owner == null);
            return freeUnits.ElementAt(GameMath.MathF.RandomInt(0, freeUnits.Count()));
        }
        private Unit NearUnit(Unit unit) =>
            _units.Where((u) => u.Owner == null).OrderBy((u) => (u.Position - unit.Position).SqrLength()).First();

        private void AddNeutrallPlayer()
        {
            var nPlayer = AI.GetNeutrallPlayer(_config);
            _players.Add(nPlayer.NetworkId, nPlayer);
            foreach (var unit in _units)
                if (unit is UnitBear)
                    unit.Rise(nPlayer);
        }

        public void Update()
        {
            //Calculate delta time
            DeltaTime = (float)_dtTimer.Elapsed.TotalSeconds;
            _dtTimer.Restart();

            //Rebuild unit tree
            _unitsTree = new OcTree(_worldZone, _units, true);

            //Get alive players count
            _alivePlayers = _players.Values.Where((p) => p.IsAlive && !p.IsNeutrall).Count();

            //Kill all units if we have winner
            if (_alivePlayers == 1)
            {
                foreach (var unit in _units)
                    unit.TakeDamage(null, float.MaxValue);
            }

            //Update players
            foreach (var player in _players.Values)
            {
                player.Update(this);
                if (!player.IsAlive && player.PlayerStatus.Place == 0)
                {
                    player.PlayerStatus.Place = _alivePlayers > 0 ? _alivePlayers : 1;
                    if (!player.IsAI) OnPlayerDead?.Invoke(player.UserId, player.PlayerStatus);
                    Logger.Log($"GAME player dead {player.PlayerStatus.Place}/{_config.MaxPlayers}");
                }
            }

            //No more players - end game
            if (_alivePlayers == 0)
                OnGameEnd?.Invoke();

            //Zone processing
            float elapsedTime = (float)(DateTime.Now - _startTime).TotalSeconds;
            switch (_worldState)
            {
                case WorldState.Static:
                    if (elapsedTime > _config.StaticTime)
                    {
                        _startTime = DateTime.Now;
                        _worldState = WorldState.Resize;
                        _beginZoneRadius = ZoneRadius;
                        Logger.Log($"GAME zone begin");
                    }
                    _timeToEnd = _config.StaticTime - elapsedTime;
                    break;
                case WorldState.Resize:
                    float percent = (_config.ResizeTime - elapsedTime) / _config.ResizeTime;
                    if (percent < 0f)
                    {
                        percent = 0; _targetZoneRadius = 0f;
                        _startTime = DateTime.Now; _worldState = WorldState.Static;
                    }
                    else
                    {
                        ZoneRadius = Lerp(_beginZoneRadius, _targetZoneRadius, 1f - percent);
                        _timeToEnd = _config.ResizeTime - elapsedTime;
                    }
                    break;
            }
            if (_timeToEnd < 0) _timeToEnd = 0;
        }

        private float Lerp(float from, float to, float percent) =>
            from * (1f - percent) + to * percent;
    }
}