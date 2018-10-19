using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;

namespace Game
{
    public partial class World
    {
        public event Action OnGameEnd;
        public event Action<long, PlayerStatus> OnPlayerDead;

        private Dictionary<int, Player> _players;

        private readonly PhysicalObject[] _obstacles;
        private readonly Unit[] _units;
        private readonly List<Rune> _runes = new List<Rune>();

        private readonly OcTree _obstaclesTree;
        private OcTree _unitsTree;
        private OcTree _runesTree;

        public float WorldScale { get; }
        private readonly BoundingBox _worldZone;
        private readonly int _mapType;

        private readonly Config _config;
        
        public float DeltaTime { get; private set; } = 0f;

        public float ZoneRadius { get; private set; }
        private float _targetZoneRadius = 0f, _beginZoneRadius = 0f;
        private WorldState _worldState = WorldState.Static;
        private DateTime _startTime = DateTime.Now;

        private GameMode _gameMode = GameMode.Royale;

        //for clients
        private int _alivePlayers = 0;
        private float _timeToEnd = 0f;
    }
}
