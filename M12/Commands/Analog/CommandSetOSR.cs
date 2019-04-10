using M12.CustomizedAttribute;
using M12.Definitions;
using System.IO;

namespace M12.Commands.Analog
{
    [CommandIndex(CommandDef.HOST_CMD_SET_ADC_OSR)]
    public class CommandSetOSR : CommandBase
    {
        public CommandSetOSR() : base()
        {

        }

        public CommandSetOSR(ADC_OSR OSR)
        {
            this.OSR = OSR;
        }
        
        public override CommandDef Command => CommandDef.HOST_CMD_SET_ADC_OSR;
        
        public ADC_OSR OSR { get; set; }

        internal override byte[] GeneratePayload()
        {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    wr.Write((byte)OSR);
                }

                data = stream.ToArray();
            }

            return data;
        }
    }
}
