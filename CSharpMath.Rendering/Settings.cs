namespace CSharpMath {
  using System.Collections.Generic;
  using CSharpMath.Structures;
  public static class Settings {
    public static Rendering.BackEnd.Typefaces GlobalTypefaces =>
      Rendering.BackEnd.Fonts.GlobalTypefaces;
    public static BiDictionary<string, System.Drawing.Color> PredefinedColors =>
      Atom.LaTeXSettings.PredefinedColors;
    public static AliasDictionary<string, Atom.Boundary> PredefinedLaTeXBoundaryDelimiters =>
      Atom.LaTeXSettings.BoundaryDelimiters;
    public static AliasDictionary<string, Atom.FontStyle> PredefinedLaTeXFontStyles =>
      Atom.LaTeXSettings.FontStyles;
    public static AliasDictionary<string, Atom.MathAtom> PredefinedLaTeXCommands =>
      Atom.LaTeXSettings.Commands;
    public static BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextLaTeXSettings.PredefinedAccents;
    public static AliasDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextLaTeXSettings.PredefinedTextSymbols;
    public static Dictionary<string, Space> PredefinedLengthUnits =>
      Space.PredefinedLengthUnits;
  }
}
