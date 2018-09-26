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
        private Dictionary<long, Player> Players;

        private readonly PhysicalObject[] Obstacles;
        private readonly Unit[] Units;
        private readonly List<Rune> Runes = new List<Rune>();

        private readonly OcTree ObstaclesTree;
        private OcTree UnitsTree;
        private OcTree RunesTree;

        private readonly float WorldScale;
        private readonly BoundingBox WorldZone;
        private readonly int MapType;

        private readonly Config Config;

        private Stopwatch DtTimer = new Stopwatch();
        public float DeltaTime { get; private set; } = 0f;
    }
}
