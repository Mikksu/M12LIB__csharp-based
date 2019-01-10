using M12.CustomizedAttribute;
using System.IO;

namespace M12.Commands.Alignment
{
    [CommandIndex(CommandDef.HOST_CMD_SNAKESEARCH)]
    public class CommandSnakeSearch : CommandBase
    {
        #region Constructors

        public CommandSnakeSearch(SnakeSearchArgs Args)
        {
            this.Args = Args;
        }

        #endregion

        #region Properties

        public override CommandDef Command => CommandDef.HOST_CMD_SNAKESEARCH;

        public SnakeSearchArgs Args { get; set; }

        #endregion

        #region Methods

        internal override byte[] GeneratePayload()
        {
            byte[] data = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(stream))
                {
                    wr.Write((byte)Args.HorizonalAxis);
                    wr.Write(Args.HorizonalRange);
                    wr.Write((byte)Args.VerticalAxis);
                    wr.Write(Args.VerticalRange);
                    wr.Write(Args.Gap);
                    wr.Write(Args.Interval);
                    wr.Write(Args.Speed);
                }

                data = stream.ToArray();
            }

            return data;

        }
        #endregion
    }
}
