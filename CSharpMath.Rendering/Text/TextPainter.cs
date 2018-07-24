namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 30) : base(fontSize) { }

    public float LineWidth { get; set; }

    protected override IDisplay<MathFonts, Glyph> CreateDisplay(MathFonts fonts) {
      var atomDisplay = Atom.ToDisplay(fonts, default);
      float lineHeight = 0f;
      void AddLineBreaks(IDisplay<MathFonts, Glyph> display) {
        if (display is Display.MathListDisplay<MathFonts, Glyph> list)
          foreach (var d in list.Displays) AddLineBreaks(d);
        else {
          if (display.Position.X >= LineWidth) {
            display.Position = new System.Drawing.PointF(0f, display.Position.Y + lineHeight);
            lineHeight = display.ComputeDisplayBounds().Height;
          } else lineHeight = System.Math.Max(lineHeight, display.ComputeDisplayBounds().Height);
        }
      }
      AddLineBreaks(atomDisplay);
      return atomDisplay;
    }

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }
  }
}
