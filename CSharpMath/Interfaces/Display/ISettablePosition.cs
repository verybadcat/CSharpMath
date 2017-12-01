using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath {
  public interface ISettablePosition: IPosition {
    void SetPosition(PointF value);
  }
}
