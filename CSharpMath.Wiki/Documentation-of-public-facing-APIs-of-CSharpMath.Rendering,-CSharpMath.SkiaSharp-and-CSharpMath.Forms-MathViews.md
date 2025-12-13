### Version: 0.1.0

# CSharpMath.Rendering
Full name: `CSharpMath.Rendering.MathPainter``2`

Definitions:
```cs
public abstract class Painter<TCanvas, TSource, TColor> : ICanvasPainter<TCanvas, TSource, TColor> where TSource : struct, ISource
public abstract class MathPainter<TCanvas, TColor> : Painter<TCanvas, MathSource, TColor>
```

Color: `TColor`

# CSharpMath.SkiaSharp
Full name: `CSharpMath.SkiaSharp.MathPainter`

Definition: 
```cs
public class MathPainter : MathPainter<SKCanvas, SKColor>, ICanvasPainter<SKCanvas, MathSource, SKColor>
```

Color: `SkiaSharp.SKColor`

# CSharpMath.Forms
Full name: `CSharpMath.Forms.MathView`

Definition: 
```cs
[XamlCompilation(XamlCompilationOptions.Compile), ContentProperty(nameof(LaTeX))]
public class MathView : BaseView<MathPainter, MathSource>, IPainter<MathSource, Color>
```

Color: `Xamarin.Forms.Color`

# Common APIs

Content
-------
### Source
Type: `CSharpMath.Rendering.MathSource`

Settable? :heavy_check_mark:

Default: `new MathSource()`

The source of content to display. If it remains as the default, then nothing will be displayed.

Notes: `MathSource` is a `readonly struct` and should not be mutated even though you can do so with its `MathList` property as that list will not be regenerated after the creation of the `MathSource`. You can cache `MathSource`s for reuse so LaTeX parsing need not be done every time.

Since version: 0.1.0
### LaTeX
Type: `System.String`

Settable? :heavy_check_mark:

Default: `null`

The LaTeX to be displayed. Will set the `ErrorMessage` property if the set value contains a syntax error. Do not set this to `null`. Basically a convenience property to `Source.LaTeX`.

Since version: 0.1.0
### MathList
Type: `CSharpMath.Interfaces.IMathList`

Settable? :heavy_check_mark:

Default: `null`

The list of math atoms to be displayed. Do not set this to `null`. Bascially a convenience property to `Source.MathList`. Please do not mutate this property with the same rationale behind the one in `Source`.

Since version: 0.1.0

Shapes
------
### LocalTypefaces
Type: `System.Collections.Generic.List<Typogaphy.OpenFont.Typeface>`

Settable? :heavy_check_mark:

Default: `new List<Typeface>()`

The list of typefaces that are local to this instance. If glyphs from different typefaces in this list are of the same Unicode codepoint, the glyph from the first typeface from the start of the list shadows later typefaces. Glyphs from typefaces from this list also shadows those from the global typeface list (`CSharpMath.Rendering.MathFonts.GlobalTypefaces`).

Notes: The global typeface list allows indices from -128 to 127, where 0 is the default typeface (Latin Modern Math). When the glyph finder searches the list, it scans from index -128 to 127 sequentially, returning if found. (The previous sentence describes the same thing with local typefaces.) The default typeface cannot be changed without using reflection or recompilation or memory edits, silently failing on all attempts to mutate the zeroth item of the global typeface list, so as to always keep at least one typeface loaded at all times.

Since version: 0.1.0
### LineStyle
Type: `CSharpMath.Enumerations.LineStyle`

Settable? :heavy_check_mark:

Default: `LineStyle.Display`

Commonly you would want to set this to either `LineStyle.Display` or `LineStyle.Text` for display mode or text mode respectively (aka display mode maths and inline mode maths respectively). May be overridden by the `\displaystyle` and `\textstyle` LaTeX commands. You could set this to `LineStyle.Script` or `LineStyle.ScriptScript` to set everything to be scripted (not superscript nor subscript, but rather 'mid'-script, so everything is like extra-small), to find bugs that otherwise would only occur in superscript or subscript context. Thanks in advance for reporting them.

Since version: 0.1.0
### FontSize
Type: `System.Float`

Settable? :heavy_check_mark:

Default: `20`

The font size in points to display the content in.

Since version: 0.1.0

Drawing
-------
### BackgroundColor
Type: Color (see above)

Settable? :heavy_check_mark:

Default: Transparent (#00000000)

The background color displayed behind the entire drawing area.

Since version: 0.1.0
### TextColor
Type: Color (see above)

Settable? :heavy_check_mark:

Default: Black (#000000)

The color to display content in. May be overridden by the `\color` LaTeX command.

Since version: 0.1.0
### PaintStyle
Type: `CSharpMath.Rendering.PaintStyle`

Settable? :heavy_check_mark:

Default: `PaintStyle.Fill`

Whether to only draw the outline of the content or also fill the content. Normally you would want to use `PaintStyle.Fill`, but you can also create great neon styles with `PaintStyle.Stroke`.

Since version: 0.1.0

### GlyphBoxColor

Spacing
-------
Bounds, Padding, TextAlignment, DrawSize

Error handling
--------------
### ErrorMessage
Type: `System.String`

Settable? :x:

Default: `null`

The error message if the previously set LaTeX contains a syntax error, and `null` if not. Basically a convenience property to `Source.Error`.

Since version: 0.1.0
### DisplayErrorInline
Type: `System.Boolean`

Settable: :heavy_check_mark:

Default: `true`

Whether to display `ErrorMessage` in place of ordinary content if the previously set LaTeX contains a syntax error. If not, then the content will not change from the previously correctly parsed content.

Since version: 0.1.0
### ErrorColor
Type: Color (see above)

Settable: :heavy_check_mark:

Default: Red (#FF0000)

The color to display `ErrorMessage` in when it is displayed. Only relevant if `DisplayErrorInline` is `true`.

Since version: 0.1.0
### ErrorFontSize
Type: `System.Nullable<System.Float>`

Settable: :heavy_check_mark:

Default: `null`

The font size in points to display `ErrorMessage` in. If set to `null`, then `FontSize` will be used. Only relevant if `DisplayErrorInline` is `true`.

Since version: 0.1.0

Gestures
--------
ScrollX, ScrollY, Magnification