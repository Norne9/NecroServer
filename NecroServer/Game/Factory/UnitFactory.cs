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
        private const float DefaultAttackRange = 1.5f;

        private ushort CurrentUnitId;
        private readonly List<Unit> UnitProtos;
        private readonly Config Config;

        public UnitFactory(Config config)
        {
            CurrentUnitId = 0;
            Config = config;

            var troll = new Unit(config, 0, MeshTroll, 100f, DefaultMoveSpeed, 0.5f, DefaultViewRadius, DefaultAttackRange, 9f, 1.25f);
            var ork = new Unit(config, 0, MeshOrk, 150f, DefaultMoveSpeed, 1.0f, DefaultViewRadius, DefaultAttackRange, 20f, 1.25f);
            var bear = new Unit(config, 0, MeshBear, 250f, DefaultMoveSpeed, 1.0f, DefaultViewRadius, DefaultAttackRange, 23f, 1.25f);
            var zombie = new Unit(config, 0, MeshZombie, 100f, DefaultMoveSpeed, 0.6f, DefaultViewRadius, DefaultAttackRange, 9.5f, 1.25f);

            UnitProtos = new List<Unit>()
            { troll, troll, troll, zombie, zombie, zombie, ork, ork, bear, };
        }

        public Unit MakeUnit()
        {
            return new Unit(Config, CurrentUnitId++, UnitProtos[GameMath.MathF.RandomInt(0, UnitProtos.Count)]);
        }
    }
}
