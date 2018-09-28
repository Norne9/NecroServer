using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NecroServer;
using Game;

namespace MasterClient
{
    public static class MasterClient
    {
        public static async Task<string[]> RequestConfigArgs()
        {
            await Task.Delay(5);
            return new string[] { };
        }

        public static async Task<ClientResponce> CheckClient(long userId, string userKey)
        {
            await Task.Delay(5);
            return new ClientResponce()
            {
                Valid = true,
                Name = "test user",
                DoubleUnits = false,
                UsedId = userId
            };
        }

        public static async Task SendState(ServerState state, int playerCount, int totalPlayers)
        {
            await Task.Delay(5);
        }

        public static async Task<PlayerStatus> SendStatus(long userId, PlayerStatus playerStatus)
        {
            await Task.Delay(5);
            return playerStatus;
        }
    }
}
