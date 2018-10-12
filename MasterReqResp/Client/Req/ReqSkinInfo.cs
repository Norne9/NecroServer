using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class ReqSkinInfo
    {
        public long UserId { get; set; } = 0;
        public long SkinId { get; set; } = 0;
        public ReqSkinCommand Command { get; set; } = ReqSkinCommand.Get;
    }
}
