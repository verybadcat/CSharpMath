using Xunit;

namespace CSharpMath.CoreTests {
  public class DictionaryTests {
    [Fact]
    public void TestRemove() {
      var testBiDictionary = new Structures.BiDictionary<int, string> {
        { 0, "0" },
        { 1, "1" },
        { 2, "8" },
        { 3, "10" }
      };
      Assert.Equal(4, testBiDictionary.FirstToSecond.Count);
      Assert.Equal(4, testBiDictionary.SecondToFirst.Count);
    }
  }
}
