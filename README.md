# CSharpMath
C# port of the wonderful iosMath LaTeX engine.

This is now working in a lot of cases. Below is an example. Ironically enough, the first front end is iOS. However, if you want to add a front end, it should be possible. You would have to define your own TypesettingContext and write an implementation of IGraphicsContext. Hopefully, you would not need to touch the core typesetting engine. (If you do, I would consider that a bug.)

![Quadratic Formula](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath/RenderedSamples/Quadratic%20Formula.png)
