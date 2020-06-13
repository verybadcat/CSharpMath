using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  partial class Extensions {
    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public static T PeekOrDefault<T>(this Stack<T> stack) => stack.Count == 0 ? default : stack.Peek();
  }
}
