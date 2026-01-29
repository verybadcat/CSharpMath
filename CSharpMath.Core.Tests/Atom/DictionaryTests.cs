using CSharpMath.Atom;
using Xunit;

namespace CSharpMath.Core.Tests {
  public class DictionaryTests {
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