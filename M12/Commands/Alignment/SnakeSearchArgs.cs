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
            this.HorizontalAxis = HAxis;
            this.HorizontalRange = HRange;
            this.VerticalAxis = VAxis;
            this.VerticalRange = VRange;
            this.Gap = Gap;
            this.SamplingInterval = Interval;
            this.Speed = Speed;
        }

        #endregion

        #region Properties

        public UnitID HorizontalAxis { get; set; }

        public int HorizontalRange { get; set; }

        public UnitID VerticalAxis { get; set; }

        public int VerticalRange { get; set; }

        public uint Gap { get; set; }

        public ushort SamplingInterval { get; set; }

        public byte Speed { get; set; }

        /// <summary>
        /// If true, the starting point will be the center of the scan area, 
        /// which means that the both horizontal and vertical axes are shifted by 
        /// -range/2 before scanning.
        /// </summary>
        public bool IsStartFromCenter { get; set; } = true;

        /// <summary>
        /// If true, move the horizontal axis in the negative direction in the scanning process.
        /// </summary>
        public bool IsFlipHorizontalScanDirection { get; set; } = false;

        /// <summary>
        /// If true, move the vertical axis in the negative direction in the scanning process.
        /// </summary>
        public bool IsFlipVertialScanDirection { get; set; } = false;

        #endregion
    }
}
