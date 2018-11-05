using System;
namespace CSharpMath.Editor {
  public interface IButton {
    string Text { get; set; }
    bool Enabled { get; set; }
    bool Selected { get; set; }
    bool Visible { get; set; }
  }
}