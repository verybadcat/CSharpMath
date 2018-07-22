using CSharpMath.Rendering;

namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 30) : base(fontSize) { }

    public float LineWidth { get; set; }

    protected override IPositionableDisplay<MathFonts, Glyph> CreateDisplay(MathFonts fonts) {
      var displays = Atom.ToDisplay(fonts, default);
      if(displays is Display.MathListDisplay<MathFonts, Glyph> list)
         foreach (var display in list.Displays) {
           if(display is i display.Position.Y >= LineWidth) display.po
         }
      return displays;
    }

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }
  }
}
