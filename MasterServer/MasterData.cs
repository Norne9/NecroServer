using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using MasterReqResp;

namespace MasterServer
{
    public class MasterData
    {
        private List<Skin> Skins = new List<Skin>();
        private List<string> Args = new List<string>();
        private List<TextMessage> Messages = new List<TextMessage>();
        private readonly Config Config;

        public MasterData(Config config)
        {
            Config = config;
            Reload().Wait();
        }

        public async Task Reload()
        {
            Logger.Log($"MDATA loading skins");
            try
            {
                string data = await File.ReadAllTextAsync(Config.ShopFile);
                Skins = JsonConvert.DeserializeObject<List<Skin>>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load skins: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading args");
            try
            {
                string[] lines = await File.ReadAllLinesAsync(Config.ParamsFile);
                Args = new List<string>(lines);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load args: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading texts");
            try
            {
                string data = await File.ReadAllTextAsync(Config.TextsFile);
                Messages = JsonConvert.DeserializeObject<List<TextMessage>>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load messages: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading finished.");
        }

        public RespConfig GetParams() =>
            new RespConfig() { Args = Args };

        public List<Skin> GetSkins() =>
            new List<Skin>(Skins);

        public Skin GetSkin(long skinId) =>
            Skins.Where((s) => s.SkinId == skinId).FirstOrDefault();

        public RespMessages GetMessages(ReqMessages req)
        {
            const int DefaultLang = 10;

            var messages = Messages.Where((m) => m.Lang == req.Lang).Select((m) => m.Message).ToList();
            if (messages.Count == 0)
                messages = Messages.Where((m) => m.Lang == DefaultLang).Select((m) => m.Message).ToList();
            return new RespMessages() { Messages = messages };
        }
    }
}
