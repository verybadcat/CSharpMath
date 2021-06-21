using System;

namespace CSharpMath.Forms {
  internal static class StringHelper {
    enum ReplacementOption { First, Last }
    internal static string ReplaceFirstOccurrence(this string source, string subString, string replacement) => source.Replace(subString, replacement, ReplacementOption.First);
    internal static string ReplaceLastOccurrence(this string source, string subString, string replacement) => source.Replace(subString, replacement, ReplacementOption.Last);
    static string Replace(this string source, string subString, string replacement, ReplacementOption replacementOption) {
      int index = replacementOption == ReplacementOption.First ? source.IndexOf(subString) : source.LastIndexOf(subString);
      return index == -1 ? source : source.Remove(index, subString.Length).Insert(index, replacement);
    }
    internal static int SubStringCount(this string text, string substring) =>
      text.Split(new[] { substring }, StringSplitOptions.None).Length - 1;
  }
}

