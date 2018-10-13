using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class RespRestore
    {
        public bool Success { get; set; } = false;
        public long UserId { get; set; } = 0;
        public string UserKey { get; set; } = "";
    }
}
