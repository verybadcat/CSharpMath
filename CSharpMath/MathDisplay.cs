namespace CSharpMath {
#pragma warning disable CA1815 // Not overriding object methods is intentional (not supposed to be used)
#pragma warning disable CA1822 // Not marking methods as static is intentional (easy extensions)
  using O = System.ComponentModel.BrowsableAttribute;
  using B = System.ObsoleteAttribute;
  using E = System.ComponentModel.EditorBrowsableAttribute;
  public readonly struct MathDisplay {
    private const System.ComponentModel.EditorBrowsableState R = System.ComponentModel.EditorBrowsableState.Never;
    private const bool K = false;
    private const bool M = true;
    private const string OO = "Get these object methods out of my sight!";
    [O(K), B(OO,M),E(R)] public new bool Equals(object o) => false;
    [O(K), B(OO,M),E(R)] public new int GetHashCode() => 0;
    [O(K), B(OO,M),E(R)] public new System.Type? GetType() => null;
    [O(K), B(OO,M),E(R)] public new string? ToString() => null;
    public Structures.Result<Atoms.MathList> MathListFromLaTeX(string latex) =>
      Atoms.LaTeXBuilder.TryMathListFromLaTeX(latex);
    public string MathListToLaTeX(Atoms.MathList mathList) =>
      Atoms.LaTeXBuilder.MathListToLaTeX(mathList);
    public Displays.IDisplay<TFont, TGlyph> MathListToDisplay<TFont, TGlyph>
      (Atoms.MathList mathList,
       TFont font,
       FrontEnd.TypesettingContext<TFont, TGlyph> context,
       Atoms.LineStyle style) where TFont : FrontEnd.IFont<TGlyph> =>
      Displays.Typesetter.CreateLine(mathList, font, context, style);
  }
}