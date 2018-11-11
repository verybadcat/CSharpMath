using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Enumerations;

namespace CSharpMath.Tests.FrontEnd {
  public readonly struct TestFont : Display.IFont<char> {
    public TestFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}
