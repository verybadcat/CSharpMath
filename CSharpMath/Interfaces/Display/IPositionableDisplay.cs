using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public interface IPositionableDisplay<TGlyph> : IDisplay<TGlyph>, ISettablePosition {
  }
}
