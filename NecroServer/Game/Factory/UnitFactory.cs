using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Game
{
    public class UnitFactory
    {
        public const byte MeshTroll = 0;
        public const byte MeshOrk = 1;
        public const byte MeshBear = 2;
        public const byte MeshZombie = 3;

        private const float DefaultMoveSpeed = 4f;
        private const float DefaultViewRadius = 6f;
        private const float DefaultAttackRange = 0.5f;

        private ushort CurrentUnitId;
        private readonly List<ConstructorInfo> UnitProtos;
        private readonly Config Config;

        public UnitFactory(Config config)
        {
            CurrentUnitId = 0;
            Config = config;

            var constructorTypes = new Type[] { typeof(Config), typeof(ushort) };

            var troll = typeof(UnitTroll).GetConstructor(constructorTypes);
            var ork = typeof(UnitOrk).GetConstructor(constructorTypes);
            var bear = typeof(UnitBear).GetConstructor(constructorTypes);
            var zombie = typeof(UnitZombie).GetConstructor(constructorTypes);

            UnitProtos = new List<ConstructorInfo>();
            void AddByChance(ConstructorInfo unitType, int chance)
            {
                for (int i = 0; i < chance; i++)
                    UnitProtos.Add(unitType);
            }

            AddByChance(troll, 4);
            AddByChance(zombie, 3);
            AddByChance(ork, 2);
            AddByChance(bear, 1);
        }

        public Unit MakeUnit() =>
            (Unit)UnitProtos[GameMath.MathF.RandomInt(0, UnitProtos.Count)]
            .Invoke(new object[] { Config, CurrentUnitId++ });
    }
}
