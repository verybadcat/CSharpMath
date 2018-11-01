using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Editor {
  using Color = Structures.Color;
  partial class MathKeyboardView<TButton, TTextView> {
    public delegate TButton ButtonCtor(RectangleF frame, string text, float textPtSize, Color title, Color titleShadow, Color highlightedTitle, Color? disabled);
    public static readonly Color DefaultTitle = new Color(0f, 0f, 0f);
    public static readonly Color DefaultShadow = new Color(0.5f, 0.5f, 0.5f);
    public static readonly Color ToBeReplacedWithImage = new Color(0.2f, 0.2f, 0.2f);
    public static readonly Color DefaultHighlight = new Color(1f, 1f, 1f);
    public static readonly Color DefaultDisabled = new Color(2f / 3f, 2f / 3f, 2f / 3f);


    public static MathKeyboardView<TButton, TTextView> Default(ButtonCtor ctor, Action<TButton, EventHandler> registerPressed) {
      var text = new StringBuilder();
      var textPosition = new Box<int>();
      return new MathKeyboardView<TButton, TTextView>(
        new MathKeyboard<TButton>(
          registerPressed,
#warning Replace "Fraction" with image
          ctor(new RectangleF(0, 90, 49.5f, 45), "Fraction", 18, DefaultTitle, DefaultShadow, ToBeReplacedWithImage, null),
          ctor(new RectangleF(199, 45, 49, 45), Constants.Symbols.Multiplication.ToString(), 20, DefaultTitle, DefaultShadow, DefaultHighlight, null),
          ctor(new RectangleF(149, 135, 50, 45), "=", 25, DefaultTitle, DefaultShadow, DefaultHighlight, DefaultDisabled),
          ctor(new RectangleF(199, 0, 49, 45), Constants.Symbols.Division.ToString(), 20, DefaultTitle, DefaultShadow, DefaultHighlight, null),
#warning Replace "Exponent" with image
          ctor(new RectangleF(0, 135, 50, 45), "Exponent", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null),
          ctor(new RectangleF())
        )
      );
    }
  }
}
