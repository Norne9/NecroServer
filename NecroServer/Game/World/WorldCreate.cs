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
                    obj = ObstacleFactory.MakeObstacle(WorldScale);
                    tree = new OcTree(new BoundingBox(-200, -200, 400, 400), objs, false);
                } while (tree.Intersect(obj.Position, obj.Radius + ObstacleFactory.SpaceBetween));
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
                    pos = new Vector2(GameMath.MathF.RandomFloat(-WorldScale, WorldScale), GameMath.MathF.RandomFloat(-WorldScale, WorldScale));
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
                    pos = new Vector2(GameMath.MathF.RandomFloat(-WorldScale, WorldScale), GameMath.MathF.RandomFloat(-WorldScale, WorldScale));
                } while (!obj.TryMove(pos, tree, ObstaclesTree, UnitsTree));
                objs.Add(obj);
            }
            return objs;
        }
    }
}
