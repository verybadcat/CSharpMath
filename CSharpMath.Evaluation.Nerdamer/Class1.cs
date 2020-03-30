using System;
using System.Collections.Generic;
using System.IO;
using Jurassic;

namespace CSharpMath.Evaluation {
  /// <summary>Usage: nerdamer[expression, subs, option, location]</summary>
  public class Nerdamer {
    class StreamScriptSource : ScriptSource, IDisposable {
      internal StreamScriptSource(Stream stream) => this.stream = stream;
      readonly Stream stream;
      public override string? Path => null;
      public void Dispose() => stream.Dispose();
      public override TextReader GetReader() => new StreamReader(stream);
    }
    [Flags] public enum Options {
      Default = 0,
      /// <summary>Expand expressions, i.e. (x+1)^2 -> x^2+2x+1</summary>
      Expand = 1,
      /// <summary>Evaluate expressions, i.e. sqrt(2) -> 1.41421356...</summary>
      Numer = 2
    }
    /// <summary>You need to create new <see cref="Nerdamer"/> instances if you want to use on multiple threads</summary>
    public static Nerdamer SharedInstance { get; } = new Nerdamer();
    public Nerdamer() =>
      _engine.Execute(new StreamScriptSource(typeof(Nerdamer).Assembly.GetManifestResourceStream("nerdamer.js")));
    readonly ScriptEngine _engine = new ScriptEngine();

    public this[string expression, IReadOnlyDictionary<string, object>? subs = null, Options options = Options.Default, int index = ] { }
  }
}
