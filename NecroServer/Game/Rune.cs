using System;
using System.Collections.Generic;
using System.Text;
using Packets;

namespace Game
{
    public class Rune : PhysicalObject
    {
        public const float RuneRadius = 0.3f;
        private VisualEffect _type = VisualEffect.None;

        public Effect GetEffect()
        {
            switch (_type)
            {
                case VisualEffect.Damage:
                    return Effect.DoubleDamage();
                case VisualEffect.Haste:
                    return Effect.Haste();
                case VisualEffect.Stealth:
                    return Effect.Stealth();
                default:
                    return null;
            }
        }

        public Rune(VisualEffect type)
        {
            _type = type;
            Radius = RuneRadius;
        }

        public RuneInfo GetRuneInfo() =>
            new RuneInfo() { PosX = Position.X, PosY = Position.Y, Rune = _type };
    }
}
