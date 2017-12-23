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
using System;
#if __IOS__
using NView = UIKit.UIView;
using NColor = UIKit.UIColor;
using NContentInsets = UIKit.UIEdgeInsets;
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
      _CreateDisplayList();
      SetNeedsLayout();
    }
    public float FontSize { get; set; } = 30f;
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;
    public NContentInsets ContentInsets { get; set; }



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
      return r;
    }

    public override void LayoutSubviews()
    {
      if (_mathList!=null) {
        float displayWidth = _displayList.Width;
        nfloat textX = 0;
        switch (TextAlignment) {
          case ColumnAlignment.Left:
            textX = ContentInsets.Left;
            break;
          case ColumnAlignment.Center:
            textX = ContentInsets.Left + (Bounds.Size.Width - ContentInsets.Left - ContentInsets.Right - displayWidth) / 2;
            break;
          case ColumnAlignment.Right:
            textX = Bounds.Size.Width - ContentInsets.Right - displayWidth;
            break;
        }
        nfloat availableHeight = Bounds.Size.Height - ContentInsets.Top - ContentInsets.Bottom;
        nfloat contentHeight = _displayList.Ascent + _displayList.Descent;
        if (contentHeight < FontSize/2) {
          contentHeight = FontSize / 2;
        }
        nfloat textY = (contentHeight / 2) + ContentInsets.Bottom + _displayList.Descent;
        _displayList.Position = new System.Drawing.PointF((float)textX, (float)textY);
      }
    }

    private void _CreateDisplayList()
    {
      var fontSize = FontSize;
      var appleFont = new TFont("latinmodern-math", fontSize);
      var typesetting = AppleTypesetters.CreateLatinMath();
      _displayList = typesetting.CreateLine(_mathList, appleFont, LineStyle.Display);
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
        _displayList.Draw(appleContext);
        cgContext.RestoreState();
      }
    }
  }
}
