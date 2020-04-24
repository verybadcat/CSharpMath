<h3 align="center">Cross-platform LaTeX rendering!</h3>

<p align="center">
<br/><img src="Icon.png" width="300px" alt="CSharpMath icon"><br/><br/>
CSharpMath is a C# port of the wonderful <a href="https://github.com/kostub/iosMath">iosMath LaTeX engine</a>.<br/>
<i>The icon is a product of this library.</i>
</p>

[Current release][NuGet]|[![NuGet release shield](https://img.shields.io/nuget/v/CSharpMath.svg)][NuGet] [![GitHub release shield](https://img.shields.io/github/release/verybadcat/CSharpMath.svg)][GitHub] [![GitHub release date shield](https://img.shields.io/github/release-date/verybadcat/CSharpMath.svg)][GitHub] [![GitHub commits since last release shield](https://img.shields.io/github/commits-since/verybadcat/CSharpMath/latest.svg)][GitHub]
-|-
[Current prerelease][NuGet-pre]|[![NuGet pre-release shield](https://img.shields.io/nuget/vpre/CSharpMath.svg)][NuGet-pre] [![GitHub pre-release shield](https://img.shields.io/github/release-pre/verybadcat/CSharpMath.svg)][GitHub-pre] [![GitHub pre-release date shield](https://img.shields.io/github/release-date-pre/verybadcat/CSharpMath.svg)][GitHub-pre] [![GitHub commits since last prerelease shield](https://img.shields.io/github/commits-since/verybadcat/CSharpMath/latest.svg?include_prereleases)][GitHub-pre]

[![NuGet downloads shield](https://img.shields.io/nuget/dt/CSharpMath.svg)][NuGet] [![GitHub contributors shield](https://img.shields.io/github/contributors/verybadcat/CSharpMath.svg)](https://github.com/verybadcat/CSharpMath/graphs/contributors) [![Average time to resolve an issue](http://isitmaintained.com/badge/resolution/verybadcat/CSharpMath.svg)](http://isitmaintained.com/project/verybadcat/CSharpMath "Average time to resolve an issue") [![Percentage of issues still open](http://isitmaintained.com/badge/open/verybadcat/CSharpMath.svg)](http://isitmaintained.com/project/verybadcat/CSharpMath "Percentage of issues still open")

[NuGet]: https://www.nuget.org/packages/CSharpMath/
[NuGet-pre]: https://www.nuget.org/packages/CSharpMath/absoluteLatest
[GitHub]: https://github.com/verybadcat/CSharpMath/releases/latest
[GitHub-pre]: https://github.com/verybadcat/CSharpMath/releases

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

# Platform support

iOS (CSharpMath.Ios) was ironically the first front end, which was added in v0.0.

Xamarin.Forms (CSharpMath.Forms) support via SkiaSharp (CSharpMath.SkiaSharp) was added in v0.1 as development continued.

Avalonia (CSharpMath.Avalonia) support was also added in v0.4.

For Windows platforms, use https://github.com/ForNeVeR/wpf-math.

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

[See an example project](CSharpMath.Ios.Example)
      
![Quadratic Formula](CSharpMath/RenderedSamples/Quadratic%20Formula.png)|![Power Series](CSharpMath/RenderedSamples/PowerSeries.png)
------------------------------------------------------------------------|-----------------------------------------------------------
![Matrix Product](CSharpMath/RenderedSamples/MatrixProduct.png)         |![Continued Fraction](CSharpMath/RenderedSamples/ContinuedFraction.png)
    
### 2. CSharpMath.SkiaSharp
```cs
var painter = CSharpMath.SkiaSharp.MathPainter();
painter.LaTeX = @"\frac\sqrt23";
paiinter.Draw(someCanvas);
```
This is used by CSharpMath.Forms below.
    
### 3. CSharpMath.Forms

```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:math="clr-namespace:CSharpMath.Forms;assembly=CSharpMath.Forms"
             x:Class="Namespace.Class">
    <math:MathView x:Name="View" HorizontalOptions="FillAndExpand" VerticalOptions="FillAndExpand">
        \frac\sqrt23
    </math:MathView>
</ContentPage>
```
or:
```cs
var view = new CSharpMath.Forms.MathView();
view.HorizontalOptions = view.VerticalOptions = LayoutOptions.FillAndExpand;
view.LaTeX = @"\frac\sqrt23";
someLayout.Children.Add(view);
```

[See an example project](CSharpMath.Forms.Example)
    
iOS | Android | Windows UWP
----|---------|------------
![1/2](https://user-images.githubusercontent.com/19922066/40612166-fd6c5b38-62ab-11e8-9cb1-b2b7eb6883be.png) | ![1+1](https://user-images.githubusercontent.com/19922066/40575043-183a6970-6110-11e8-887f-820e14efc588.jpeg) | ![Panning a view](https://user-images.githubusercontent.com/19922066/40731183-18a09b68-6463-11e8-8095-1a4cc9df9eae.gif) ![Colors!](https://user-images.githubusercontent.com/19922066/40972206-8abc247c-68f2-11e8-8684-561b5e833c21.png)

### 4. CSharpMath.Avalonia

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:math="clr-namespace:CSharpMath.Avalonia;assembly=CSharpMath.Avalonia"
             x:Class="Namespace.Class">
    <math:MathView LaTeX="x + 2 \sqrt{x} + 1 = (\sqrt x+1)^2" />
</UserControl>
```
or:
```cs
var view = new CSharpMath.Avalonia.MathView();
view.LaTeX = @"\frac\sqrt23";
somePanel.Children.Add(view);
```

[See an example project](CSharpMath.Avalonia.Example)

![MathViewPage](https://user-images.githubusercontent.com/19922066/78373612-692db400-75fd-11ea-89c3-2f2a4f47784a.png)

## But I want a button instead!

For Xamarin.Forms, you can make use of `CSharpMath.Forms.MathButton` to make a clickable math button. It wraps a `MathView` inside and will use its properties to draw math on the button.
```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:math="clr-namespace:CSharpMath.Forms;assembly=CSharpMath.Forms"
             x:Class="Namespace.Class">
    <math:MathButton x:Name="MathButton">
        <math:MathView x:Name="MathView">
            \frac\sqrt23
        </math:MathView>
    </math:MathButton>
</ContentPage>
```
For Avalonia, `Avalonia.Controls.Button` already supports arbitrary content. Use it instead.
```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:math="clr-namespace:CSharpMath.Avalonia;assembly=CSharpMath.Avalonia"
             x:Class="Namespace.Class">
    <Button x:Name="MathButton">
        <math:MathView x:Name="MathView">
            \frac\sqrt23
        </math:MathView>
    </Button>
</UserControl>
```

## But I want to display a majority of normal text with a minority of math!
CSharpMath also provides a `TextView` exactly for this purpose. You can use `$`, `\(` and `\)` to delimit inline math and `$$`, `\[` and `\]` to delimit display math.
There is also a `TextButton` for the Xamarin.Forms equivalent of `MathButton`.
Xamarin.Forms:
```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:math="clr-namespace:CSharpMath.Forms;assembly=CSharpMath.Forms"
             x:Class="Namespace.Class">
    <math:TextView LaTeX="Text text text text text \( \frac{\sqrt a}{b} \) text text text text text" />
</ContentPage>
```
Avalonia:
```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:math="clr-namespace:CSharpMath.Avalonia;assembly=CSharpMath.Avalonia"
             x:Class="Namespace.Class">
    <math:TextView LaTeX="Text text text text text \( \frac{\sqrt a}{b} \) text text text text text" />
</UserControl>
```
Xamarin.Forms|Avalonia
-|-
![Xamarin.Forms](https://user-images.githubusercontent.com/19922066/80187259-e7e8a080-8641-11ea-8c15-63e36b85047e.png)|![Avalonia](https://user-images.githubusercontent.com/19922066/80187422-31d18680-8642-11ea-81b3-aea7e027a7ea.png)

## What about rendering to an image instead of displaying in a view?
Warning: There are still some rough edges on image rendering to be resolved, such as [this](CSharpMath.Rendering.Tests/Display/AccentOverF.png) and [this](CSharpMath.Rendering.Tests/Text/WideDisplayMaths.png). However, it is already usable for the majority of cases.

For SkiaSharp:
```cs
using CSharpMath.SkiaSharp;
var painter = new MathPainter { LaTeX = @"\frac23" }; // or TextPainter
using var png = painter.DrawAsStream();
// or painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Jpeg) for JPEG
// or painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Gif) for GIF
// or painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Bmp) for BMP
// or... you get it.
```
For Xamarin.Forms:
```cs
using CSharpMath.SkiaSharp;
var painter = someMathView.Painter; // or someTextView.Painter
using var png = painter.DrawAsStream();
// or painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Jpeg) for JPEG
// or painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Gif) for GIF
// or painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Bmp) for BMP
// or... you get it.
```
For Avalonia:
```cs
using CSharpMath.Avalonia;
var painter = someMathView.Painter; // or someTextView.Painter
// Due to limitations of the Avalonia API, you can only render as PNG to a target stream
painter.DrawAsPng(someStream);
```

![Cell 1](CSharpMath.Rendering.Tests/Display/ExponentWithFraction.png)|![Cell 2](CSharpMath.Rendering.Tests/Display/SummationWithCup.png)|![Cell 3](CSharpMath.Rendering.Tests/Display/Color.png)
-|-|-
![Cell 4](CSharpMath.Rendering.Tests/Display/RaiseBox.png)|![Cell 5](CSharpMath.Rendering.Tests/Display/SomeLimit.png)|![Cell 6](CSharpMath.Rendering.Tests/Display/VectorProjection.png)
![Cell 7](CSharpMath.Rendering.Tests/Display/Matrix3.png)|![Cell 8](CSharpMath.Rendering.Tests/Display/IntegralColorBoxCorrect.png)|![Cell 9](CSharpMath.Rendering.Tests/Display/Taylor.png)


## This looks great and all, but is there a way to edit and evaluate the math?
Yes! You can use a `CSharpMath.Rendering.FrontEnd.MathKeyboard` to process key presses and generate a `CSharpMath.Atom.MathList` or a LaTeX string. You can then call `CSharpMath.Evaluation.MathListToEntity` to get an `AngouriMath.Entity` that you can simplify. For all uses of an `AngouriMath.Entity`, check out https://github.com/asc-community/AngouriMath.

NOTE: `CSharpMath.Evaluation` is not released yet. It will be part of the 0.5.0 update.
```cs
var keyboard = new CSharpMath.Rendering.FrontEnd.MathKeyboard();
keyboard.KeyPress(CSharpMath.Editor.MathKeyboardInput.Sine, CSharpMath.Editor.MathKeyboardInput.SmallTheta);
CSharpMath.Evaluation.MathListToEntity(keyboard.MathList)
  .Match(entity => {
    var resultLaTeX = CSharpMath.Evaluation.MathListFromEntity(entity.Simplify());
  }, error => { /* Handle invalid math */ });
```
or more conveniently:
```cs
var keyboard = new CSharpMath.Rendering.FrontEnd.MathKeyboard();
keyboard.KeyPress(CSharpMath.Editor.MathKeyboardInput.Sine, CSharpMath.Editor.MathKeyboardInput.SmallTheta);
// Displays errors as red text, also automatically chooses between
// simplifying an expression and solving an equation depending on the presence of an equals sign
var resultLaTeX = CSharpMath.Evaluation.Interpret(keyboard.MathList);
```

Simplifying an expression | Solving an equation
-|-
![Simplifying an expression](https://user-images.githubusercontent.com/19922066/80185029-3e53e000-863e-11ea-8bea-8b7f96ab2837.jpeg)|![Solving an equation](https://user-images.githubusercontent.com/19922066/80185543-15801a80-863f-11ea-9514-a59072c180b4.jpeg)

# [Documentation](https://github.com/verybadcat/CSharpMath/wiki/Documentation-of-public-facing-APIs-of-CSharpMath.Rendering,-CSharpMath.SkiaSharp-and-CSharpMath.Forms-MathViews)

# Project structure

<!--
https://en.wikipedia.org/wiki/Box-drawing_character
┌─┬┐  ╔═╦╗  ╓─╥╖  ╒═╤╕
│ ││  ║ ║║  ║ ║║  │ ││
├─┼┤  ╠═╬╣  ╟─╫╢  ╞═╪╡
└─┴┘  ╚═╩╝  ╙─╨╜  ╘═╧╛
-->
```
<_Core>
CSharpMath
├── CSharpMath.CoreTests
│   <iOS>
├── CSharpMath.Apple
│   └── CSharpMath.Ios
│       └── CSharpMath.Ios.Example
└── CSharpMath.Editor
    ├── CSharpMath.Editor.Tests
    │   └── CSharpMath.Editor.Tests.Visualizer
    ├── CSharpMath.Editor.Tests.FSharp
    ├── CSharpMath.Evaluation ───────────────────────┐
    │   └── CSharpMath.Evaluation.Tests              │
    └── CSharpMath.Rendering                         │
        ├── CSharpMath.Rendering.Text.Tests          │
        │   <Avalonia>                               │
        ├── CSharpMath.Avalonia ─────────────────────────┬───┐
        │   └── CSharpMath.Avalonia.Example          │   │   │
        │   <SkiaSharp>                              │   │   │
        └── CSharpMath.SkiaSharp ────────────────────────┤   │
            │   <Xamarin.Forms>                      │   │   │
            └── CSharpMath.Forms ─────────────────────── │ ──┤
                └── CSharpMath.Forms.Example ────────┘   │   │
                    ├── CSharpMath.Forms.Example.Android │   │
                    ├── CSharpMath.Forms.Example.iOS     │   │
                    ├── CSharpMath.Forms.Example.Ooui    │   │
                    ├── CSharpMath.Forms.Example.WPF     │   │
                    └── CSharpMath.Forms.Example.UWP     │   │
                            CSharpMath.Rendering.Tests ──┘   │
                                     CSharpMath.Xaml.Tests ──┘
```

# Extending to more platforms

There are a few ways to extend this to more platforms:
(Hopefully, you would not need to touch the core typesetting engine. If you do, we would consider that a bug.)

### 1. Branching off from CSharpMath.Rendering (recommended)

As CSharpMath.Rendering provides font lookup through [the Typography library](https://github.com/LayoutFarm/Typography), you would only need to write adapter classes to connect this library to your chosen graphics library.

You would have to implement [ICanvas](CSharpMath.Rendering/FrontEnd/ICanvas.cs) and feed it into the Draw method of [MathPainter](CSharpMath.Rendering/FrontEnd/MathPainter.cs).

### 2. Forking from CSharpMath the project

This path would require the most effort to implement, but allows you to plug in any font library and graphics library.

You would have to define your own [TypesettingContext](CSharpMath/Display/FrontEnd/TypesettingContext.cs) and write an implementation of [IGraphicsContext](CSharpMath/Display/FrontEnd/IGraphicsContext.cs).

The TypesettingContext in turn has several components, including choosing a font.

### 3. Referencing CSharpMath.SkiaSharp

You can extend this library to other SkiaSharp-supported platforms by feeding the SKCanvas given in the OnPaintSurface override of a SkiaSharp view into the Draw method of [MathPainter](CSharpMath.SkiaSharp/MathPainter.cs).

### 4. Building on top of CSharpMath.Apple

You can use this library on other appleOSes by making use of [AppleMathView](CSharpMath.Apple/AppleMathView.cs).
      
# Project needs

We need more contributors! Maybe you can contribute something to this repository. Whether they are bug reports, feature proposals or pull requests, you are welcome to send them to us. We are sure that we will take a look at them!

Here is an idea list if you cannot think of anything right now:
- A new example for the Example projects (please open pull requests straight away)
- A new LaTeX command (please link documentation of it)
- A new front end (please describe what it is and why should it be supported)
- A new math syntax (please describe what it is and why should it be supported)

# License

CSharpMath is licensed by [the MIT license](LICENSE).

Dependency|Used by|License
-|-|-
[Typography project](https://github.com/LayoutFarm/Typography)|CSharpMath.Rendering|[MIT](https://github.com/LayoutFarm/Typography/blob/master/LICENSE.md)
[AngouriMath project](https://github.com/asc-community/AngouriMath)|CSharpMath.Evaluation|[MIT](https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md)
[Latin Modern Math font](http://www.gust.org.pl/projects/e-foundry/lm-math)|CSharpMath.Ios, CSharpMath.Rendering|[GUST Font License](http://www.gust.org.pl/projects/e-foundry/licenses/GUST-FONT-LICENSE.txt/view)
[Cyrillic Modern font](https://sourceforge.net/projects/cyrillic-modern/)|CSharpMath.Rendering|[SIL Open Font License](https://ctan.org/license/ofl)
[AMS Capital Blackboard Bold font](https://github.com/Happypig375/AMSFonts-Ttf-Otf) (extracted by @Happypig375 from [the amsfonts package](https://ctan.org/pkg/amsfonts))|CSharpMath.Rendering|[SIL Open Font License](https://ctan.org/license/ofl)
[ComicNeue font](http://comicneue.com)|CSharpMath.Rendering.Tests, CSharpMath.Xaml.Tests|[SIL Open Font License](http://scripts.sil.org/OFL)

# Authors

[@verybadcat](https://github.com/verybadcat)

[@Happypig375](https://github.com/Happypig375)

[@charlesroddie](https://github.com/charlesroddie)

[@FoggyFinder](https://github.com/FoggyFinder)

Thanks for reading.
<!--
The roadmap isn't happening bois

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
-->
