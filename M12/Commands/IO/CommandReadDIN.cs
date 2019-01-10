namespace M12.Commands.IO
{
    public class CommandReadDIN  : CommandBase
    {
        public CommandReadDIN() : base()
        {

        }
        
        public override CommandDef Command
        {
            get
            {
                return CommandDef.HOST_CMD_READ_DIN;
            }
        }

    }
}
