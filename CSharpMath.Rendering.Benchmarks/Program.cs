using Avalonia;
using Avalonia.Headless;
using BenchmarkDotNet.Attributes;
using CSharpMath.Atom;
using CSharpMath.SkiaSharp;
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
    [Arguments(nameof(Data.Matrix))]
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

  [MemoryDiagnoser]
  public class ViewUpdateChurnBenchmarks {
    const string Sample = @"x_1 = 1.234e50";
    [Params(100, 1000)] public int Iterations;
    readonly MathPainter painter = new();
    readonly MathList content = new MathPainter { LaTeX = Sample }.Content!;

    [Benchmark]
    public void LaTeXAssignment() {
      for (var i = 0; i < Iterations; i++) {
        painter.LaTeX = Sample;
        painter.Measure(1000);
      }
    }

    [Benchmark]
    public void ContentAssignment() {
      for (var i = 0; i < Iterations; i++) {
        painter.Content = content;
        painter.Measure(1000);
      }
    }

    [Benchmark]
    public void DirectPainterReuse() {
      painter.LaTeX = Sample;
      for (var i = 0; i < Iterations; i++) painter.Measure(1000);
    }
  }

  [MemoryDiagnoser]
  public class AvaloniaViewUpdateChurnBenchmarks {
    const string FirstSample = @"x_1 = 1.234e50";
    const string SecondSample = @"x_2 = 5.678e90";
    [Params(100, 1000)] public int Iterations;
    static AvaloniaViewUpdateChurnBenchmarks() {
      _ = CSharpMath.Rendering.BackEnd.Fonts.GlobalTypefaces.ToString();
      AppBuilder.Configure<BenchmarkApplication>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions())
        .SetupWithoutStarting();
    }
    sealed class BenchmarkApplication : Application { }
    readonly CSharpMath.Avalonia.MathView view = new();
    readonly MathList firstContent = new MathPainter { LaTeX = FirstSample }.Content!;
    readonly MathList secondContent = new MathPainter { LaTeX = SecondSample }.Content!;

    [Benchmark]
    public void ViewLaTeXAssignment() {
      for (var i = 0; i < Iterations; i++) {
        view.LaTeX = i % 2 == 0 ? FirstSample : SecondSample;
        view.Measure(new Size(1000, 1000));
      }
    }

    [Benchmark]
    public void ViewContentAssignment() {
      for (var i = 0; i < Iterations; i++) {
        view.Content = i % 2 == 0 ? firstContent : secondContent;
        view.Measure(new Size(1000, 1000));
      }
    }

    [Benchmark]
    public void DirectPainterReuse() {
      view.Painter.LaTeX = FirstSample;
      for (var i = 0; i < Iterations; i++) view.Painter.Measure(1000);
    }
  }
}
