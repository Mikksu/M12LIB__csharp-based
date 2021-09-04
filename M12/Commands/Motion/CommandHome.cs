using System.IO;
using M12.Definitions;

namespace M12.Commands.Motion
{
    public class CommandHome : CommandBase
    {
        //TODO The acceleration, speed will be cancelled and write to the flash in the furture.
        public CommandHome(UnitID UnitID, ushort Acc, byte LowSpeed, byte HighSpeed)
        {
            this.UnitID = UnitID;
            Acceleration = Acc;
            SpeedLow = LowSpeed;
            SpeedHigh = HighSpeed;
        }

        #region Properties

        public override CommandDef Command
        {
            get
            {
                return CommandDef.HOST_CMD_HOME;
            }
        }

        public ushort Acceleration { get; set; }

        public byte SpeedLow { get; set; }

        public byte SpeedHigh { get; set; }


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
                    wr.Write(Acceleration);
                    wr.Write(SpeedLow);
                    wr.Write(SpeedHigh);
                }

                data = stream.ToArray();
            }

            return data;

        }
        #endregion
    }
}
