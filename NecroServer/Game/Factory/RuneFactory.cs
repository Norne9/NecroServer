using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class RuneFactory
    {
        public static Rune MakeRune()
        {
            return new Rune((VisualEffect)GameMath.MathF.RandomInt((int)VisualEffect.None + 1, (int)VisualEffect.MAX));
        }
    }
}