using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public interface IPath : Typography.OpenFont.IGlyphTranslator {
    void AddRect(float left, float top, float width, float height);
  }
}