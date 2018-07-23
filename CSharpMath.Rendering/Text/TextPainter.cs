using CSharpMath.Rendering;

namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 30) : base(fontSize) { }

    public float LineWidth { get; set; }

    protected override IDisplay<MathFonts, Glyph> CreateDisplay(MathFonts fonts) {
      var displays = Atom.ToDisplay(fonts, default);
      float y = 0f, lineHeight = 0f;
      void AddLineBreaks(IDisplay<MathFonts, Glyph> display) {
        if (displays is Display.MathListDisplay<MathFonts, Glyph> list)
          foreach (var d in list.Displays) AddLineBreaks(d);
        else if (display.Position.X >= LineWidth) y += lineHeight;

      }
      return displays;
    }

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }
  }
}
