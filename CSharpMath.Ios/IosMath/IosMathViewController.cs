using System;
using UIKit;

namespace CSharpMath.Ios
{
  public class IosMathViewController: UIViewController
  {
    public IosMathViewController()
    {
    }

    public override void ViewDidLoad()
    {
      var view = IosMathLabels.LatexView("x");
      view.BackgroundColor = UIColor.LightGray;
      view.SizeToFit();
      View.Add(view);
    }
  }
}
