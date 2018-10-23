using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitWitch : Unit
    {
        private static readonly UnitStats _witchStats = new UnitStats(UnitStats.GetDefaultStats())
        {
            MaxHealth = 150f,
            AttackDelay = 1.5f,
            Damage = 40f,
            AttackRange = 3.5f,
        };
        public UnitWitch(Config config, ushort unitId) : base(config, unitId, UnitFactory.MeshWitch, _witchStats)
        { }
    }
}
