using M12.CustomizedAttribute;
using M12.Definitions;

namespace M12.Commands.General
{
    [CommandIndex(CommandDef.HOST_CMD_SAV_MCSU_ENV)]
    public class CommandSaveUnitENV : CommandBase
    {

        public CommandSaveUnitENV(UnitID UnitID)
        {
            this.UnitID = UnitID;
        }

        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_SAV_MCSU_ENV;

        #endregion
    }
}
