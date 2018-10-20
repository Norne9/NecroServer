using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using MasterReqResp;
using System.Threading.Tasks;
using System.Threading;

namespace MasterServer
{
    public class UserBase : IDisposable
    {
        public long UserNumber = 0;

        private Dictionary<long, User> _users = new Dictionary<long, User>();
        private IOrderedEnumerable<KeyValuePair<long, User>> _sortedUsers = null;
        private Config _config;
        private MasterData _masterData;
        private int _newUsers = 0;

        private SemaphoreSlim _modifiUsers = new SemaphoreSlim(1, 1);

        public UserBase(Config config, MasterData masterData)
        {
            _masterData = masterData;
            _config = config;
            try
            {
                Logger.Log("BASE loading");
                var files = Directory.GetFiles(_config.UserBasePath, $"*{_config.UserBaseExt}", SearchOption.AllDirectories);
                Logger.Log($"BASE found {files.Length} records");
                int loaded = 0, errors = 0;
                foreach (var fileName in files)
                {
                    try
                    {
                        var data = File.ReadAllBytes(fileName);
                        var user = new User(data, _config);
                        if (user.UserId > UserNumber) UserNumber = user.UserId;
                        if ((DateTime.Now - user.LastGame).TotalSeconds < _config.LeaderboardTime)
                        {
                            _users.Add(user.UserId, user);
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
            try
            {
                var count10min = _users.Values.Where((u) => (DateTime.Now - u.LastGame).TotalSeconds < _config.DebugTime).Count();
                var countLeader = _users.Count;
                var countTotal = UserNumber;
                Logger.Log($"DEBUG user statistics:\nRegistred: {_newUsers}\n{(_config.DebugTime / 60f).ToString("f0")} Min play: {count10min}\nLeaderboard: {countLeader}\nTotal: {countTotal}");
                _newUsers = 0;
            }
            catch (Exception)
            { }
        }

        //Server
        public async Task<RespStatus> UpdateUserStatus(ReqSendStatus status)
        {
            var user = await GetUser(status.UserId);
            if (user == null) //Unknown user
                return new RespStatus();

            var moneyEarn = user.UpdateUser(status);

            await _modifiUsers.WaitAsync();
            try
            {
                if (!_users.ContainsKey(user.UserId))
                    _users.Add(user.UserId, user);
            }
            catch (Exception e)
            { throw e; }
            finally
            { _modifiUsers.Release(); }

            
            await UpdateUsers();

            return new RespStatus() { Rating = user.WorldPlace, UserId = user.UserId, GoldEarned = moneyEarn };
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
            if (name.Length > _config.MaxNameLenght)
                name = name.Substring(0, _config.MaxNameLenght);

            var user = new User(this, name);
            await SaveUser(user); _newUsers++;

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
            if (_sortedUsers == null)
                await UpdateUsers();
            var scores = _sortedUsers
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
                if (!_users.ContainsKey(userId)) await SaveUser(user);
            }
        }
        public async Task<RespSkinInfo> GetSkinInfo(ReqSkinInfo reqSkinInfo)
        {
            var user = await GetUser(reqSkinInfo.UserId);
            if (user == null) return new RespSkinInfo();
            var skin = _masterData.GetSkin(reqSkinInfo.SkinId);
            var timeToAd = (float)Math.Max(0, _config.WatchAdTime - (DateTime.Now - user.LastAdWatch).TotalSeconds);
            switch (reqSkinInfo.Command)
            {
                case ReqSkinCommand.Get:
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(_masterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                        WaitAdTime = timeToAd,
                        AdMoney = (int)_config.MoneyForAd,
                    };
                case ReqSkinCommand.Select:
                    user.SelectSkin(skin, _masterData.GetSkins());
                    if (!_users.ContainsKey(user.UserId)) await SaveUser(user);
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(_masterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                        WaitAdTime = timeToAd,
                        AdMoney = (int)_config.MoneyForAd,
                    };
                case ReqSkinCommand.Buy:
                    user.BuySkin(skin);
                    if (!_users.ContainsKey(user.UserId)) await SaveUser(user);
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(_masterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                        WaitAdTime = timeToAd,
                        AdMoney = (int)_config.MoneyForAd,
                    };
                case ReqSkinCommand.WatchAd:
                    if (timeToAd <= 0)
                    {
                        user.Money += _config.MoneyForAd;
                        user.LastAdWatch = DateTime.Now;
                        timeToAd = _config.WatchAdTime;
                        if (!_users.ContainsKey(user.UserId)) await SaveUser(user);
                    }
                    return new RespSkinInfo()
                    {
                        Skins = user.GetSkinInfo(_masterData.GetSkins()),
                        Money = (int)Math.Floor(user.Money),
                        WaitAdTime = timeToAd,
                        AdMoney = (int)_config.MoneyForAd,
                    };
                default:
                    return new RespSkinInfo();
            }
        }
        public async Task<RespRestore> RestoreUser(ReqRestore reqRestore)
        {
            string[] data = reqRestore.RestoreKey.Split('-');
            if (data.Length != 3) return new RespRestore();

            if (!byte.TryParse(data[0], System.Globalization.NumberStyles.HexNumber, null, out var b1))
                return new RespRestore();
            if (!long.TryParse(data[1], System.Globalization.NumberStyles.HexNumber, null, out var userId))
                return new RespRestore();
            if (!byte.TryParse(data[2], System.Globalization.NumberStyles.HexNumber, null, out var b2))
                return new RespRestore();

            var user = await GetUser(userId);
            if (user == null)
                return new RespRestore();

            var guid = Guid.Parse(user.UserKey).ToByteArray();
            if (guid[0] != b1 || guid[1] != b2)
                return new RespRestore();

            return new RespRestore()
            {
                Success = true,
                UserId = userId,
                UserKey = user.UserKey
            };
        }

        private async Task UpdateUsers()
        {
            List<long> toRemove = new List<long>();
            await _modifiUsers.WaitAsync();
            try
            {
                foreach (var (userId, user) in _users)
                    if ((DateTime.Now - user.LastGame).TotalSeconds > _config.LeaderboardTime)
                    {
                        await SaveUser(user);
                        toRemove.Add(userId);
                    }
                foreach (var userId in toRemove)
                    _users.Remove(userId);
                _sortedUsers = _users.OrderByDescending((user) => user.Value.Rating);
                int place = 0;
                foreach (var (userId, user) in _sortedUsers)
                    user.WorldPlace = ++place;
            }
            catch (Exception e)
            { throw e; }
            finally
            { _modifiUsers.Release(); }
        }

        private async Task<bool> SaveUser(User user)
        {
            var dir = GetUserDir(user.UserId);
            var path = GetUserPath(user.UserId);
            for (int i = 0; i < _config.MaxFileError; i++)
            {
                try
                {
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    using (var file = File.Create(path))
                    {
                        var data = user.SaveUser();
                        await file.WriteAsync(data, 0, data.Length);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    if (i == _config.MaxFileError - 1)
                        Logger.Log($"BASE file '{path}' save error:\n{e.ToString()}", true);
                    else
                        await Task.Delay(_config.WaitFileError);
                }
            }
            return false;
        }
        private async Task<User> GetUser(long userId)
        {
            if (_users.ContainsKey(userId))
                return _users[userId];
            else
            {
                var path = GetUserPath(userId);
                if (!File.Exists(path))
                {
                    Logger.Log($"BASE file '{path}' not found", true);
                    return null;
                }
                for (int i = 0; i < _config.MaxFileError; i++)
                {
                    try
                    {
                        var data = await File.ReadAllBytesAsync(path);
                        return new User(data, _config);
                    }
                    catch (Exception e)
                    {
                        if (i == _config.MaxFileError - 1)
                            Logger.Log($"BASE file '{path}' read error:\n{e.ToString()}", true);
                        else
                            await Task.Delay(_config.WaitFileError);
                    }
                }
            }
            return null;
        }

        private string GetUserDir(long userId)
        {
            var bytes = BitConverter.GetBytes(userId);
            return Path.Combine(_config.UserBasePath, bytes[0].ToString("X"), bytes[1].ToString("X"));
        }
        private string GetUserPath(long userId)
        {
            var bytes = BitConverter.GetBytes(userId);
            return Path.Combine(_config.UserBasePath, bytes[0].ToString("X"), bytes[1].ToString("X"), userId.ToString("X") + _config.UserBaseExt);
        }

        public async Task Save()
        {
            Logger.Log("BASE saving");
            int saved = 0;
            await _modifiUsers.WaitAsync();
            try
            {
                foreach (var (userId, user) in _users)
                    saved += await SaveUser(user) ? 1 : 0;
            }
            catch (Exception e)
            { throw e; }
            finally
            { _modifiUsers.Release(); }
            
            Logger.Log($"BASE saving done: {saved}/{_users.Count}");
        }
        public void Dispose() =>
            Save().Wait();
    }
}
