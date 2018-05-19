using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.DevUtils.TypographyTest {
  static class Get {
    public static List<int> Codepoints(string str) {
      return Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray()).ToList();
    }
  }
}
