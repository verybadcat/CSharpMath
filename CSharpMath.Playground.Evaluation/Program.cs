using System;
using System.IO;
using Jurassic;

namespace CSharpMath.Playground.Evaluation {
  class Program {
    static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string? path = null) =>
        Path.GetDirectoryName(path ?? throw new ArgumentNullException(nameof(path)))
        ?? throw new ArgumentException(nameof(path), "Top level directory is invalid for this file!");
    static void Main() {
      Console.WriteLine("Hello World!");
      var engine = new ScriptEngine();
      engine.ExecuteFile(Path.Combine(ThisDirectory(), "nerdamer.js"));
      Console.WriteLine(engine.Evaluate("nerdamer('x^2+x^2+x')"));
    }
  }
}
