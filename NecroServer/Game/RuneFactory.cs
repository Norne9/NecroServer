using System;
using System.Collections.Generic;
using System.Text;
using Packets;

namespace Game
{
    public static class RuneFactory
    {
        public static Rune MakeRune()
        {
            return new Rune() { RuneType = (RuneType)GameMath.MathF.RandomInt(0, (int)RuneType.MAX), Radius = 1f };
        }
    }
}
