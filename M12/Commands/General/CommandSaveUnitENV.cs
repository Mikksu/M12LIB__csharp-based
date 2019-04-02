using M12.CustomizedAttribute;
using M12.Definitions;
using System.IO;

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

        #region Methods

        internal override byte[] GeneratePayload()
        {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    wr.Write((byte)UnitID);
                }

                data = stream.ToArray();
            }

            return data;
        }

        #endregion
    }
}
