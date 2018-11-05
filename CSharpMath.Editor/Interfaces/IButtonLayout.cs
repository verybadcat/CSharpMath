namespace CSharpMath.Editor {
  public interface IButtonLayout<TButton, TLayout> where TButton : IButton where TLayout : IButtonLayout<TButton, TLayout> {
    void Add(TButton button);
    void Add(TLayout button);
    System.Drawing.RectangleF Bounds { get; set; }
    bool Visible { get; set; }
  }
}
