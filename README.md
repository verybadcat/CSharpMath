# CSharpMath
This is a C# port of the wonderful iosMath LaTeX engine.

It is now working in a lot of cases. Some examples are below. Ironically enough, the first front end is iOS. However, if you want to add a front end, it should be possible. You would have to define your own TypesettingContext and write an implementation of IGraphicsContext. The TypesettingContext in turn has several components, including choosing a font. Hopefully, you would not need to touch the core typesetting engine. (If you do, I would consider that a bug.)

![Quadratic Formula](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/Quadratic%20Formula.png)

![Power Series](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/PowerSeries.png)

![Matrix Product](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/MatrixProduct.png)

![Continued Fraction](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/ContinuedFraction.png)

The latin-modern-math font is licenced by the [GUST Font License](./fonts/GUST-FONT-LICENSE.txt)
