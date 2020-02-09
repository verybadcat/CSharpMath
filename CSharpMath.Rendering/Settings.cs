namespace CSharpMath {
  using System.Collections.Generic;
  public static class Settings {
    public static bool DisableEnhancedTextPainterColors {
      get => Rendering.TextBuilder.NoEnhancedColors;
      set => Rendering.TextBuilder.NoEnhancedColors = value;
    }
    public static Rendering.Typefaces GlobalTypefaces => Rendering.Fonts.GlobalTypefaces;
    public static Structures.BiDictionary<string, Structures.Color> PredefinedColors =>
      Structures.Color.PredefinedColors;
    public static Structures.AliasDictionary<string, string> PredefinedBoundaryDelimiters =>
      Atoms.MathAtoms.BoundaryDelimiters;
    public static Structures.AliasDictionary<string, Atoms.FontStyle> PredefinedFontStyles =>
      Atoms.FontStyleExtensions.FontStyles;
    public static Structures.AliasDictionary<string, Atoms.MathAtom> PredefinedLaTeXMathSymbols =>
      Atoms.MathAtoms.Commands;
    public static Structures.BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.TextAtoms.PredefinedAccents;
    public static Structures.AliasDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.TextAtoms.PredefinedTextSymbols;
    public static Dictionary<string, Structures.Space> PredefinedLengthUnits =>
      Structures.Space.PredefinedLengthUnits;
  }
}
