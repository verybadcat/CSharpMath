using System;
using System.Linq;
using Xunit;
namespace CSharpMath {
  public partial class EvaluationTests {
    public class PredictTests {
      [Theory]
      [InlineData(1, 2, 3, 4)]
      [InlineData(1, 2, 3, 4, 5)]
      [InlineData(1, 3, 5, 7)]
      [InlineData(1, 3, 5, 7, 9)]
      [InlineData(1, 4, 7, 10)]
      [InlineData(1, 4, 7, 10, 13)]
      [InlineData(4, 3, 2, 1)]
      [InlineData(5, 4, 3, 2, 1)]
      [InlineData(7, 5, 3, 1)]
      [InlineData(9, 7, 5, 3, 1)]
      [InlineData(10, 7, 4, 1)]
      [InlineData(13, 10, 7, 4, 1)]
      [InlineData(1, 1, 1, 1)]
      [InlineData(1, 1, 1, 1, 1)]
      [InlineData(1, 3, 1, 3, 1)]
      [InlineData(1, 3, 1, 3, 1, 3)]
      [InlineData(1, 2, 4, 8)]
      [InlineData(1, 2, 4, 8, 16)]
      [InlineData(8, 4, 2, 1)]
      [InlineData(16, 8, 4, 2, 1)]
      // Use AngouriMath.Core.Number instead of double because https://github.com/xunit/xunit/issues/1670#issuecomment-373566797
      public void Integer(params int[] input) =>
        Assert.Equal(input[^1], Evaluation.Predict(input.Select(x => (double)x).SkipLast(1).ToArray()));
      [Theory]
      [InlineData(new int[0])]
      [InlineData(1)]
      [InlineData(1, 2)]
      [InlineData(2, 2)]
      [InlineData(1, 2, 1)]
      [InlineData(1, 2, 1, 3)]
      [InlineData(1, 1, 2)]
      public void IntegerFail(params int[] input) =>
        Assert.Equal(null, Evaluation.Predict(input.Select(x => (double)x).ToArray()));
    }
  }
}