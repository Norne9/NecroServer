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
        public static void Init(string discordLog = "")
        {
            Inst = new Logger(discordLog);
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

        private readonly string WebHookUrl;

        private readonly HttpClient client = new HttpClient();
        private readonly ConcurrentQueue<Message> msgQueue = new ConcurrentQueue<Message>();
        private readonly Task msgTask;
        private bool Work = true;

        private Logger(string discordLog)
        {
            WebHookUrl = discordLog;
            msgTask = Task.Run(() => LoggerTask());
        }

        private async Task LoggerTask()
        {
            while (Work || msgQueue.Count > 0)
            {
                while (msgQueue.TryDequeue(out Message msg))
                {
                    try
                    {
                        var col = Console.ForegroundColor;
                        Console.ForegroundColor = msg.IsCritical ? ConsoleColor.DarkRed : col;
                        Console.WriteLine($"{(msg.IsCritical ? "!" : " ")}[{msg.Time.ToString("dd.MM.yyyy HH:mm:ss.fff")}]\t{msg.Text}");
                        Console.ForegroundColor = col;
                        await DiscordSend(msg.Time, msg.Text, msg.IsCritical);
                    }
                    catch (Exception) { }
                }
                await Task.Delay(MsgTaskDelay);
            }
        }

        private async Task DiscordSend(DateTime time, string log, bool crit)
        {
            if (string.IsNullOrEmpty(WebHookUrl)) return;

            try
            {
                var message = $"{(crit ? "@everyone **CRITICAL ERROR**\n" : "")}```ml\n[{time.ToString("dd.MM.yyyy HH:mm:ss.fff")}] {log}```";
                var values = new Dictionary<string, string> { { "content", message }, };
                var content = new FormUrlEncodedContent(values);
                _ = await client.PostAsync(WebHookUrl, content);
            }
            catch (Exception e)
            { Console.WriteLine($"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")}] Discord fail: {e.Message}"); }
        }

        private void AddMessage(string message, bool crit)
        {
            msgQueue.Enqueue(new Message()
            {
                Time = DateTime.Now,
                Text = message,
                IsCritical = crit,
            });
        }

        public void Dispose()
        {
            Work = false;
            msgTask?.Wait();
            client?.Dispose();
        }
    }
}
