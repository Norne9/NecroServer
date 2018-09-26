using GameMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class UnitFactory
    {
        public static Unit MakeUnit()
        {
            return new Unit() { Radius = 1f };
        }
    }
}
