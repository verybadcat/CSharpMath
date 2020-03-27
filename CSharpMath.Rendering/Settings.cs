namespace CSharpMath {
  using System.Collections.Generic;
  public static class Settings {
    public static bool DisableEnhancedTextPainterColors {
      get => Rendering.Text.TextLaTeXBuilder.NoEnhancedColors;
      set => Rendering.Text.TextLaTeXBuilder.NoEnhancedColors = value;
    }
    public static Rendering.BackEnd.Typefaces GlobalTypefaces =>
      Rendering.BackEnd.Fonts.GlobalTypefaces;
    public static Structures.BiDictionary<string, Structures.Color> PredefinedColors =>
      Structures.Color.PredefinedColors;
    public static Structures.AliasDictionary<string, Atom.Boundary> PredefinedLaTeXBoundaryDelimiters =>
      Atom.LaTeXDefaults.BoundaryDelimiters;
    public static Structures.AliasDictionary<string, Atom.FontStyle> PredefinedLaTeXFontStyles =>
      Atom.LaTeXDefaults.FontStyles;
    public static Structures.AliasDictionary<string, Atom.MathAtom> PredefinedLaTeXCommands =>
      Atom.LaTeXDefaults.Commands;
    public static Structures.BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextLaTeXDefaults.PredefinedAccents;
    public static Structures.AliasDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextLaTeXDefaults.PredefinedTextSymbols;
    public static Dictionary<string, Structures.Space> PredefinedLengthUnits =>
      Structures.Space.PredefinedLengthUnits;
  }
}
