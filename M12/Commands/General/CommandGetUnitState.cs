using M12.CustomizedAttribute;
using M12.Definitions;

namespace M12.Commands.General
{
    [CommandIndex(CommandDef.HOST_CMD_GET_MCSU_STA)]
    public class CommandGetUnitState : CommandBase
    {
        public CommandGetUnitState(UnitID unitId)
        {
            UnitID = unitId;
        }

        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_GET_MCSU_STA;

        #endregion

        #region Methods

        internal override byte[] GeneratePayload()
        {
            return new byte[] { (byte)UnitID };
        }

        #endregion
    }
}
