using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using MasterReqResp;

namespace MasterServer
{
    public class UserBase : IDisposable
    {
        private const string UserNumberFile = "UserNumber.bin";
        public long UserNumber = 0;

        private Dictionary<long, User> Users = new Dictionary<long, User>();
        private IOrderedEnumerable<KeyValuePair<long, User>> SortedUsers = null;
        private Config Config;
        private int NewUsers = 0;

        public UserBase(Config config)
        {
            Config = config;
            try
            {
                Logger.Log("BASE loading");
                if (File.Exists(UserNumberFile))
                {
                    var numBytes = File.ReadAllBytes(UserNumberFile);
                    UserNumber = BitConverter.ToInt64(numBytes, 0);
                    Logger.Log($"BASE user number: {UserNumber}");
                }
                var files = Directory.GetFiles(Config.UserBasePath, $"*{Config.UserBaseExt}", SearchOption.AllDirectories);
                Logger.Log($"BASE found {files.Length} records");
                int loaded = 0, errors = 0;
                foreach (var fileName in files)
                {
                    try
                    {
                        using (var file = File.OpenRead(fileName))
                        {
                            var user = new User(file);
                            if ((DateTime.Now - user.LastGame).TotalSeconds < Config.LeaderboardTime)
                            {
                                Users.Add(user.UserId, user);
                                loaded++;
                            }
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
        }

        public void DebugUsers()
        {
            var countHour = Users.Values.Where((u) => (DateTime.Now - u.LastGame).TotalHours < 1).Count();
            var countLeader = Users.Count;
            var countTotal = UserNumber;
            Logger.Log($"DEBUG user statistics:\n\treg\t\thour\t\tleader\t\ttotal\n\t{NewUsers}\t\t{countHour}\t\t{countLeader}\t\t{countTotal}");
            NewUsers = 0;
        }

        //Server
        public RespStatus UpdateUserStatus(ReqSendStatus status)
        {
            var user = GetUser(status.UserId);
            if (user == null) //Unknown user
                return new RespStatus() { Rating = 0, UserId = status.UserId };

            user.UpdateUser(status);
            if (!Users.ContainsKey(user.UserId))
                Users.Add(user.UserId, user);
            UpdateUsers();

            return new RespStatus() { Rating = user.WorldPlace, UserId = user.UserId };
        }
        public RespClient GetClinetData(ReqClient reqClient)
        {
            var user = GetUser(reqClient.UserId);
            if (user == null || user.UserKey != reqClient.UserKey)
                return new RespClient() { Valid = false };

            var doubleUnits = user.DoubleUnits;
            user.DoubleUnits = false;

            return new RespClient()
            {
                Valid = true,
                DoubleUnits = doubleUnits,
                Name = user.UserName,
                UserId = user.UserId
            };
        }

        //Client
        public RespRegister RegisterUser(ReqRegister reqRegister)
        {
            string name = reqRegister.UserName;
            if (name.Length > Config.MaxNameLenght)
                name = name.Substring(0, Config.MaxNameLenght);

            var user = new User(this, name);
            SaveUser(user); NewUsers++;

            return new RespRegister()
            {
                UserId = user.UserId,
                UserKey = user.UserKey
            };
        }
        public RespUserStatus GetUserStatus(ReqUserStatus reqUserStatus)
        {
            var user = GetUser(reqUserStatus.UserId);
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
        public RespLeaderboard GetLeaderboard(ReqLeaderboard reqLeaderboard)
        {
            if (SortedUsers == null)
                UpdateUsers();
            var scores = SortedUsers
                .Skip(reqLeaderboard.Page * reqLeaderboard.Count).Take(reqLeaderboard.Count)
                .Select((u) => new Score() { Name = u.Value.UserName, Place = u.Value.WorldPlace, WinCount = u.Value.WinCount })
                .ToList();
            return new RespLeaderboard() { Scores = scores };
        }
        public void SetDoubleUnits(long userId, bool doubleUnits)
        {
            var user = GetUser(userId);
            if (user != null)
            {
                user.DoubleUnits = doubleUnits;
                if (!Users.ContainsKey(userId)) SaveUser(user);
            }
        }

        private void UpdateUsers()
        {
            List<long> toRemove = new List<long>();
            foreach (var (userId, user) in Users)
                if ((DateTime.Now - user.LastGame).TotalSeconds > Config.LeaderboardTime)
                {
                    SaveUser(user);
                    toRemove.Add(userId);
                }
            foreach (var userId in toRemove)
                Users.Remove(userId);

            SortedUsers = Users.OrderByDescending((user) => user.Value.Rating);
            int place = 0;
            foreach (var (userId, user) in SortedUsers)
                user.WorldPlace = ++place;
        }

        private void SaveUser(User user)
        {
            var dir = GetUserDir(user.UserId);
            var path = GetUserPath(user.UserId);
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (var file = File.Create(path))
                {
                    user.SaveUser(file);
                }
            }
            catch (Exception e)
            {
                Logger.Log($"BASE file '{path}' save error:\n{e.ToString()}", true);
            }
        }
        private User GetUser(long userId)
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
                    using (var stream = File.OpenRead(path))
                    {
                        return new User(stream);
                    }
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
            SaveNumber();
            foreach (var (userId, user) in Users)
                SaveUser(user);
            Logger.Log("BASE saving done");
        }
        private void SaveNumber() =>
            File.WriteAllBytes(UserNumberFile, BitConverter.GetBytes(UserNumber));
        public void Dispose() =>
            Save();
    }
}
