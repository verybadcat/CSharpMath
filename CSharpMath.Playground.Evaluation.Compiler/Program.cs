using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.ClearScript.V8;
using obj = Microsoft.ClearScript.ScriptObject;

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
      var outputDir = Path.Combine(ThisDirectory(), "..", "CSharpMath.Evaluation.Nerdamer");
      if (!Directory.Exists(outputDir))
        throw new DirectoryNotFoundException(outputDir + " not found!");
      using var http = new HttpClient();
      using var clearScript = new V8ScriptEngine();
      clearScript.Execute("var module = {};");

      var nerdamerDocs =
        (obj)clearScript.Evaluate(await http.GetStringAsync("https://raw.githubusercontent.com/jiggzson/nerdamer/gh-pages/docgen/function_docs.js"));
      using var nerdamercs =
        new System.CodeDom.Compiler.IndentedTextWriter(File.CreateText(Path.Combine(outputDir, "nerdamer.cs")), "  ");
      nerdamercs.WriteLine("using System;");
      nerdamercs.WriteLine("using System.Collections.Generic;");
      nerdamercs.WriteLine("using System.IO;");
      nerdamercs.WriteLine("using Jurassic;");
      nerdamercs.WriteLine("namespace CSharpMath.Evaluation {");
      nerdamercs.Indent++;
      nerdamercs.WriteLine("public class Nerdamer {");
      nerdamercs.Indent++;
      nerdamercs.WriteLine("class StreamScriptSource : ScriptSource, IDisposable {");
      nerdamercs.Indent++;
      nerdamercs.WriteLine("internal StreamScriptSource(Stream stream) => this.stream = stream;");
      nerdamercs.WriteLine("readonly Stream stream;");
      nerdamercs.WriteLine("public override string? Path => null;");
      nerdamercs.WriteLine("public void Dispose() => stream.Dispose();");
      nerdamercs.WriteLine("public override TextReader GetReader() => new StreamReader(stream);");
      nerdamercs.Indent--;
      nerdamercs.WriteLine("}"); // class StreamScriptSource : ScriptSource, IDisposable {
      nerdamercs.WriteLine("/// <summary>You need to create new <see cref=\"Nerdamer\"/> instances if you want to use on multiple threads</summary>");
      nerdamercs.WriteLine("public static Nerdamer SharedInstance { get; } = new Nerdamer();");
      nerdamercs.WriteLine("public Nerdamer() => _engine.Execute(new StreamScriptSource(typeof(Nerdamer).Assembly.GetManifestResourceStream(\"nerdamer.js\")));");
      nerdamercs.WriteLine("readonly ScriptEngine _engine = new ScriptEngine();");
      nerdamercs.WriteLine();
      foreach (var functionId in nerdamerDocs.PropertyNames) {
        var function = (obj)nerdamerDocs.GetProperty(functionId);
        string usage = (string)function.GetProperty(nameof(usage));
        string returns = (string)function.GetProperty(nameof(returns));
        if (returns == "undefined") returns = "void";
        nerdamercs.Write($"public {returns} {Regex.Replace(usage, @"nerdamer\([^)]+\)\.(.+)", "$1")}(");

        obj parameters = (obj)function.GetProperty(nameof(parameters));
        int parameteri = 0;
        foreach (var parameterName in parameters.PropertyNames) {
          var parameter = (obj)parameters.GetProperty(parameterName);
          string type = (string)parameter.GetProperty(nameof(type));
          if (parameterName == "none" && type == "") break;
          if (parameteri > 0) nerdamercs.Write(", ");
          nerdamercs.Write($"{type} {parameterName}");
          parameteri++;
        }
        nerdamercs.Write($") => _engine.Evaluate($\"nerdamer(");
        parameteri = 0;
        foreach (var parameterName in parameters.PropertyNames) {
          var parameter = (obj)parameters.GetProperty(parameterName);
          string type = (string)parameter.GetProperty(nameof(type));
          if (parameterName == "none" && type == "") break;
          if (parameteri > 0) nerdamercs.Write(", ");
          nerdamercs.Write($"{{{parameterName}}}");
          parameteri++;
        }
        nerdamercs.Write(")\");");
        nerdamercs.WriteLine();
      }
      nerdamercs.Indent--;
      nerdamercs.WriteLine("}"); // public class Nerdamer {
      nerdamercs.Indent--;
      nerdamercs.WriteLine("}"); // namespace CSharpMath.Evaluation {
      nerdamercs.Flush();
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
      var nerdamerjs = (string)clearScript.Evaluate(@"Babel.transform(nerdamer.Code, { presets: ['env'], comments:false }).code");
      Console.WriteLine("3: Transformed Nerdamer");

      using var output = File.CreateText(Path.Combine(outputDir, "nerdamer.js"));
      output.Write(nerdamerjs);
      Console.WriteLine("4: Saved the transformed output");
    }
  }
}