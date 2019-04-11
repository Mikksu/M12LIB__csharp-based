using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M12.Definitions;

namespace M12.Commands.Motion
{
    public class CommandFastMove : CommandBase
    {
        public CommandFastMove(UnitID UnitID, int Steps, byte Speed, ushort Microsteps)
        {
            this.UnitID = UnitID;
            this.Steps = Steps;
            this.Speed = Speed;
            this.Microsteps = Microsteps;
        }

        #region Properties

        public override CommandDef Command
        {
            get
            {
                return CommandDef.HOST_CMD_FAST_MOVE;
            }
        }
      
        public int Steps { get; set; }

        public byte Speed { get; set; }

        public ushort Microsteps { get; set; }


        #endregion

        #region Methods

        internal override byte[] GeneratePayload()
        {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    wr.Write((byte)UnitID);
                    wr.Write(Steps);
                    wr.Write(Speed);
                    wr.Write(Microsteps);
                }

                data = stream.ToArray();
            }

            return data;

        }
        #endregion
    }
}
