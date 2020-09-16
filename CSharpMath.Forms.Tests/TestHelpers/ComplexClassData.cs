using System.Collections.Generic;
namespace CSharpMath.Forms.Tests.TestHelpers {
  public abstract class ComplexClassData<T> : IEnumerable<object[]> where T : class {
    public abstract IEnumerable<T> theData { get; }
    IEnumerator<object[]> IEnumerable<object[]>.GetEnumerator() {
      foreach (var item in theData)
        yield return new object[] { item };
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<object[]>)this).GetEnumerator();
  }
}
