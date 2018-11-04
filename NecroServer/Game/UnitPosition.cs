using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameMath;
using Force.Crc32;

namespace Game
{
    public static class UnitPosition
    {
        private static Dictionary<uint, Vector2[]> _positionsChache = new Dictionary<uint, Vector2[]>();
        private static Vector2[] _posiblePoints = null;

        public static void Warmup()
        {
            float[][] testPoses = new float[][] {
                new float[] {0.5f},
                new float[] {0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},

                new float[] {0.7f},
                new float[] {0.7f, 0.5f},
                new float[] {0.7f, 0.5f, 0.5f},
                new float[] {0.7f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},

                new float[] {0.7f},
                new float[] {0.7f, 0.7f},
                new float[] {0.7f, 0.7f, 0.5f},
                new float[] {0.7f, 0.7f, 0.5f, 0.5f},
                new float[] {0.7f, 0.7f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.7f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.7f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
                new float[] {0.7f, 0.7f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f},
            };

            foreach (var poses in testPoses)
                GetPositions(poses);
        }

        public static Vector2[] GetPositions(float[] radiuses)
        {
            var radByte = radiuses.Select((r) => (byte)(r / 10f * byte.MaxValue)).ToArray();
            var hash = Crc32Algorithm.Compute(radByte);
            if (_positionsChache.TryGetValue(hash, out var chachedResult))
                return chachedResult;

            var poses = new Vector2[radiuses.Length];
            for (int i = 0; i < poses.Length; i++)
                poses[i] = new Vector2(-1000, -1000);
            poses[0] = Vector2.Empty;

            if (_posiblePoints == null)
            {
                var pointList = new List<Vector2>();
                for (int i = 1; i <= 64; i++)
                    for (int j = 1; j <= 64; j++)
                        pointList.Add(new Vector2(i / 8f, j / 8f));
                _posiblePoints = pointList.OrderBy((p) => p.SqrLength()).ToArray();
            }

            bool CheckCollision(int index)
            {
                for (int i = 0; i < poses.Length; i++)
                {
                    if (i == index) continue;
                    float rSqr = radiuses[i] + radiuses[index]; rSqr *= rSqr;
                    if ((poses[i] - poses[index]).SqrLength() < rSqr)
                        return true;
                }
                return false;
            }

            for (int i = 1; i < poses.Length; i++)
            {
                foreach (var point in _posiblePoints)
                {
                    poses[i] = point.Clone();
                    if (!CheckCollision(i))
                        break;
                }
            }

            var avgPoint = Vector2.Empty;
            for (int i = 0; i < poses.Length; i++)
            {
                float ox = poses[i].X;
                float oy = poses[i].Y;
                float x = ox * 0.7071074f - oy * 0.7071074f;
                float y = ox * 0.7071074f + oy * 0.7071074f;
                poses[i] = new Vector2(x, -y);
                avgPoint += poses[i];
            }

            avgPoint /= poses.Length;
            for (int i = 0; i < poses.Length; i++)
                poses[i] = poses[i] - avgPoint;

            _positionsChache.Add(hash, poses);
            return poses;
        }
    }
}
