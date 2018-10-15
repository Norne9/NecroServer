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

        private ushort _currentUnitId;
        private readonly List<ConstructorInfo> _unitProtos;
        private readonly Config _config;

        public UnitFactory(Config config)
        {
            _currentUnitId = 0;
            _config = config;

            var constructorTypes = new Type[] { typeof(Config), typeof(ushort) };

            var troll = typeof(UnitTroll).GetConstructor(constructorTypes);
            var ork = typeof(UnitOrk).GetConstructor(constructorTypes);
            var bear = typeof(UnitBear).GetConstructor(constructorTypes);
            var zombie = typeof(UnitZombie).GetConstructor(constructorTypes);

            _unitProtos = new List<ConstructorInfo>();
            void AddByChance(ConstructorInfo unitType, int chance)
            {
                for (int i = 0; i < chance; i++)
                    _unitProtos.Add(unitType);
            }

            AddByChance(troll, 4);
            AddByChance(zombie, 3);
            AddByChance(ork, 2);
            AddByChance(bear, 1);
        }

        public Unit MakeUnit() =>
            (Unit)_unitProtos[GameMath.MathF.RandomInt(0, _unitProtos.Count)]
            .Invoke(new object[] { _config, _currentUnitId++ });
    }
}
