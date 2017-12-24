using System;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms
{
  public class MathListBuildResult
  {
    public MathListBuildResult(IMathList builtList, string error) {
      MathList = builtList;
      Error = error;
    }
    public IMathList MathList { get; set; }
    public string Error { get; set; }
  }
}
