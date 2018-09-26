using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Packets
{
    public static class Packet
    {
        public static void Register(NetSerializer netSerializer)
        {
            netSerializer.RegisterCustomType<ObstacleInfo>();
            netSerializer.RegisterCustomType<PlayerInfo>();
            netSerializer.RegisterCustomType<PlayerCameraInfo>();
            netSerializer.RegisterCustomType<UnitInfo>();
            netSerializer.RegisterCustomType<RuneInfo>();
        }
    }
}
