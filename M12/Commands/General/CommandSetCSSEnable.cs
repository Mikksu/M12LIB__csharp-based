using M12.CustomizedAttribute;
using M12.Definitions;
using System.IO;

namespace M12.Commands.General
{
    [CommandIndex(CommandDef.HOST_CMD_EN_CSS)]
    public class CommandSetCSSEnable : CommandBase
    {
        public CommandSetCSSEnable(CSSCH Channel, bool IsEnabled)
        {
            this.CSSChannel = Channel;
            this.IsEnabled = IsEnabled;
        }

        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_EN_CSS;

        public CSSCH CSSChannel { get; }

        public bool IsEnabled { get; }

        #endregion

        #region Methods

        internal override byte[] GeneratePayload()
        {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    wr.Write((byte)CSSChannel);
                    wr.Write(IsEnabled ? (byte)1 : (byte)0);
                }

                data = stream.ToArray();
            }

            return data;
        }

        #endregion
    }
}
