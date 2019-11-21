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

    [Fact]
    public void TestAddOrReplace() {
      var testBiDictionary = new BiDictionary<int, string>();

      testBiDictionary.AddOrReplace(0, "Value1");
      Assert.Equal("Value1", testBiDictionary[0]);
      Assert.Equal(0, testBiDictionary["Value1"]);

      testBiDictionary.AddOrReplace(2, "Value10");
      Assert.Equal("Value10", testBiDictionary[2]);
      Assert.Equal(2, testBiDictionary["Value10"]);

      testBiDictionary.AddOrReplace(2, "Value2");
      Assert.Equal("Value2", testBiDictionary[2]);
      Assert.Equal(2, testBiDictionary["Value2"]);
      Assert.Equal(2, testBiDictionary.Firsts.Count);
      Assert.Equal(2, testBiDictionary.Seconds.Count);

      testBiDictionary.AddOrReplace(3, "Value3");
      Assert.Equal("Value3", testBiDictionary[3]);
      Assert.Equal(3, testBiDictionary["Value3"]);

      testBiDictionary.AddOrReplace(10, "Value3");
      Assert.Equal("Value3", testBiDictionary[10]);
      Assert.Equal(10, testBiDictionary["Value3"]);
      Assert.Equal(3, testBiDictionary.Firsts.Count);
      Assert.Equal(3, testBiDictionary.Seconds.Count);
    }
  }
}
