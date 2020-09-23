using System.Drawing;
using CSharpMath.Editor;
namespace CSharpMath.Forms.Example.Controls {
  public class MyMathInputButton : MathInputButton {
    protected override string GetButtonDisplayLaTeX(MathKeyboardInput key) {
      switch (key) {
        case MathKeyboardInput.Space: return @"\  space  \ ";
        default: return base.GetButtonDisplayLaTeX(key).Replace(@"\square", "■").Replace("■", @"\color{Gray}■");
      }
    }
  }
}