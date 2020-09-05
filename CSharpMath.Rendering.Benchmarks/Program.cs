using BenchmarkDotNet.Attributes;
namespace CSharpMath.Rendering.Benchmarks {
  using SkiaSharp;
  using Data = Tests.TestRenderingMathData;
  public class Program {
    // In Windows, selecting text inside the Visual Sutdio Debug console pauses program execution.
    // A simple press of the spacebar continues program execution, so don't be fooled into thinking it hanged!

    // Place a long benchmark before microbenchmarks to work around https://github.com/dotnet/BenchmarkDotNet/issues/1338
    [Benchmark]
    public void AllConstantValues() { using (new MathPainter { LaTeX = Data.AllConstantValues }.DrawAsStream()) { } }
    [Benchmark]
    [Arguments(nameof(Data.Cases))]
    [Arguments(nameof(Data.Color))]
    [Arguments(nameof(Data.Commands))]
    [Arguments(nameof(Data.Cyrillic))]
    [Arguments(nameof(Data.ErrorMissingArgument))]
    [Arguments(nameof(Data.Exponential))]
    [Arguments(nameof(Data.Matrix))]
    [Arguments(nameof(Data.Nothing))]
    [Arguments(nameof(Data.QuadraticFormula))]
    [Arguments(nameof(Data.QuarticSolutions))]
    [Arguments(nameof(Data.TangentPeriodShift))]
    [Arguments(nameof(Data.VectorProjection))]
    public void IndividualTests(string key) { using (new MathPainter { LaTeX = Data.AllConstants[key] }.DrawAsStream()) { } }
    static void Main(string[] args) {
#if DEBUG
      System.Console.WriteLine("Starting in Debug configuration...");
      static string ThisFile([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
      var p = new System.Diagnostics.Process {
        StartInfo = {
          FileName = "dotnet", // The -- separator between arguments to dotnet and arguments to this Program is optional :)
          Arguments = $"run -p \"{ThisFile()}/../CSharpMath.Rendering.Benchmarks.csproj\" -c Release -v n --exporters json --filter * --artifacts \"{ThisFile()}/../../.benchmarkresults\""
        }
      };
      p.Start();
      p.WaitForExit();
      System.Console.WriteLine("Ending in Debug configuration...");
#else
      System.Console.WriteLine("Starting in Release configuration...");
      BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
      System.Console.WriteLine("Ending in Release configuration...");
#endif
    }
  }
}