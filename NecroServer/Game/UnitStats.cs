using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public struct UnitStats
    {
        public const float DefaultMoveSpeed = 4f;
        public const float DefaultViewRadius = 4f;
        public const float DefaultAttackRange = 0.5f;

        public float MaxHealth { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackDelay { get; set; }
        public float RiseTime { get; set; }
        public float ViewRadius { get; set; }
        public float AttackRange { get; set; }
        public float Damage { get; set; }
        public float TakeDamageMultiplier { get; set; }
        public float HealthPerSecond { get; set; }
        public bool UnitVisible { get; set; }
        public int KillsToUpgrade { get; set; }
        public bool ZoneDamage { get; set; }

        public UnitStats(UnitStats stats)
        {
            MaxHealth = stats.MaxHealth;
            MoveSpeed = stats.MoveSpeed;
            AttackDelay = stats.AttackDelay;
            RiseTime = stats.RiseTime;
            ViewRadius = stats.ViewRadius;
            AttackRange = stats.AttackRange;
            Damage = stats.Damage;
            TakeDamageMultiplier = stats.TakeDamageMultiplier;
            HealthPerSecond = stats.HealthPerSecond;
            UnitVisible = stats.UnitVisible;
            KillsToUpgrade = stats.KillsToUpgrade;
            ZoneDamage = stats.ZoneDamage;
        }

        public static UnitStats GetDefaultStats() =>
            new UnitStats()
            {
                MaxHealth = 100f,
                MoveSpeed = DefaultMoveSpeed,
                AttackDelay = 1f,
                RiseTime = 1.333f,
                ViewRadius = DefaultViewRadius,
                AttackRange = DefaultAttackRange,
                Damage = 10f,
                TakeDamageMultiplier = 1f,
                HealthPerSecond = 0f,
                UnitVisible = true,
                KillsToUpgrade = 3,
                ZoneDamage = true,
            };

        public static UnitStats GetDefaultEffect() =>
            new UnitStats()
            {
                MaxHealth = 1f,
                MoveSpeed = 1f,
                AttackDelay = 1f,
                RiseTime = 1f,
                ViewRadius = 1f,
                AttackRange = 1f,
                Damage = 1f,
                TakeDamageMultiplier = 1f,
                HealthPerSecond = 0f,
                UnitVisible = true,
                KillsToUpgrade = 3,
                ZoneDamage = true,
            };

        public static UnitStats GetUpgradeStats()
        {
            var upgradeStats = GetDefaultEffect();
            upgradeStats.TakeDamageMultiplier = 0.5f;
            upgradeStats.Damage = 1.2f;
            upgradeStats.HealthPerSecond = 8f;
            return upgradeStats;
        }

        public static UnitStats operator *(UnitStats u1, UnitStats u2) =>
            new UnitStats()
            {
                MaxHealth = u1.MaxHealth * u2.MaxHealth,
                MoveSpeed = u1.MoveSpeed * u2.MoveSpeed,
                AttackDelay = u1.AttackDelay * u2.AttackDelay,
                RiseTime = u1.RiseTime * u2.RiseTime,
                ViewRadius = u1.ViewRadius * u2.ViewRadius,
                AttackRange = u1.AttackRange * u2.AttackRange,
                Damage = u1.Damage * u2.Damage,
                TakeDamageMultiplier = u1.TakeDamageMultiplier * u2.TakeDamageMultiplier,
                HealthPerSecond = u1.HealthPerSecond + u2.HealthPerSecond,
                UnitVisible = u1.UnitVisible && u2.UnitVisible,
                KillsToUpgrade = u1.KillsToUpgrade < u2.KillsToUpgrade ? u1.KillsToUpgrade : u2.KillsToUpgrade,
                ZoneDamage = u1.ZoneDamage && u2.ZoneDamage,
            };
    }
}
