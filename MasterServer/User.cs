using System;
using System.Collections.Generic;
using System.Text;
using MasterReqResp;
using System.Linq;
using System.IO;

namespace MasterServer
{
    public class User
    {
        public const int RatingGameCount = 64;

        public long UserId { get; set; } = 0;
        public string UserKey { get; set; } = "";
        public string UserName { get; set; } = "";
        public bool DoubleUnits { get; set; } = false;

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

        public User(Stream data)
        {
            using (var br = new BinaryReader(data))
            {
                UserId = br.ReadInt64();
                UserKey = br.ReadString();
                UserName = br.ReadString();
                DoubleUnits = br.ReadBoolean();

                LastGame = new DateTime(br.ReadInt64());

                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                    GamePlaces.Enqueue(br.ReadInt32());

                WinCount = br.ReadInt32();
                GameCount = br.ReadInt32();

                AvgAliveTime = br.ReadDouble();
                AvgPlace = br.ReadDouble();
                AvgDamageDeal = br.ReadDouble();
                AvgDamageReceive = br.ReadDouble();
                AvgUnitRise = br.ReadDouble();
                AvgUnitKill = br.ReadDouble();

                TotalAliveTime = br.ReadDouble();
                TotalDamageDeal = br.ReadDouble();
                TotalDamageReceive = br.ReadDouble();
                TotalUnitRise = br.ReadInt32();
                TotalUnitKill = br.ReadInt32();
            }
            WorldPlace = 0;
            Rating = GamePlaces.Select((p) => GetScore(p)).Sum();
        }
        public void SaveUser(Stream stream)
        {
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write(UserId);
                bw.Write(UserKey);
                bw.Write(UserName);
                bw.Write(DoubleUnits);

                bw.Write(LastGame.Ticks);

                bw.Write(GamePlaces.Count);
                foreach (var place in GamePlaces)
                    bw.Write(place);

                bw.Write(WinCount);
                bw.Write(GameCount);

                bw.Write(AvgAliveTime);
                bw.Write(AvgPlace);
                bw.Write(AvgDamageDeal);
                bw.Write(AvgDamageReceive);
                bw.Write(AvgUnitRise);
                bw.Write(AvgUnitKill);

                bw.Write(TotalAliveTime);
                bw.Write(TotalDamageDeal);
                bw.Write(TotalDamageReceive);
                bw.Write(TotalUnitRise);
                bw.Write(TotalUnitKill);
            }
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
