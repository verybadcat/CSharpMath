namespace CSharpMath {
  using System.Collections.Generic;
  public static class Settings {
    public static bool DisableEnhancedTextPainterColors {
      get => Rendering.Text.TextLaTeXParser.NoEnhancedColors;
      set => Rendering.Text.TextLaTeXParser.NoEnhancedColors = value;
    }
    public static Rendering.BackEnd.Typefaces GlobalTypefaces =>
      Rendering.BackEnd.Fonts.GlobalTypefaces;
    public static Structures.BiDictionary<string, Structures.Color> PredefinedColors =>
      Structures.Color.PredefinedColors;
    public static Structures.AliasDictionary<string, Atom.Boundary> PredefinedLaTeXBoundaryDelimiters =>
      Atom.LaTeXSettings.BoundaryDelimiters;
    public static Structures.AliasDictionary<string, Atom.FontStyle> PredefinedLaTeXFontStyles =>
      Atom.LaTeXSettings.FontStyles;
    public static Structures.AliasDictionary<string, Atom.MathAtom> PredefinedLaTeXCommands =>
      Atom.LaTeXSettings.CommandSymbols;
    public static Structures.BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextLaTeXSettings.PredefinedAccents;
    public static Structures.AliasDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextLaTeXSettings.PredefinedTextSymbols;
    public static Dictionary<string, Structures.Space> PredefinedLengthUnits =>
      Structures.Space.PredefinedLengthUnits;
  }
}
