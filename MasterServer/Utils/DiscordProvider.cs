using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MasterServer
{
    public class DiscordProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) =>
            new DiscordLogger();

        public void Dispose()
        { }
    }
}
