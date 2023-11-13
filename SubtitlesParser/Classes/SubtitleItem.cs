using System;
using System.Collections.Generic;

namespace SubtitlesParser.Classes
{
    public record SubtitleItem
    {

        //Properties------------------------------------------------------------------

        /// <summary>
        /// Start time in milliseconds.
        /// </summary>
        public TimeSpan StartTime { get; set; }
        /// <summary>
        /// End time in milliseconds.
        /// </summary>
        public TimeSpan EndTime { get; set; }
        /// <summary>
        /// The raw subtitle string from the file
        /// May include formatting
        /// </summary>
        public List<string> Lines { get; set; }
        /// <summary>
        /// The plain-text string from the file
        /// Does not include formatting
        /// </summary>
        public List<string> PlaintextLines { get; set; }


        //Constructors-----------------------------------------------------------------

        /// <summary>
        /// The empty constructor
        /// </summary>
        public SubtitleItem()
        {
            this.Lines = new List<string>();
            this.PlaintextLines = new List<string>();
        }


        // Methods --------------------------------------------------------------------------

        public override string ToString()
        {
            var res = string.Format("{0} --> {1}: {2}", StartTime.ToString("G"), EndTime.ToString("G"), string.Join(Environment.NewLine, Lines));
            return res;
        }

    }
}