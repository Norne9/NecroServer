using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

namespace Game
{
    public static class ObstacleFactory
    {
        public const float SpaceBetween = 2f;

        const float MiniMin = 0.3f;
        const float MiniMax = 2.0f;

        const float BigMin = 3.0f;
        const float BigMax = 6.0f;

        public static PhysicalObject MakeObstacle()
        {
            var size = GameMath.MathF.RandomFloat(0, 1) < 0.85f ?
                GameMath.MathF.RandomFloat(MiniMin, MiniMax) : GameMath.MathF.RandomFloat(BigMin, BigMax);
            return new PhysicalObject() { Radius = size };
        }
    }
}
