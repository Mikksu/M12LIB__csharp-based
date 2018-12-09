using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M12.Definitions;

namespace M12.Base
{
    public class DigitalInput
    {
        public DigitalInput(byte[] Data)
        {
            BitArray bits = new BitArray(Data);
            this.DIN1 = bits.Get(0) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN2 = bits.Get(1) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN3 = bits.Get(2) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN4 = bits.Get(3) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN5 = bits.Get(4) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN6 = bits.Get(5) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN7 = bits.Get(6) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
            this.DIN8 = bits.Get(7) == false ? DigitalIOStatus.OFF : DigitalIOStatus.ON;
        }

        public DigitalIOStatus DIN1 { get; set; }
        public DigitalIOStatus DIN2 { get; set; }
        public DigitalIOStatus DIN3 { get; set; }
        public DigitalIOStatus DIN4 { get; set; }
        public DigitalIOStatus DIN5 { get; set; }
        public DigitalIOStatus DIN6 { get; set; }
        public DigitalIOStatus DIN7 { get; set; }
        public DigitalIOStatus DIN8 { get; set; }
    }
}
