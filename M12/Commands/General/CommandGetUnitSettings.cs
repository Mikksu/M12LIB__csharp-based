using M12.CustomizedAttribute;
using M12.Definitions;

namespace M12.Commands.General
{
    [CommandIndex(CommandDef.HOST_CMD_GET_MCSU_SETTINGS)]
    public class CommandGetUnitSettings : CommandBase
    {

        public CommandGetUnitSettings(UnitID UnitID)
        {
            this.UnitID = UnitID;
        }

        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_GET_MCSU_SETTINGS;

        #endregion

        #region Methods

        internal override byte[] GeneratePayload()
        {
            return new byte[] { (byte)UnitID };
        }

        #endregion
    }
}
