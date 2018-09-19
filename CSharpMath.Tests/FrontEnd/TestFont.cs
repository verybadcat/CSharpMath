using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Enumerations;

namespace CSharpMath.Tests.FrontEnd {
  public struct TestMathFont : Display.IMathFont<char> {
    public TestMathFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}
