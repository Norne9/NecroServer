using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitOrk : Unit
    {
        private static readonly UnitStats OrkStats = new UnitStats(UnitStats.GetDefaultStats())
        {
            MaxHealth = 150f,
            AttackDelay = 0.76f,
            Damage = 10f
        };
        public UnitOrk(Config config, ushort unitId) : base(config, unitId, UnitFactory.MeshOrk, OrkStats)
        { }
    }
}
