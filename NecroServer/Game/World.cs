using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;

namespace Game
{
    public class World
    {
        private Dictionary<long, Player> Players;

        private readonly PhysicalObject[] Obstacles;
        private readonly Unit[] Units;
        private readonly List<Rune> Runes = new List<Rune>();

        private readonly OcTree ObstaclesTree;
        private OcTree UnitsTree;
        private OcTree RunesTree;

        private readonly float WorldScale;
        private readonly BoundingBox WorldZone;
        private readonly int MapType;

        private readonly Config Config;

        public World(Config config)
        {
            Config = config;

            Logger.Log("WORLD creating world");
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
            for (int i = 0; i < count; i++)
            {
                var tree = new OcTree(new BoundingBox(-200, -200, 400, 400), objs, false);
                var obj = UnitFactory.MakeUnit();
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
                do {
                    pos = new Vector2(GameMath.MathF.RandomFloat(-WorldScale, WorldScale), GameMath.MathF.RandomFloat(-WorldScale, WorldScale));
                } while (!obj.TryMove(pos, tree, ObstaclesTree, UnitsTree));
                objs.Add(obj);
            }
            return objs;
        }

        public void DebugMap()
        {
            const int drawRes = 2048;

            Logger.Log("DEBUG drawing trees");
            Logger.Log($"DEBUG drawing {nameof(ObstaclesTree)}");
            ObstaclesTree.DrawTree(drawRes, $"{nameof(ObstaclesTree)}.png");
            Logger.Log($"DEBUG drawing {nameof(UnitsTree)}");
            UnitsTree.DrawTree(drawRes, $"{nameof(UnitsTree)}.png");
            Logger.Log($"DEBUG drawing {nameof(RunesTree)}");
            RunesTree.DrawTree(drawRes, $"{nameof(RunesTree)}.png");
            Logger.Log("DEBUG drawing finished");
        }
    }
}
