Additional front ends are highly desired. Creating one would involve writing an implementation of the various files in the FrontEnd folder.

https://github.com/verybadcat/CSharpMath/tree/master/CSharpMath/FrontEnd

Primarily, this means a TypesettingContext object that the core will use when it needs to know how much space to allow for something, and a GraphicsContext object it will use for the actual drawing.

You will also eventually need a json file to keep track of the spacings around various glyphs. For an example, see https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.Ios/Resources/latinmodern-math.json. This particular file was created by converting the corresponding plist file in the iosMath project. https://github.com/kostub/iosMath
