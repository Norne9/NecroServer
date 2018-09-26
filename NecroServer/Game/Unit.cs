using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;
using System.Linq;

namespace Game
{
    public class Unit : PhysicalObject
    {
        public ushort UnitId { get; }

        public byte UnitMesh { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float AttackDelay { get; }
        public float ViewRadius { get; }
        public float AttackRange { get; }
        public float Damage { get; }
        
        public float Health { get; private set; } = 0f;
        public bool IsAlive { get => Health > 0f; }
        public Player Owner { get; private set; } = null;
        public bool Attack { get; private set; } = false;

        private Vector2 lookDirection = Vector2.One;

        private DateTime lastAttack = DateTime.Now;

        private readonly Config Config;

        public Unit(Config config, ushort id, byte mesh, float maxHealth, float moveSpeed, float attackDelay, float viewRadius, float attackRange, float damage)
        {
            Config = config;
            Radius = 1f;

            UnitId = id;

            UnitMesh = mesh;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            AttackDelay = attackDelay;
            ViewRadius = viewRadius;
            AttackRange = attackRange;
            Damage = damage;
        }

        public Unit(Config config, ushort id, Unit proto) : this(config, id,
            proto.UnitMesh, proto.MaxHealth, proto.MoveSpeed, proto.AttackDelay, proto.ViewRadius, proto.AttackRange, proto.Damage)
        { }

        public void Update(World world, Vector2 cmdPos, bool cmdAttack)
        {
            if (!IsAlive) return;

            var (damage, speed) = ApplyRune();

            lookDirection = (cmdPos - Position); //Default look direction
            Attack = false; //Reset attack

            if (cmdAttack) //Find target and attack or stay calm
            {
                var target = world.OverlapUnits(Position, ViewRadius)
                    .Where((u) => u.Owner != Owner && u.Owner != null) //Find other player unit
                    .OrderBy((u) => (Position - u.Position).SqrLength()) //Sort by distance
                    .FirstOrDefault();
                if (target != null) //We have target
                {
                    lookDirection = (target.Position - Position); //Look at enemy
                    if ((target.Position - Position).SqrLength() > AttackRange * AttackRange) //We far
                        world.MoveUnit(this, CalcNewPos(target.Position, speed, world.DeltaTime)); //go
                    else
                    {
                        Attack = true; //Play attack animation
                        if ((DateTime.Now - lastAttack).TotalSeconds > AttackDelay) //Is it time to attack?
                            target.TakeDamage(this, damage);
                    }
                }
                else //No target - Just move
                    world.MoveUnit(this, CalcNewPos(cmdPos, speed, world.DeltaTime));
            }
            else //Just move
                world.MoveUnit(this, CalcNewPos(cmdPos, speed, world.DeltaTime));

            //Take rune
            var rune = world.TakeRune(this);
            if (rune != RuneType.None)
                Owner.SetRune(rune);
        }

        private (float dmg, float spd) ApplyRune()
        {
            var damage = Damage;
            var speed = MoveSpeed;
            switch (Owner.UnitsRune)
            {
                case RuneType.Damage:
                    damage *= 2f;
                    break;
                case RuneType.Haste:
                    speed *= 2f;
                    break;
            }
            return (damage, speed);
        }

        private Vector2 CalcNewPos(Vector2 target, float speed, float dt)
        {
            var vec = (Position - target);
            var tpDst = speed * 2f * dt;
            if (vec.SqrLength() < tpDst * tpDst) //We near target - teleport
                return target;
            return Position + vec.Normalize() * dt * speed;
        }

        public void Rise(Player player)
        {
            if (player.Units.Count >= Config.MaxUnitCount)
                return;

            Logger.Log($"GAME player '{player.Name}' rised unit {UnitId}");
            Health = MaxHealth;
            Owner = player;
            player.Units.Add(this);
        }

        public void TakeDamage(Unit damager, float damage)
        {
            if (!IsAlive) return;
            damage = GameMath.MathF.RandomFloat(0.9f, 1.1f);
            Health -= damage;
            if (!IsAlive)
            {
                Logger.Log($"GAME unit {damager.UnitId}@{damager.Owner?.Name ?? "null"} killed {UnitId}@{Owner?.Name ?? "null"}");
                Owner?.Units.Remove(this);
                Owner = null;
            }
        }
    }
}
