using System;
using CSharpMath.Atom;
using Xunit;

namespace CSharpMath.Core.AtomTests {
  public class DictionaryTests {
    [Theory]
    [InlineData(@"\sin'", 4)]
    [InlineData(@"\mu=", 3)]
    [InlineData(@"\Delta=", 6)]
    [InlineData(@"\@foo=", 5)]
    [InlineData(@"\=x", 2)]
    [InlineData(@"\= x", 2)]
    [InlineData(@"\sin*", 4)]
    public void CommandLookupUsesTeXControlSequenceBoundary(string input, int expectedSplitIndex) {
      var dictionary = new LaTeXCommandDictionary<int>(
        consume => Result.Err("default parser: " + consume.ToString()),
        consume => Result.Err("default command parser: " + consume.ToString())) {
        { @"\sin", 1 },
        { @"\mu", 2 },
        { @"\Delta", 3 },
        { @"\@foo", 4 },
        { @"\=", 5 }
      };

      var ((result, splitIndex), error) = dictionary.TryLookup(input.AsSpan());
      Assert.Null(error);
      Assert.Equal(expectedSplitIndex, splitIndex);
      Assert.NotEqual(0, result);
    }

    [Fact]
    public void CommandLookupConsumesWhitespaceAfterControlWord() {
      var dictionary = new LaTeXCommandDictionary<int>(
        _ => Result.Err("default parser"), _ => Result.Err("default command parser")) {
        { @"\sin", 1 }
      };

      var ((result, splitIndex), error) = dictionary.TryLookup((@"\sin   x").AsSpan());
      Assert.Null(error);
      Assert.Equal(7, splitIndex);
      Assert.Equal(1, result);
    }

    private AliasBiDictionary<string, int> InitTestDict() =>
      new AliasBiDictionary<string, int> {
        { "0", 0 },
        { "zero", 0 },
        { "1", 1 }
      };
    [Theory]
    [InlineData("0", 2, 2, true)]
    [InlineData("zero", 2, 2, true)]
    [InlineData("1", 2, 1, true)]
    [InlineData("2", 3, 2, false)]
    public void TestRemoveByFirst(string remove, int expectedFTS, int expectedSTF, bool expectedRemoved) {
      var bd = InitTestDict();
      var removed = bd.RemoveByFirst(remove);
      Assert.Equal(expectedFTS, bd.FirstToSecond.Count);
      Assert.Equal(expectedSTF, bd.SecondToFirst.Count);
      Assert.Equal(expectedRemoved, removed);
    }
    [Theory]
    [InlineData(0, 1, 1, true)]
    [InlineData(1, 2, 1, true)]
    [InlineData(2, 3, 2, false)]
    public void TestRemoveBySecond(int remove, int expectedFTS, int expectedSTF, bool expectedRemoved) {
      var bd = InitTestDict();
      var removed = bd.RemoveBySecond(remove);
      Assert.Equal(expectedFTS, bd.FirstToSecond.Count);
      Assert.Equal(expectedSTF, bd.SecondToFirst.Count);
      Assert.Equal(expectedRemoved, removed);
    }
  }
}
