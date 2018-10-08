using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NecroServer;
using Game;
using System.Net.Http;
using MasterReqResp;
using Newtonsoft.Json;

namespace NecroServer
{
    public class MasterClient
    {
        private HttpClient HttpClient;
        private Config Config;

        public MasterClient(Config config)
        {
            Config = config;
            HttpClient = new HttpClient();
        }

        public async Task<RespConfig> RequestConfig()
        {
            try
            {
                var data = await HttpClient.GetStringAsync(Config.MasterServer + "config");
                return JsonConvert.DeserializeObject<RespConfig>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MASTER request config error:\n{e.ToString()}", true);
            }
            return new RespConfig();
        }

        public async Task<RespClient> RequestClientInfo(long userId, string userKey)
        {
            try
            {
                string json = JsonConvert.SerializeObject(new ReqClient()
                {
                    UserId = userId,
                    UserKey = userKey,
                });
                var result = await HttpClient.PostAsync(Config.MasterServer + "client", new StringContent(json, Encoding.UTF8, "application/json"));
                var data = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RespClient>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MASTER request client info error:\n{e.ToString()}", true);
            }
            return new RespClient();
        }

        public async Task SendState(ServerState state, int playerCount, int totalPlayers, string version)
        {
            try
            {
                string json = JsonConvert.SerializeObject(new ReqState()
                {
                    InLobby = state == ServerState.Started || state == ServerState.WaitingPlayers,
                    ConnectedPlayers = playerCount,
                    TotalPlayers = totalPlayers,
                    Port = Config.Port,
                    ServerVersion = version,
                });
                await HttpClient.PostAsync(Config.MasterServer + "state", new StringContent(json, Encoding.UTF8, "application/json"));
            }
            catch (Exception e)
            {
                Logger.Log($"MASTER send state error:\n{e.ToString()}", true);
            }
        }

        public async Task<RespStatus> SendStatus(PlayerStatus status, long userId)
        {
            try
            {
                string json = JsonConvert.SerializeObject(new ReqSendStatus()
                {
                    UserId = userId,
                    AliveTime = status.AliveTime,
                    DamageDeal = status.DamageDeal,
                    DamageReceive = status.DamageReceive,
                    Place = status.Place,
                    UnitKill = status.UnitKill,
                    UnitRise = status.UnitRise,
                });
                var result = await HttpClient.PostAsync(Config.MasterServer + "sendstatus", new StringContent(json, Encoding.UTF8, "application/json"));
                var data = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RespStatus>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MASTER send status error:\n{e.ToString()}", true);
            }
            return new RespStatus();
        }
    }
}
