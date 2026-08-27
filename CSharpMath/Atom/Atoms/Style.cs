namespace CSharpMath.Atom.Atoms {
  ///<summary>Style changes during rendering</summary>
  public sealed class Style : MathAtom {
    public Style(LineStyle style) : base(string.Empty) => LineStyle = style;
    public override string Nucleus {
      get => base.Nucleus;
      set {
        if (!string.IsNullOrEmpty(value)) throw new System.InvalidOperationException("Style atoms cannot have a nucleus.");
        base.Nucleus = value;
      }
    }
    public override FontStyle FontStyle {
      get => base.FontStyle;
      set {
        if (value != FontStyle.Default) throw new System.InvalidOperationException("Style atoms cannot have a font style.");
        base.FontStyle = value;
      }
    }
    public LineStyle LineStyle { get; }
    public override bool ScriptsAllowed => false;
    public new Style Clone(bool finalize) => (Style)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Style(LineStyle);
    public override string DebugString =>
#pragma warning disable CA1308 // Normalize strings to uppercase
      @"\" + LineStyle.ToString().ToLowerInvariant() + "style";
#pragma warning restore CA1308 // Normalize strings to uppercase
    public bool EqualsStyle(Style otherStyle) =>
      EqualsAtom(otherStyle) && LineStyle == otherStyle.LineStyle;
    public override bool Equals(object obj) => obj is Style s ? EqualsStyle(s) : false;
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 107 * LineStyle.GetHashCode());
  }
}
