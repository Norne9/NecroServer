using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;
using System.Linq;
using Packets;
using MasterReqResp;

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

        public bool Upgraded { get; private set; } = false;
        public bool SpawnNeutrall { get; }
        public byte UnitSkin { get; private set; } = 0;

        public float Kills { get; set; } = 0;

        private readonly List<Effect> _unitEffects = new List<Effect>();

        private float _rotation = 0f;
        private bool _attackCommand = false;
        private Unit _myTarget = null;

        private DateTime _lastAttack = DateTime.Now;
        private DateTime _lastRise = DateTime.Now;

        private readonly Config _config;

        public Unit(Config config, ushort id, byte mesh, UnitStats stats, bool spawnNeutrall, float radius = 0.5f)
        {
            SpawnNeutrall = spawnNeutrall;
            _config = config;
            Radius = radius;

            UnitId = id;

            UnitMesh = mesh;
            CurrentStats = UnitStats = stats;

            _rotation = GameMath.MathF.RandomFloat(0f, GameMath.MathF.PI / 2f);
        }

        public UnitInfo GetUnitInfo(World world, Player player)
        {
            const byte DefaultRadius = (byte)(0.5f / 10f * byte.MaxValue);

            var unitRadius = (byte)(Radius / 10f * byte.MaxValue);
            var playerOwned = Owner == player;
            var visualEffect = Owner?.UnitsEffect?.VisualEffect ?? Effect.GetVisual(_unitEffects);

            return new UnitInfo()
            {
                UnitId = UnitId,
                UnitMesh = UnitMesh,
                PosX = (short)(Position.X / world.WorldScale * short.MaxValue),
                PosY = (short)(Position.Y / world.WorldScale * short.MaxValue),
                
                Attack = AttackAnimation,
                PlayerOwned = playerOwned,
                HasEffect = visualEffect != VisualEffect.None,
                HasHealth = playerOwned && Health > 0,
                Alive = Health > 0,
                HasExp = playerOwned && !Upgraded,
                Upgrade = Upgraded,
                CustomSize = unitRadius != DefaultRadius,

                Health = (byte)(Health / CurrentStats.MaxHealth * byte.MaxValue),
                Rot = (byte)((_rotation > 0 ? _rotation : _rotation + GameMath.MathF.PI * 2f) / GameMath.MathF.PI / 2f * byte.MaxValue),
                VisualEffect = visualEffect,
                Target = _myTarget?.UnitId ?? UnitId,
                Exp = (byte)(Kills / CurrentStats.KillsToUpgrade * byte.MaxValue),
                UnitSkin = UnitSkin,
                UnitSize = unitRadius,
            };
        }

        public void Update(World world, Vector2 cmdPos, bool cmdAttack)
        {
            if (!IsAlive) return;
            CurrentStats = ApplyEffects(); //Process & apply effects
            if ((DateTime.Now - _lastRise).TotalSeconds < CurrentStats.RiseTime) return; //We rising - don't do anything

            var lookDirection = CalcLook(); //Default look direction
            AttackAnimation = false; //Reset attack
            _attackCommand = cmdAttack;
            if (cmdAttack) //Find target and attack or stay calm
            {
                _myTarget = SelectTarget(world);
                if (_myTarget != null) lookDirection = (_myTarget.Position - Position); //Look at enemy
                AttackAnimation = _myTarget != null; //Play attack animation

                //We can attack
                if ((DateTime.Now - _lastAttack).TotalSeconds > CurrentStats.AttackDelay)
                {
                    if (_myTarget != null) //We have target
                    {
                        var aRange = CurrentStats.AttackRange + _myTarget.Radius + Radius; aRange *= aRange;
                        if ((_myTarget.Position - Position).SqrLength() > aRange) //We far
                        {
                            world.MoveUnit(this, CalcNewPos(_myTarget.Position, CurrentStats.MoveSpeed, world.DeltaTime)); //go
                            AttackAnimation = false; //Disable attack animation
                        }
                        else
                        {
                            Effect.RemoveAttack(_unitEffects);
                            Owner?.UnitAttack();
                            _lastAttack = DateTime.Now;
                            AttackUnit(_myTarget);
                        }
                    }
                    else //No target - Just move
                    {
                        var tPos = CalcNewPos(cmdPos, CurrentStats.MoveSpeed, world.DeltaTime);
                        lookDirection = (tPos - Position);
                        world.MoveUnit(this, tPos);
                    }
                }
                else //We cant attack
                {
                    AttackAnimation = true;
                    CurrentStats = new UnitStats(CurrentStats) { MoveSpeed = 0.25f };
                    world.MoveUnit(this, CalcNewPos(Position, CurrentStats.MoveSpeed, world.DeltaTime)); //stay
                }
            }
            else //Just move
            {
                _myTarget = null;
                var tPos = CalcNewPos(cmdPos, CurrentStats.MoveSpeed, world.DeltaTime);
                lookDirection = (tPos - Position);
                world.MoveUnit(this, tPos);
            }

            //Take rune
            var rune = world.TakeRune(this);
            if (rune != null) Owner.TakeRune(rune);

            //Apply damage from zone
            if (Position.SqrLength() > world.ZoneRadius * world.ZoneRadius)
                TakeDamage(null, _config.ZoneDps * world.DeltaTime);

            //Calculate rotation
            if (lookDirection.SqrLength() < 0.001f)
                lookDirection = CalcLook();
            _rotation = System.MathF.Atan2(lookDirection.X, lookDirection.Y);

            //Apply health change
            if (CurrentStats.HealthPerSecond != 0 && !AttackAnimation)
                TakeDamage(this, -CurrentStats.HealthPerSecond * world.DeltaTime);
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
            Effect.ProcessEffects(_unitEffects);
            foreach (var effect in _unitEffects)
                result *= effect.StatsChange;
            if (Owner?.UnitsEffect != null)
                result *= Owner.UnitsEffect.StatsChange;

            if (Kills >= result.KillsToUpgrade)
            {
                Upgraded = true;
                if (Owner?.UnitSkins.ContainsKey(UnitMesh) ?? false)
                    UnitSkin = Owner.UnitSkins[UnitMesh];
                else
                    UnitSkin = 1;
            }
            else
            {
                Upgraded = false;
                UnitSkin = 0;
            }

            if (Upgraded)
                result *= UnitStats.GetUpgradeStats();

            return result;
        }

        public void Rise(Player player)
        {
            if (Owner != null || (player.Units.Count >= _config.MaxUnitCount && !player.IsNeutrall))
                return;

            Logger.Log($"GAME player '{player.Name}' rised unit {UnitId}");
            Health = CurrentStats.MaxHealth;
            Owner = player;
            player.Units.Add(this);

            player.PlayerStatus.UnitRise++;
            _lastRise = DateTime.Now;
        }

        public bool Heal(bool doub = false)
        {
            bool heal = Health < CurrentStats.MaxHealth;
            if (heal)
                Health += CurrentStats.MaxHealth * _config.HealValue * (doub ? 2f : 1f);
            if (Health > CurrentStats.MaxHealth)
                Health = CurrentStats.MaxHealth;
            return heal;
        }

        public void TakeDamage(Unit damager, float damage)
        {
            if (!IsAlive) return;
            damage *= GameMath.MathF.RandomFloat(1f - _config.RandomDamage / 2f, 1f + _config.RandomDamage / 2f);
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
            Effect.RemoveDamage(_unitEffects);

            if (!IsAlive) //Die
            {
                Health = 0f;
                Kills = 0;
                UnitSkin = 0;

                _unitEffects.Clear();
                if (damager == null)
                    Logger.Log($"GAME unit {UnitId}@{Owner?.Name ?? "null"} killed by zone");
                else
                    Logger.Log($"GAME unit {damager.UnitId}@{damager.Owner?.Name ?? "null"} killed {UnitId}@{Owner?.Name ?? "null"}");
                Owner?.Units.Remove(this);
                Owner = null;
                AttackAnimation = false;

                if (damager?.Owner != null)
                {
                    damager.Owner.PlayerStatus.UnitKill++;
                    damager.Owner.GotKill();
                }   
            }
        }

        public void TakeEffect(Effect effect) =>
            Effect.AddEffect(_unitEffects, effect);

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
                Position = CalcNewPos(Position + (Position - vec), CurrentStats.MoveSpeed * (_attackCommand ? 2.0f : pushPower), dt);
        }
        private bool CheckIntersect(OcTree[] trees, out Vector2 result, out float pushPower)
        {
            result = Vector2.Empty;
            var poses = new List<Vector2>();
            pushPower = 0.1f;
            foreach (var tree in trees)
            {
                var objs = tree.Overlap<PhysicalObject>(Position, Radius);
                foreach (var obj in objs)
                {
                    if (!(obj is Unit unit))
                    {
                        poses.Add(obj.Position);
                        pushPower = pushPower < 0.85f ? 0.85f : pushPower;
                        continue;
                    }
                    if (unit.Owner == null || obj == this) continue;
                    if (unit.Owner != Owner) pushPower = pushPower < 0.6f ? 0.6f : pushPower;
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
