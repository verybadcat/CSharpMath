namespace CSharpMath {
  using System.Collections.Generic;
  using Atom;
  public static class Settings {
    public static Rendering.BackEnd.Typefaces GlobalTypefaces =>
      Rendering.BackEnd.Fonts.GlobalTypefaces;
    public static AliasBiDictionary<string, System.Drawing.Color> PredefinedColors =>
      LaTeXSettings.PredefinedColors;
    public static LaTeXCommandDictionary<Boundary> PredefinedLaTeXBoundaryDelimiters =>
      LaTeXSettings.BoundaryDelimiters;
    public static AliasBiDictionary<string, FontStyle> PredefinedLaTeXFontStyles =>
      LaTeXSettings.FontStyles;
    public static LaTeXCommandDictionary<System.Func
      <LaTeXParser, MathList, char, Result<(MathAtom? Atom, MathList? Return)>>
    > PredefinedLaTeXCommands =>
      LaTeXSettings.Commands;
    public static AliasBiDictionary<string, MathAtom> PredefinedLaTeXCommandSymbols =>
      LaTeXSettings.CommandSymbols;
    public static AliasBiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextLaTeXSettings.PredefinedAccents;
    public static AliasBiDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextLaTeXSettings.PredefinedTextSymbols;
    public static Dictionary<string, Length> PredefinedLengthUnits =>
     Length.PredefinedLengthUnits;
  }
}