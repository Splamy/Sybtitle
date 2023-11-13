using System.Collections.Generic;

namespace SubtitlesParser.Classes
{
    public class SubtitlesFormat
    {
        // Properties -----------------------------------------

        public string Name { get; set; }
        public string Extension { get; set; }


        // Private constructor to avoid duplicates ------------

        private SubtitlesFormat() { }


        // Predefined instances -------------------------------

        public static readonly SubtitlesFormat SubRipFormat = new()
        {
            Name = "SubRip",
            Extension = @"\.srt"
        };
        public static readonly SubtitlesFormat MicroDvdFormat = new()
        {
            Name = "MicroDvd",
            Extension = @"\.sub"
        };
        public static readonly SubtitlesFormat SubViewerFormat = new()
        {
            Name = "SubViewer",
            Extension = @"\.sub"
        };
        public static readonly SubtitlesFormat SubStationAlphaFormat = new()
        {
            Name = "SubStationAlpha",
            Extension = @"\.ssa"
        };
        public static readonly SubtitlesFormat TtmlFormat = new()
        {
            Name = "TTML",
            Extension = @"\.ttml"
        };
        public static readonly SubtitlesFormat WebVttFormat = new()
        {
            Name = "WebVTT",
            Extension = @"\.vtt"
        };
        public static readonly SubtitlesFormat YoutubeXmlFormat = new()
        {
            Name = "YoutubeXml",
            //Extension = @"\.*"
        };

        public static readonly List<SubtitlesFormat> SupportedSubtitlesFormats = new()
            {
                SubRipFormat,
                MicroDvdFormat,
                SubViewerFormat,
                SubStationAlphaFormat,
                TtmlFormat,
                WebVttFormat,
                YoutubeXmlFormat
            };

    }


}
