using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;
using System.Linq;

namespace Game
{
    public partial class World
    {
        public List<Unit> OverlapUnits(Vector2 pos, float rad) =>
            UnitsTree.Overlap<Unit>(pos, rad);

        public void MoveUnit(Unit unit, Vector2 newPos)
        {
            if ((unit.Position - newPos).SqrLength() < 0.001f) //Don't move if on pos
                return;
            unit.Move(newPos, UnitsTree, ObstaclesTree);//,
        }

        public RuneType TakeRune(Unit unit)
        {
            var rune = RunesTree.Overlap<Rune>(unit.Position, unit.Radius).SingleOrDefault();
            if (rune == null) return RuneType.None;
            Logger.Log($"GAME user '{unit.Owner?.Name ?? "null"}' take rune {rune.RuneType}");
            Runes.Remove(rune);
            RunesTree = new OcTree(WorldZone, Runes, true);
            return rune.RuneType;
        }
    }
}
