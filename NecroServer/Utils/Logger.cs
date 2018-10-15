using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace NecroServer
{
    public class Logger : IDisposable
    {
        //STATIC
        private static Logger Inst;
        public static void Init(string discordLog, bool logAll)
        {
            Inst = new Logger(discordLog, logAll);
        }
        public static void Log(string message, bool crit = false)
        {
            if (Inst == null)
                Console.WriteLine($"#[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")}]\t{message}");
            else
                Inst.AddMessage(message, crit);
        }
        public static void Stop()
        {
            Inst?.Dispose();
        }

        private struct Message
        {
            public DateTime Time { get; set; }
            public string Text { get; set; }
            public bool IsCritical { get; set; }
        }

        //INSTANCE
        private const int MsgTaskDelay = 500;

        private readonly string _webHookUrl;

        private readonly HttpClient _client = new HttpClient();
        private readonly ConcurrentQueue<Message> _msgQueue = new ConcurrentQueue<Message>();
        private readonly Task _msgTask;
        private bool _work = true, _logAll = false;

        private Logger(string discordLog, bool logAll)
        {
            _logAll = logAll;
            _webHookUrl = discordLog;
            _msgTask = Task.Run(() => LoggerTask());
        }

        private async Task LoggerTask()
        {
            while (_work || _msgQueue.Count > 0)
            {
                while (_msgQueue.TryDequeue(out Message msg))
                {
                    try
                    {
                        var col = Console.ForegroundColor;
                        Console.ForegroundColor = msg.IsCritical ? ConsoleColor.DarkRed : col;
                        Console.WriteLine($"{(msg.IsCritical ? "!" : " ")}[{msg.Time.ToString("dd.MM.yyyy HH:mm:ss.fff")}]\t{msg.Text}");
                        Console.ForegroundColor = col;
                        if (_logAll || msg.IsCritical)
                            await DiscordSend(msg.Time, msg.Text, msg.IsCritical);
                    }
                    catch (Exception) { }
                }
                await Task.Delay(MsgTaskDelay);
            }
        }

        private async Task DiscordSend(DateTime time, string log, bool crit)
        {
            if (string.IsNullOrEmpty(_webHookUrl)) return;

            try
            {
                var message = $"{(crit ? "@everyone **CRITICAL ERROR**\n" : "")}```ml\n[{time.ToString("dd.MM.yyyy HH:mm:ss.fff")}] {log}```";
                var values = new Dictionary<string, string> { { "content", message }, };
                var content = new FormUrlEncodedContent(values);
                _ = await _client.PostAsync(_webHookUrl, content);
            }
            catch (Exception e)
            { Console.WriteLine($"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")}] Discord fail: {e.Message}"); }
        }

        private void AddMessage(string message, bool crit)
        {
            _msgQueue.Enqueue(new Message()
            {
                Time = DateTime.Now,
                Text = message,
                IsCritical = crit,
            });
        }

        public void Dispose()
        {
            _work = false;
            _msgTask?.Wait();
            _client?.Dispose();
        }
    }
}
