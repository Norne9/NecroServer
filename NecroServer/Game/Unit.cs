using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;
using System.Linq;
using Packets;

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

        private Vector2 lastPosition = Vector2.Empty;
        private Vector2 lastDir = Vector2.Empty;
        private float Rotation = 0f;

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

        public UnitInfo GetUnitInfo(World world, Player player) =>
            new UnitInfo()
            {
                UnitId = UnitId,
                UnitMesh = UnitMesh,
                PosX = (short)(Position.X / world.WorldScale * short.MaxValue),
                PosY = (short)(Position.Y / world.WorldScale * short.MaxValue),
                Rot = (byte)((Rotation > 0 ? Rotation : Rotation + GameMath.MathF.PI * 2f) / GameMath.MathF.PI / 2f * byte.MaxValue),
                Health = (byte)(Health / MaxHealth * byte.MaxValue),
                Rune = Owner?.UnitsRune ?? RuneType.None,
                Attack = Attack,
                PlayerOwned = Owner == player
            };

        public void Update(World world, Vector2 cmdPos, bool cmdAttack)
        {
            if (!IsAlive) return;

            var (damage, speed) = ApplyRune();

            var lookDirection = CalcLook(); //Default look direction
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
                        {
                            lastAttack = DateTime.Now;
                            target.TakeDamage(this, damage);
                        }
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

            //Apply damage from zone
            if (Position.SqrLength() > world.ZoneRadius * world.ZoneRadius)
                TakeDamage(null, Config.ZoneDps * world.DeltaTime);
            //Instakil outside world
            if (Position.SqrLength() > world.WorldScale * world.WorldScale)
                TakeDamage(null, MaxHealth * 2f);

            //Calculate rotation
            Rotation = System.MathF.Atan2(lookDirection.Y, lookDirection.X);
        }

        private Vector2 CalcLook()
        {
            var dir = lastPosition - Position;
            if (dir.SqrLength() > 0f)
                lastDir = dir;
            lastPosition = Position;
            return lastDir;
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
            if (Owner != null || player.Units.Count >= Config.MaxUnitCount)
                return;

            Logger.Log($"GAME player '{player.Name}' rised unit {UnitId}");
            Health = MaxHealth;
            Owner = player;
            player.Units.Add(this);

            player.PlayerStatus.UnitRise++;
        }

        public void Heal()
        {
            if (Health < MaxHealth)
                Health += MaxHealth * Config.HealValue;
            if (Health > MaxHealth)
                Health = MaxHealth;
        }

        public void TakeDamage(Unit damager, float damage)
        {
            if (!IsAlive) return;
            damage = GameMath.MathF.RandomFloat(1f - Config.RandomDamage / 2f, 1f + Config.RandomDamage / 2f);
            Health -= damage;

            if (damager?.Owner != null)
                damager.Owner.PlayerStatus.DamageDeal += damage;

            if (Owner != null)
                Owner.PlayerStatus.DamageReceive += damage;

            if (!IsAlive)
            {
                if (damager == null)
                    Logger.Log($"GAME unit {UnitId}@{Owner?.Name ?? "null"} killed by zone");
                else
                    Logger.Log($"GAME unit {damager.UnitId}@{damager.Owner?.Name ?? "null"} killed {UnitId}@{Owner?.Name ?? "null"}");
                Owner?.Units.Remove(this);
                Owner = null;

                if (damager?.Owner != null)
                    damager.Owner.PlayerStatus.UnitKill++;
            }
        }
    }
}
