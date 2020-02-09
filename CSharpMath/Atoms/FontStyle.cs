namespace CSharpMath.Atoms {
  public enum FontStyle {
    ///<summary>\mathnormal</summary>
    Default,
    ///<summary>\mathrm</summary>
    Roman,
    ///<summary>\mathbf</summary>
    Bold,
    ///<summary>\mathcal</summary>
    Caligraphic,
    ///<summary>Monospace, i.e. \mathtt</summary>
    Typewriter,
    ///<summary>\mathit</summary>
    Italic,
    ///<summary>\mathss</summary>
    SansSerif,
    ///<summary>\mathfrak</summary>
    Fraktur,
    ///<summary>\mathbb</summary>
    Blackboard,
    ///<summary>\mathbfit</summary>
    BoldItalic
  }
  public static class FontStyleExtensions {
    public static string FontName(this FontStyle style) => FontStyles[style];
    public static Structures.AliasDictionary<string, FontStyle> FontStyles { get; } =
      new Structures.AliasDictionary<string, FontStyle> {
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