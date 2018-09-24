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

        private readonly OcTree<PhysicalObject> ObstaclesTree;
        private OcTree<Unit> UnitsTree;
        private OcTree<Rune> RunesTree;

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
            ObstaclesTree = new OcTree<PhysicalObject>(WorldZone, Obstacles, true);


        }

        private PhysicalObject[] GenerateObstacles(int count)
        {
            var objs = new List<PhysicalObject>();
            for (int i = 0; i < count; i++)
            {
                OcTree<PhysicalObject> tree = null;
                PhysicalObject obj;
                do
                {
                    obj = ObstacleFactory.MakeObstacle(WorldScale);
                    tree = new OcTree<PhysicalObject>(new BoundingBox(-200, -200, 400, 400), objs, false);
                } while (tree.Intersect(obj.Position, obj.Radius + ObstacleFactory.SpaceBetween));
                objs.Add(obj);
            }
            return objs.ToArray();
        }
    }
}
