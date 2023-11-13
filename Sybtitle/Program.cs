using CommandLine;
using SubtitlesParser.Classes;
using SubtitlesParser.Classes.Parsers;
using SubtitlesParser.Classes.Writers;
using Sybtitle;
using System.Collections.Immutable;
using System.Text;

var options = Parser.Default.ParseArguments<Options>(args).WithNotParsed(errs =>
{
    var result = -2;
    Console.WriteLine("errors {0}", errs.Count());
    if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
        result = -1;
    Console.WriteLine("Exit code {0}", result);
    Environment.Exit(result);
}).Value;

// Read

ImmutableArray<SubtitleItem> inSubtitles;
List<SubtitleItem> outSubtiles = new();
using (var fileStream = File.Open(options.Input, FileMode.Open, FileAccess.Read, FileShare.Read))
{
    var parser = new SrtParser();
    inSubtitles = parser.ParseStream(fileStream, Encoding.UTF8).ToImmutableArray();
}

// Apply modifications

if (options.Offset is { } offset)
{
    foreach (var subtitle in inSubtitles)
    {
        outSubtiles.Add(subtitle with
        {
            StartTime = subtitle.StartTime + offset,
            EndTime = subtitle.EndTime + offset
        });
    }
}
else if (options.Rescales is { } rescalesInput)
{
    List<RescaleRule> rescales = new() { };

    foreach (var part in rescalesInput.Chunk(4))
    {
        if (part.Length == 4)
        {
            var timeParts = part.Select(HumanTimeSpan.Parse).ToList();
            if (timeParts.IndexOf(null) is var num && num >= 0)
                throw new InvalidDataException($"Invalid syncpoint: {part[num]}");
            rescales.Add(new RescaleRule(timeParts[0]!.Value, timeParts[1]!.Value, timeParts[2]!.Value, timeParts[3]!.Value));
        }
        else if (part.Length == 1 && part[0].Equals("auto", StringComparison.InvariantCultureIgnoreCase))
        {
            // Rescale evertying that comes after the last rescale linearly
            var lastRescale = rescales.Last();
            var linearOffset = lastRescale.NewEnd - lastRescale.OrigEnd;

            var autoScaleOrigStart = new TimeSpan(lastRescale.OrigEnd.Ticks + 1);
            var autoScaleOrigEnd = TimeSpan.MaxValue / 2;
            var autoScaleNewStart = autoScaleOrigStart + linearOffset;
            var autoScaleNewEnd = autoScaleOrigEnd + linearOffset;

            rescales.Add(new RescaleRule(autoScaleOrigStart, autoScaleOrigEnd, autoScaleNewStart, autoScaleNewEnd));
        }
    }

    if (rescales.Count == 0)
        throw new InvalidDataException("No syncpoints provided");

    RescaleRule? FindRescale(TimeSpan timeSpan) => rescales.FirstOrDefault(x => x.OrigStart <= timeSpan && timeSpan <= x.OrigEnd);

    foreach (var subtitle in inSubtitles)
    {
        var startRescale = FindRescale(subtitle.StartTime);
        var endRescale = FindRescale(subtitle.EndTime);

        TimeSpan Rescale(TimeSpan time, RescaleRule rule) => rule.NewStart + (time - rule.OrigStart) * rule.Ratio;
        TimeSpan RescaleOpt(TimeSpan time, RescaleRule? rule) => rule is { } r ? Rescale(time, r) : time;

        outSubtiles.Add(subtitle with
        {
            StartTime = RescaleOpt(subtitle.StartTime, startRescale),
            EndTime = RescaleOpt(subtitle.EndTime, endRescale)
        });
    }
}

// Create output filename if not provided

var output = options.Output;
if (output is null)
{
    var rawName = Path.GetFileNameWithoutExtension(options.Input);
    var ext = Path.GetExtension(options.Input);
    output = $"{rawName}_out{ext}";
}

// Write

using (var fileStream = File.Open(output, FileMode.Create, FileAccess.Write))
{
    var writer = new SrtWriter();
    writer.WriteStream(fileStream, outSubtiles);
}

public class Options
{
    [Option('i', "input", Required = true, HelpText = "Subtitle file to load")]
    public required string Input { get; set; }

    [Option('o', "output", Required = true, HelpText = "Subtitle file to store")]
    public string? Output { get; set; }

    [Option("offset")]
    public TimeSpan? Offset { get; set; }

    [Option("rescale")]
    public IEnumerable<string>? Rescales { get; set; }
}

record struct RescaleRule(TimeSpan OrigStart, TimeSpan OrigEnd, TimeSpan NewStart, TimeSpan NewEnd)
{
    public readonly TimeSpan OrigDuration => OrigEnd - OrigStart;
    public readonly TimeSpan NewDuration => NewEnd - NewStart;

    public readonly double Ratio => NewDuration.Ticks / (double)OrigDuration.Ticks;
}