using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameMath;
using NecroServer;
using Packets;
using NecroServer.Utils;

namespace Game
{
    public class Player
    {
        public int NetworkId { get; }
        public long UserId { get; }
        public string Name { get; }
        public bool DoubleUnits { get; }

        public bool IsAlive { get => Units.Count > 0; }
        public bool IsAI { get => UserId < 0; }
        public bool IsNeutrall { get => UserId < -1000; }

        public List<Unit> Units { get; private set; } = new List<Unit>();
        public Vector2 AvgPosition { get; private set; } = new Vector2(0, 0);
        public Effect UnitsEffect { get; private set; } = null;

        public Dictionary<byte, byte> UnitSkins { get; set; } = new Dictionary<byte, byte>();
        public readonly PlayerStatus PlayerStatus = new PlayerStatus();

        public ClientInput ViewZone { get; private set; } = new ClientInput();
        public bool InFight { get; private set; }

        public Vector2 SmallInput { get; private set; } = new Vector2(0, 1);
        private Vector2 _inputMove = new Vector2(0, 0);
        private bool _inputRise = false;

        private DateTime _lastRise = DateTime.Now.AddYears(-1);
        private int _riseCount = 0;

        private readonly Config _config;

        public Player(long userId, int netId, string name, bool doubleUnits, Config config)
        {
            UserId = userId;
            NetworkId = netId;
            Name = name;
            DoubleUnits = doubleUnits;
            _config = config;

            if (IsNeutrall) UnitsEffect = Effect.Neutrall();
        }

        public PlayerInfo GetPlayerInfo() =>
            new PlayerInfo() { UserId = UserId, UserName = Name };

        public PlayerCameraInfo GetPlayerCameraInfo() =>
            new PlayerCameraInfo() { PosX = AvgPosition.X, PosY = AvgPosition.Y, UserId = UserId };

        public float GetCooldown() =>
            GameMath.MathF.Clamp((_config.RiseCooldown + _riseCount * _config.RiseAddCooldown) - (float)(DateTime.Now - _lastRise).TotalSeconds,
                0f, (_config.RiseCooldown + _riseCount * _config.RiseAddCooldown));

        public void SetInput(ClientInput input)
        {
            ViewZone = input;
            _inputMove = new Vector2(input.MoveX, input.MoveY);
            if (_inputMove.SqrLength() > _config.InputDeadzone)
                SmallInput = _inputMove;
            _inputRise = input.Rise;
        }

        private void NeutrallUpdate(World world)
        {
            //process units
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                //Update unit
                Units[i].Update(world, Units[i].Position + RandomPosition.GetRandomPosition(Units[i].UnitId) * 2f, true);
            }
            AvgPosition = new Vector2(-1000, -1000);
        }

        public void Update(World world)
        {
            if (!IsAlive) return;
            if (IsNeutrall)
            {
                NeutrallUpdate(world);
                return;
            }
            PlayerStatus.AliveTime += world.DeltaTime;

            //Check effect time
            if (UnitsEffect != null && DateTime.Now > UnitsEffect.EndTime)
                UnitsEffect = null;

            //Check rise or heal
            if (_inputRise && GetCooldown() < 0.001f)
            {
                var unitsRise = world.OverlapUnits(AvgPosition, _config.RiseRadius)
                    .Where((u) => u.Owner == null);
                var hasRise = unitsRise.Count() > 0 && Units.Count < _config.MaxUnitCount;

                if (hasRise) //Rise
                {
                    unitsRise = unitsRise.OrderByDescending((u) => u.CurrentStats.Damage / u.CurrentStats.AttackDelay);
                    foreach (var unit in unitsRise)
                        unit.Rise(this);
                }

                //Heal
                bool hasHeal = false;
                foreach (var unit in Units)
                    hasHeal |= unit.Heal(!hasRise);

                //Increase cooldown
                if (hasRise || hasHeal)
                {
                    _lastRise = DateTime.Now;
                    _riseCount++;
                }
            }

            var attack = _inputMove.SqrLength() < 0.001f;
            var move = _inputMove.SqrLength() > _config.InputDeadzone;

            Units.Sort((u1, u2) =>
            {
                if (Math.Abs(u1.Radius - u2.Radius) > 0.01f)
                    return u2.Radius.CompareTo(u1.Radius);
                if (Math.Abs(u1.UnitStats.AttackRange - u2.UnitStats.AttackRange) > 0.1f)
                    return u1.UnitStats.AttackRange.CompareTo(u2.UnitStats.AttackRange);
                return u2.UnitStats.MaxHealth.CompareTo(u1.UnitStats.MaxHealth);
            });
            var poses = ProcessPositions(UnitPosition.GetPositions(Units.Select((u) => u.Radius).ToArray()),
                SmallInput, move);

            //process units
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                //Update unit
                Units[i].Update(world, poses[i], attack);
            }

            //Update avg position
            InFight = false;
            AvgPosition = new Vector2(0, 0);
            foreach (var unit in Units)
            {
                AvgPosition += unit.Position;
                InFight |= unit.AttackAnimation;
            }
            AvgPosition /= Units.Count;
        }

        private Vector2[] ProcessPositions(Vector2[] positions, Vector2 dir, bool move)
        {
            var result = new Vector2[positions.Length];
            if (dir.SqrLength() > 1f)
                dir = dir.Normalize();
            float rot = System.MathF.Atan2(-dir.X, dir.Y);
            float cs = System.MathF.Cos(rot);
            float sn = System.MathF.Sin(rot);
            for (int i = 0; i < positions.Length; i++)
            {
                float ox = positions[i].X;
                float oy = positions[i].Y;
                float x = ox * cs - oy * sn;
                float y = ox * sn + oy * cs;
                var pos = AvgPosition + new Vector2(x, y);
                result[i] = move ? pos + dir * 0.3f : pos;
            }
            return result;
        }

        public void GotKill()
        {
            if (IsNeutrall) return;
            var count = Units.Select((u) => u.Upgraded ? 0.7f : 1f).DefaultIfEmpty(0).Sum();
            if (count == 0) return;
            var kill = 1f / count;
            foreach (var unit in Units)
                unit.Kills += kill;
        }

        public void UnitAttack()
        {
            if (UnitsEffect != null && UnitsEffect.RemoveByAttack)
                UnitsEffect = null;
        }

        public void UnitDamage()
        {
            if (UnitsEffect != null && UnitsEffect.RemoveByDamage)
                UnitsEffect = null;
        }

        public void TakeRune(Rune rune)
        {
            if (IsNeutrall) return;
            UnitsEffect = rune.GetEffect();
        }
    }
}
