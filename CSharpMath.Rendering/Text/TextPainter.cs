using CSharpMath.Rendering;

namespace CSharpMath.Rendering {
  [System.Obsolete("The Text classes are not yet usable in this prerelease.", true)]
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize) : base(fontSize) { }

    protected override IPositionableDisplay<MathFonts, Glyph> CreateDisplay(MathFonts fonts) {
      return Atom.ToDisplay(fonts);
    }

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }
  }
}
