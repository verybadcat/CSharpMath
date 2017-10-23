using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IScripts {
    IMathList Superscript { get; set; }
    IMathList Subscript { get; set; }
  }
}
