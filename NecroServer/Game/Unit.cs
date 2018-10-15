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
        public UnitStats UnitStats { get; }
        public UnitStats CurrentStats { get; private set; }

        public float Health { get; private set; } = 0f;
        public bool IsAlive { get => Health > 0f; }
        public Player Owner { get; private set; } = null;
        public bool AttackAnimation { get; private set; } = false;

        private readonly List<Effect> UnitEffects = new List<Effect>();

        private float Rotation = 0f;
        private bool AttackCommnd = false;
        private Unit MyTarget = null;

        private DateTime lastAttack = DateTime.Now;
        private DateTime lastRise = DateTime.Now;

        private readonly Config Config;

        public Unit(Config config, ushort id, byte mesh, UnitStats stats)
        {
            Config = config;
            Radius = 0.5f;

            UnitId = id;

            UnitMesh = mesh;
            CurrentStats = UnitStats = stats;

            Rotation = GameMath.MathF.RandomFloat(0f, GameMath.MathF.PI / 2f);
        }

        public UnitInfo GetUnitInfo(World world, Player player) =>
            new UnitInfo()
            {
                UnitId = UnitId,
                UnitMesh = UnitMesh,
                PosX = (short)(Position.X / world.WorldScale * short.MaxValue),
                PosY = (short)(Position.Y / world.WorldScale * short.MaxValue),
                Rot = (byte)((Rotation > 0 ? Rotation : Rotation + GameMath.MathF.PI * 2f) / GameMath.MathF.PI / 2f * byte.MaxValue),
                Health = (byte)(Health / CurrentStats.MaxHealth * byte.MaxValue),
                Rune = Owner?.UnitsEffect?.VisualEffect ?? Effect.GetVisual(UnitEffects),
                Attack = AttackAnimation,
                PlayerOwned = Owner == player
            };

        public void Update(World world, Vector2 cmdPos, bool cmdAttack)
        {
            if (!IsAlive) return;
            CurrentStats = ApplyEffects(); //Process & apply effects
            if ((DateTime.Now - lastRise).TotalSeconds < CurrentStats.RiseTime) return; //We rising - don't do anything

            var lookDirection = CalcLook(); //Default look direction
            AttackAnimation = false; //Reset attack
            AttackCommnd = cmdAttack;
            if (cmdAttack) //Find target and attack or stay calm
            {
                MyTarget = SelectTarget(world);
                if (MyTarget != null) lookDirection = (MyTarget.Position - Position); //Look at enemy
                AttackAnimation = MyTarget != null; //Play attack animation

                //We cant do anithing if we attack
                if ((DateTime.Now - lastAttack).TotalSeconds > CurrentStats.AttackDelay)
                {
                    if (MyTarget != null) //We have target
                    {
                        var aRange = CurrentStats.AttackRange + MyTarget.Radius + Radius; aRange *= aRange;
                        if ((MyTarget.Position - Position).SqrLength() > aRange) //We far
                        {
                            world.MoveUnit(this, CalcNewPos(MyTarget.Position, CurrentStats.MoveSpeed, world.DeltaTime)); //go
                            AttackAnimation = false; //Disable attack animation
                        }
                        else
                        {
                            Effect.RemoveAttack(UnitEffects);
                            Owner?.UnitAttack();
                            lastAttack = DateTime.Now;
                            AttackUnit(MyTarget);
                        }
                    }
                    else //No target - Just move
                    {
                        var tPos = CalcNewPos(cmdPos, CurrentStats.MoveSpeed, world.DeltaTime);
                        lookDirection = (tPos - Position);
                        world.MoveUnit(this, tPos);
                    }
                }
                else
                    world.MoveUnit(this, Position); //stay
            }
            else //Just move
            {
                var tPos = CalcNewPos(cmdPos, CurrentStats.MoveSpeed, world.DeltaTime);
                lookDirection = (tPos - Position);
                world.MoveUnit(this, tPos);
            }

            //Take rune
            var rune = world.TakeRune(this);
            if (rune != null) Owner.TakeRune(rune);

            //Apply damage from zone
            if (Position.SqrLength() > world.ZoneRadius * world.ZoneRadius)
                TakeDamage(null, Config.ZoneDps * world.DeltaTime);

            //Calculate rotation
            if (lookDirection.SqrLength() < 0.001f)
                lookDirection = CalcLook();
            Rotation = System.MathF.Atan2(lookDirection.X, lookDirection.Y);

            //Apply health change
            if (CurrentStats.HealthPerSecond != 0)
                TakeDamage(this, -CurrentStats.HealthPerSecond);
        }

        protected virtual Unit SelectTarget(World world)
        {
            return world.OverlapUnits(Position, CurrentStats.ViewRadius)
                    .Where((u) => u.Owner != Owner && u.Owner != null && u.CurrentStats.UnitVisible) //Find other player unit
                    .OrderBy((u) => (Position - u.Position).SqrLength()) //Sort by distance
                    .FirstOrDefault();
        }

        protected virtual void AttackUnit(Unit target)
        {
            target.TakeDamage(this, CurrentStats.Damage);
        }

        private UnitStats ApplyEffects()
        {
            var result = new UnitStats(UnitStats);
            Effect.ProcessEffects(UnitEffects);
            foreach (var effect in UnitEffects)
                result *= effect.StatsChange;
            if (Owner?.UnitsEffect != null)
                result *= Owner.UnitsEffect.StatsChange;

            return result;
        }

        public void Rise(Player player)
        {
            if (Owner != null || player.Units.Count >= Config.MaxUnitCount)
                return;

            Logger.Log($"GAME player '{player.Name}' rised unit {UnitId}");
            Health = CurrentStats.MaxHealth;
            Owner = player;
            player.Units.Add(this);

            player.PlayerStatus.UnitRise++;
            lastRise = DateTime.Now;
        }

        public void Heal()
        {
            if (Health < CurrentStats.MaxHealth)
                Health += CurrentStats.MaxHealth * Config.HealValue;
            if (Health > CurrentStats.MaxHealth)
                Health = CurrentStats.MaxHealth;
        }

        public void TakeDamage(Unit damager, float damage)
        {
            if (!IsAlive) return;
            damage *= GameMath.MathF.RandomFloat(1f - Config.RandomDamage / 2f, 1f + Config.RandomDamage / 2f);
            damage *= CurrentStats.TakeDamageMultiplier;

            Health -= damage;
            if (Health > CurrentStats.MaxHealth)
                Health = CurrentStats.MaxHealth;

            damage = System.MathF.Max(0, System.MathF.Min(damage, CurrentStats.MaxHealth));
            if (damager?.Owner != null)
                damager.Owner.PlayerStatus.DamageDeal += damage;

            if (Owner != null)
                Owner.PlayerStatus.DamageReceive += damage;

            Owner?.UnitDamage();
            Effect.RemoveDamage(UnitEffects);

            if (!IsAlive) //Die
            {
                Health = 0f;
                UnitEffects.Clear();
                if (damager == null)
                    Logger.Log($"GAME unit {UnitId}@{Owner?.Name ?? "null"} killed by zone");
                else
                    Logger.Log($"GAME unit {damager.UnitId}@{damager.Owner?.Name ?? "null"} killed {UnitId}@{Owner?.Name ?? "null"}");
                Owner?.Units.Remove(this);
                Owner = null;
                AttackAnimation = false;

                if (damager?.Owner != null)
                    damager.Owner.PlayerStatus.UnitKill++;
            }
        }

        public void TakeEffect(Effect effect) =>
            Effect.AddEffect(UnitEffects, effect);

        #region UnitMovement

        private Vector2 CalcLook() =>
            Owner?.SmallInput ?? new Vector2(0, 1);
        private Vector2 CalcNewPos(Vector2 target, float speed, float dt)
        {
            var vec = (target - Position);
            var tpDst = speed * dt;
            if (vec.SqrLength() < tpDst * tpDst) //We near target - teleport
                return target;
            return Position + vec.Normalize() * dt * speed;
        }
        public void Move(Vector2 newPosition, float dt, params OcTree[] trees)
        {
            Position = newPosition;

            if (CheckIntersect(trees, out Vector2 vec, out float pushPower))
                Position = CalcNewPos(Position + (Position - vec), CurrentStats.MoveSpeed * (AttackCommnd ? 2.0f : pushPower), dt);
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

        #endregion
    }
}
