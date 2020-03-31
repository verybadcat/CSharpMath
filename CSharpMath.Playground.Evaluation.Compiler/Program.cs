using System;
using System.IO;
using System.Net.Http;
using Microsoft.ClearScript.V8;
using obj = Microsoft.ClearScript.ScriptObject;
using System.Threading.Tasks;

namespace CSharpMath.Playground.Evaluation.Compiler {
  class Program {
    static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string? path = null) =>
        Path.GetDirectoryName(path ?? throw new ArgumentNullException(nameof(path)))
        ?? throw new ArgumentException(nameof(path), "Top level directory is invalid for this file!");
    static async Task Main() {
      if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        throw new PlatformNotSupportedException("As ClearScriptV8 is written in C++/CLI, this can only be run on Windows");
      Console.WriteLine("1: Entered Main");

      static string ReadNeradmerFile(string file) =>
        File.ReadAllText(Path.Combine(ThisDirectory(), "..", "nerdamer", file));
      using var http = new HttpClient();
      using var clearScript = new V8ScriptEngine();
      clearScript.Execute("var module = {};");
      
      var nerdamerDocs =
        (obj)clearScript.Evaluate(await http.GetStringAsync("https://raw.githubusercontent.com/Happypig375/nerdamer/patch-22/docgen/function_docs.js"));
      foreach (var functionName in nerdamerDocs.PropertyNames) {
        var function = (obj)nerdamerDocs.GetProperty(functionName);
        string returns = (string)function.GetProperty(nameof(returns));

        obj parameters = (obj)function.GetProperty(nameof(parameters));
        foreach (var parameterName in parameters.PropertyNames) {
          var parameter = (obj)function.GetProperty(parameterName);
          string type = (string)parameter.GetProperty(nameof(type));
        }

        System.Diagnostics.Debugger.Break();
      }
      return;
      clearScript.Execute(await http.GetStringAsync("https://unpkg.com/@babel/standalone@7.9.4/babel.min.js"));
      Console.WriteLine("2: Loaded Babel");

      clearScript.AddHostObject("nerdamer", new {
        Code = string.Concat(
          ReadNeradmerFile("nerdamer.core.js"),
          ReadNeradmerFile("Algebra.js"),
          ReadNeradmerFile("Calculus.js"),
          ReadNeradmerFile("Solve.js"),
          ReadNeradmerFile("Extra.js")
        )
      });
      var nerdamer = (string)clearScript.Evaluate(@"Babel.transform(nerdamer.Code, { presets: ['env'], comments:false }).code");
      Console.WriteLine("3: Transformed Nerdamer");

      var outputDir = Path.Combine(ThisDirectory(), "..", "CSharpMath.Evaluation.Nerdamer");
      if (!Directory.Exists(outputDir))
        throw new DirectoryNotFoundException(outputDir + " not found!");
      using var output = File.CreateText(Path.Combine(outputDir, "nerdamer.js"));
      output.Write(nerdamer);
      Console.WriteLine("4: Saved the transformed output");

    }
  }
}