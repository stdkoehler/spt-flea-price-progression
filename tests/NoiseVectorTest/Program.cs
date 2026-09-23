using System.Text.Json;
using FleaPriceLevelScaling;

// Verifies the C# port against golden vectors emitted by analysis/fleanoise.py.
static string FindRepoRoot()
{
    foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var dir = new DirectoryInfo(startPath); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "spt-flea-price-level-scaling.sln")) ||
                File.Exists(Path.Combine(dir.FullName, "src", "data", "noise_test_vectors.json")))
            {
                return dir.FullName;
            }
        }
    }

    return Directory.GetCurrentDirectory();
}

var repoRoot = FindRepoRoot();
var noiseTestVectorsPath = Path.Combine(repoRoot, "src", "data", "noise_test_vectors.json");
var priceCurvesPath = Path.Combine(repoRoot, "src", "data", "price_curves.json");
var priceTableTestVectorsPath = Path.Combine(repoRoot, "src", "data", "pricetable_test_vectors.json");
var configPath = Path.Combine(repoRoot, "src", "FleaPriceLevelScaling", "config.jsonc");

if (!File.Exists(noiseTestVectorsPath))
{
    Console.Error.WriteLine($"missing {Path.GetFullPath(noiseTestVectorsPath)}");
    return 2;
}

using var doc = JsonDocument.Parse(File.ReadAllText(noiseTestVectorsPath));
var root = doc.RootElement;
int pass = 0, fail = 0;
const double TOL = 1e-12;

void Check(string what, double got, double want, double tol = TOL)
{
    if (Math.Abs(got - want) <= tol) { pass++; return; }
    fail++;
    Console.WriteLine($"  FAIL {what}\n       got  {got:R}\n       want {want:R}\n       diff {Math.Abs(got-want):E3}");
}

// ---- splitmix64 -------------------------------------------------------
Console.WriteLine("splitmix64:");
foreach (var p in root.GetProperty("splitmix64").EnumerateObject())
{
    ulong input = ulong.Parse(p.Name);
    ulong want = ulong.Parse(p.Value.GetString()!);
    ulong got = FleaNoise.SplitMix64(input);
    if (got == want) pass++;
    else { fail++; Console.WriteLine($"  FAIL splitmix64({input})\n       got  {got}\n       want {want}"); }
}

// ---- unitSd -----------------------------------------------------------
Check("unitSd", FleaNoise.UnitSd, root.GetProperty("unitSd").GetDouble(), 1e-6);

// ---- worldSeed --------------------------------------------------------
Console.WriteLine("worldSeed:");
foreach (var e in root.GetProperty("worldSeed").EnumerateArray())
{
    ulong want = ulong.Parse(e.GetProperty("seed").GetString()!);
    ulong got = FleaNoise.WorldSeed(e.GetProperty("profileId").GetString()!,
                                    e.GetProperty("registrationDate").GetInt32());
    if (got == want) pass++;
    else { fail++; Console.WriteLine($"  FAIL worldSeed({e.GetProperty("registrationDate").GetInt32()})\n       got  {got}\n       want {want}"); }
}

// ---- seedOf -----------------------------------------------------------
Console.WriteLine("seedOf:");
foreach (var e in root.GetProperty("seedOf").EnumerateArray())
{
    ulong want = ulong.Parse(e.GetProperty("seed").GetString()!);
    ulong got = FleaNoise.SeedOf(e.GetProperty("item").GetString()!,
                                 ulong.Parse(e.GetProperty("world").GetString()!));
    if (got == want) pass++;
    else { fail++; Console.WriteLine($"  FAIL seedOf\n       got  {got}\n       want {want}"); }
}

// ---- z ----------------------------------------------------------------
Console.WriteLine("z:");
int zn = 0;
foreach (var e in root.GetProperty("z").EnumerateArray())
{
    var item = e.GetProperty("item").GetString()!;
    var world = ulong.Parse(e.GetProperty("world").GetString()!);
    var hour = e.GetProperty("hour").GetDouble();
    Check($"z({item[..8]}.., world {world}, h {hour})",
          FleaNoise.Z(FleaNoise.SeedOf(item, world), hour), e.GetProperty("z").GetDouble());
    zn++;
}

Console.WriteLine($"\n{zn} z-vectors checked");

// ---- price table ------------------------------------------------------
Console.WriteLine("");
Console.WriteLine("PriceTable:");
var (tp, tf) = NoiseVectorTest.PriceTableCheck.Run(priceCurvesPath, priceTableTestVectorsPath);
pass += tp; fail += tf;

// ---- data/code contract for the noise parameters ----------------------
Console.WriteLine("");
Console.WriteLine("Noise parameter contract:");
var (mp, mf) = NoiseVectorTest.MetaCheck.Run(priceCurvesPath);
pass += mp; fail += mf;

// ---- end-to-end smoke -------------------------------------------------
Console.WriteLine("");
Console.WriteLine("Smoke test (real config.jsonc + real table):");
var smoke = NoiseVectorTest.SmokeCheck.Run(configPath, priceCurvesPath);
fail += smoke;


Console.WriteLine(fail == 0 ? $"ALL {pass} CHECKS PASSED (tolerance {TOL:E0})"
                            : $"{fail} FAILED, {pass} passed");
return fail == 0 ? 0 : 1;
