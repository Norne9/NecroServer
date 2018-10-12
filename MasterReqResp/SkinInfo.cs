using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class SkinInfo
    {
        public long SkinId { get; set; } = 0;
        public string Name { get; set; } = "";
        public byte UnitModel { get; set; } = 0;
        public byte SkinMesh { get; set; } = 0;
        public int Price { get; set; } = 0;
        public bool Owned { get; set; } = false;
        public bool Selected { get; set; } = false;
    }
}
