Getting Started with Xamarin.iOS
--------------------------------
Visual Studio for Mac is recommended because the iOS Simulator comes free of charge!

1. Create a Xamarin.iOS project. Choose a Single View App to start.

![Create](@GettingStarted~iOS.1.png)

2. After going through the configuration window, open the `Add Packages...` window and install CSharpMath.Ios.

![Add package](@GettingStarted~iOS.2a.png)
![Install package](@GettingStarted~iOS.2b.png)

3. After installation, open ViewController.cs.

![Open file](@GettingStarted~iOS.3.png)

4. Add
```cs
var latexView = CSharpMath.Ios.IosMathLabels.MathView(@"x = -b \pm \frac{\sqrt{b^2-4ac}}{2a}", 15);
latexView.ContentInsets = new UIEdgeInsets(10, 10, 10, 10);
var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 180));
latexView.Frame = new CoreGraphics.CGRect(new CoreGraphics.CGPoint(0, 20), size);
this.Add(latexView);
```
below 
`// Perform any additional setup after loading the view, typically from a nib.` under the `ViewDidLoad()` method.

![Add code](@GettingStarted~iOS.4.png)

5. Click the arrow button to run.

![Result](@GettingStarted~iOS.5.png)

6. [Discover what you can do](@Introduction)