﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class ReqState
    {
        public bool InLobby { get; set; } = true;
        public int ConnectedPlayers { get; set; } = 0;
        public int TotalPlayers { get; set; } = 0;
    }
}
