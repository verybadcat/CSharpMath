﻿using CSharpMath.Display;
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
#if __IOS__
using NView = UIKit.UIView;
using NColor = UIKit.UIColor;
#else
using NView = AppKit.NSView;
#endif

namespace CSharpMath.Apple
{
    public class AppleLatexView: NView
    {
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
            _mathList = MathLists.FromString(latex);
            InvalidateIntrinsicContentSize();
            SetNeedsLayout();
        }
        public ColumnAlignment TextAlignment { get; set; } = ColumnAlignment.Left;

        private IMathList _mathList;

        public string Latex { get; private set; }
        private MathListDisplay<TGlyph> _displayList { get; set; }

        public bool DisplayErrorInline { get; set; } = true;
        public NColor TextColor { get; set; }
        public AppleLatexView()
        {
            Layer.GeometryFlipped = true;
            BackgroundColor = NColor.Clear;
            TextColor = NColor.Black;
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            if (_mathList!=null)
            {
                var cgContext = UIGraphics.GetCurrentContext();
                cgContext.SaveState();

                cgContext.RestoreState();
            }
        }
    }
}