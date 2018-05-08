using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using TFont = CSharpMath.SkiaSharp.SkiaMathFont;
using Glyph = Typography.OpenFont.Glyph;

using SkiaSharp;
using NColor = SkiaSharp.SKColor;
using NColors = SkiaSharp.SKColors;

namespace CSharpMath.SkiaSharp
{
  public readonly struct Padding {
    public float Top { get; }
    public float Bottom { get; }
    public float Left { get; }
    public float Right { get; }
  }
  public class SkiaLatexPainter {
    public string ErrorMessage { get; set; }
    public float FontSize { get; set; } = 20f;
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;
    public Padding Padding { get; set; }
    public bool DisplayErrorInline { get; set; } = true;
    public NColor TextColor { get; set; }
    public NColor BackgroundColor { get; set; }

    private MathListDisplay<TFont, Glyph> _displayList;
    
    private readonly TypesettingContext<TFont, Glyph> _typesettingContext;

    private IMathList _mathList;
    public IMathList MathList {
      get => _mathList;
      set {
        _mathList = value;
        Latex = MathListBuilder.MathListToString(value);
        Repaint();
      }
    }

    private string _latex;
    public string Latex {
      get => _latex;
      set {
        _latex = value;
        var buildResult = MathLists.BuildResultFromString(value);
        _mathList = buildResult.MathList;
        ErrorMessage = buildResult.Error;
        if (_mathList != null) {
          _CreateDisplayList();
        }
        Repaint();
      }
    }

    public SkiaLatexPainter(TypesettingContext<TFont, Glyph> typesettingContext, float fontSize) {
      BackgroundColor = new NColor(230, 230, 230);
      TextColor = NColors.Black;
      FontSize = fontSize;
      _typesettingContext = typesettingContext;
    }

    public override CGSize SizeThatFits(CGSize size) {
      CGSize r;
      if (_displayList != null) {
        r = _displayList.ComputeDisplayBounds().Size;
        r.Width += Padding.Left + Padding.Right;
        r.Height += Padding.Top + Padding.Bottom;
      } else {
        r = new CGSize(320, 40);
      }

      return r;
    }

    public override void LayoutSubviews() {
      if (_mathList != null) {
        float displayWidth = _displayList.Width;
        float textX = 0;
        switch (TextAlignment) {
          case ColumnAlignment.Left:
            textX = Padding.Left;
            break;
          case ColumnAlignment.Center:
            textX = Padding.Left + (Bounds.Size.Width - Padding.Left - Padding.Right - displayWidth) / 2;
            break;
          case ColumnAlignment.Right:
            textX = Bounds.Size.Width - Padding.Right - displayWidth;
            break;
        }
        float availableHeight = Bounds.Size.Height - Padding.Top - Padding.Bottom;
        float contentHeight = _displayList.Ascent + _displayList.Descent;
        if (contentHeight < FontSize / 2) {
          contentHeight = FontSize / 2;
        }
        float textY = ((availableHeight - contentHeight) / 2) + Padding.Bottom + _displayList.Descent;
        _displayList.Position = new System.Drawing.PointF((float)textX, (float)textY);
      }
    }

    private void _CreateDisplayList() {
      var fontSize = FontSize;
      var skiaFont = SkiaFontManager.LatinMath(fontSize);
      _displayList = _typesettingContext.CreateLine(_mathList, skiaFont, LineStyle.Display);
    }

    public override void Draw(CGRect rect) {
      base.Draw(rect);
      var cgContext = UIGraphics.GetCurrentContext();
      if (_mathList != null) {

        var appleContext = new AppleGraphicsContext() {
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
        var attributes = new UIStringAttributes {
          ForegroundColor = NColors.Red,
          Font = UIFont.SystemFontOfSize(errorFontSize),
        };
        var attributedString = new NSAttributedString(ErrorMessage, attributes);
        var ctLine = new CTLine(attributedString);
        cgContext.TextPosition = new CGPoint(0, Bounds.Size.Height - errorFontSize);
        ctLine.Draw(cgContext);
        cgContext.RestoreState();
      }
    }

    public void Repaint() {

    }

    public void PaintSurface(SKSurface surface, SKImageInfo imageInfo) {
      
    }
  }
}
