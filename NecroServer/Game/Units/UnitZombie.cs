﻿using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class UnitZombie : Unit
    {
        private static readonly UnitStats _zombieStats = new UnitStats(UnitStats.GetDefaultStats())
        {
            MaxHealth = 250f,
            AttackDelay = 0.83f,
            Damage = 16f
        };
        public UnitZombie(Config config, ushort unitId) : base(config, unitId, UnitFactory.MeshZombie, _zombieStats)
        { }
    }
}
