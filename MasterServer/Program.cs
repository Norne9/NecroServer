using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MasterReqResp;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace MasterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Config(args);
            Logger.Init(config.DiscordLog);

            var masterData = new MasterData(config);
            var userBase = new UserBase(config, masterData);
            var serverBase = new ServerBase(config);

            Task.Run(async () => //Debug task
            {
                DateTime lastLog = DateTime.Now;
                while (true)
                {
                    if ((DateTime.Now - lastLog).TotalSeconds > config.DebugTime)
                    {
                        lastLog = DateTime.Now;
                        userBase.DebugUsers();
                        serverBase.DebugServers();
                    }
                    await Task.Delay(10000);
                }
            });

            Task.Run(async () => //Console task
            {
                while (true)
                {
                    string[] cmd = Console.ReadLine().Split(' ');
                    switch (cmd[0])
                    {
                        case "set":
                            var arg = cmd.Skip(1).ToArray();
                            if (arg.Length == 2)
                                config.AppendArgs(arg);
                            break;
                        case "save":
                            await userBase.Save();
                            break;
                        case "debug":
                            userBase.DebugUsers();
                            serverBase.DebugServers();
                            break;
                        case "reload":
                            await masterData.Reload();
                            break;
                        default:
                            Logger.Log("CONSOLE unknown command");
                            break;
                    }
                    await Task.Delay(1000);
                }
            });

            var host = new WebHostBuilder()
                .UseKestrel((o) => o.Listen(IPAddress.Any, 8856))
                //.UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, sconfig) =>
                {
                    sconfig.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, l) =>
                {
                    l.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    l.SetMinimumLevel(LogLevel.Warning);
                    l.AddConsole();
                    l.AddProvider(new DiscordProvider());
                })
                .UseIISIntegration()
                .ConfigureServices(s => s.AddRouting())
                .Configure(app =>
                {
                    // define all API endpoints
                    app.UseRouter(r =>
                    {
                        //Server

                        //ReqConfig
                        r.MapGet("config", async (request, response, routeData) =>
                        {
                            response.WriteJson(masterData.GetParams());
                        });

                        //ReqClient
                        r.MapPost("client", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqClient>();
                            response.WriteJson(await userBase.GetClinetData(req, masterData.GetSkins()));
                        });

                        //ReqState
                        r.MapPost("state", async (request, response, routeData) =>
                        {
                            var ip = request.HttpContext.Connection.RemoteIpAddress;
                            var req = request.HttpContext.ReadFromJson<ReqState>();
                            serverBase.UpdateServer(req, ip);
                        });

                        //ReqSendStatus
                        r.MapPost("sendstatus", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqSendStatus>();
                            var resp = await userBase.UpdateUserStatus(req);
                            response.WriteJson(resp);
                        });

                        //Client

                        //ReqRegister
                        r.MapPost("register", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqRegister>();
                            var resp = await userBase.RegisterUser(req);
                            response.WriteJson(resp);
                        });

                        //ReqUserStatus
                        r.MapPost("userstatus", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqUserStatus>();
                            var resp = await userBase.GetUserStatus(req);
                            if (resp == null)
                                using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
                                { writer.WriteLine("empty"); }
                            else
                                response.WriteJson(resp);
                        });

                        //ReqLeaderboard
                        r.MapPost("leaderboard", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqLeaderboard>();
                            var resp = await userBase.GetLeaderboard(req);
                            response.WriteJson(resp);
                        });

                        //ReqServer
                        r.MapPost("server", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqServer>();
                            await userBase.SetDoubleUnits(req.UserId, req.DoubleUnits);
                            var server = serverBase.FindServer(req.ServerVersion);
                            if (server != null)
                                response.WriteJson(new RespServer() {
                                    Address = server.Address.ToString(),
                                    Port = server.Port
                                });
                            else
                                response.WriteJson(new RespServer() { Address = "", Port = 0 });
                        });

                        //ReqMessages
                        r.MapPost("messages", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqMessages>();
                            var resp = masterData.GetMessages(req);
                            response.WriteJson(resp);
                        });

                        //ReqSkinInfo
                        r.MapPost("skininfo", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqSkinInfo>();
                            var resp = await userBase.GetSkinInfo(req);
                            response.WriteJson(resp);
                        });

                        //ReqRestore
                        r.MapPost("restore", async (request, response, routeData) =>
                        {
                            var req = request.HttpContext.ReadFromJson<ReqRestore>();
                            var resp = await userBase.RestoreUser(req);
                            response.WriteJson(resp);
                        });
                    });
                })
                .Build();

            Logger.Log("SERVER starting server");
            host.Run();
            userBase.Dispose();
            Logger.Stop();
        }
    }
}
