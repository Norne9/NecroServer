using System;
using System.Globalization;

namespace NecroServer
{
    public class Config
    {
        public int Port { get; set; } = 15364;

        public int MaxUnitCount { get; set; } = 8;
        public int MaxPlayers { get; set; } = 30;
        public int UpdateDelay { get; set; } = 30;

        public float PlayerWaitTime { get; set; } = 30f;
        public float MinWaitTime { get; set; } = 10f;
        public float EndWaitTime { get; set; } = 15f;

        public string DiscordLog { get; set; } = null;
        public string ConnectionKey { get; set; } = "Release_v0.2";
        public int DiscordAll { get; set; } = 0;

        public string MasterServer { get; set; } = "http://85.143.173.253:8856/";
        public int MasterUpdateDelay { get; set; } = 2000;

        public int MaxUnitsPacket { get; set; } = 60;
        public int AdditionalUnitCount { get; set; } = 2;
        public int UnitCount { get; set; } = 150;
        public int RuneCount { get; set; } = 16;
        public int ObstacleCount { get; set; } = 200;

        public int MaxWorldScale { get; set; } = 80;
        public int MinWorldScale { get; set; } = 60;

        public float RuneTime { get; set; } = 20f;

        public float UnitRange { get; set; } = 0.98f;
        public float ObstacleRange { get; set; } = 1.05f;
        public float RuneRange { get; set; } = 0.88f;

        public float ViewRange { get; set; } = 20f;

        public float RiseRadius { get; set; } = 5f;
        public float RiseCooldown { get; set; } = 10f;
        public float RiseAddCooldown { get; set; } = 0.6f;

        public float HealValue { get; set; } = 0.2f;
        public float RandomDamage { get; set; } = 0.1f;

        public float ZoneDps { get; set; } = 12f;

        public float InputDeadzone { get; set; } = 0.001f;

        public float StaticTime { get; set; } = 50f;
        public float ResizeTime { get; set; } = 60f;

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