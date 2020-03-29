using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Playground.TypographyTest {
  static class Get {
    public static List<int> Codepoints(string str) =>
      Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray()).ToList();
  }
}
