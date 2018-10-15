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
            var w2 = WorldScale - 0.5f; w2 *= w2;
            if (newPos.SqrLength() > w2) //Don't go outside world
                newPos = newPos.Normalize() * (WorldScale - 0.5f);

            unit.Move(newPos, DeltaTime, UnitsTree, ObstaclesTree);
        }

        public Rune TakeRune(Unit unit)
        {
            var rune = RunesTree.Overlap<Rune>(unit.Position, unit.Radius).SingleOrDefault();
            if (rune == null) return null;
            Logger.Log($"GAME user '{unit.Owner?.Name ?? "null"}' take rune");
            Runes.Remove(rune);
            RunesTree = new OcTree(WorldZone, Runes, true);
            return rune;
        }
    }
}
