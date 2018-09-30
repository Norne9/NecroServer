using System;
using System.Threading.Tasks;

namespace NecroServer
{
    class Program
    {
        static void Main(string[] args) =>
            AMain(args).Wait();

        static async Task AMain(string[] args)
        {
            var config = new Config(args);
            Logger.Init(config.DiscordLog);
            Logger.Log("LOGGER init");

            var masterClient = new MasterClient(config);

            var additionalArgs = await masterClient.RequestConfig();
            config.AppendArgs(additionalArgs.Args.ToArray());

            var server = new Server(config, masterClient);

            try
            { await server.Run(); }
            catch (Exception e)
            { Logger.Log($"SERVER ERROR: {e.Message}", true); }

            await Task.Delay(3000);
            Logger.Stop();
            await Task.Delay(2000);
        }
    }
}
