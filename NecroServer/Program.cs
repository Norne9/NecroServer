﻿using System;
using System.Threading.Tasks;

namespace NecroServer
{
    class Program
    {
        static void Main(string[] args) =>
            AMain(args).GetAwaiter().GetResult();

        static async Task AMain(string[] args)
        {
            var config = new Config(args);
            Logger.Init(config.DiscordLog, config.DiscordAll > 0);
            Logger.Log("LOGGER init");

            var masterClient = new MasterClient(config);

            var additionalArgs = await masterClient.RequestConfig();
            config.AppendArgs(additionalArgs.Args.ToArray());

            try
            {
                var server = new Server(config, masterClient);
                await server.Run();
            }
            catch (Exception e)
            { Logger.Log($"SERVER ERROR: {e.ToString()}", true); }

            await Task.Delay(3000);
            Logger.Stop();
            await Task.Delay(2000);
        }
    }
}
