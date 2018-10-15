using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

namespace NecroServer.Utils
{
    public class RandomPosition
    {
        private static RandomPosition Inst = new RandomPosition();
        public static Vector2 GetRandomPosition(int offset = 0) =>
            Inst.GetPos(offset);

        private Vector2[] Positions = new Vector2[1000];
        private DateTime StartTime = DateTime.Now;

        private RandomPosition()
        {
            for (int i = 0; i < Positions.Length; i++)
                Positions[i] = new Vector2(GameMath.MathF.RandomFloat(-1f, 1f), GameMath.MathF.RandomFloat(-1f, 1f)).Normalize();
        }

        private Vector2 GetPos(int offset)
        {
            float t = (float)(DateTime.Now - StartTime).TotalSeconds / 5f;
            int i1 = (int)System.MathF.Floor(t) + offset;
            int i2 = i1 + 1;
            i1 %= Positions.Length;
            i2 %= Positions.Length;
            float frac = System.MathF.Abs(t - System.MathF.Truncate(t));
            return Positions[i1] * (1f - frac) + Positions[i2] * frac;
        }
    }
}
