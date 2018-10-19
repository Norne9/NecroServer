using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitOrk : Unit
    {
        private static readonly UnitStats _orkStats = new UnitStats(UnitStats.GetDefaultStats())
        {
            MaxHealth = 150f,
            AttackDelay = 0.76f,
            Damage = 12f
        };
        public UnitOrk(Config config, ushort unitId) : base(config, unitId, UnitFactory.MeshOrk, _orkStats)
        { }
    }
}
