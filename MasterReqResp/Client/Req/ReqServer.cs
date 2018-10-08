using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class ReqServer
    {
        public long UserId { get; set; } = 1;
        public string UserKey { get; set; } = "";
        public string ServerVersion { get; set; } = "";
        public bool DoubleUnits { get; set; } = false;
    }
}
