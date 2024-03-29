﻿using System.IO;
using M12.CustomizedAttribute;
using M12.Definitions;

namespace M12.Commands.General
{
    [CommandIndex(CommandDef.HOST_CMD_SET_CSSTHD)]
    public class CommandSetCSSThreshold : CommandBase
    {
        public CommandSetCSSThreshold(CSSCH Channel, ushort LowThreshold, ushort HighThreshold)
        {
            CSSChannel = Channel;
            this.LowThreshold = LowThreshold;
            this.HighThreshold = HighThreshold;
        }

        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_SET_CSSTHD;

        public CSSCH CSSChannel { get; }

        public ushort LowThreshold { get; }

        public ushort HighThreshold { get; }
        
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
                    wr.Write((ushort)LowThreshold);
                    wr.Write((ushort)HighThreshold);
                }

                data = stream.ToArray();
            }

            return data;
        }

        #endregion
    }
}
