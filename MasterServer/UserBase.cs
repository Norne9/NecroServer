﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using MasterReqResp;
using System.Threading.Tasks;

namespace MasterServer
{
    public class UserBase : IDisposable
    {
        public long UserNumber = 0;

        private Dictionary<long, User> Users = new Dictionary<long, User>();
        private IOrderedEnumerable<KeyValuePair<long, User>> SortedUsers = null;
        private Config Config;
        private MasterData MasterData;
        private int NewUsers = 0;

        public UserBase(Config config, MasterData masterData)
        {
            MasterData = masterData;
            Config = config;
            try
            {
                Logger.Log("BASE loading");
                var files = Directory.GetFiles(Config.UserBasePath, $"*{Config.UserBaseExt}", SearchOption.AllDirectories);
                Logger.Log($"BASE found {files.Length} records");
                int loaded = 0, errors = 0;
                foreach (var fileName in files)
                {
                    try
                    {
                        var data = File.ReadAllBytes(fileName);
                        var user = new User(data);
                        if (user.UserId > UserNumber) UserNumber = user.UserId;
                        if ((DateTime.Now - user.LastGame).TotalSeconds < Config.LeaderboardTime)
                        {
                            Users.Add(user.UserId, user);
                            loaded++;
                        }
                    }
                    catch (Exception)
                    {
                        errors++;
                    }
                }
                Logger.Log($"BASE loaded {loaded}/{files.Length}, errors: {errors}", errors > 0);
            }
            catch (Exception e)
            {
                Logger.Log($"BASE load error: {e.ToString()}", true);
            }
            Logger.Log($"BASE user number: {++UserNumber}");
        }

        public void DebugUsers()
        {
            var count10min = Users.Values.Where((u) => (DateTime.Now - u.LastGame).TotalMinutes < 10).Count();
            var countLeader = Users.Count;
            var countTotal = UserNumber;
            Logger.Log($"DEBUG user statistics:\nRegistred: {NewUsers}\n10 Min play: {count10min}\nLeaderboard: {countLeader}\nTotal: {countTotal}");
            NewUsers = 0;
        }

        //Server
        public async Task<RespStatus> UpdateUserStatus(ReqSendStatus status)
        {
            var user = await GetUser(status.UserId);
            if (user == null) //Unknown user
                return new RespStatus();

            user.UpdateUser(status);
            if (!Users.ContainsKey(user.UserId))
                Users.Add(user.UserId, user);
            await UpdateUsers();

            return new RespStatus() { Rating = user.WorldPlace, UserId = user.UserId };
        }
        public async Task<RespClient> GetClinetData(ReqClient reqClient, List<Skin> skins)
        {
            var user = await GetUser(reqClient.UserId);
            if (user == null || user.UserKey != reqClient.UserKey)
                return new RespClient() { Valid = false };

            var doubleUnits = user.DoubleUnits;
            user.DoubleUnits = false;

            var skinDict = new Dictionary<byte, byte>();
            var selectedSkins = user.GetSelected(skins);
            foreach (var skin in selectedSkins)
                if (!skinDict.ContainsKey(skin.UnitModel))
                    skinDict.Add(skin.UnitModel, skin.SkinMesh);

            return new RespClient()
            {
                Valid = true,
                DoubleUnits = doubleUnits,
                Name = user.UserName,
                UserId = user.UserId,
                UnitSkins = skinDict
            };
        }

