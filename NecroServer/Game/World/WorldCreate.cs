using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public partial class World
    {
        private const int MaxTryCount = short.MaxValue;

        public World(Config config, GameMode gameMode)
        {
            _config = config;
            _gameMode = gameMode;

            Logger.Log("WORLD creating world");
            _mapType = GameMath.MathF.RandomInt(int.MinValue, int.MaxValue);
            WorldScale = GameMath.MathF.RandomInt(config.MinWorldScale, config.MaxWorldScale);
            ZoneRadius = WorldScale;
            _targetZoneRadius = 0.3f * ZoneRadius;
            _worldZone = new BoundingBox(-WorldScale, -WorldScale, WorldScale * 2, WorldScale * 2);

            Logger.Log("WORLD creating obstacles");
            _obstacles = GenerateObstacles(config.ObstacleCount);
            Logger.Log("WORLD creating obstacle tree");
            _obstaclesTree = new OcTree(_worldZone, _obstacles, true);

            Logger.Log("WORLD creating units");
            _units = GenerateUnits(config.UnitCount);
            Logger.Log("WORLD creating unit tree");
            _unitsTree = new OcTree(_worldZone, _units, true);

            Logger.Log("WORLD creating runes");
            _runes = GenerateRunes(config.RuneCount);
            Logger.Log("WORLD creating rune tree");
            _runesTree = new OcTree(_worldZone, _runes, true);

            Logger.Log("WORLD created");
        }

        private PhysicalObject[] GenerateObstacles(int count)
        {
            var objs = new List<PhysicalObject>();
            for (int i = 0; i < count; i++)
            {
                OcTree tree = null;
                PhysicalObject obj = null;
                for (int n = 0; n < MaxTryCount; n++)
                {
                    obj = ObstacleFactory.MakeObstacle();

                    obj.Position = GetPointInCircle(WorldScale * _config.ObstacleRange);
                    if (GameMath.MathF.Abs(obj.Position.X) > WorldScale - 2f) continue;
                    if (GameMath.MathF.Abs(obj.Position.Y) > WorldScale - 2f) continue;

                    tree = new OcTree(_worldZone, objs, false);

                    if (tree != null && !tree.Intersect(obj.Position, obj.Radius + ObstacleFactory.SpaceBetween))
                        break;
                }
                objs.Add(obj);
            }
            return objs.ToArray();
        }

        private Unit[] GenerateUnits(int count)
        {
            var objs = new List<Unit>();
            var factory = new UnitFactory(_config);
            for (int i = 0; i < count; i++)
            {
                var tree = new OcTree(_worldZone, objs, false);
                var obj = factory.MakeUnit();
                Vector2 pos;
                for (int n = 0; n < MaxTryCount; n++)
                {
                    pos = GetPointInCircle(WorldScale * _config.UnitRange);
                    if (obj.TryMove(pos, tree, _obstaclesTree))
                        break;
                }
                objs.Add(obj);
            }
            return objs.ToArray();
        }

        private List<Rune> GenerateRunes(int count)
        {
            var objs = new List<Rune>();
            for (int i = 0; i < count; i++)
            {
                var tree = new OcTree(_worldZone, objs, false);
                var obj = RuneFactory.MakeRune();
                
                Vector2 pos;
                for (int n = 0; n < MaxTryCount; n++)
                {
                    pos = GetPointInCircle(ZoneRadius * _config.RuneRange);
                    obj.Radius = ZoneRadius / 3f;
                    if (!obj.TryMove(pos, tree)) continue;
                    obj.Radius = Rune.RuneRadius;

                    if (obj.TryMove(pos, tree, _obstaclesTree, _unitsTree))
                        break;
                }
                objs.Add(obj);
            }
            return objs;
        }

        private Vector2 GetPointInCircle(float Radius)
        {
            float sRad = Radius * Radius;
            Vector2 pos = Vector2.Empty;
            for (int n = 0; n < MaxTryCount; n++)
            {
                pos = new Vector2(GameMath.MathF.RandomFloat(-Radius, Radius), GameMath.MathF.RandomFloat(-Radius, Radius));
                if (pos.SqrLength() < sRad) break;
            }
            return pos;
        }
    }
}
