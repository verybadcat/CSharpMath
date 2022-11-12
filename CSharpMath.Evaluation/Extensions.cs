
using AngouriMath;
using CSharpMath.Atom;
namespace CSharpMath {

  public static class Extensions {
    public static Entity ToEntity(this MathList self) {
      var mathstring = Evaluation.ConvertToMathString(self);
      return MathS.FromString(mathstring);
    }
  }
}
