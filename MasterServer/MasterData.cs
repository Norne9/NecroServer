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
        private List<Skin> _skins = new List<Skin>();
        private List<string> _args = new List<string>();
        private List<TextMessage> _messages = new List<TextMessage>();
        private List<UnitProto> _units = new List<UnitProto>();
        private readonly Config _config;

        public MasterData(Config config)
        {
            _config = config;
            Reload().Wait();
        }

        public async Task Reload()
        {
            Logger.Log($"MDATA loading skins");
            try
            {
                string data = await File.ReadAllTextAsync(_config.ShopFile);
                _skins = JsonConvert.DeserializeObject<List<Skin>>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load skins: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading args");
            try
            {
                string[] lines = await File.ReadAllLinesAsync(_config.ParamsFile);
                _args = new List<string>(lines);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load args: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading texts");
            try
            {
                string data = await File.ReadAllTextAsync(_config.TextsFile);
                _messages = JsonConvert.DeserializeObject<List<TextMessage>>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load messages: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading units");
            try
            {
                string data = await File.ReadAllTextAsync(_config.UnitsFile);
                _units = JsonConvert.DeserializeObject<List<UnitProto>>(data);
            }
            catch (Exception e)
            {
                Logger.Log($"MDATA failed to load units: {e.ToString()}", true);
            }

            Logger.Log($"MDATA loading finished.");
        }

        public RespConfig GetParams() =>
            new RespConfig() { Args = _args };

        public List<Skin> GetSkins() =>
            new List<Skin>(_skins);

        public Skin GetSkin(long skinId) =>
            _skins.Where((s) => s.SkinId == skinId).FirstOrDefault();

        public RespUnits GetUnits() =>
            new RespUnits() { Units = _units };

        public RespMessages GetMessages(ReqMessages req)
        {
            const int DefaultLang = 10;

            var messages = _messages.Where((m) => m.Lang == req.Lang).Select((m) => m.Message).ToList();
            if (messages.Count == 0)
                messages = _messages.Where((m) => m.Lang == DefaultLang).Select((m) => m.Message).ToList();
            return new RespMessages() { Messages = messages };
        }
    }
}
