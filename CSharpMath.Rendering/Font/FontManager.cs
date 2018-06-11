using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public static class FontManager {
    static FontManager() {
      var bytes = Resources.Otf;
      var reader = new OpenFontReader();
      var latinMathTypeface = reader.Read(new MemoryStream(bytes, false));
      latinMathTypeface.UpdateAllCffGlyphBounds();
      GlobalTypefaces = new Typefaces(latinMathTypeface);
    }

    public static Typefaces GlobalTypefaces { get; }
  }
}
