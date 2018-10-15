using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitTroll : Unit
    {
        private static readonly UnitStats _trollStats = new UnitStats(UnitStats.GetDefaultStats()) {
            MaxHealth = 100f,
            AttackDelay = 1.33f,
            Damage = 10f
        };
        public UnitTroll(Config config, ushort unitId) : base(config, unitId, UnitFactory.MeshTroll, _trollStats)
        { }
    }
}
