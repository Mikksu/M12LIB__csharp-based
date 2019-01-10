using M12.CustomizedAttribute;

namespace M12.Commands.General
{
    [CommandIndex(CommandDef.HOST_CMD_GET_SYS_STA)]
    public class CommandGetSystemState : CommandBase
    {
        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_GET_SYS_STA;

        #endregion
    }
}