        //Client
        public async Task<RespRegister> RegisterUser(ReqRegister reqRegister)
        {
            string name = reqRegister.UserName;
            if (name.Length > Config.MaxNameLenght)
                name = name.Substring(0, Config.MaxNameLenght);

            var user = new User(this, name);
            await SaveUser(user); NewUsers++;

            return new RespRegister()
            {
                UserId = user.UserId,
                UserKey = user.UserKey
            };
        }
        public async Task<RespUserStatus> GetUserStatus(ReqUserStatus reqUserStatus)
        {
            var user = await GetUser(reqUserStatus.UserId);
            if (user == null) //Unknown user
                return null;

            return new RespUserStatus()
            {
                UserName = user.UserName,
                Rating = user.WorldPlace,
                WinCount = user.WinCount,
                GameCount = user.GameCount,

                AvgAliveTime = (float)user.AvgAliveTime,
                AvgPlace = (float)user.AvgPlace,
                AvgDamageDeal = (float)user.AvgDamageDeal,
                AvgDamageReceive = (float)user.AvgDamageReceive,
                AvgUnitKill = (float)user.AvgUnitKill,
                AvgUnitRise = (float)user.AvgUnitRise,

                TotalAliveTime = (float)user.TotalAliveTime,
                TotalDamageDeal = (float)user.TotalDamageDeal,
                TotalDamageReceive = (float)user.TotalDamageReceive,
                TotalUnitRise = user.TotalUnitRise,
                TotalUnitKill = user.TotalUnitKill,
            };
        }
        public async Task<RespLeaderboard> GetLeaderboard(ReqLeaderboard reqLeaderboard)
        {
            if (SortedUsers == null)
                await UpdateUsers();
            var scores = SortedUsers
                .Skip(reqLeaderboard.Page * reqLeaderboard.Count).Take(reqLeaderboard.Count)
                .Select((u) => new Score() { Name = u.Value.UserName, Place = u.Value.WorldPlace, WinCount = u.Value.WinCount, UserId = u.Value.UserId })
                .ToList();
            return new RespLeaderboard() { Scores = scores };
        }
        public async Task SetDoubleUnits(long userId, bool doubleUnits)
        {
            var user = await GetUser(userId);
            if (user != null)
            {
                user.DoubleUnits = doubleUnits;
                if (!Users.ContainsKey(userId)) await SaveUser(user);
            }
        }
        public async Task<RespSkinInfo> GetSkinInfo(ReqSkinInfo reqSkinInfo)
        {
            var user = await GetUser(reqSkinInfo.UserId);
            if (user == null) return new RespSkinInfo();
            var skin = MasterData.GetSkin(reqSkinInfo.SkinId);
            switch (reqSkinInfo.Command)
            {
                case ReqSkinCommand.Get:
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(MasterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                    };
                case ReqSkinCommand.Select:
                    user.SelectSkin(skin, MasterData.GetSkins());
                    if (!Users.ContainsKey(user.UserId)) await SaveUser(user);
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(MasterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                    };
                case ReqSkinCommand.Buy:
                    user.BuySkin(skin);
                    if (!Users.ContainsKey(user.UserId)) await SaveUser(user);
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(MasterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                    };
                default:
                    return new RespSkinInfo();
            }
        }

        private async Task UpdateUsers()
        {
            List<long> toRemove = new List<long>();
            foreach (var (userId, user) in Users)
                if ((DateTime.Now - user.LastGame).TotalSeconds > Config.LeaderboardTime)
                {
                    await SaveUser(user);
                    toRemove.Add(userId);
                }
            foreach (var userId in toRemove)
                Users.Remove(userId);

            SortedUsers = Users.OrderByDescending((user) => user.Value.Rating);
            int place = 0;
            foreach (var (userId, user) in SortedUsers)
                user.WorldPlace = ++place;
        }

        private async Task SaveUser(User user)
        {
            var dir = GetUserDir(user.UserId);
            var path = GetUserPath(user.UserId);
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (var file = File.Create(path))
                {
                    var data = user.SaveUser();
                    await file.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"BASE file '{path}' save error:\n{e.ToString()}", true);
            }
        }
        private async Task<User> GetUser(long userId)
        {
            if (Users.ContainsKey(userId))
                return Users[userId];
            else
            {
                var path = GetUserPath(userId);
                if (!File.Exists(path))
                {
                    Logger.Log($"BASE file '{path}' not found", true);
                    return null;
                }
                try
                {
                    var data = await File.ReadAllBytesAsync(path);
                    return new User(data);
                }
                catch (Exception e)
                {
                    Logger.Log($"BASE file '{path}' read error:\n{e.ToString()}", true);
                    return null;
                }
            }
        }

        private string GetUserDir(long userId)
        {
            var bytes = BitConverter.GetBytes(userId);
            return Path.Combine(Config.UserBasePath, bytes[0].ToString("X"), bytes[1].ToString("X"));
        }
        private string GetUserPath(long userId)
        {
            var bytes = BitConverter.GetBytes(userId);
            return Path.Combine(Config.UserBasePath, bytes[0].ToString("X"), bytes[1].ToString("X"), userId.ToString("X") + Config.UserBaseExt);
        }

        public void Save()
        {
            Logger.Log("BASE saving");
            foreach (var (userId, user) in Users)
                SaveUser(user).Wait();
            Logger.Log("BASE saving done");
        }
        public void Dispose() =>
            Save();
    }
}
