using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using CoreGraphics;
using UIKit;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.AppleMathFont;
using CoreText;
using CSharpMath.Apple.Drawing;
#if __IOS__
using NView = UIKit.UIView;
using NColor = UIKit.UIColor;
#else
using NView = AppKit.NSView;
#endif

namespace CSharpMath.Apple {
  public class AppleLatexView : NView {
    public void SetMathList(IMathList mathList) {
      _mathList = mathList;
      Latex = MathListBuilder.MathListToString(mathList);
      InvalidateIntrinsicContentSize();
      SetNeedsLayout();
    }
    public void SetLatex(string latex) {
      Latex = latex;
      _mathList = MathLists.FromString(latex);
      InvalidateIntrinsicContentSize();
      var fontSize = 40;
      var appleFont = new TFont("latinmodern-math", fontSize);
      var typesetting = AppleTypesetters.CreateLatinMath();
      _displayList = typesetting.CreateLine(_mathList, appleFont, LineStyle.Display);
      SetNeedsLayout();
    }
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;

    private IMathList _mathList;

    public string Latex { get; private set; }
    private MathListDisplay<TFont, TGlyph> _displayList { get; set; }

    public bool DisplayErrorInline { get; set; } = true;
    public NColor TextColor { get; set; }

    private readonly TypesettingContext<TFont, ushort> _typesettingContext;

    public AppleLatexView(TypesettingContext<TFont, TGlyph> typesettingContext) {
      Layer.GeometryFlipped = true;
      BackgroundColor = NColor.Clear;
      TextColor = NColor.Black;
      _typesettingContext = typesettingContext;
    }

    public override CGSize SizeThatFits(CGSize size)
    {
      var r = _displayList.ComputeDisplayBounds().Size;
      r.Height += 50;
      r.Width += 50;
      return r;
    }

    public override void Draw(CGRect rect) {
      base.Draw(rect);
      if (_mathList != null) {
        var cgContext = UIGraphics.GetCurrentContext();
        var appleContext = new AppleGraphicsContext(_typesettingContext.GlyphFinder)
        {
          CgContext = cgContext
        };
        cgContext.SaveState();
        cgContext.TranslateCTM(10, 40);
        _displayList.Draw(appleContext);
        cgContext.RestoreState();
      }
    }
  }
}
