using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitBear : Unit
    {
        private static readonly UnitStats BearStats = new UnitStats(UnitStats.GetDefaultStats())
        {
            MaxHealth = 250f,
            AttackDelay = 0.83f,
            Damage = 16f
        };
        public UnitBear(Config config, ushort unitId) : base(config, unitId, UnitFactory.MeshBear, BearStats)
        { }
    }
}
