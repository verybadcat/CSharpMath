Getting Started with Xamarin.tvOS
--------------------------------
1. Create a Xamarin.tvOS project. Choose a Single View App to start.

![Create](@GettingStarted~tvOS.1.png)

2. After going through the configuration window, open the `Add Packages...` window and install CSharpMath.SkiaSharp and SkiaSharp.Views.

![Add package](@GettingStarted~tvOS.2a.png)
![Install CSharpMath](@GettingStarted~tvOS.2b.png)
![Install SkiaSharp](@GettingStarted~tvOS.2c.png)

3. After installation, open ViewController.cs.

![Open file](@GettingStarted~tvOS.3.png)

4. Add
```cs
var mathPainter = new CSharpMath.SkiaSharp.MathPainter(80) { LaTeX = "1+1=2", TextColor = SkiaSharp.SKColors.White };
var mathView = new SkiaSharp.Views.tvOS.SKCanvasView();
mathView.PaintSurface += (sender, e) =>
{
    e.Surface.Canvas.Clear();
    mathPainter.Draw(e.Surface.Canvas);
};
View = mathView;
```
below 
`// Do any additional setup after loading the view.` under the `ViewDidLoad()` method.

![Add code](@GettingStarted~tvOS.4.png)

5. Click the arrow button to run.

![Result](@GettingStarted~tvOS.5.png)

6. [Discover what you can do](@Introduction)