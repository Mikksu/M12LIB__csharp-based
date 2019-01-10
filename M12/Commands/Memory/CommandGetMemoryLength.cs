﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M12.Commands.Memory
{
    public class CommandGetMemoryLength : CommandBase
    {

        #region Properties

        public override CommandDef Command
        {
            get
            {
                return CommandDef.HOST_CMD_GET_MEM_LEN;
            }
        }
        
        #endregion
        
    }
}
