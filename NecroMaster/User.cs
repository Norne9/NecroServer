using System;
using System.Collections.Generic;
using System.Text;
using MasterReqResp;
using System.Linq;

namespace NecroMaster
{
    public class User
    {
        public const int RatingGameCount = 64;

        public long UserId { get; set; } = 0;
        public string UserKey { get; set; } = "";
        public string UserName { get; set; } = "";

        public DateTime LastGame { get; set; } = DateTime.Now.AddYears(-1);

        public Queue<int> GamePlaces { get; set; } = new Queue<int>();
        public int Rating { get; set; } = 0;
        public int WorldPlace { get; set; } = 0;

        public int WinCount { get; set; } = 0;
        public int GameCount { get; set; } = 0;

        public double AvgAliveTime { get; set; } = 0;
        public double AvgPlace { get; set; } = 0;
        public double AvgDamageDeal { get; set; } = 0;
        public double AvgDamageReceive { get; set; } = 0;
        public double AvgUnitRise { get; set; } = 0;
        public double AvgUnitKill { get; set; } = 0;

        public double TotalAliveTime { get; set; } = 0;
        public double TotalDamageDeal { get; set; } = 0;
        public double TotalDamageReceive { get; set; } = 0;
        public int TotalUnitRise { get; set; } = 0;
        public int TotalUnitKill { get; set; } = 0;

        public User(byte[] data)
        {

        }

        public User(UserBase uBase, string name)
        {
            UserId = uBase.UserNumber++;
            UserKey = Guid.NewGuid().ToString();
            UserName = name;
        }

        public void UpdateUser(ReqSendStatus status)
        {
            LastGame = DateTime.Now;
            if (status.Place == 1)
                WinCount++;

            TotalAliveTime += status.AliveTime;
            TotalDamageDeal += status.DamageDeal;
            TotalDamageReceive += status.DamageReceive;
            TotalUnitKill += status.UnitKill;
            TotalUnitRise += status.UnitRise;

            GamePlaces.Enqueue(status.Place);
            while (GamePlaces.Count > RatingGameCount)
                GamePlaces.Dequeue();

            AvgPlace = GamePlaces.Average();
            AvgAliveTime = (AvgAliveTime * GameCount + status.AliveTime) / (GameCount + 1);
            AvgDamageDeal = (AvgDamageDeal * GameCount + status.DamageDeal) / (GameCount + 1);
            AvgDamageReceive = (AvgDamageReceive * GameCount + status.DamageReceive) / (GameCount + 1);
            AvgUnitKill = (AvgUnitKill * GameCount + status.UnitKill) / (GameCount + 1);
            AvgUnitRise = (AvgUnitRise * GameCount + status.UnitRise) / (GameCount + 1);

            GameCount++;
            Rating = GamePlaces.Select((p) => GetScore(p)).Sum();
        }

        private static int GetScore(int place)
        {
            if (place <= 1)
                return 10;
            else if (place == 2)
                return 5;
            else if (place == 3)
                return 3;
            else if (place == 4)
                return 2;
            else if (place <= 10)
                return 1;

            return -1;
        }
    }
}
