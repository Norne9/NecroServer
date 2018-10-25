using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MasterReqResp;

namespace Game
{
    public class UnitFactory
    {
        public const byte MeshTroll = 0;
        public const byte MeshOrk = 1;
        public const byte MeshBear = 2;
        public const byte MeshZombie = 3;
        public const byte MeshWitch = 4;

        private ushort _currentUnitId;
        private readonly List<UnitProto> _unitProtos;
        private readonly Config _config;

        public UnitFactory(Config config)
        {
            _currentUnitId = 0;
            _config = config;

            var units = new MasterClient(_config).RequestUnits().GetAwaiter().GetResult()?.Units;
            if (units == null || units.Count == 0)
                throw new Exception("Empty unit array");

            _unitProtos = new List<UnitProto>();
            void AddByChance(UnitProto unitType, int chance)
            {
                for (int i = 0; i < chance; i++)
                    _unitProtos.Add(unitType);
            }

            foreach (var unit in units)
            {
                Logger.Log($"UNITS loaded '{unit.UnitName}'");
                AddByChance(unit, unit.UnitRate);
            }
        }

        public Unit MakeUnit()
        {
            var proto = _unitProtos[GameMath.MathF.RandomInt(0, _unitProtos.Count)];
            return new Unit(_config, _currentUnitId++, (byte)proto.UnitMesh, proto.UnitStats, proto.SpawnNeutrall, proto.UnitRadius);
        }
    }
}
