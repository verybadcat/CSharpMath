# CSharpMath
This is a C# port of the wonderful [iosMath LaTeX engine](https://github.com/kostub/iosMath).

It is now working in most cases. Some examples are below. Ironically enough, the first front end is iOS. However, if you want to add a front end, such as Xamarin.Forms or a Windows environment, it should be possible. You would have to define your own [TypesettingContext](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/FrontEnd/TypesettingContext.cs) and write an implementation of [IGraphicsContext](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/FrontEnd/IGraphicsContext.cs). The TypesettingContext in turn has several components, including choosing a font. Hopefully, you would not need to touch the core typesetting engine. (If you do, I would consider that a bug.)

![Quadratic Formula](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/Quadratic%20Formula.png)

![Power Series](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/PowerSeries.png)

![Matrix Product](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/MatrixProduct.png)

![Continued Fraction](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/ContinuedFraction.png)

# Usage

To get started, do something like this:

      var latexView = IosMathLabels.LatexView(@"x = -b \pm \frac{\sqrt{b^2-4ac}}{2a}", 15);
      latexView.ContentInsets = new UIEdgeInsets(10, 10, 10, 10);
      var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 180));
      latexView.Frame = new CoreGraphics.CGRect(0, 20, size.Width, size.Height);
      someSuperview.Add(latexView);

#License

The code is licenced by the MIT license. The latin-modern-math font is licenced by the [GUST Font License](./fonts/GUST-FONT-LICENSE.txt).
