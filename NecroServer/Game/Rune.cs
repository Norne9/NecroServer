using System;
using System.Collections.Generic;
using System.Text;
using Packets;

namespace Game
{
    public class Rune : PhysicalObject
    {
        public const float RuneRadius = 0.3f;
        private RuneType _type = RuneType.None;

        public Effect GetEffect()
        {
            switch (_type)
            {
                case RuneType.Damage:
                    return Effect.DoubleDamage();
                case RuneType.Haste:
                    return Effect.Haste();
                case RuneType.Stealth:
                    return Effect.Stealth();
                default:
                    return null;
            }
        }

        public Rune(RuneType type)
        {
            _type = type;
            Radius = RuneRadius;
        }

        public RuneInfo GetRuneInfo() =>
            new RuneInfo() { PosX = Position.X, PosY = Position.Y, Rune = _type };
    }
}
