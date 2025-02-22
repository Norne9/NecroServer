﻿using System;
using System.Globalization;

namespace MasterServer
{
    public class Config
    {
        public int ServerPort { get; set; } = 7821;
        public string DiscordLog { get; set; } = null;
        public string UserBasePath { get; set; } = "Users";
        public string UserBaseExt { get; set; } = ".usr";

        public string ParamsFile { get; set; } = "Data/params.txt";
        public string ShopFile { get; set; } = "Data/shop.json";
        public string TextsFile { get; set; } = "Data/text.json";
        public string UnitsFile { get; set; } = "Data/units.json";

        public float LeaderboardTime { get; set; } = 60f * 60f * 24f * 3f;
        public float ServerTime { get; set; } = 3f;
        public float DebugTime { get; set; } = 15f * 60f;
        public float WatchAdTime { get; set; } = 5f * 60f;
        public float MoneyForAd { get; set; } = 8f;
        public float MoneyForKill { get; set; } = 0.1f;

        public int LastPlace { get; set; } = 30;
        public int MaxNameLenght { get; set; } = 16;
        public int MaxFileError { get; set; } = 3;
        public int WaitFileError { get; set; } = 300;

        public Config(string[] args)
        {
            AppendArgs(args);
        }

        public void AppendArgs(string[] args)
        {
            Type t = typeof(Config);
            var props = t.GetProperties();
            foreach (var prop in props)
            {
                var propName = prop.Name.ToLower();
                for (int i = 0; i < args.Length; i++)
                {
                    var cmd = args[i].Replace("-", "").ToLower();
                    if (cmd != propName) continue;
                    switch (Type.GetTypeCode(prop.PropertyType))
                    {
                        case TypeCode.String:
                            var paramString = args[++i];
                            prop.SetValue(this, paramString);
                            Logger.Log($"CONFIG '{prop.Name}': '{paramString}'");
                            break;
                        case TypeCode.Int32:
                            var paramInt = int.Parse(args[++i]);
                            prop.SetValue(this, paramInt);
                            Logger.Log($"CONFIG '{prop.Name}': '{paramInt}'");
                            break;
                        case TypeCode.Single:
                            var paramFloat = float.Parse(args[++i].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                            prop.SetValue(this, paramFloat);
                            Logger.Log($"CONFIG '{prop.Name}': '{paramFloat}'");
                            break;
                        default:
                            Logger.Log($"CONFIG unknown type: {prop.PropertyType.Name}");
                            break;
                    }
                }
            }
        }
    }
}
