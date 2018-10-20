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
        public double Rating { get; set; } = 0;
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

        public double Money { get; set; } = 0;
        public List<long> OwnedSkins { get; set; } = new List<long>();
        public List<long> SelectedSkins { get; set; } = new List<long>();

        public DateTime LastAdWatch { get; set; } = DateTime.Now.AddYears(-1);

        private Config _config;

        public User(byte[] data, Config config)
        {
            _config = config;
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                UserId = br.ReadInt64();
                UserKey = br.ReadString();
                UserName = br.ReadString();
                DoubleUnits = br.ReadBoolean();

                LastGame = new DateTime(br.ReadInt64());

                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                    GamePlaces.Enqueue(br.ReadInt32());
                while (GamePlaces.Count < RatingGameCount)
                    GamePlaces.Enqueue(_config.LastPlace);

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

                try //Version 2
                {
                    Money = br.ReadDouble();
                    int ownedSkinCount = br.ReadInt32();
                    for (int i = 0; i < ownedSkinCount; i++)
                        OwnedSkins.Add(br.ReadInt64());
                    int selectedSkinCount = br.ReadInt32();
                    for (int i = 0; i < selectedSkinCount; i++)
                        SelectedSkins.Add(br.ReadInt64());
                }
                catch (Exception)
                { Money = 100; }

                try //Version 3
                {
                    LastAdWatch = new DateTime(br.ReadInt64());
                }
                catch (Exception)
                { LastAdWatch = DateTime.Now.AddYears(-1); }
            }
            WorldPlace = 0;
            Rating = GamePlaces.Select((p) => GetScore(p)).Sum();
        }
        public byte[] SaveUser()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
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

                //Version 2
                bw.Write(Money);
                bw.Write(OwnedSkins.Count);
                foreach (var skin in OwnedSkins)
                    bw.Write(skin);
                bw.Write(SelectedSkins.Count);
                foreach (var skin in SelectedSkins)
                    bw.Write(skin);

                //Version 3
                bw.Write(LastAdWatch.Ticks);

                return ms.ToArray();
            }
        }

        public User(UserBase uBase, string name)
        {
            UserId = uBase.UserNumber++;
            UserKey = Guid.NewGuid().ToString();
            UserName = name;
        }

        public void BuySkin(Skin skin)
        {
            if (skin == null) return;
            if (OwnedSkins.Contains(skin.SkinId)) return;
            if (Money > skin.Price)
            {
                Money -= skin.Price;
                OwnedSkins.Add(skin.SkinId);
            }
        }
        public List<SkinInfo> GetSkinInfo(List<Skin> skins) =>
            skins.Select((s) => new SkinInfo()
            {
                Name = s.Name,
                SkinId = s.SkinId,
                UnitModel = s.UnitModel,
                SkinMesh = s.SkinMesh,
                Price = s.Price,
                Owned = OwnedSkins.Contains(s.SkinId),
                Selected = SelectedSkins.Contains(s.SkinId),
            }).ToList();
        public List<Skin> GetSelected(List<Skin> skins) =>
            skins.Where((s) => SelectedSkins.Contains(s.SkinId)).ToList();
        public void SelectSkin(Skin skin, List<Skin> skins)
        {
            if (skin == null) return;
            if (!OwnedSkins.Contains(skin.SkinId)) return;
            var unit = skin.UnitModel;
            var selected = GetSelected(skins).Where((s) => s.UnitModel != unit).ToList();
            selected.Add(skin);
            SelectedSkins = selected.Select((s) => s.SkinId).ToList();
        }

        public int UpdateUser(ReqSendStatus status)
        {
            LastGame = DateTime.Now;

            double moneyEarn = 0;
            if (status.GameMode == 0)
            {
                if (status.Place == 1)
                    WinCount++;
                TotalAliveTime += status.AliveTime;
                TotalDamageDeal += status.DamageDeal;
                TotalDamageReceive += status.DamageReceive;
                TotalUnitKill += status.UnitKill;
                TotalUnitRise += status.UnitRise;
                GamePlaces.Enqueue(status.Place);
                AvgPlace = GamePlaces.Average();
                AvgAliveTime = (AvgAliveTime * GameCount + status.AliveTime) / (GameCount + 1);
                AvgDamageDeal = (AvgDamageDeal * GameCount + status.DamageDeal) / (GameCount + 1);
                AvgDamageReceive = (AvgDamageReceive * GameCount + status.DamageReceive) / (GameCount + 1);
                AvgUnitKill = (AvgUnitKill * GameCount + status.UnitKill) / (GameCount + 1);
                AvgUnitRise = (AvgUnitRise * GameCount + status.UnitRise) / (GameCount + 1);
                GameCount++;

                while (GamePlaces.Count < RatingGameCount)
                    GamePlaces.Enqueue(_config.LastPlace);
                while (GamePlaces.Count > RatingGameCount)
                    GamePlaces.Dequeue();

                moneyEarn = Math.Max(0, GetScore(status.Place));
            }
            else
            {
                moneyEarn = status.UnitKill * _config.MoneyForKill;
            }

            Rating = GamePlaces.Select((p) => GetScore(p)).Sum();
            Money += moneyEarn;
            return (int)moneyEarn;
        }

        private static double GetScore(int place)
        {
            if (place > 5)
                return -0.2 * place;
            double score = 10;
            for (int i = 1; i < place; i++)
                score /= 2;
            return score;
        }
    }
}
