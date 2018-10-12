using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitFactory
    {
        private const byte MeshTroll  = 0;
        private const byte MeshOrk    = 1;
        private const byte MeshBear   = 2;
        private const byte MeshZombie = 3;

        private const float DefaultMoveSpeed = 4f;
        private const float DefaultViewRadius = 6f;
        private const float DefaultAttackRange = 0.5f;

        private ushort CurrentUnitId;
        private readonly List<Unit> UnitProtos;
        private readonly Config Config;

        public UnitFactory(Config config)
        {
            CurrentUnitId = 0;
            Config = config;

            var troll = new Unit(config, 0, MeshTroll, 100f, DefaultMoveSpeed, 1.33f, DefaultViewRadius, DefaultAttackRange, 10f, 1.33f);
            var ork = new Unit(config, 0, MeshOrk, 150f, DefaultMoveSpeed, 0.76f, DefaultViewRadius, DefaultAttackRange, 10f, 1.33f);
            var bear = new Unit(config, 0, MeshBear, 250f, DefaultMoveSpeed, 0.83f, DefaultViewRadius, DefaultAttackRange, 16f, 1.33f);
            var zombie = new Unit(config, 0, MeshZombie, 100f, DefaultMoveSpeed, 0.66f, DefaultViewRadius, DefaultAttackRange, 9f, 1.33f);

            UnitProtos = new List<Unit>()
            { troll, troll, troll, zombie, zombie, zombie, ork, ork, bear, };
        }

        public Unit MakeUnit() =>
            new Unit(Config, CurrentUnitId++, UnitProtos[GameMath.MathF.RandomInt(0, UnitProtos.Count)]);
    }
}
