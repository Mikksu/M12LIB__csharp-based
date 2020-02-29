using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M12.ProgressReport
{
    public class BlindSearchProgressReport
    {
        public enum ProgressStage
        {
            /// <summary>
            /// Scanning stage
            /// </summary>
            SCAN,

            /// <summary>
            /// Data transmitting stage
            /// </summary>
            TRANS
        }

        public BlindSearchProgressReport(ProgressStage Stage, double Progress, string Message = "")
        {
            this.Stage = Stage;
            this.Progress = Progress;
            this.Message = Message;
        }


        #region Properties

        /// <summary>
        /// Get the stage of the progress.
        /// <para>SCAN</para> Scanning stage.
        /// <para>TRANS</para> Data transferring stage.
        /// </summary>
        public ProgressStage Stage { get; }

        public double Progress { get; }

        public string Message { get; }

        #endregion
    }
}
