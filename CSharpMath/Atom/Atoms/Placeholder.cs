using System.Drawing;
namespace CSharpMath.Atom.Atoms {
  /// <summary>A placeholder for future input</summary>
  public sealed class Placeholder : MathAtom {
    public Color? ForeColor { get; set; }
    public Placeholder(string nucleus, Color? color) : base(nucleus) => ForeColor = color;
    public override bool ScriptsAllowed => true;
    public new Placeholder Clone(bool finalize) => (Placeholder)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Placeholder(Nucleus, ForeColor);
  }
}