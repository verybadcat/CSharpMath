using System;
using System.Linq;
using Xunit;

namespace CSharpMath.Rendering.Text.Tests {
  public class TextLaTeXParserTests {
    TextAtom Parse(string latex) =>
      TextLaTeXParser.TextAtomFromLaTeX(latex)
      .Match(atom => atom, error => throw new Xunit.Sdk.XunitException(error));
    [Theory]
    [InlineData("123")]
    [InlineData("123.456")]
    [InlineData("abc")]
    [InlineData("α", "β", "γ")]
    [InlineData("中", "文")]
    [InlineData("😀", "Text")]
    [InlineData("Chinese", "中", "文")]
    [InlineData("Chinese", "中", "12345", "文", "😄")]
    public void Text(params string[] text) {
      var input = string.Concat(text);
      var atom = Parse(input);
      Assert.Equal(
        text.Length == 1
        ? (TextAtom)new TextAtom.Text(text[0])
        : new TextAtom.List(text.Select(t => new TextAtom.Text(t)).ToArray()), atom);
      Assert.Equal(input, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }
  }
}
