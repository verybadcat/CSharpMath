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
using Foundation;
#if __IOS__
using NView = UIKit.UIView;
using NColor = UIKit.UIColor;
using NContentInsets = UIKit.UIEdgeInsets;
#else
using NView = AppKit.NSView;
#endif

namespace CSharpMath.Apple {
  public class AppleMathView : NView {
    public string ErrorMessage { get; set; }
    public void SetMathList(IMathList mathList)
    {
      _mathList = mathList;
      Latex = MathListBuilder.MathListToString(mathList);
      InvalidateIntrinsicContentSize();
      SetNeedsLayout();
    }
    public void SetLatex(string latex)
    {
      Latex = latex;
      (_mathList, ErrorMessage) = MathLists.BuildResultFromString(latex);
      if (_mathList != null)
      {
        _CreateDisplayList();
      }
      InvalidateIntrinsicContentSize();

      SetNeedsLayout();
    }
    public float FontSize { get; set; } = 20f;
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;
    public NContentInsets ContentInsets { get; set; }

    private IMathList _mathList;

    public string Latex { get; private set; }
    private MathListDisplay<TFont, TGlyph> _displayList { get; set; }

    public bool DisplayErrorInline { get; set; } = true;
    public NColor TextColor { get; set; }

    private readonly TypesettingContext<TFont, TGlyph> _typesettingContext;

    public AppleMathView(TypesettingContext<TFont, TGlyph> typesettingContext, float fontSize) {
      Layer.GeometryFlipped = true;
      BackgroundColor = NColor.FromRGB(0.9f, 0.9f, 0.9f);
      TextColor = NColor.Black;
      FontSize = fontSize;
      _typesettingContext = typesettingContext;
    }

    public override CGSize SizeThatFits(CGSize size)
    {
      CGSize r;
      if (_displayList!=null) {
        r = _displayList.ComputeDisplayBounds().Size;
        r.Width += ContentInsets.Left + ContentInsets.Right;
        r.Height += ContentInsets.Top + ContentInsets.Bottom;
      } else {
        r = new CGSize(320, 40);
      }

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
        nfloat textY = ((availableHeight - contentHeight) / 2) + ContentInsets.Bottom + _displayList.Descent;
        _displayList.Position = new System.Drawing.PointF((float)textX, (float)textY);
      }
    }

    private void _CreateDisplayList()
    {
      var fontSize = FontSize;
      var appleFont = AppleFontManager.LatinMath(fontSize);
      _displayList = _typesettingContext.CreateLine(_mathList, appleFont, LineStyle.Display);
    }

    public override void Draw(CGRect rect) {
      base.Draw(rect);
      var cgContext = UIGraphics.GetCurrentContext();
      if (_mathList != null) {

        var appleContext = new AppleGraphicsContext()
        {
          CgContext = cgContext
        };
        cgContext.SaveState();
        cgContext.SetStrokeColor(TextColor.CGColor);
        cgContext.SetFillColor(TextColor.CGColor);
        _displayList.Draw(appleContext);
        cgContext.RestoreState();
      } else if (ErrorMessage.IsNonEmpty()) {
        cgContext.SaveState();
        float errorFontSize = 20;
        var attributes = new UIStringAttributes
        {
          ForegroundColor = NColor.Red,
          Font = UIFont.SystemFontOfSize(errorFontSize),
        };
        var attributedString = new NSAttributedString(ErrorMessage, attributes);
        var ctLine = new CTLine(attributedString);
        cgContext.TextPosition = new CGPoint(0, Bounds.Size.Height - errorFontSize);
        ctLine.Draw(cgContext);
        cgContext.RestoreState();
      }
    }
  }
}
