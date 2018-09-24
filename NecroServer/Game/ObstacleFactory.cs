using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

namespace Game
{
    public static class ObstacleFactory
    {
        public const float SpaceBetween = 2f;

        const float MiniMin = 0.5f;
        const float MiniMax = 2.0f;

        const float BigMin = 3.0f;
        const float BigMax = 6.0f;

        public static PhysicalObject MakeObstacle(float worldScale)
        {
            var size = GameMath.MathF.RandomFloat(0, 1) > 0.5f ?
                GameMath.MathF.RandomFloat(MiniMin, MiniMax) : GameMath.MathF.RandomFloat(BigMin, BigMax);
            var pos = new Vector2(GameMath.MathF.RandomFloat(-worldScale, worldScale), GameMath.MathF.RandomFloat(-worldScale, worldScale));
            return new PhysicalObject() { Position = pos, Radius = size };
        }
    }
}
