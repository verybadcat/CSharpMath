using System;
namespace CSharpMath.Editor {
  public interface ITextView {
    void Insert(int position, char value);
    void Insert(int position, string value);
    void Remove(int position);
  }
}