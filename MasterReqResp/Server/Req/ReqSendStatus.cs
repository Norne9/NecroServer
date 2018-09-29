using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class ReqSendStatus
    {
        public long UserId { get; set; } = 0;
        public float AliveTime { get; set; } = 0f;
        public int Place { get; set; } = 0;
        public float DamageDeal { get; set; } = 0f;
        public float DamageReceive { get; set; } = 0f;
        public int UnitRise { get; set; } = 0;
        public int UnitKill { get; set; } = 0;
    }
}
