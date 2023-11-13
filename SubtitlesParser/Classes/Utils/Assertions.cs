using System;
using System.IO;

namespace SubtitlesParser.Classes.Utils;

internal class Assertions
{
    // test if stream if readable and seekable (just a check, should be good)
    public static void ValidateStream(Stream stream)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException($"Stream must be readable in a subtitles parser. Operation interrupted; isReadable: {stream.CanRead} - isSeekable: {stream.CanSeek}");
        }
    }
}
