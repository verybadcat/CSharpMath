using CSharpMath.Display;
using CSharpMath.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using CoreGraphics;
using UIKit;
using TGlyph = System.UInt16;
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
      var fontSize = 50;
      var uiFont = UIFont.SystemFontOfSize(fontSize);
      var descriptor = new CTFontDescriptor(uiFont.Name, uiFont.PointSize);
      var ctFont = new CTFont(descriptor, fontSize);
      var typesetting = AppleTypesetters.CreateTypesettingContext(ctFont);
      _displayList = typesetting.CreateLine(_mathList, new AppleMathFont(uiFont.Name, fontSize), LineStyle.Display);
      SetNeedsLayout();
    }
    public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;

    private IMathList _mathList;

    public string Latex { get; private set; }
    private MathListDisplay<TGlyph> _displayList { get; set; }

    public bool DisplayErrorInline { get; set; } = true;
    public NColor TextColor { get; set; }
    public AppleLatexView() {
      Layer.GeometryFlipped = true;
      BackgroundColor = NColor.Clear;
      TextColor = NColor.Black;
    }

    public override CGSize SizeThatFits(CGSize size)
    {
      var r = _displayList.ComputeDisplayBounds().Size;
      return r;
    }

    public override void Draw(CGRect rect) {
      base.Draw(rect);
      if (_mathList != null) {
        var cgContext = UIGraphics.GetCurrentContext();
        var appleContext = new AppleGraphicsContext
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
