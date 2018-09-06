### Ever wanted to display LaTeX math formulae in C# but cannot find any tool to help you? Here is what you may be looking for.

![icon](Icon.png)

CSharpMath is a C# port of the wonderful [iosMath LaTeX engine](https://github.com/kostub/iosMath).

Current NuGet version|Current stable version|Commits since last version
-|-|-
![NuGet release shield](https://img.shields.io/nuget/v/CSharpMath.svg)|![GitHub Releases shield](https://img.shields.io/github/release/verybadcat/CSharpMath.svg)|![GitHub commits since last version shield](https://img.shields.io/github/commits-since/verybadcat/CSharpMath/latest.svg)
 <!--
## Choose your platform
Really, any one you like!

[//]: # (Primary platforms)

[SkiaSharp](wiki/@GettingStarted~SkiaSharp.md)
[Xamarin.Forms](wiki/@GettingStarted~Forms.md)
[Xamarin.iOS](wiki/@GettingStarted~iOS.md)

[//]: # (Through SkiaSharp)

[tvOS](wiki/@GettingStarted~tvOS.md)
[watchOS](wiki/@GettingStarted~watchOS.md)
[Xamarin.Android](wiki/@GettingStarted~Android.md)
[Xamarin.Mac](wiki/@GettingStarted~Mac.md)
[.NET Core](wiki/@GettingStarted~NetCore.md)
[Windows Forms](wiki/@GettingStarted~WinForms.md)
[Windows Presentation Framework](wiki/@GettingStarted~WPF.md)
[Universal Windows Platform](wiki/@GettingStarted~UWP.md)
[Gtk#](wiki/@GettingStarted~Gtk.md)
[Tizen](wiki/@GettingStarted~Tizen.md)

[//]: # (Future)

[Unity](wiki/@GettingStarted~Unity.md)
[ASP.NET](wiki/@GettingStarted~ASP.md)
[Ooui.Wasm](wiki/@GettingStarted~Ooui.md)-->

Ironically enough, the first front end was iOS (CSharpMath.Ios).
As development continued, Xamarin.Forms (CSharpMath.Forms)<!-- and Windows environments--> is now supported via SkiaSharp (CSharpMath.SkiaSharp) as of 0.1.0.

# Usage and Examples

To get started, do something like this:

### 1. CSharpMath.Ios

```cs
var latexView = IosMathLabels.MathView(@"x = -b \pm \frac{\sqrt{b^2-4ac}}{2a}", 15);
latexView.ContentInsets = new UIEdgeInsets(10, 10, 10, 10);
var size = latexView.SizeThatFits(new CoreGraphics.CGSize(370, 180));
latexView.Frame = new CoreGraphics.CGRect(0, 20, size.Width, size.Height);
someSuperview.Add(latexView);
```

[See an example project](CSharpMath.IosExample)
      
![Quadratic Formula](CSharpMath/RenderedSamples/Quadratic%20Formula.png)

![Power Series](CSharpMath/RenderedSamples/PowerSeries.png)

![Matrix Product](CSharpMath/RenderedSamples/MatrixProduct.png)

![Continued Fraction](CSharpMath/RenderedSamples/ContinuedFraction.png)
      
### 2. CSharpMath.Forms

```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:math="clr-namespace:CSharpMath.Forms;assembly=CSharpMath.Forms"
             x:Class="Namespace.Class">
    <math:FormsMathView x:Name="View" HorizontalOptions="FillAndExpand" VerticalOptions="FillAndExpand">
        \frac\sqrt23
    </math:FormsMathView>
</ContentPage>
```
or you can use the prehistoric way:
```cs
var view = new FormsMathView();
view.HorizontalOptions = view.VerticalOptions = LayoutOptions.FillAndExpand;
view.LaTeX = @"\frac\sqrt23";
someLayout.Children.Add(view);
```

[See an example project](CSharpMath.Forms.Example)
    
iOS | Android | Windows UWP
----|---------|------------
![1/2](https://user-images.githubusercontent.com/19922066/40612166-fd6c5b38-62ab-11e8-9cb1-b2b7eb6883be.png) | ![1+1](https://user-images.githubusercontent.com/19922066/40575043-183a6970-6110-11e8-887f-820e14efc588.jpeg) | ![Panning a view](https://user-images.githubusercontent.com/19922066/40731183-18a09b68-6463-11e8-8095-1a4cc9df9eae.gif) ![Colors!](https://user-images.githubusercontent.com/19922066/40972206-8abc247c-68f2-11e8-8684-561b5e833c21.png)

# [Documentation](https://github.com/verybadcat/CSharpMath/wiki/Documentation-of-public-facing-APIs-of-CSharpMath.Rendering,-CSharpMath.SkiaSharp-and-CSharpMath.Forms)

# Extending to more platforms

There are a few ways to extend this to more platforms:
(Hopefully, you would not need to touch the core typesetting engine. If you do, we would consider that a bug.)

### 1. Branching off from CSharpMath (the project)

This path would require the most effort to implement, but allows you to plug in any font library and graphics library.

You would have to define your own [TypesettingContext](CSharpMath/FrontEnd/TypesettingContext.cs) and write an implementation of [IGraphicsContext](CSharpMath/FrontEnd/IGraphicsContext.cs).

The TypesettingContext in turn has several components, including choosing a font.

### 2. Forking from CSharpMath.Rendering

As CSharpMath.Rendering provides font lookup through [the Typography library](https://github.com/LayoutFarm/Typography), you would only need to write adapter classes to connect this library to your chosen graphics library.

You would have to implement [ICanvas](CSharpMath.Rendering/Drawing/ICanvas.cs) and feed it into the Draw method of [MathPainter](CSharpMath.Rendering/MathPainter.cs).

### 3. Referencing CSharpMath.SkiaSharp

You can extend this library to other SkiaSharp-supported platforms by feeding the SKCanvas given in the OnPaintSurface override of a SkiaSharp view into the Draw method of [SkiaMathPainter](CSharpMath.SkiaSharp/SkiaMathPainter.cs).

### 4. Building on top of CSharpMath.Apple

You can use this library on other appleOSes by making use of [AppleMathView](CSharpMaath.Apple/AppleMathView.cs).
      
# Project needs

We need more contributors! Maybe you can contribute something to this repository. Whether they are bug reports, feature proposals or pull requests, you are welcome to send them to us. We are sure that we will take a look at them!

Here is an idea list if you cannot think of anything right now:
- A new example for the Exmple projects (please open pull requests straight away)
- A new LaTeX command (please link documentation of it)
- A new front end (please describe what it is and why should it be supported)
- A new math syntax (please describe what it is and why should it be supported)

# License

CSharpMath is licensed by [the MIT license](LICENSE).

Dependency|Used by|License
-|-|-
Typography project|CSharpMath.Rendering|[MIT](https://github.com/LayoutFarm/Typography/blob/master/LICENSE.md)
latin-modern-math font|CSharpMath.Ios, CSharpMath.Rendering|[GUST Font License](http://www.gust.org.pl/projects/e-foundry/licenses/GUST-FONT-LICENSE.txt/view)
AMS-Capital-Blackboard-Bold font (extracted by @Happypig375 from [the amsfonts package](https://ctan.org/pkg/amsfonts))|CSharpMath.Rendering|[SIL Open Font License](https://ctan.org/license/ofl)

# Authors

[@verybadcat](https://github.com/verybadcat)

[@Happypig375](https://github.com/Happypig375)

[@charlesroddie](https://github.com/charlesroddie)

[@FoggyFinder](https://github.com/FoggyFinder)

Thanks for reading.
<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
You can take a look at the code now.<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
Really, there is nothing here.<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
I bet you scrolled past and came back to read me.<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
Will you stop scrolling?<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
Ok, fine... I give up.

###### A sneak peek at the future?

Shhh... Don't tell anybody!

![Future?](https://github.com/Happypig375/CSharpMath/blob/master/Roadmap.png)
0.2.0: MathML?
0.3.0: AsciiMath?
0.4.0: Infix?

0.4.0: Math evaluation??
0.5.0: Handwritten math recognition???
