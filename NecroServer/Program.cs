using System;
using GameMath;
using Game;
using System.Collections.Generic;

namespace NecroServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TestTree();
            return;

            var config = new Config(args);

            Logger.Init(config.DiscordLog);
            Logger.Log("LOGGER init");

            var server = new Server(config);

            try
            { server.Run().Wait(); }
            catch (Exception e)
            { Logger.Log($"SERVER ERROR: {e.Message}", true); }

            Logger.Stop();
        }

        static void TestTree()
        {
            var objs = new List<PhysicalObject>();
            OcTree<PhysicalObject> tree = null;
            for (int i = 0; i < 400; i++)
            {
                PhysicalObject obj;
                do
                {
                    obj = new PhysicalObject()
                    {
                        Position = new Vector2(GameMath.MathF.RandomFloat(-190f, 190f), GameMath.MathF.RandomFloat(-190f, 190f)),
                        Radius = GameMath.MathF.RandomInt(2, 6) / 2f,
                    };
                    tree = new OcTree<PhysicalObject>(new BoundingBox(-200, -200, 400, 400), objs);
                } while (tree.Intersect(obj.Position, obj.Radius + 2f));
                objs.Add(obj);
                Console.WriteLine(i);
            }
            tree.DrawTree();
        }
    }
}
