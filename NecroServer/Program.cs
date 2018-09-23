using System;
using GameMath;
using Game;
using System.Collections.Generic;
using System.Diagnostics;

namespace NecroServer
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
                TestTree();
            Console.ReadLine();
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
                    tree = new OcTree<PhysicalObject>(new BoundingBox(-200, -200, 400, 400), objs, true);
                } while (tree.Intersect(obj.Position, obj.Radius + 2f));
                objs.Add(obj);
            }

            Stopwatch sw = new Stopwatch();

            sw.Restart();
            for (int i = 0; i < 3000; i++)
            {
                tree = new OcTree<PhysicalObject>(new BoundingBox(-200, -200, 400, 400), objs, false);
            }
            sw.Stop();
            Console.WriteLine("Tree 1: " + sw.ElapsedMilliseconds / 1000.0);

            sw.Restart();
            for (int i = 0; i < 50000; i++)
            {
                var obj = objs[i % objs.Count];
                tree.Overlap(obj.Position, obj.Radius * 10f);
            }
            sw.Stop();
            Console.WriteLine("Tree 1 Check: " + sw.ElapsedMilliseconds / 1000.0);

            sw.Restart();
            for (int i = 0; i < 3000; i++)
            {
                tree = new OcTree<PhysicalObject>(new BoundingBox(-200, -200, 400, 400), objs, true);
            }
            sw.Stop();
            Console.WriteLine("Tree 2: " + sw.ElapsedMilliseconds / 1000.0);

            sw.Restart();
            for (int i = 0; i < 50000; i++)
            {
                var obj = objs[i % objs.Count];
                tree.Overlap(obj.Position, obj.Radius * 10f);
            }
            sw.Stop();
            Console.WriteLine("Tree 2 Check: " + sw.ElapsedMilliseconds / 1000.0);

            //tree.DrawTree();
        }
    }
}
