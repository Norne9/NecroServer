using System;
using System.Collections.Generic;
using System.Text;

namespace NecroServer
{
    public enum ServerState : byte
    {
        Started = 0,
        WaitingPlayers,
        Playing,
        EndGame
    }
}
