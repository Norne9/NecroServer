using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class RespClient
    {
        public bool Valid { get; set; } = false;
        public string Name { get; set; } = "";
        public bool DoubleUnits { get; set; } = false;
        public long UserId { get; set; } = -1;
    }
}
