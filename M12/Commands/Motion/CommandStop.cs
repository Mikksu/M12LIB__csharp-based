using System.IO;
using M12.Definitions;

namespace M12.Commands.Motion
{
    public class CommandStop : CommandBase
    {
        public CommandStop(UnitID UnitID)
        {
            this.UnitID = UnitID;
        }

        #region Properties

        public override CommandDef Command
        {
            get
            {
                return CommandDef.HOST_CMD_STOP;
            }
        }


        #endregion

        #region Methods

        internal override byte[] GeneratePayload()
        {
            return new byte[] { (byte)UnitID };

        }
        #endregion
    }
}
