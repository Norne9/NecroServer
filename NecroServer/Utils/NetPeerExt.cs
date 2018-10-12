using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace NecroServer
{
    public static class NetPeerExt
    {
        public static int GetUId(this NetPeer peer) =>
            peer.EndPoint.GetHashCode();
    }
}
