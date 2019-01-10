using M12.CustomizedAttribute;
using M12.Definitions;
using System.IO;

namespace M12.Commands.Analog
{
    [CommandIndex(CommandDef.HOST_CMD_READ_AD)]
    public class CommandReadADC : CommandBase
    {
        public CommandReadADC() : base()
        {

        }

        public CommandReadADC(ADCChannels ChannelEnabled)
        {
            this.ChannelEnabled = ChannelEnabled;
        }
        
        public override CommandDef Command => CommandDef.HOST_CMD_READ_AD;
        
        public ADCChannels ChannelEnabled { get; set; }

        internal override byte[] GeneratePayload()
        {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    wr.Write((byte)ChannelEnabled);
                }

                data = stream.ToArray();
            }

            return data;
        }
    }
}
