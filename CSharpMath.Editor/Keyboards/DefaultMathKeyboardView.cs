using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Editor {
  using Color = Structures.Color;
  internal static class TButtonExtensions {
    public static TButton RegisterPressed<TButton>(this TButton button, Action<TButton, EventHandler> addToPressed, Action handler) {
      addToPressed(button, delegate { handler(); });
      return button;
    }
  }
  partial class MathKeyboardView<TButton, TTextView> {
    public delegate TButton ButtonCtor(RectangleF frame, string text, float textPtSize, Color titleColor, Color titleShadowColor, Color highlightedTitleColor);
    public static readonly Color DefaultTitle = default;
    public static readonly Color DefaultShadow = new Color(0.5f, 0.5f, 0.5f);
    public static readonly Color ToBeReplacedWithImage = new Color(0.2f, 0.2f, 0.2f);


    public static MathKeyboardView<TButton, TTextView> Default(ButtonCtor ctor, Action<TButton, EventHandler> addToPressed) =>
      new MathKeyboardView<TButton, TTextView>(
        ctor(
          new RectangleF(0, 90, 49.5f, 45),
#warning Replace with image
          "Fraction",
          18,
          DefaultTitle,
          DefaultShadow,
          ToBeReplacedWithImage
        ).RegisterPressed(addToPressed, 
      );
  }
}
