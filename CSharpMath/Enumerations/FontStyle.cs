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
    public static string FontName(this FontStyle style) => FontStyles[style][0];
    public static MultiDictionary<string, FontStyle> FontStyles { get; } =
      new MultiDictionary<string, FontStyle> {
        {"mathnormal", FontStyle.Default },
        {"mathrm", FontStyle.Roman },
        {"rm", FontStyle.Roman },
        {"mathbf", FontStyle.Bold },
        {"bf", FontStyle.Bold },
        {"mathcal", FontStyle.Caligraphic },
        {"cal", FontStyle.Caligraphic },
        {"mathtt", FontStyle.Typewriter },
        {"mathit", FontStyle.Italic },
        {"mit", FontStyle.Italic },
        {"mathsf", FontStyle.SansSerif },
        {"mathfrak", FontStyle.Fraktur },
        {"frak", FontStyle.Fraktur },
        {"mathbb", FontStyle.Blackboard },
        {"mathbfit", FontStyle.BoldItalic },
        {"bm", FontStyle.BoldItalic },
        {"text", FontStyle.Roman }
      };
  }
}
