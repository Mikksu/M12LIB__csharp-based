namespace M12.ProgressReport
{
    public class MemoryReadProgressReport
    {
        public MemoryReadProgressReport(int Read, int Total)
        {
            this.Read = Read;
            this.Total = Total;
        }

        #region Properties

        /// <summary>
        /// Get the amount of the data points to be read.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// Get how many data points have been read.
        /// </summary>
        public int Read { get; }

        /// <summary>
        /// Get the level of finish in percent.
        /// </summary>
        public double Complete
        {
            get
            {
                return (double)Read / (double)Total;
            }
        }

        #endregion
    }
}
