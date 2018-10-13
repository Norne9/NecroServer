using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class RespSkinInfo
    {
        public int Money { get; set; } = 0;
        public float WaitAdTime { get; set; } = 0f;
        public int AdMoney { get; set; } = 0;
        public List<SkinInfo> Skins { get; set; } = new List<SkinInfo>();
    }
}