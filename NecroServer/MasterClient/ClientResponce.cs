using System;
using System.Collections.Generic;
using System.Text;

namespace MasterClient
{
    public class ClientResponce
    {
        public bool Valid { get; set; }
        public long UsedId { get; set; }
        public string Name { get; set; }
        public bool DoubleUnits { get; set; }
    }
}
