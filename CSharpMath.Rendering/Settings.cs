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
    public static Structures.LaTeXCommandDictionary<Atom.Boundary> PredefinedLaTeXBoundaryDelimiters =>
      Atom.LaTeXSettings.BoundaryDelimiters;
    public static Structures.BiDictionary<string, Atom.FontStyle> PredefinedLaTeXFontStyles =>
      Atom.LaTeXSettings.FontStyles;
    public static Structures.LaTeXCommandDictionary<System.Func
      <Atom.LaTeXParser, Atom.MathList, char, Structures.Result<(Atom.MathAtom? Atom, Atom.MathList? Return)>>
    > PredefinedLaTeXCommands =>
      Atom.LaTeXSettings.Commands;
    public static Structures.BiDictionary<string, Atom.MathAtom> PredefinedLaTeXCommandSymbols =>
      Atom.LaTeXSettings.CommandSymbols;
    public static Structures.BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextLaTeXSettings.PredefinedAccents;
    public static Structures.BiDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextLaTeXSettings.PredefinedTextSymbols;
    public static Dictionary<string, Structures.Space> PredefinedLengthUnits =>
      Structures.Space.PredefinedLengthUnits;
  }
}
