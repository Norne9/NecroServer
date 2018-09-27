using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class RuneFactory
    {
        public static Rune MakeRune()
        {
            return new Rune() { RuneType = (RuneType)GameMath.MathF.RandomInt((int)RuneType.None + 1, (int)RuneType.MAX), Radius = 1f };
        }
    }
}