using GameMath;
using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public partial class World
    {
        public World(Config config)
        {
            Config = config;

            Logger.Log("WORLD creating world");
            MapType = GameMath.MathF.RandomInt(0, 100);
            WorldScale = GameMath.MathF.RandomInt(config.MinWorldScale, config.MaxWorldScale);
            WorldZone = new BoundingBox(-WorldScale, -WorldScale, WorldScale * 2, WorldScale * 2);

            Logger.Log("WORLD creating obstacles");
            Obstacles = GenerateObstacles(config.ObstacleCount);
            Logger.Log("WORLD creating obstacle tree");
            ObstaclesTree = new OcTree(WorldZone, Obstacles, true);

            Logger.Log("WORLD creating units");
            Units = GenerateUnits(config.UnitCount);
            Logger.Log("WORLD creating unit tree");
            UnitsTree = new OcTree(WorldZone, Units, true);

            Logger.Log("WORLD creating runes");
            Runes = GenerateRunes(config.RuneCount);
            Logger.Log("WORLD creating rune tree");
            RunesTree = new OcTree(WorldZone, Runes, true);

            Logger.Log("WORLD created");
        }

        private PhysicalObject[] GenerateObstacles(int count)
        {
            var objs = new List<PhysicalObject>();
            for (int i = 0; i < count; i++)
            {
                OcTree tree = null;
                PhysicalObject obj;
                do
                {
                    obj = ObstacleFactory.MakeObstacle();

                    obj.Position = GetPointInCircle(WorldScale * Config.ObstacleRange);
                    if (GameMath.MathF.Abs(obj.Position.X) > WorldScale) continue;
                    if (GameMath.MathF.Abs(obj.Position.Y) > WorldScale) continue;

                    tree = new OcTree(new BoundingBox(-200, -200, 400, 400), objs, false);
                } while (tree == null || tree.Intersect(obj.Position, obj.Radius + ObstacleFactory.SpaceBetween));
                objs.Add(obj);
            }
            return objs.ToArray();
        }

        private Unit[] GenerateUnits(int count)
        {
            var objs = new List<Unit>();
            var factory = new UnitFactory(Config);
            for (int i = 0; i < count; i++)
            {
                var tree = new OcTree(new BoundingBox(-200, -200, 400, 400), objs, false);
                var obj = factory.MakeUnit();
                Vector2 pos;
                do
                {
                    pos = GetPointInCircle(WorldScale * Config.UnitRange);
                } while (!obj.TryMove(pos, tree, ObstaclesTree));
                objs.Add(obj);
            }
            return objs.ToArray();
        }

        private List<Rune> GenerateRunes(int count)
        {
            var objs = new List<Rune>();
            for (int i = 0; i < count; i++)
            {
                var tree = new OcTree(new BoundingBox(-200, -200, 400, 400), objs, false);
                var obj = RuneFactory.MakeRune();
                Vector2 pos;
                do
                {
                    pos = GetPointInCircle(WorldScale * Config.RuneRange);
                } while (!obj.TryMove(pos, tree, ObstaclesTree, UnitsTree));
                objs.Add(obj);
            }
            return objs;
        }

        private Vector2 GetPointInCircle(float Radius)
        {
            float sRad = Radius * Radius;
            while (true)
            {
                var pos = new Vector2(GameMath.MathF.RandomFloat(-Radius, Radius), GameMath.MathF.RandomFloat(-Radius, Radius));
                if (pos.SqrLength() < sRad)
                    return pos;
            }
        }
    }
}
