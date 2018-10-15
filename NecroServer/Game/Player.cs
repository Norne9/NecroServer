using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameMath;
using NecroServer;
using Packets;

namespace Game
{
    public class Player
    {
        public int NetworkId { get; }
        public long UserId { get; }
        public bool IsAI { get => UserId < 0; }
        public string Name { get; }
        public bool DoubleUnits { get; }
        public bool IsAlive { get => Units.Count > 0; }

        public List<Unit> Units { get; private set; } = new List<Unit>();
        public Vector2 AvgPosition { get; private set; } = new Vector2(0, 0);
        public Effect UnitsEffect { get; private set; } = null;

        public readonly PlayerStatus PlayerStatus = new PlayerStatus();

        private Vector2 InputMove = new Vector2(0, 0);
        public Vector2 SmallInput { get; private set; } = new Vector2(0, 1);
        private bool InputRise = false;

        private DateTime LastRise = DateTime.Now.AddYears(-1);
        private int RiseCount = 0;

        private readonly Config Config;

        public Player(long userId, int netId, string name, bool doubleUnits, Config config)
        {
            UserId = userId;
            NetworkId = netId;
            Name = name;
            DoubleUnits = doubleUnits;
            Config = config;
        }

        public PlayerInfo GetPlayerInfo() =>
            new PlayerInfo() { UserId = UserId, UserName = Name };

        public PlayerCameraInfo GetPlayerCameraInfo() =>
            new PlayerCameraInfo() { PosX = AvgPosition.X, PosY = AvgPosition.Y, UserId = UserId };

        public float GetCooldown() =>
            GameMath.MathF.Clamp((Config.RiseCooldown + RiseCount * Config.RiseAddCooldown) - (float)(DateTime.Now - LastRise).TotalSeconds,
                0f, (Config.RiseCooldown + RiseCount * Config.RiseAddCooldown));

        public void SetInput(Vector2 input, bool rise)
        {
            InputMove = input;
            if (InputMove.SqrLength() > Config.InputDeadzone)
                SmallInput = InputMove;
            InputRise = rise;
        }

        public void Update(World world)
        {
            if (!IsAlive) return;
            PlayerStatus.AliveTime += world.DeltaTime;

            //Check effect time
            if (UnitsEffect != null && DateTime.Now > UnitsEffect.EndTime)
                UnitsEffect = null;

            //Check rise or heal
            if (InputRise && GetCooldown() < 0.001f)
            {
                LastRise = DateTime.Now;
                var unitsRise = world.OverlapUnits(AvgPosition, Config.RiseRadius)
                    .Where((u) => u.Owner == null);
                if (unitsRise.Count() > 0 && Units.Count < Config.MaxUnitCount)
                {
                    unitsRise = unitsRise.OrderBy((u) => (AvgPosition - u.Position).SqrLength());
                    foreach (var unit in unitsRise)
                        unit.Rise(this);
                    RiseCount++;
                }
                else
                    foreach (var unit in Units)
                        unit.Heal();
            }

            //get input for units
            var attack = InputMove.SqrLength() < Config.InputDeadzone;
            var poses = UnitPosition.GetPositions(Units.Count);
            var movePoses = ProcessPositions(poses, SmallInput, !attack);

            var sortedUnits = Units.OrderBy((u) => u.BestDist);
            foreach (var unit in sortedUnits)
            {
                unit.BestDist = float.MaxValue;
                foreach (var pos in movePoses)
                {
                    var dist = (unit.Position - pos.stay).SqrLength();
                    if (dist < unit.BestDist)
                    {
                        unit.BestDist = dist;
                        unit.TargetPos = pos;
                    }
                }
                movePoses.Remove(unit.TargetPos);
            }

            //process units
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                //Update unit
                Units[i].Update(world, Units[i].TargetPos.move, attack);
            }

            //Update avg position
            AvgPosition = new Vector2(0, 0);
            foreach (var unit in Units)
                AvgPosition += unit.Position;
            AvgPosition /= Units.Count;
        }

        private List<(Vector2 stay, Vector2 move)> ProcessPositions(List<Vector2> positions, Vector2 dir, bool move)
        {
            var result = new List<(Vector2 stay, Vector2 move)>();
            if (dir.SqrLength() > 1f)
                dir = dir.Normalize();
            float rot = System.MathF.Atan2(-dir.X, dir.Y);
            float cs = System.MathF.Cos(rot);
            float sn = System.MathF.Sin(rot);
            for (int i = 0; i < positions.Count; i++)
            {
                float ox = positions[i].X;
                float oy = positions[i].Y;
                float x = ox * cs - oy * sn;
                float y = ox * sn + oy * cs;
                positions[i] = AvgPosition + new Vector2(x, y);
                result.Add((positions[i], move ? positions[i] + dir * 0.3f : positions[i]));
            }
            return result;
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
            UnitsEffect = rune.GetEffect();
        }
    }
}
