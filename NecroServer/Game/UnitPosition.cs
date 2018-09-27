using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameMath;

namespace Game
{
    public static class UnitPosition
    {
        private static readonly Vector2[][] Positions = new Vector2[][]
        {
            new Vector2[] { new Vector2(0, 0), },
            new Vector2[] { new Vector2(-0.545, 0), new Vector2(0.545, 0), },
            new Vector2[] { new Vector2(-0.5956667, -0.3266667), new Vector2(0.5743333, -0.3466667), new Vector2(0.02133334, 0.6733333), },
            new Vector2[] { new Vector2(-0.545, -0.535), new Vector2(0.545, -0.535), new Vector2(-0.545, 0.535), new Vector2(0.545, 0.535), },
            new Vector2[] { new Vector2(-0.569, -0.761), new Vector2(0.521, -0.761), new Vector2(-0.924, 0.309), new Vector2(0.956, 0.309), new Vector2(0.01599992, 0.904), },
            new Vector2[] { new Vector2(-0.5925, 0.31), new Vector2(0.5775, 0.31), new Vector2(0.02450002, 1.24), new Vector2(-1.1765, -0.62), new Vector2(1.1635, -0.62), new Vector2(0.003500016, -0.62), },
            new Vector2[] { new Vector2(-1.12, 0.01428571), new Vector2(0, 0.01428571), new Vector2(-0.56, 0.9642857), new Vector2(-0.56, -0.9857143), new Vector2(1.12, 0.01428571), new Vector2(0.56, -0.9857143), new Vector2(0.56, 0.9642857), },
            new Vector2[] { new Vector2(-0.582625, -1.2045), new Vector2(-0.01562499, -0.2385), new Vector2(-1.118625, -0.2405), new Vector2(0.563375, -1.2275), new Vector2(0.550375, 0.7275), new Vector2(1.129375, -0.2615), new Vector2(-0.551625, 0.7255), new Vector2(0.02537501, 1.7195), },
            new Vector2[] { new Vector2(-1.1, -1.1), new Vector2(0, 0), new Vector2(-1.1, 0), new Vector2(1.1, -1.1), new Vector2(1.1, 1.1), new Vector2(1.1, 0), new Vector2(-1.1, 1.1), new Vector2(0, 1.1), new Vector2(0, -1.1), },
            new Vector2[] { new Vector2(0.02589998, 1.773), new Vector2(0.006899983, 0.2180001), new Vector2(0.7939, 0.9860001), new Vector2(-1.5491, 0.237), new Vector2(-0.5961, -1.656), new Vector2(-0.7801, -0.5509999), new Vector2(1.5629, 0.1990001), new Vector2(0.7749, -0.5699999), new Vector2(-0.7611001, 1.005), new Vector2(0.5218999, -1.641), },
            new Vector2[] { new Vector2(0.03063634, 1.756273), new Vector2(-0.5253636, -0.08672725), new Vector2(0.7986364, 0.9692727), new Vector2(-1.595364, 0.2632727), new Vector2(-0.6153637, -1.166727), new Vector2(-1.645364, -0.8267273), new Vector2(1.624636, 0.2432727), new Vector2(1.584636, -0.8567272), new Vector2(-0.7563636, 0.9882727), new Vector2(0.5446364, -1.176727), new Vector2(0.5546364, -0.1067273), },
            new Vector2[] { new Vector2(-0.5133333, 0.2836667), new Vector2(0.5716667, 1.400667), new Vector2(-1.508333, 0.7806667), new Vector2(-0.6033333, -0.7963333), new Vector2(-1.588333, -0.2993333), new Vector2(1.551667, 0.8406667), new Vector2(1.551667, -0.2593333), new Vector2(-0.5683333, 1.410667), new Vector2(0.5566667, -0.8063333), new Vector2(0.5666667, 0.2636667), new Vector2(1.511667, -1.399333), new Vector2(-1.528333, -1.419333), },
            new Vector2[] { new Vector2(-0.5133077, 0.1013846), new Vector2(0.5716923, 1.218385), new Vector2(-1.508308, 0.5983846), new Vector2(-0.6033077, -0.9786154), new Vector2(-1.588308, -0.4816154), new Vector2(1.551692, 0.6583847), new Vector2(1.551692, -0.4416154), new Vector2(-0.5683077, 1.228385), new Vector2(0.5566923, -0.9886154), new Vector2(0.5666923, 0.08138463), new Vector2(1.511692, -1.581615), new Vector2(-1.528308, -1.601615), new Vector2(-0.0003076792, 2.187385), },
            new Vector2[] { new Vector2(-0.5132857, 0.2413571), new Vector2(0.5717143, 1.358357), new Vector2(-1.508286, 0.7383571), new Vector2(-0.6032857, -0.8386428), new Vector2(-1.588286, -0.3416429), new Vector2(1.551714, 0.7983571), new Vector2(1.551714, -0.3016429), new Vector2(-0.5682857, 1.368357), new Vector2(0.5567143, -0.8486429), new Vector2(0.5667143, 0.2213571), new Vector2(1.511714, -1.441643), new Vector2(-1.528286, -1.461643), new Vector2(-0.0002857021, 2.327357), new Vector2(-0.0002857021, -1.819643), },
            new Vector2[] { new Vector2(0.586, 1.290667), new Vector2(-0.574, 0.08066678), new Vector2(1.576, 0.7206668), new Vector2(-1.594, 0.7006668), new Vector2(0.566, 0.1106668), new Vector2(-0.604, 1.260667), new Vector2(0.005999997, -0.8693332), new Vector2(1.586, -0.4193332), new Vector2(-1.544, -0.4793332), new Vector2(0.9859999, -1.369333), new Vector2(-0.964, -1.429333), new Vector2(0.016, -1.979333), new Vector2(-2.604, 0.01066685), new Vector2(2.566, 0.1706668), new Vector2(-0.004000004, 2.200667), },
            new Vector2[] { new Vector2(-1.7, -1.7), new Vector2(-0.6, -1.7), new Vector2(0.6, -1.7), new Vector2(1.7, -1.7), new Vector2(-1.7, -0.6), new Vector2(-0.6, -0.6), new Vector2(0.6, -0.6), new Vector2(1.7, -0.6), new Vector2(-1.7, 0.6), new Vector2(-0.6, 0.6), new Vector2(0.6, 0.6), new Vector2(1.7, 0.6), new Vector2(-1.7, 1.7), new Vector2(-0.6, 1.7), new Vector2(0.6, 1.7), new Vector2(1.7, 1.7), },
        };

        public static List<Vector2> GetPositions(int count)
        {
            if (count < 1 || count > Positions.Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            return Positions[count - 1].ToList();
        }
    }
}
