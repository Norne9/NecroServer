using NecroServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class AI
    {
        private static long AiUserId = -1;
        public static Player GetAiPlayer(Config config)
        {
            var id = AiUserId--;
            return new Player(id, id, "AI", false, config);
        }
    }
}
