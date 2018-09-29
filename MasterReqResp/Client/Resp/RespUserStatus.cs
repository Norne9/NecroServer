using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class RespUserStatus
    {
        public string UserName { get; set; } = "";
        public int Rating { get; set; } = 0;
        public int WinCount { get; set; } = 0;
        public int GameCount { get; set; } = 0;

        public float AvgAliveTime { get; set; } = 0;
        public int AvgPlace { get; set; } = 0;
        public float AvgDamageDeal { get; set; } = 0;
        public float AvgDamageReceive { get; set; } = 0;
        public int AvgUnitRise { get; set; } = 0;
        public int AvgUnitKill { get; set; } = 0;

        public float TotalAliveTime { get; set; } = 0;
        public float TotalDamageDeal { get; set; } = 0;
        public float TotalDamageReceive { get; set; } = 0;
        public int TotalUnitRise { get; set; } = 0;
        public int TotalUnitKill { get; set; } = 0;
    }
}
