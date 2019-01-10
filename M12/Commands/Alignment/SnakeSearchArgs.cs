using M12.Definitions;

namespace M12.Commands.Alignment
{

    public class SnakeSearchArgs
    {
        #region Constructors

        public SnakeSearchArgs() { }

        public SnakeSearchArgs(UnitID HAxis, int HRange, UnitID VAxis, 
            int VRange, uint Gap, ushort Interval, byte Speed)
        {
            this.HorizonalAxis = HAxis;
            this.HorizonalRange = HRange;
            this.VerticalAxis = VAxis;
            this.VerticalRange = VRange;
            this.Gap = Gap;
            this.Interval = Interval;
            this.Speed = Speed;
        }

        #endregion

        #region Properties

        public UnitID HorizonalAxis { get; set; }

        public int HorizonalRange { get; set; }

        public UnitID VerticalAxis { get; set; }

        public int VerticalRange { get; set; }

        public uint Gap { get; set; }

        public ushort Interval { get; set; }

        public byte Speed { get; set; }

        #endregion
    }
}
