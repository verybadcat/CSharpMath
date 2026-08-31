using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CSharpMath.SkiaSharp;
using Typography.OpenFont;

namespace CSharpMath.Rendering.Benchmarks {
  using Data = Tests.TestRenderingMathData;
  public class Program {
    [Benchmark] public void AllConstantValues() { using (new MathPainter { LaTeX = Data.AllConstantValues }.DrawAsStream()) { } }
    [Benchmark]
    [Arguments(nameof(Data.Cases))]
    [Arguments(nameof(Data.Color))]
    [Arguments(nameof(Data.Commands))]
    [Arguments(nameof(Data.Cyrillic))]
    [Arguments(nameof(Data.ErrorMissingArgument))]
    [Arguments(nameof(Data.Matrix))]
    [Arguments(nameof(Data.QuadraticFormula))]
    [Arguments(nameof(Data.QuarticSolutions))]
    [Arguments(nameof(Data.TangentPeriodShift))]
    [Arguments(nameof(Data.VectorProjection))]
    public void IndividualTests(string key) { using (new MathPainter { LaTeX = Data.AllConstants[key] }.DrawAsStream()) { } }
    static void Main(string[] args) {
      if (args.Contains("--font-profile-worker")) { FontProfile.Worker(args.SkipWhile(a => a != "--font-profile-worker").Skip(1).FirstOrDefault() ?? "global", args); return; }
      if (args.Contains("--font-profile") || args.Contains("--font-profile-smoke")) { FontProfile.Run(args); return; }
#if DEBUG
      System.Console.WriteLine("Starting in Debug configuration...");
      static string ThisFile([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
      var p = new Process {
        StartInfo = {
          FileName = "dotnet",
          Arguments = $"run -p \"{ThisFile()}/../CSharpMath.Rendering.Benchmarks.csproj\" -c Release -v n --exporters json --filter * --artifacts \"{ThisFile()}/../../.benchmarkresults\""
        }
      };
      p.Start();
      p.WaitForExit();
      System.Console.WriteLine("Ending in Debug configuration...");
#else
      System.Console.WriteLine("Starting in Release configuration...");
      BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
      System.Console.WriteLine("Ending in Release configuration...");
#endif
    }
  }
  internal static class FontProfile {
#if DEBUG
    const string BuildConfiguration = "Debug";
#elif RELEASE
    const string BuildConfiguration = "Release";
#else
#error Unsupported build configuration: define DEBUG or RELEASE.
#endif
    const string Math = @"\frac{\alpha+\beta}{\sqrt{x^2+y^2}}";
    const string Mixed = @"Area: \frac{\pi r^2}{2}";
    static readonly string[] Bundled = { "latinmodern-math.otf", "AMS-Capital-Blackboard-Bold.otf", "cyrillic-modern-nmr10.otf" };
    static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static void Run(string[] args) {
      bool smoke = args.Contains("--font-profile-smoke"); string output = GetOption(args, "--font-profile-output") ?? Environment.GetEnvironmentVariable("CSHARP_MATH_FONT_PROFILE_OUTPUT") ?? Path.Combine(".fontprofile", "font-profile.json");
      int samples = smoke ? 1 : GetInt(args, "--font-profile-samples", 3), iterations = smoke ? 1 : GetInt(args, "--font-profile-iterations", 10), batches = smoke ? 1 : GetInt(args, "--font-profile-batches", 3);
      var report = new FontReport { Schema = "csharpmath.font-startup/v2", Runtime = Environment.Version.ToString(), OS = RuntimeInformation.OSDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), Configuration = BuildConfiguration, GitCommit = ReadGitCommit(), ColdProcess = true, Samples = samples, Iterations = iterations, WarmBatches = batches, Corpus = new[] { Math, Mixed } };
      foreach (var name in Bundled) report.Fonts.Add(Describe(name, ReadBundled(name), "embedded Reference Fonts")); string? custom = FindCustomFont();
      if (custom == null) report.Errors.Add("custom font asset is missing from benchmark output"); else report.Fonts.Add(Describe("ComicNeue_Bold.otf", File.ReadAllBytes(custom), "runtime LocalTypeface"));
      report.Packaging = new PackageEvidence { BundledEmbeddedBytes = report.Fonts.Where(f => f.Source.StartsWith("embedded")).Sum(f => f.Bytes), CustomAssetBytes = custom == null ? 0 : new FileInfo(custom).Length, GeneratedFontDataBytes = 0, GeneratedFontDataCount = 0, RenderingAssemblyBytes = new FileInfo(typeof(CSharpMath.Rendering.BackEnd.Fonts).Assembly.Location).Length };
      foreach (var scenario in Bundled.Select(n => "parse:" + n).Concat(Bundled.Select(n => "font:" + n)).Concat(new[] { "first-math", "first-mixed", "global-batch", "custom-comic-neue" })) for (int i = 0; i < samples; i++) report.Raw.Add(RunChild(scenario, iterations, batches));
      if (report.Fonts.Count != Bundled.Length + 1) report.Errors.Add("not all bundled and custom faces were reachable"); if (report.Raw.Any(s => s.Errors.Count != 0)) report.Errors.Add("one or more worker samples failed");
      if (report.Raw.Where(s => s.Scenario.StartsWith("font:", StringComparison.Ordinal)).Any(s => !s.GlyphsAvailable) || report.Raw.Where(s => s.Scenario == "custom-comic-neue").Any(s => !s.GlyphsAvailable)) report.Errors.Add("one or more LocalTypeface corpora were not confirmed by cmap");
      var switching = report.Raw.FirstOrDefault(s => s.Scenario == "global-batch"); if (switching != null && (!switching.SwitchingSequence.All(n => switching.SelectedFaces.Contains(n)) || switching.GlobalFaces.Count != 4 || switching.GlobalFaces.Any(f => !f.CmapValidated || !f.LocalTypeface || !f.DrawSucceeded))) report.Errors.Add("global switching did not prove four cmap-validated LocalTypeface renders");
      Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!); File.WriteAllText(output, JsonSerializer.Serialize(report, Json)); Console.WriteLine($"font profile: {output} ({report.Raw.Count} raw samples)"); if (report.Errors.Count != 0) Environment.ExitCode = 2;
    }
    public static void Worker(string scenario, string[] args) {
      try {
        int iterations = GetInt(args, "--font-profile-iterations", 10), batches = GetInt(args, "--font-profile-batches", 3); var result = new Sample { Scenario = scenario }; long before = GC.GetTotalAllocatedBytes(true), retainedBefore = GC.GetTotalMemory(true); int[] gcBefore = { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) }; var timer = Stopwatch.StartNew();
        if (scenario.StartsWith("parse:", StringComparison.Ordinal)) result.Font = Describe(scenario[6..], ReadBundled(scenario[6..]), "embedded Reference Fonts"); else if (scenario.StartsWith("font:", StringComparison.Ordinal)) { byte[] face = ReadBundled(scenario[5..]); result.Font = Describe(scenario[5..], face, "embedded Reference Fonts"); result.ExercisedPaths.Add("LocalTypeface:" + scenario[5..]); result.GlyphCorpus = GlyphCorpusFor(face, scenario[5..]); result.GlyphsAvailable = HasCorpusGlyphs(face, result.GlyphCorpus); for (int i = 0; i < iterations; i++) RenderMath(result.GlyphCorpus, face); } else if (scenario == "first-math") { result.ExercisedPaths.Add("global math parse/layout/draw"); RenderMath(Math); } else if (scenario == "first-mixed") { result.ExercisedPaths.Add("TextPainter text+math layout/draw"); RenderText(Mixed); } else if (scenario == "global-batch" || scenario == "custom-comic-neue") { byte[] custom = ReadCustom(); var faces = Bundled.Select(ReadBundled).Concat(new[] { custom }).ToArray(); result.SwitchingSequence = Bundled.Concat(new[] { "ComicNeue_Bold.otf" }).ToArray(); var corpora = faces.Select((face, i) => { string corpus = GlyphCorpusFor(face, result.SwitchingSequence[i]); return new FaceRenderEvidence { Face = result.SwitchingSequence[i], RequestedCorpus = corpus, CmapGlyphIndices = GlyphIndicesFor(face, corpus), CmapValidated = HasCorpusGlyphs(face, corpus), LocalTypeface = true }; }).ToArray(); if (scenario == "custom-comic-neue") { result.GlyphCorpus = corpora[^1].RequestedCorpus; result.GlyphsAvailable = corpora[^1].CmapValidated; } int iterationsToRun = System.Math.Max(iterations, faces.Length); for (int batch = 0; batch < batches; batch++) { long batchBefore = GC.GetTotalMemory(true); for (int i = 0; i < iterationsToRun; i++) { int selected = i % faces.Length; var evidence = corpora[selected]; RenderMath(evidence.RequestedCorpus, faces[selected]); RenderText(Mixed); evidence.DrawSucceeded = true; result.SelectedFaces.Add(evidence.Face); if (!result.GlobalFaces.Any(f => f.Face == evidence.Face)) result.GlobalFaces.Add(evidence); } GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); result.Batches.Add(new BatchResult { Number = batch, RetainedManagedBytesAfterGc = GC.GetTotalMemory(true) - batchBefore }); } result.ExercisedPaths.Add("repeated local-face switching with cmap-validated corpora"); } else throw new ArgumentException("unknown profile worker scenario: " + scenario);
        timer.Stop(); result.WallMilliseconds = timer.Elapsed.TotalMilliseconds; result.TotalManagedAllocatedBytes = GC.GetTotalAllocatedBytes(true) - before; result.RetainedManagedBytesAfterGc = GC.GetTotalMemory(true) - retainedBefore; result.GcCollections = new[] { GC.CollectionCount(0) - gcBefore[0], GC.CollectionCount(1) - gcBefore[1], GC.CollectionCount(2) - gcBefore[2] }; Console.Write(JsonSerializer.Serialize(result, Json));
      } catch (Exception ex) { Console.Write(JsonSerializer.Serialize(new Sample { Scenario = scenario, Errors = new List<string> { ex.ToString() } }, Json)); Environment.ExitCode = 1; }
    }
    static void RenderMath(string latex, byte[]? custom = null) { MemoryStream? stream = null; Typeface[] faces = Array.Empty<Typeface>(); if (custom != null) { var face = ReadTypeface(custom, out stream); faces = new[] { face }; } using (stream) { var painter = new MathPainter { LaTeX = latex, LocalTypefaces = faces }; using (painter.DrawAsStream()) { } } }
    static void RenderText(string latex) { var painter = new TextPainter { LaTeX = latex }; using (painter.DrawAsStream()) { } }
    static string CorpusFor(string name) => name.StartsWith("cyrillic", StringComparison.Ordinal) ? "\u0410 \u0411 \u0412 \u0413 \u0414 \u0416 \u042F" : name.StartsWith("AMS-", StringComparison.Ordinal) ? FindAvailableGlyphs(ReadBundled(name), 7) : name.StartsWith("Comic", StringComparison.Ordinal) ? "Comic Neue sample" : Math;
    static string GlyphCorpusFor(byte[] bytes, string name) => name.StartsWith("AMS-", StringComparison.Ordinal) ? "ℂℍℕℙℚℝℤ" : name.StartsWith("cyrillic", StringComparison.Ordinal) ? "АБВГДЖЯ" : name.StartsWith("Comic", StringComparison.Ordinal) ? "Comic Neue sample" : "xy";
    static bool HasCorpusGlyphs(byte[] bytes, string corpus) { using var stream = new MemoryStream(bytes, false); var face = new OpenFontReader().Read(stream) ?? throw new InvalidDataException("font parse failed"); var codepoints = corpus.EnumerateRunes().Where(r => !char.IsWhiteSpace((char)r.Value)).Select(r => r.Value).Distinct(); return codepoints.All(cp => face.GetGlyphIndex(cp) != 0); }
    static ushort[] GlyphIndicesFor(byte[] bytes, string corpus) { using var stream = new MemoryStream(bytes, false); var face = new OpenFontReader().Read(stream) ?? throw new InvalidDataException("font parse failed"); return corpus.EnumerateRunes().Where(r => !char.IsWhiteSpace((char)r.Value)).Select(r => face.GetGlyphIndex(r.Value)).Distinct().ToArray(); }
    static string FindAvailableGlyphs(byte[] bytes, int count) { using var stream = new MemoryStream(bytes, false); var face = new OpenFontReader().Read(stream) ?? throw new InvalidDataException("font parse failed"); var chars = Enumerable.Range(0, 0x10000).Where(cp => !char.IsControl((char)cp) && !char.IsWhiteSpace((char)cp) && face.GetGlyphIndex(cp) != 0).Take(count).Select(cp => (char)cp).ToArray(); return chars.Length == count ? new string(chars) : throw new InvalidDataException("font lacks expected mapped glyphs"); }
    static Typeface ReadTypeface(byte[] bytes, out MemoryStream stream) { stream = new MemoryStream(bytes, writable: false); return new OpenFontReader().Read(stream) ?? throw new InvalidDataException("custom font could not be parsed"); }
    static byte[] ReadCustom() { var p = FindCustomFont() ?? throw new FileNotFoundException("ComicNeue_Bold.otf"); return File.ReadAllBytes(p); }
    static FontInfo Describe(string name, byte[] bytes, string source) { using var stream = new MemoryStream(bytes, false); _ = new OpenFontReader().Read(stream) ?? throw new InvalidDataException(name); var tables = ParseTableDirectory(bytes); return new FontInfo { Name = name, Source = source, Bytes = bytes.Length, Sha256 = Convert.ToHexString(SHA256.HashData(bytes)), Tables = tables, Outline = tables.Any(t => t == "CFF " || t == "CFF2") ? "CFF" : tables.Contains("glyf") ? "glyf" : "unknown", Notes = "OpenFontReader parsed the font; the reachable SFNT table directory is recorded for availability only. Actual rendering paths are reported per sample." }; }
    static string[] ParseTableDirectory(byte[] bytes) { if (bytes.Length < 12) throw new InvalidDataException("truncated SFNT"); int count = (bytes[4] << 8) | bytes[5]; if (12 + count * 16 > bytes.Length) throw new InvalidDataException("truncated SFNT directory"); var names = new string[count]; for (int i = 0; i < count; i++) names[i] = System.Text.Encoding.ASCII.GetString(bytes, 12 + i * 16, 4); return names.OrderBy(x => x, StringComparer.Ordinal).ToArray(); }
    static byte[] ReadBundled(string name) => typeof(CSharpMath.Rendering.BackEnd.Fonts).Assembly.GetManifestResourceStream("CSharpMath.Rendering.Reference_Fonts." + name) is Stream s ? ReadAll(s) : throw new FileNotFoundException(name);
    static byte[] ReadAll(Stream s) { using (s) { using var m = new MemoryStream(); s.CopyTo(m); return m.ToArray(); } }
    static string? FindCustomFont() { var p = Path.Combine(AppContext.BaseDirectory, "ComicNeue_Bold.otf"); return File.Exists(p) ? p : null; }
    static Sample RunChild(string scenario, int iterations, int batches) { var psi = new ProcessStartInfo(Environment.ProcessPath ?? "dotnet") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true }; if (Path.GetFileNameWithoutExtension(psi.FileName).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) psi.ArgumentList.Add(typeof(Program).Assembly.Location); psi.ArgumentList.Add("--font-profile-worker"); psi.ArgumentList.Add(scenario); psi.ArgumentList.Add("--font-profile-iterations"); psi.ArgumentList.Add(iterations.ToString()); psi.ArgumentList.Add("--font-profile-batches"); psi.ArgumentList.Add(batches.ToString()); using var p = Process.Start(psi)!; var stdoutTask = p.StandardOutput.ReadToEndAsync(); var stderrTask = p.StandardError.ReadToEndAsync(); Task.WaitAll(stdoutTask, stderrTask); p.WaitForExit(); string stdout = stdoutTask.Result, stderr = stderrTask.Result; if (p.ExitCode != 0) return new Sample { Scenario = scenario, Errors = new List<string> { $"worker exit {p.ExitCode}: {stderr} {stdout}" } }; try { return JsonSerializer.Deserialize<Sample>(stdout.Trim(), Json) ?? throw new InvalidDataException("empty worker output"); } catch (Exception e) { return new Sample { Scenario = scenario, Errors = new List<string> { $"invalid worker JSON: {e.Message}; {stdout}" } }; } }
    static int GetInt(string[] a, string key, int fallback) => int.TryParse(GetOption(a, key), out var n) && n > 0 ? n : fallback; static string? GetOption(string[] a, string key) { int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
    static string ReadGitCommit() { try { using var p = Process.Start(new ProcessStartInfo("git", "rev-parse HEAD") { RedirectStandardOutput = true, UseShellExecute = false }); return p!.StandardOutput.ReadToEnd().Trim(); } catch { return "unavailable"; } }
    sealed class FontReport { public string Schema { get; set; } = ""; public string Runtime { get; set; } = ""; public string OS { get; set; } = ""; public string Architecture { get; set; } = ""; public string Configuration { get; set; } = ""; public string GitCommit { get; set; } = ""; public bool ColdProcess { get; set; } public int Samples { get; set; } public int Iterations { get; set; } public int WarmBatches { get; set; } public string[] Corpus { get; set; } = Array.Empty<string>(); public List<FontInfo> Fonts { get; set; } = new(); public PackageEvidence Packaging { get; set; } = new(); public List<Sample> Raw { get; set; } = new(); public List<string> Errors { get; set; } = new(); }
    sealed class PackageEvidence { public long BundledEmbeddedBytes { get; set; } public long CustomAssetBytes { get; set; } public long RenderingAssemblyBytes { get; set; } public long GeneratedFontDataBytes { get; set; } public int GeneratedFontDataCount { get; set; } }
    sealed class FontInfo { public string Name { get; set; } = ""; public string Source { get; set; } = ""; public int Bytes { get; set; } public string Sha256 { get; set; } = ""; public string[] Tables { get; set; } = Array.Empty<string>(); public string Outline { get; set; } = ""; public string Notes { get; set; } = ""; }
    sealed class Sample { public string Scenario { get; set; } = ""; public double WallMilliseconds { get; set; } public long TotalManagedAllocatedBytes { get; set; } public long RetainedManagedBytesAfterGc { get; set; } public int[] GcCollections { get; set; } = Array.Empty<int>(); public FontInfo? Font { get; set; } public bool GlyphsAvailable { get; set; } public string GlyphCorpus { get; set; } = ""; public string[] SwitchingSequence { get; set; } = Array.Empty<string>(); public List<string> SelectedFaces { get; set; } = new(); public List<FaceRenderEvidence> GlobalFaces { get; set; } = new(); public List<string> ExercisedPaths { get; set; } = new(); public List<BatchResult> Batches { get; set; } = new(); public List<string> Errors { get; set; } = new(); }
    sealed class FaceRenderEvidence { public string Face { get; set; } = ""; public string RequestedCorpus { get; set; } = ""; public ushort[] CmapGlyphIndices { get; set; } = Array.Empty<ushort>(); public bool CmapValidated { get; set; } public bool LocalTypeface { get; set; } public bool DrawSucceeded { get; set; } }
    sealed class BatchResult { public int Number { get; set; } public long RetainedManagedBytesAfterGc { get; set; } }
  }
}
