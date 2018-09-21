namespace CSharpMath {
  using System.Collections.Generic;
  public static class Settings {
    public static bool DisableEnhancedTextPainterColors {
      get => Rendering.TextBuilder.NoEnhancedColors;
      set => Rendering.TextBuilder.NoEnhancedColors = value;
    }
    public static bool DisableWarnings {
      get => Warnings.DisableWarnings;
      set => Warnings.DisableWarnings = value;
    }
    public static Rendering.Typefaces GlobalTypefaces => Rendering.Fonts.GlobalTypefaces;

    public static BiDictionary<string, Structures.Color> PredefinedColors =>
      Structures.Color.PredefinedColors;
    public static AliasDictionary<string, string> PredefinedBoundaryDelimiters =>
      Atoms.MathAtoms.BoundaryDelimiters;
    public static AliasDictionary<string, Enumerations.FontStyle> PredefinedFontStyles =>
      Enumerations.FontStyleExtensions.FontStyles;
    public static AliasDictionary<string, Atoms.MathAtom> PredefinedLaTeXMathSymbols =>
      Atoms.MathAtoms.Commands;
    public static BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.TextAtoms.PredefinedAccents;
    public static AliasDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.TextAtoms.PredefinedTextSymbols;
    public static Dictionary<string, Structures.Space> PredefinedLengthUnits =>
      Structures.Space.PredefinedLengthUnits;
  }
}
