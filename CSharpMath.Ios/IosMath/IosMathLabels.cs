using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpMath.Apple;
using Foundation;
using UIKit;

namespace CSharpMath.Ios.IosMath {
  static class IosMathLabels {
    public static AppleLatexView LatexView(string latex) {
      var view = new AppleLatexView();
      view.SetLatex(latex);
      return view;
    }
  }
}