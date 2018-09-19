using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Display;

namespace CSharpMath {
  public interface IDownshiftableDisplay<TFont, TGlyph>: IDisplay<TFont, TGlyph>, IDownShift 
    where TFont : IMathFont<TGlyph>{
  }
}
