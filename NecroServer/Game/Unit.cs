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
        public (Vector2 stay, Vector2 move) TargetPos;
        public float BestDist;

        public ushort UnitId { get; }

        public byte UnitMesh { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float AttackDelay { get; }
        public float RiseTime { get; }
        public float ViewRadius { get; }
        public float AttackRange { get; }
        public float Damage { get; }
        
        public float Health { get; private set; } = 0f;
        public bool IsAlive { get => Health > 0f; }
        public Player Owner { get; private set; } = null;
        public bool Attack { get; private set; } = false;

        private float Rotation = 0f;
        private bool AttackCommnd = false;

        private DateTime lastAttack = DateTime.Now;
        private DateTime lastRise = DateTime.Now;

        private readonly Config Config;

        public Unit(Config config, ushort id, byte mesh, float maxHealth, float moveSpeed, float attackDelay, float viewRadius, float attackRange, float damage, float riseTime)
        {
            Config = config;
            Radius = 0.5f;

            UnitId = id;

            UnitMesh = mesh;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            AttackDelay = attackDelay;
            ViewRadius = viewRadius;
            AttackRange = attackRange;
            RiseTime = riseTime;
            Damage = damage;
        }

        public Unit(Config config, ushort id, Unit proto) : this(config, id,
            proto.UnitMesh, proto.MaxHealth, proto.MoveSpeed, proto.AttackDelay, proto.ViewRadius, proto.AttackRange, proto.Damage, proto.RiseTime)
        { Rotation = GameMath.MathF.RandomFloat(0f, GameMath.MathF.PI / 2f); }

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
            if ((DateTime.Now - lastRise).TotalSeconds < RiseTime) return;

            var (damage, speed) = ApplyRune();

            var lookDirection = CalcLook(); //Default look direction
            Attack = false; //Reset attack
            AttackCommnd = cmdAttack;
            if (cmdAttack) //Find target and attack or stay calm
            {
                var target = world.OverlapUnits(Position, ViewRadius)
                    .Where((u) => u.Owner != Owner && u.Owner != null && u.Owner.UnitsRune != RuneType.Stealth) //Find other player unit
                    .OrderBy((u) => (Position - u.Position).SqrLength()) //Sort by distance
                    .FirstOrDefault();
                if (target != null) lookDirection = (target.Position - Position); //Look at enemy

                //We cant do anithing if we attack
                if ((DateTime.Now - lastAttack).TotalSeconds > AttackDelay)
                {
                    if (target != null) //We have target
                    {
                        if ((target.Position - Position).SqrLength() > AttackRange * AttackRange) //We far
                            world.MoveUnit(this, CalcNewPos(target.Position, speed, world.DeltaTime)); //go
                        else
                        {
                            if (Owner?.UnitsRune == RuneType.Stealth) //Remove stealth
                                Owner.SetRune(RuneType.None);
                            Attack = true; //Play attack animation
                            lastAttack = DateTime.Now;
                            target.TakeDamage(this, damage);
                        }
                    }
                    else //No target - Just move
                    {
                        var tPos = CalcNewPos(cmdPos, speed, world.DeltaTime);
                        lookDirection = (tPos - Position);
                        world.MoveUnit(this, tPos);
                    }
                }
                else
                    Attack = true;
            }
            else //Just move
            {
                var tPos = CalcNewPos(cmdPos, speed, world.DeltaTime);
                lookDirection = (tPos - Position);
                world.MoveUnit(this, tPos);
            }

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
            if (lookDirection.SqrLength() < 0.001f)
                lookDirection = CalcLook();
            Rotation = System.MathF.Atan2(lookDirection.X, lookDirection.Y);
        }

        private Vector2 CalcLook() =>
            Owner?.SmallInput ?? new Vector2(0, 1);

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
            var vec = (target - Position);
            var tpDst = speed * dt;
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
            lastRise = DateTime.Now;
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
            damage *= GameMath.MathF.RandomFloat(1f - Config.RandomDamage / 2f, 1f + Config.RandomDamage / 2f);
            Health -= damage;

            if (damager?.Owner != null)
                damager.Owner.PlayerStatus.DamageDeal += damage;

            if (Owner != null)
                Owner.PlayerStatus.DamageReceive += damage;

            if (Owner?.UnitsRune == RuneType.Stealth) //Remove stealth
                Owner.SetRune(RuneType.None);

            if (!IsAlive) //Die
            {
                Health = 0f;
                if (damager == null)
                    Logger.Log($"GAME unit {UnitId}@{Owner?.Name ?? "null"} killed by zone");
                else
                    Logger.Log($"GAME unit {damager.UnitId}@{damager.Owner?.Name ?? "null"} killed {UnitId}@{Owner?.Name ?? "null"}");
                Owner?.Units.Remove(this);
                Owner = null;
                Attack = false;

                if (damager?.Owner != null)
                    damager.Owner.PlayerStatus.UnitKill++;
            }
        }

        public void Move(Vector2 newPosition, float dt, params OcTree[] trees)
        {
            Position = newPosition;

            if (CheckIntersect(trees, out Vector2 vec, out float pushPower))
                Position = CalcNewPos(Position + (Position - vec), MoveSpeed * (AttackCommnd ? 2.0f : pushPower), dt);
        }

        private bool CheckIntersect(OcTree[] trees, out Vector2 result, out float pushPower)
        {
            result = Vector2.Empty;
            var poses = new List<Vector2>();
            pushPower = 0.2f;
            foreach (var tree in trees)
            {
                var objs = tree.Overlap<PhysicalObject>(Position, Radius);
                foreach (var obj in objs)
                {
                    var unit = obj as Unit;
                    if (unit == null)
                    {
                        poses.Add(obj.Position);
                        pushPower = pushPower < 0.9f ? 0.9f : pushPower;
                        continue;
                    }
                    if (unit.Owner == null || obj == this) continue;
                    if (unit.Owner != Owner) pushPower = pushPower < 2.0f ? 2.0f : pushPower;
                    poses.Add(unit.Position);
                }
            }
            if (poses.Count == 0)
                return false;
            
            foreach (var vec in poses)
                result += vec;
            result /= poses.Count;
            return true;
        }
    }
}
