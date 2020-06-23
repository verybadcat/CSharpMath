using CSharpMath.Atom;
using CSharpMath.Display;
using CSharpMath.Display.FrontEnd;
using CoreGraphics;
using UIKit;
using TGlyph = System.UInt16;
using TFont = CSharpMath.Apple.AppleMathFont;
using CoreText;
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
    public string? ErrorMessage { get; set; }
    private IDisplay<TFont, TGlyph>? _displayList = null;
    public float FontSize { get; set; } = 20f;
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;
    public NContentInsets ContentInsets { get; set; }
    private LineStyle _style = LineStyle.Display;
    public LineStyle LineStyle {
      get => _style;
      set {
        _style = value;
        if (_mathList != null) {
          _displayList = Typesetter.CreateLine(_mathList,
            TFont.LatinMath(FontSize), _typesettingContext, _style);
        }
        InvalidateIntrinsicContentSize();
        SetNeedsLayout();
      }
    }
    private MathList _mathList = new MathList();
    public MathList MathList {
      get => _mathList;
      set {
        _mathList = value;
        LaTeX = LaTeXParser.MathListToLaTeX(value).ToString();
        InvalidateIntrinsicContentSize();
        SetNeedsLayout();
      }
    }
    private string _latex = "";
    public string LaTeX {
      get => _latex;
      set {
        _latex = value;
        (_mathList, ErrorMessage) = LaTeXParser.MathListFromLaTeX(value);
        if (_mathList != null) {
          _displayList = Typesetter.CreateLine(_mathList,
            TFont.LatinMath(FontSize), _typesettingContext, _style);
        }
        InvalidateIntrinsicContentSize();
        SetNeedsLayout();
      }
    }
    public bool DisplayErrorInline { get; set; } = true;
    public NColor TextColor { get; set; }
    private readonly TypesettingContext<TFont, TGlyph> _typesettingContext;
    public AppleMathView
      (TypesettingContext<TFont, TGlyph> typesettingContext, float fontSize) {
      Layer.GeometryFlipped = true;
      BackgroundColor = NColor.FromRGB(0.9f, 0.9f, 0.9f);
      TextColor = NColor.Black;
      FontSize = fontSize;
      _typesettingContext = typesettingContext;
    }
    public override CGSize SizeThatFits(CGSize size) {
      CGSize r;
      if (_displayList != null) {
        r = _displayList.DisplayBounds().Size;
        r.Width += ContentInsets.Left + ContentInsets.Right;
        r.Height += ContentInsets.Top + ContentInsets.Bottom;
      } else {
        r = new CGSize(320, 40);
      }
      return r;
    }
    public override void LayoutSubviews() {
      if (_mathList != null && _displayList != null) {
        float displayWidth = _displayList.Width;
        var textX = TextAlignment switch
        {
          ColumnAlignment.Left => ContentInsets.Left,
          ColumnAlignment.Center =>
            (Bounds.Size.Width - ContentInsets.Right - displayWidth) / 2,
          ColumnAlignment.Right =>
            Bounds.Size.Width - ContentInsets.Right - displayWidth,
          _ => 0,
        };
        var availableHeight =
          Bounds.Size.Height - ContentInsets.Top - ContentInsets.Bottom;
        var contentHeight =
          Math.Max(_displayList.Ascent + _displayList.Descent, FontSize / 2);
        var textY =
          (availableHeight - contentHeight) / 2
          + ContentInsets.Bottom + _displayList.Descent;
        _displayList.Position = new System.Drawing.PointF((float)textX, (float)textY);
      }
    }
    public override void Draw(CGRect rect) {
      base.Draw(rect);
      var cgContext = UIGraphics.GetCurrentContext();
      if (_mathList != null && _displayList != null) {
        cgContext.SaveState();
        cgContext.SetStrokeColor(TextColor.CGColor);
        cgContext.SetFillColor(TextColor.CGColor);
        _displayList.Draw(new AppleGraphicsContext(cgContext));
        cgContext.RestoreState();
      } else if (ErrorMessage != null) {
        cgContext.SaveState();
        float errorFontSize = 20;
        cgContext.TextPosition = new CGPoint(0, Bounds.Size.Height - errorFontSize);
        new CTLine(new NSAttributedString(ErrorMessage, new UIStringAttributes {
          ForegroundColor = NColor.Red,
          Font = UIFont.SystemFontOfSize(errorFontSize),
        })).Draw(cgContext);
        cgContext.RestoreState();
      }
    }
  }
}