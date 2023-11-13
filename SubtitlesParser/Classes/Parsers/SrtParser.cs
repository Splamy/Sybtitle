using SubtitlesParser.Classes.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitlesParser.Classes.Parsers
{
    /// <summary>
    /// Parser for the .srt subtitles files
    /// 
    /// A .srt file looks like:
    /// 1
    /// 00:00:10,500 --> 00:00:13,000
    /// Elephant's Dream
    ///
    /// 2
    /// 00:00:15,000 --> 00:00:18,000
    /// At the left we can see...[12]
    /// </summary>
    public partial class SrtParser : ISubtitlesParser
    {

        // Properties -----------------------------------------------------------------------

        private readonly string[] _delimiters = { "-->", "- >", "->" };


        // Constructors --------------------------------------------------------------------

        public SrtParser() { }


        // Methods -------------------------------------------------------------------------

        public List<SubtitleItem> ParseStream(Stream srtStream, Encoding encoding)
        {
            Assertions.ValidateStream(srtStream);

            var reader = new StreamReader(srtStream, encoding, true);

            var items = new List<SubtitleItem>();
            var srtSubParts = GetSrtSubTitleParts(reader).ToList();
            if (srtSubParts.Any())
            {
                foreach (var srtSubPart in srtSubParts)
                {
                    var lines =
                        srtSubPart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                            .Select(s => s.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

                    var item = new SubtitleItem();
                    foreach (var line in lines)
                    {
                        if (item.StartTime == TimeSpan.Zero && item.EndTime == TimeSpan.Zero)
                        {
                            // we look for the timecodes first
                            if (TryParseTimecodeLine(line) is (var startTc, var endTc))
                            {
                                item.StartTime = startTc;
                                item.EndTime = endTc;
                            }
                        }
                        else
                        {
                            // we found the timecode, now we get the text
                            item.Lines.Add(line);
                            // strip formatting by removing anything within curly braces or angle brackets, which is how SRT styles text according to wikipedia (https://en.wikipedia.org/wiki/SubRip#Formatting)
                            item.PlaintextLines.Add(StripFormatRegex().Replace(line, string.Empty));
                        }
                    }

                    if ((item.StartTime != TimeSpan.Zero || item.EndTime != TimeSpan.Zero) && item.Lines.Any())
                    {
                        // parsing succeeded
                        items.Add(item);
                    }
                }

                if (items.Any())
                {
                    return items;
                }
                else
                {
                    throw new ArgumentException("Stream is not in a valid Srt format");
                }
            }
            else
            {
                throw new FormatException("Parsing as srt returned no srt part.");
            }
        }

        /// <summary>
        /// Enumerates the subtitle parts in a srt file based on the standard line break observed between them. 
        /// A srt subtitle part is in the form:
        /// 
        /// 1
        /// 00:00:20,000 --> 00:00:24,400
        /// Altocumulus clouds occur between six thousand
        /// 
        /// </summary>
        /// <param name="reader">The textreader associated with the srt file</param>
        /// <returns>An IEnumerable(string) object containing all the subtitle parts</returns>
        private static IEnumerable<string> GetSrtSubTitleParts(TextReader reader)
        {
            string line;
            var sb = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    // return only if not empty
                    var res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                    {
                        yield return res;
                    }
                    sb = new StringBuilder();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private (TimeSpan start, TimeSpan end)? TryParseTimecodeLine(string line)
        {
            var parts = line.Split(_delimiters, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var startTc = ParseSrtTimecode(parts[0]);
                var endTc = ParseSrtTimecode(parts[1]);

                if (startTc.HasValue && endTc.HasValue)
                {
                    return (startTc.Value, endTc.Value);
                }
            }

            // this is not a timecode line
            return null;
        }

        /// <summary>
        /// Takes an SRT timecode as a string and parses it into a double (in seconds). A SRT timecode reads as follows: 
        /// 00:00:20,000
        /// </summary>
        /// <param name="s">The timecode to parse</param>
        /// <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
        private static TimeSpan? ParseSrtTimecode(string s)
        {
            var match = SrtTimecodeRegex().Match(s);
            if (match.Success)
            {
                s = match.Value;
                if (TimeSpan.TryParse(s.Replace(',', '.'), out TimeSpan result))
                {
                    return result;
                }
            }
            return null;
        }

        [GeneratedRegex(@"[0-9]+:[0-9]+:[0-9]+([,\.][0-9]+)?")]
        private static partial Regex SrtTimecodeRegex();

        [GeneratedRegex(@"\{.*?\}|<.*?>")]
        private static partial Regex StripFormatRegex();
    }
}
