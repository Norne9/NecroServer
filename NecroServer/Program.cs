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
    }
}
