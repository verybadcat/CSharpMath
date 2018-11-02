using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Editor {
  using Color = Structures.Color;
  partial class MathKeyboardView<TButton> {
    public delegate TButton ButtonCtor(RectangleF frame, string text, float textPtSize, Color title, Color titleShadow, Color highlightedTitle, Color? disabled);
    public static readonly Color DefaultTitle = new Color(0f, 0f, 0f);
    public static readonly Color DefaultShadow = new Color(0.5f, 0.5f, 0.5f);
    public static readonly Color ToBeReplacedWithImage = new Color(0.2f, 0.2f, 0.2f);
    public static readonly Color DefaultHighlight = new Color(1f, 1f, 1f);
    public static readonly Color DefaultDisabled = new Color(2f / 3f, 2f / 3f, 2f / 3f);
    public const float DefaultFontSize = 20;
    
    public static MathKeyboardView<TButton> Default(ButtonCtor ctor, Action<TButton, EventHandler> registerPressed) {
      var text = new StringBuilder();
      var textPosition = new Box<int>();
      return new MathKeyboardView<TButton>(
        new MathKeyboard<TButton>(
          registerPressed,
#warning Replace "Fraction" with image
            ctor(new RectangleF(0, 90, 49.5f, 45), "Fraction", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null),
            ctor(new RectangleF(199, 45, 49, 45), Constants.Symbols.Multiplication.ToString(), DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            ctor(new RectangleF(149, 135, 50, 45), "=", 25, DefaultTitle, DefaultShadow, DefaultHighlight, DefaultDisabled),
            ctor(new RectangleF(199, 0, 49, 45), Constants.Symbols.Division.ToString(), DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
#warning Replace "Exponent" with image
            ctor(new RectangleF(0, 135, 50, 45), "Exponent", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null),
            null,
            null,
            null,
            text,
            textPosition,
            new[] {
              ctor(new RectangleF(50, 135, 49, 45), "0", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(50, 90, 49, 45), "1", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(99.5f, 90, 49.5f, 45), "2", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(149, 90, 50, 45), "3", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(50, 45, 49, 45), "4", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(99.5f, 45, 49.5f, 45), "5", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(149, 45, 50, 45), "6", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(50, 0, 49, 45), "7", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(99.5f, 0, 49.5f, 45), "8", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(149, 0, 50, 45), "9", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            },
            new[] {
              ctor(new RectangleF(0, 0, 49.5f, 45), "x", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(0, 45, 49.5f, 45), "y", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            },
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            ctor(new RectangleF(248.5f, 0, 71.5f, 45), "Backspace", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            ctor(new RectangleF(248.5f, 135, 71.5f, 45), "Dismiss", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            ctor(new RectangleF(248.5f, 45, 71.5f, 90), "Enter", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null)
        ),
        new MathKeyboard<TButton>(
          registerPressed,
            ctor(new RectangleF(0, 90, 50, 45), "Fraction", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null),
            null,
            null,
            null,
#warning Replace "Exponent" with image
            ctor(new RectangleF(0, 135, 50, 45), "Exponent", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null),
            null,
            null,
            null,
            text,
            textPosition,
            null,
            new[] {
              ctor(new RectangleF(0, 0, 50, 45), "x", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(0, 45, 50, 45), "y", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            },
            new[] {
              ctor(new RectangleF(50, 90, 50, 45), "{", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(150, 90, 49, 45), $"|{Constants.Symbols.WhiteSquare}|", 24, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(100, 45, 50, 45), "]", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(150, 45, 49, 45), Constants.Symbols.LessEqual.ToString(), DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(50, 0, 50, 45), "(", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(100, 0, 50, 45), ")", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(150, 0, 49, 45), "<", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(100, 135, 50, 45), Constants.Symbols.Infinity.ToString(), DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(50, 135, 50, 45), "!", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(199, 90, 50, 45), "%", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(199, 45, 50, 45), Constants.Symbols.GreaterEqual.ToString(), DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(199, 0, 50, 45), ">", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(150, 135, 49, 45), ":", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(199, 135, 50, 45), ",", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(50, 45, 50, 45), "[", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
              ctor(new RectangleF(100, 90, 50, 45), "}", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null)
            },
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            ctor(new RectangleF(249, 0, 71, 45), "Back", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null),
            ctor(new RectangleF(249, 135, 71, 45), "Dismiss", DefaultFontSize, DefaultTitle, DefaultShadow, DefaultHighlight, null),
            ctor(new RectangleF(249, 45, 71, 90), "Enter", 18, ToBeReplacedWithImage, DefaultShadow, ToBeReplacedWithImage, null)
      ),
      new MathKeyboard<TButton>(

      ));
    }
  }
}
