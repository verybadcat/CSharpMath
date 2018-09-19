using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Enumerations {
  public enum FontStyle {
    Default = 0,
    ///<summary>\mathrm</summary>
    Roman,
    Bold,
    Caligraphic,
    ///<summary>Monospace, i.e.</summary>
    Typewriter,
    ///<summary>\mathit</summary>
    Italic,
    ///<summary>\mathss</summary>
    SansSerif,
    ///<summary>\mathfrak</summary>
    Fraktur,
    ///<summary>\mathbb</summary>
    Blackboard,
    BoldItalic
  }

  public static class FontStyleExtensions {
    public static string FontName(this FontStyle style) => FontStyles[style];
    public static AliasDictionary<string, FontStyle> FontStyles { get; } =
      new AliasDictionary<string, FontStyle> {
        { "mathnormal", FontStyle.Default },
        { "mathrm", "rm", "text", FontStyle.Roman },
        { "mathbf", "bf", FontStyle.Bold },
        { "mathcal", "cal", FontStyle.Caligraphic },
        { "mathtt", FontStyle.Typewriter },
        { "mathit", "mit", FontStyle.Italic },
        { "mathsf", FontStyle.SansSerif },
        { "mathfrak", "frak", FontStyle.Fraktur },
        { "mathbb", FontStyle.Blackboard },
        { "mathbfit", "bm", FontStyle.BoldItalic },
      };
  }
}
