using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class RespSkinInfo
    {
        public int Money { get; set; } = 0;
        public List<SkinInfo> Skins { get; set; } = new List<SkinInfo>();
    }
}
