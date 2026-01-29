using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using BackEnd;
  public class TestCommandDisplay {
    public TestCommandDisplay() =>
      typefaces = Fonts.GlobalTypefaces.ToArray();
    readonly Typography.OpenFont.Typeface[] typefaces;
    public static IEnumerable<object[]> AllCommandValues =>
      Atom.LaTeXSettings.CommandSymbols.SecondToFirst.Keys
      .SelectMany(v => v.Nucleus.EnumerateRunes())
      .Distinct()
      .OrderBy(r => r.Value)
      .Select(rune => new object[] { rune });
    [Theory]
    [MemberData(nameof(AllCommandValues))]
    public void CommandsAreDisplayable(Rune ch) =>
      Assert.Contains(typefaces, font => font.GetGlyphIndex(ch.Value) != 0);
  }
}