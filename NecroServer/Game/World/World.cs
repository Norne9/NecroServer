using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GameMath;
using NecroServer;

namespace Game
{
    public partial class World
    {
        public event Action<string> OnGameEnd;

        private Dictionary<long, Player> Players;

        private readonly PhysicalObject[] Obstacles;
        private readonly Unit[] Units;
        private readonly List<Rune> Runes = new List<Rune>();

        private readonly OcTree ObstaclesTree;
        private OcTree UnitsTree;
        private OcTree RunesTree;

        public readonly float WorldScale;
        private readonly BoundingBox WorldZone;
        private readonly int MapType;

        private readonly Config Config;

        private Stopwatch DtTimer = new Stopwatch();
        public float DeltaTime { get; private set; } = 0f;

        public float ZoneRadius { get; private set; }
        private WorldState WorldState = WorldState.Static;
        private DateTime StartTime = DateTime.Now;

        //for clients
        private int AlivePlayers = 0;
        private float TimeToEnd = 0f;
    }
}
