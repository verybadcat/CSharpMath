namespace CSharpMath {
  using System.Collections.Generic;
  public static class Settings {
    public static bool DisableEnhancedTextPainterColors {
      get => Rendering.Text.TextBuilder.NoEnhancedColors;
      set => Rendering.Text.TextBuilder.NoEnhancedColors = value;
    }
    public static Rendering.FrontEnd.Typefaces GlobalTypefaces =>
      Rendering.FrontEnd.Fonts.GlobalTypefaces;
    public static Structures.BiDictionary<string, Structures.Color> PredefinedColors =>
      Structures.Color.PredefinedColors;
    public static Structures.AliasDictionary<string, string> PredefinedBoundaryDelimiters =>
      Atoms.MathAtoms.BoundaryDelimiters;
    public static Structures.AliasDictionary<string, Atoms.FontStyle> PredefinedFontStyles =>
      Atoms.FontStyleExtensions.FontStyles;
    public static Structures.AliasDictionary<string, Atoms.MathAtom> PredefinedLaTeXMathSymbols =>
      Atoms.MathAtoms.Commands;
    public static Structures.BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextAtoms.PredefinedAccents;
    public static Structures.AliasDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextAtoms.PredefinedTextSymbols;
    public static Dictionary<string, Structures.Space> PredefinedLengthUnits =>
      Structures.Space.PredefinedLengthUnits;
  }
}
