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
            _players = players;
            Logger.Log($"GAME starting with {_players.Count} players");

            AddNeutrallPlayer();

            //Rise units for players
            foreach (var player in _players.Values)
                AddPlayer(player);

            //Add AI players
            switch (_gameMode)
            {
                case GameMode.Royale:
                    AppendAiPlayers(_config.MaxPlayers);
                    break;
                case GameMode.Free:
                    AppendAiPlayers(_config.MaxAiPlayers);
                    break;
            }

            _startTime = DateTime.Now;
            _worldState = WorldState.Static;
            Logger.Log($"GAME started");
        }

        public void AddPlayer(Player player)
        {
            if (!_players.ContainsKey(player.NetworkId))
                _players.Add(player.NetworkId, player);

            var unit = RandomUnit();
            if (unit == null) {
                Logger.Log("GAME error - no more units", true);
                return;
            }
            unit.Rise(player);
            unit.Position = FindSpawnPoint();
            if (player.DoubleUnits)
            {
                for (int i = 0; i < _config.AdditionalUnitCount; i++)
                    NearUnit(unit)?.Rise(player);

                var poses = UnitPosition.GetPositions(player.Units.Count);
                var targetPosition = FindSpawnPoint();
                for (int i = 0; i < player.Units.Count; i++)
                    player.Units[i].Position = targetPosition + poses[i];
            }

            _unitsTree = new OcTree(_worldZone, _units, true);
        }
        public bool AppendAiPlayers(int count)
        {
            var result = false;
            var toRemove = new List<int>(_players.Values.Where((u) => u.IsAI && !u.IsAlive).Select((u) => u.NetworkId));
            result = toRemove.Count() > 0;
            foreach (var id in toRemove)
                _players.Remove(id);
            //Add ai players
            while (_players.Count < count + 1)
            {
                var aiPlayer = AI.GetAiPlayer(_config);
                AddPlayer(aiPlayer);
                result = true;
            }
            return result;
        }

        private Vector2 FindSpawnPoint()
        {
            Vector2 spawnPoint = Vector2.Empty;
            for (int i = 0; i < byte.MaxValue; i++)
            {
                spawnPoint = GetPointInCircle(ZoneRadius);
                if (_unitsTree.Overlap<Unit>(spawnPoint, _config.RiseRadius).Count == 0)
                    return spawnPoint;
            }
            return spawnPoint;
        }

        private Unit RandomUnit()
        {
            var freeUnits = _units.Where((u) => u.Owner == null);
            if (!freeUnits.Any()) return null;
            return freeUnits.ElementAt(GameMath.MathF.RandomInt(0, freeUnits.Count()));
        }
        private Unit NearUnit(Unit unit) =>
            _units.Where((u) => u.Owner == null).OrderBy((u) => (u.Position - unit.Position).SqrLength()).FirstOrDefault();

        private void AddNeutrallPlayer()
        {
            var nPlayer = AI.GetNeutrallPlayer(_config);
            _players.Add(nPlayer.NetworkId, nPlayer);
            foreach (var unit in _units)
                if (unit.SpawnNeutrall)
                    unit.Rise(nPlayer);
        }

        public void Update(float deltaTime)
        {
            DeltaTime = deltaTime;

            //Rebuild unit tree
            _unitsTree = new OcTree(_worldZone, _units, true);

            //Get alive players count
            if (_gameMode == GameMode.Royale)
            {
                _alivePlayers = _players.Values.Where((p) => p.IsAlive && !p.IsNeutrall).Count();
                //Kill all units if we have winner
                if (_alivePlayers == 1)
                {
                    foreach (var unit in _units)
                        unit.TakeDamage(null, float.MaxValue);
                }
            }
            else
                _alivePlayers = _players.Values.Where((p) => p.IsAlive && !p.IsAI).Count();

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

            //Mode processing
            switch (_gameMode)
            {
                case GameMode.Royale: //Royale mode - process zone
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
                    break;
                case GameMode.Free: //Free mode - no zone
                    _timeToEnd = 0;
                    //Add runes if empty
                    if (_runes.Count == 0)
                    {
                        Logger.Log("WORLD creating runes");
                        _runes.AddRange(GenerateRunes(_config.RuneCount));
                        Logger.Log("WORLD creating rune tree");
                        _runesTree = new OcTree(_worldZone, _runes, true);
                    }
                    break;
            }
        }

        private float Lerp(float from, float to, float percent) =>
            from * (1f - percent) + to * percent;
    }
}