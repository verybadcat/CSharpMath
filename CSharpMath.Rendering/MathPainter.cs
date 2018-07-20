using CSharpMath.Display;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;

namespace CSharpMath.Rendering {
  public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, MathSource, TColor> {
    public MathPainter(float fontSize = DefaultFontSize) : base(fontSize) { }

    protected override IPositionableDisplay<MathFonts, Glyph> CreateDisplay(MathFonts fonts) =>
      TypesettingContext.Instance.CreateLine(MathList, fonts, LineStyle);

    public IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
    public override string ErrorMessage => Source.Error;
  }
}
