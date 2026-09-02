using LightBurn.Format;
using LightBurn.Probes;

// Writes the probe files that settle open format questions. Each probe isolates one question
// and puts the competing answers side by side, so that a probe which renders plausibly under
// either answer proves nothing and is not worth writing.
//
//     dotnet run --project LightBurn.Probes -- <output-directory> [probe-name]

var directory = args.Length > 0 ? args[0] : ".";
var only = args.Length > 1 ? args[1] : null;

Directory.CreateDirectory(directory);

var probes = ProbeCatalogue.All;
if (only is not null)
{
    probes = probes.Where(probe => probe.Name == only).ToArray();
    if (probes.Count == 0)
    {
        Console.Error.WriteLine($"No probe named '{only}'. Known: {string.Join(", ", ProbeCatalogue.All.Select(p => p.Name))}");
        return 1;
    }
}

foreach (var probe in probes)
{
    var path = Path.Combine(directory, $"{probe.Name}.lbrn");
    new LightBurnWriter().Save(probe.Build(), path);
    Console.WriteLine($"{path}");
    Console.WriteLine($"    asks: {probe.Question}");
    Console.WriteLine($"    look: {probe.HowToTell}");
}

return 0;
