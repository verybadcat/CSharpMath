using System;
using System.Text;

namespace CSharpMath {
  public static class MathAtomTypeExtensions {
    public static string ToText(this MathAtomType itemType) {
      // Insert a space before every capital letter other than the first one.
      var itemString = itemType.ToString();
      var chars = new StringBuilder(itemString);
      for (int i = itemString.Length - 1; i > 0; i--) {
        if (char.IsUpper(chars[i])) {
          chars.Insert(i, ' ');
        }
      }
      return chars.ToString();
    }
  }
}
