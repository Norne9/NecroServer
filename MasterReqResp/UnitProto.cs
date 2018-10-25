using System;
using System.Collections.Generic;
using System.Text;

namespace MasterReqResp
{
    public class UnitProto
    {
        public string UnitName { get; set; } = "Default";
        public int UnitMesh { get; set; } = 0;
        public float UnitRadius { get; set; } = 0.5f;
        public UnitStats UnitStats { get; set; } = UnitStats.GetDefaultStats();
        public int UnitRate { get; set; } = 1;
        public bool SpawnNeutrall { get; set; } = false;
    }
}
