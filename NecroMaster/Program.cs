using System;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Labs.EmbedIO.Constants;

namespace NecroMaster
{
    class Program
    {
        static void Main(string[] args) =>
            AMain(args).Wait();

        static async Task AMain(string[] args)
        {
            var config = new Config(args);
            Logger.Init(config.DiscordLog);

            using (var server = new WebServer(config.ServerPort, RoutingStrategy.Wildcard))
            {
                _ = server.RunAsync();

                string line = "";
                while ((line = Console.ReadLine()) != "quit")
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
