using System;
using System.Linq;
using Xunit;
namespace CSharpMath {
  public partial class EvaluationTests {
    public class PredictTests {
      [Theory]
      [InlineData(1, 1, 1)]
      [InlineData(2, 2, 2)]
      [InlineData(1, 2, 3, 4)]
      [InlineData(3, 4, 5, 6)]
      [InlineData(1, 2, 3, 4, 5)]
      [InlineData(3, 4, 5, 6, 7)]
      [InlineData(1, 3, 5, 7)]
      [InlineData(8, 10, 12, 14)]
      [InlineData(1, 3, 5, 7, 9)]
      [InlineData(8, 10, 12, 14, 16)]
      [InlineData(1, 4, 7, 10)]
      [InlineData(1, 4, 7, 10, 13)]
      [InlineData(24, 31, 38, 45)]
      [InlineData(24, 31, 38, 45, 52)]
      [InlineData(17, 21, 25, 29)]
      [InlineData(17, 21, 25, 29, 33)]
      [InlineData(1, 1, 1, 1)]
      [InlineData(1, 1, 1, 1, 1)]
      [InlineData(1, 3, 1, 3, 1)]
      [InlineData(1, 3, 1, 3, 1, 3)]
      [InlineData(1, 2, 4, 8)]
      [InlineData(1, 2, 4, 8, 16)]
      [InlineData(1, 3, 9, 27)]
      [InlineData(1, 3, 9, 27, 81)]
      [InlineData(1, -2, 4, -8)]
      [InlineData(-1, 2, -4, 8)]
      [InlineData(30, 31, 40, 41, 50, 51)]
      [InlineData(61, 62, 63, 71, 72, 73, 81, 82)]
      [InlineData(2, 4, 8, 24, 48, 96, 288, 576, 1152)]
      [InlineData(0, 0, 0, 1, 0, 0, 0, 1, 0)]
      [InlineData(39, 34, 27, 22, 15, 10)]
      [InlineData(39, 34, 27, 22, 15, 10, 3)]
      [InlineData(31, 30, 22, 21, 13, 12)]
      [InlineData(31, 30, 22, 21, 13, 12, 4)]
      // Use int instead of double because https://github.com/xunit/xunit/issues/1670#issuecomment-373566797
      public void Integer(params int[] input) {
        static void Test(System.Collections.Generic.IEnumerable<int> seq) =>
          Assert.Equal(seq.Last(), Assert.IsType<double>(Evaluation.Predict(seq.Select(x => (double)x).SkipLast(1).ToArray())), 12);
        Test(input);
        Test(input.Reverse());
      }
      [Theory]
      [InlineData(new int[0])]
      [InlineData(1)]
      [InlineData(1, 2)]
      [InlineData(2, 1)]
      [InlineData(1, 2, 1)]
      [InlineData(1, 2, 1, 3)]
      [InlineData(39, 34, 27, 22)]
      [InlineData(31, 30, 22, 21)]
      public void IntegerFail(params int[] input) =>
        Assert.Null(Evaluation.Predict(input.Select(x => (double)x).ToArray()));
    }
  }
}