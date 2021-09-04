using M12.Commands;
using System;

namespace M12.CustomizedAttribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandIndexAttribute : Attribute
    {
        public CommandIndexAttribute(CommandDef ID)
        {
            CommandID = ID;
        }

        public CommandDef CommandID { get; set; }
    }
}
