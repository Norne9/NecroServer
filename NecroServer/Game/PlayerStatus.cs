using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class PlayerStatus
    {
        public float AliveTime { get; set; } = 0f;
        public int Place { get; set; } = 0;
        public int Rating { get; set; } = 0;
        public float DamageDeal { get; set; } = 0f;
        public float DamageReceive { get; set; } = 0f;
        public int UnitRise { get; set; } = 0;
        public int UnitKill { get; set; } = 0;
    }
}
