using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterServer
{
    public class Skin
    {
        public long SkinId { get; set; } = 0;
        public string Name { get; set; } = "";
        public byte UnitModel { get; set; } = 0;
        public byte SkinMesh { get; set; } = 0;
        public int Price { get; set; } = 0;
    }
}
