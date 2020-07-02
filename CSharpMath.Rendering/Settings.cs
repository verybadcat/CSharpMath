namespace CSharpMath {
  using System.Collections.Generic;
  using Structures;
  public static class Settings {
    public static Rendering.BackEnd.Typefaces GlobalTypefaces =>
      Rendering.BackEnd.Fonts.GlobalTypefaces;
    public static BiDictionary<string, System.Drawing.Color> PredefinedColors =>
      Atom.LaTeXSettings.PredefinedColors;
    public static LaTeXCommandDictionary<Atom.Boundary> PredefinedLaTeXBoundaryDelimiters =>
      Atom.LaTeXSettings.BoundaryDelimiters;
    public static BiDictionary<string, Atom.FontStyle> PredefinedLaTeXFontStyles =>
      Atom.LaTeXSettings.FontStyles;
    public static LaTeXCommandDictionary<System.Func
      <Atom.LaTeXParser, Atom.MathList, char, Result<(Atom.MathAtom? Atom, Atom.MathList? Return)>>
    > PredefinedLaTeXCommands =>
      Atom.LaTeXSettings.Commands;
    public static BiDictionary<string, Atom.MathAtom> PredefinedLaTeXCommandSymbols =>
      Atom.LaTeXSettings.CommandSymbols;
    public static BiDictionary<string, string> PredefinedLaTeXTextAccents =>
      Rendering.Text.TextLaTeXSettings.PredefinedAccents;
    public static BiDictionary<string, string> PredefinedLaTeXTextSymbols =>
      Rendering.Text.TextLaTeXSettings.PredefinedTextSymbols;
    public static Dictionary<string, Space> PredefinedLengthUnits =>
      Space.PredefinedLengthUnits;
  }
}
