using Xunit;

namespace CSharpMath.Tests.Generic {
  public class BiDictionaryTests {
    [Fact]
    public void TestRemove() {
      var testBiDictionary = new BiDictionary<int, string> {
        { 0, "0" },
        { 1, "1" },
        { 2, "8" },
        { 3, "10" }
      };
      Assert.Equal(4, testBiDictionary.Firsts.Count);
      Assert.Equal(4, testBiDictionary.Seconds.Count);

      Assert.True(testBiDictionary.Remove(2, "8"));
      Assert.False(testBiDictionary.ContainsByFirst(2));
      Assert.False(testBiDictionary.ContainsBySecond("8"));
      Assert.Equal(3, testBiDictionary.Firsts.Count);
      Assert.Equal(3, testBiDictionary.Seconds.Count);

      // Remove with wrong first key
      Assert.False(testBiDictionary.Remove(4, "10"));
      Assert.False(testBiDictionary.ContainsByFirst(4));
      Assert.True(testBiDictionary.ContainsBySecond("10"));
      Assert.Equal(3, testBiDictionary.Firsts.Count);
      Assert.Equal(3, testBiDictionary.Seconds.Count);

      // Remove with wrong second key
      Assert.False(testBiDictionary.Remove(3, "15"));
      Assert.True(testBiDictionary.ContainsByFirst(3));
      Assert.False(testBiDictionary.ContainsBySecond("15"));
      Assert.Equal(3, testBiDictionary.Firsts.Count);
      Assert.Equal(3, testBiDictionary.Seconds.Count);

      // Remove when both exists but not corresponding to each other
      Assert.True(testBiDictionary.Remove(0, "1"));
      Assert.False(testBiDictionary.ContainsByFirst(0));
      Assert.False(testBiDictionary.ContainsBySecond("1"));
      Assert.Single(testBiDictionary.Firsts);
      Assert.Single(testBiDictionary.Seconds);
    }
  }
}
