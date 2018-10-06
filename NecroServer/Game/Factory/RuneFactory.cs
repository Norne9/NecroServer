using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class RuneFactory
    {
        public const float RuneRadius = 0.3f;
        public static Rune MakeRune()
        {
            return new Rune() { RuneType = (RuneType)GameMath.MathF.RandomInt((int)RuneType.None + 1, (int)RuneType.MAX), Radius = RuneRadius };
        }
    }
}