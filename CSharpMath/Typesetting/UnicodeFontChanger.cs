using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSharpMath.FrontEnd;
using TChar = System.Char;
using TLongChar = System.Int32;

namespace CSharpMath
{
  public class UnicodeFontChanger : IFontChanger
  {

    private const TChar UnicodeGreekLowerStart = '\x03B1';
    private const TChar UnicodeGreekLowerEnd = '\x03C9';
    private const TChar UnicodeGreekUpperStart = '\x0391';
    private const TChar UnicodeGreekUpperEnd = '\x03A9';

    private const TLongChar UnicodeMathCapitalBoldStart = 0x1D400;
    private const TLongChar UnicodeMathLowerBoldStart = 0x1D41A;
    private const TLongChar UnicodeGreekCapitalBoldStart = 0x1D6A8;
    private const TLongChar UnicodeGreekLowerBoldStart = 0x1D6C2;
    private const TLongChar UnicodeGreekSymbolBoldStart = 0x1D6DC;
    private const TLongChar UnicodeNumberBoldStart = 0x1D7CE;

    private const TChar kMTUnicodePlanksConstant = '\x210e';
    private const TLongChar UnicodeMathCapitalItalicStart = 0x1D434;
    private const TLongChar UnicodeMathLowerItalicStart = 0x1D44E;
    private const TLongChar UnicodeGreekCapitalItalicStart = 0x1D6E2;
    private const TLongChar UnicodeGreekLowerItalicStart = 0x1D6FC;
    private const TLongChar UnicodeGreekSymbolItalicStart = 0x1D716;

    private const TLongChar UnicodeMathCapitalBoldItalicStart = 0x1D468;
    private const TLongChar UnicodeMathLowerBoldItalicStart = 0x1D482;
    private const TLongChar UnicodeGreekCapitalBoldItalicStart = 0x1D71C;
    private const TLongChar UnicodeGreekLowerBoldItalicStart = 0x1D736;
    private const TLongChar UnicodeGreekSymbolBoldItalicStart = 0x1D750;


    private const TLongChar UnicodeMathCapitalTTStart = 0x1D670;
    private const TLongChar UnicodeMathLowerTTStart = 0x1D68A;
    private const TLongChar UnicodeNumberTTStart = 0x1D7F6;


    private const TLongChar UnicodeMathCapitalSansSerifStart = 0x1D5A0;
    private const TLongChar UnicodeMathLowerSansSerifStart = 0x1D5BA;
    private const TLongChar UnicodeNumberSansSerifStart = 0x1D7E2;

    private const TLongChar UnicodeMathCapitalFrakturStart = 0x1D504;
    private const TLongChar UnicodeMathLowerFrakturStart = 0x1D51E;

    private const TLongChar UnicodeMathCapitalBlackboardStart = 0x1D538;
    private const TLongChar UnicodeMathLowerBlackboardStart = 0x1D552;
    private const TLongChar UnicodeNumberBlackboardStart = 0x1D7D8;

    private const TLongChar UnicodeMathCapitalScriptStart = 0x1D49C;

    private UnicodeFontChanger()
    {
    }

    public static UnicodeFontChanger Instance { get; } = new UnicodeFontChanger();

    private bool IsLowerEn(TChar c)
      => c >= 'a' && c <= 'z';

    private bool IsUpperEn(TChar c)
      => c >= 'A' && c <= 'Z';

    private bool IsNumber(TChar c)
      => c >= '0' && c <= '9';

    private bool IsLowerGreek(TChar c)
      => c >= UnicodeGreekLowerStart && c <= UnicodeGreekLowerEnd;

    private bool IsUpperGreek(TChar c)
      => c >= UnicodeGreekUpperStart && c <= UnicodeGreekUpperEnd;

    private int GreekSymbolOrder(TChar c)
    {
      // These greek symbols that always appear in unicode in this particular order
      // after the alphabet. 
      // The symbols are epsilon, vartheta, varkappa, phi, varrho, and varpi.
      TChar[] greekSymbols = { '\x03F5', '\x03D1', '\x03F0', '\x03D5', '\x03F1', '\x03D6' };
      return Array.IndexOf(greekSymbols, c);
    }

    private bool IsGreekSymbol(TChar c)
      => GreekSymbolOrder(c) != -1;

    private TLongChar StyleCharacter(char c, FontStyle fontStyle)
    {
      switch (fontStyle)
      {
        case FontStyle.Default:
          return GetDefaultStyle(c);
        case FontStyle.Roman:
          return c;
        case FontStyle.Bold:
          return GetBold(c);
        case FontStyle.Italic:
          return GetItalicized(c);
        case FontStyle.BoldItalic:
          return GetBoldItalic(c);
        case FontStyle.Caligraphic:
          return GetCaligraphic(c);
        case FontStyle.Typewriter:
          return GetTypewriter(c);
        case FontStyle.SansSerif:
          return GetSansSerif(c);
        case FontStyle.Fraktur:
          return GetFraktur(c);
        case FontStyle.Blackboard:
          return GetBlackboard(c);
        default: throw new NotImplementedException("Unknown font style " + fontStyle);
      }
    }

    private TLongChar GetDefaultStyle(TChar c)
    {
      if (IsLowerEn(c) || IsUpperEn(c) || IsLowerGreek(c) || IsGreekSymbol(c))
      {
        return GetItalicized(c);
      }
      if (IsNumber(c) || IsUpperGreek(c))
      {
        return c;
      }
      if (c == '.')
      {
        return c;
      }
      else
      {
        throw new InvalidOperationException(@"Illegal character " + c);
      }
    }

    private TLongChar GetItalicized(TChar c)
    {
      TLongChar r = c;
      if (c == 'h')
      {
        r = kMTUnicodePlanksConstant;
      }
      else if (IsUpperEn(c))
      {
        r = UnicodeMathCapitalItalicStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        r = UnicodeMathLowerItalicStart + (TLongChar)(c - 'a');
      }
      else if (IsLowerGreek(c))
      {
        r = UnicodeGreekLowerItalicStart + (TLongChar)(c - UnicodeGreekLowerStart);
      }
      else if (IsUpperGreek(c))
      {
        r = UnicodeGreekCapitalItalicStart + (TLongChar)(c - UnicodeGreekUpperStart);
      }
      else if (IsGreekSymbol(c))
      {
        r = UnicodeGreekSymbolItalicStart + (TLongChar)GreekSymbolOrder(c);
      }
      return r;
    }

    private TLongChar GetBold(TChar c)
    {
      TLongChar r = c;
      if (IsUpperEn(c))
      {
        r = UnicodeMathCapitalBoldStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        r = UnicodeMathLowerBoldStart + (TLongChar)(c - 'a');
      }
      else if (IsUpperGreek(c))
      {
        r = UnicodeGreekCapitalBoldStart + (TLongChar)(c - UnicodeGreekUpperStart);
      }
      else if (IsLowerGreek(c))
      {
        r = UnicodeGreekLowerBoldStart + (TLongChar)(c - UnicodeGreekLowerStart);
      }
      else if (IsNumber(c))
      {
        r = UnicodeNumberBoldStart + (TLongChar)(c - '0');
      }
      return r;
    }

    private TLongChar GetBoldItalic(TChar c)
    {
      TLongChar r = c;
      if (IsLowerEn(c))
      {
        r = UnicodeMathLowerBoldItalicStart + (TLongChar)(c - 'a');
      }
      else if (IsUpperEn(c))
      {
        r = UnicodeMathCapitalBoldItalicStart + (TLongChar)(c - 'A');
      }
      else if (IsUpperGreek(c))
      {
        r = UnicodeGreekCapitalBoldItalicStart + (TLongChar)(c - UnicodeGreekUpperStart);
      }
      else if (IsLowerGreek(c))
      {
        r = UnicodeGreekLowerBoldItalicStart + (TLongChar)(c - UnicodeGreekLowerStart);
      }
      else if (IsGreekSymbol(c))
      {
        r = UnicodeGreekSymbolBoldItalicStart + (TLongChar)GreekSymbolOrder(c);
      }
      else if (IsNumber(c))
      {
        // no bold italic for numbers, so we just bold them.
        r = GetBold(c);
      }
      return r;
    }

    private TLongChar GetCaligraphic(TChar c)
    {
      // Caligraphic has lots of exceptions:
      switch (c)
      {
        case 'B':
          return 0x212C;   // Script B (bernoulli)
        case 'E':
          return 0x2130;   // Script E (emf)
        case 'F':
          return 0x2131;   // Script F (fourier)
        case 'H':
          return 0x210B;   // Script H (hamiltonian)
        case 'I':
          return 0x2110;   // Script I
        case 'L':
          return 0x2112;   // Script L (laplace)
        case 'M':
          return 0x2133;   // Script M (M-matrix)
        case 'R':
          return 0x211B;   // Script R (Riemann integral)
        case 'e':
          return 0x212F;   // Script e (Natural exponent)
        case 'g':
          return 0x210A;   // Script g (real number)
        case 'o':
          return 0x2134;   // Script o (order)
      }
      TLongChar r;
      if (IsUpperEn(c))
      {
        r = UnicodeMathCapitalScriptStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        // Latin modern math doesn't have lower case caligraphic characters
        r = GetDefaultStyle(c);
      }
      else
      {
        // doesn't exist for greek or numbers.
        r = GetDefaultStyle(c);
      }
      return r;
    }

    // mathsf
    private TLongChar GetSansSerif(TChar c)
    {
      if (IsUpperEn(c))
      {
        return UnicodeMathCapitalSansSerifStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        return UnicodeMathLowerSansSerifStart + (TLongChar)(c - 'a');
      }
      else if (IsNumber(c))
      {
        return UnicodeNumberSansSerifStart + (TLongChar)(c - 0);
      }
      // SansSerif doesn't exist for greek
      return GetDefaultStyle(c);
    }

    // mathfrak
    private TLongChar GetFraktur(TChar c)
    {
      // Fraktur has exceptions:
      switch (c)
      {
        case 'C':
          return 0x212D;   // C Fraktur
        case 'H':
          return 0x210C;   // Hilbert space
        case 'I':
          return 0x2111;   // Imaginary
        case 'R':
          return 0x211C;   // Real
        case 'Z':
          return 0x2128;   // Z Fraktur
      }
      if (IsUpperEn(c))
      {
        return UnicodeMathCapitalFrakturStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        return UnicodeMathLowerFrakturStart + (TLongChar)(c - 'a');
      }
      return GetDefaultStyle(c);
    }

    // mathtt
    private TLongChar GetTypewriter(TChar c)
    {
      if (IsUpperEn(c))
      {
        return UnicodeMathCapitalTTStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        return UnicodeMathLowerTTStart + (TLongChar)(c - 'a');
      }
      else if (IsNumber(c))
      {
        return UnicodeNumberTTStart + (TLongChar)(c - '0');
      }
      // monospace doesn't exist for Greek, so use the default treatment
      return GetDefaultStyle(c);
    }

    private TLongChar GetBlackboard(TChar c)
    {
      // Blackboard has lots of exceptions:
      switch (c)
      {
        case 'C':
          return 0x2102;   // Complex numbers
        case 'H':
          return 0x210D;   // Quarternions
        case 'N':
          return 0x2115;   // Natural numbers
        case 'P':
          return 0x2119;   // Primes
        case 'Q':
          return 0x211A;   // Rationals
        case 'R':
          return 0x211D;   // Reals
        case 'Z':
          return 0x2124;   // Integers
        default:
          break;
      }
      if (IsUpperEn(c))
      {
        return UnicodeMathCapitalBlackboardStart + (TLongChar)(c - 'A');
      }
      else if (IsLowerEn(c))
      {
        return UnicodeMathLowerBlackboardStart + (TLongChar)(c - 'a');
      }
      else if (IsNumber(c))
      {
        return UnicodeNumberBlackboardStart + (TLongChar)(c - '0');
      }
      return GetDefaultStyle(c);
    }

    private TLongChar ToLittleEndian(TLongChar c) => BitConverter.IsLittleEndian ? c :
      (c & 0b11110000) >> 4 + (c & 0b00001111) << 4;

    public string ChangeFont(char c, FontStyle outputFontStyle)
    {
      TLongChar unicode = StyleCharacter(c, outputFontStyle);
      unicode = ToLittleEndian(unicode);
      string utf32String = char.ConvertFromUtf32(unicode);
      return utf32String;
    }

    public string ChangeFont(string inputString, FontStyle outputFontStyle)
    {
      StringBuilder builder = new StringBuilder();
      var encoding = new UnicodeEncoding();
      var chars = inputString.ToCharArray();
      foreach (TChar c in chars)
      {
        var changedGlyph = ChangeFont(c, outputFontStyle);
        builder.Append(changedGlyph);
      }
      var r = builder.ToString();
      return r;
    }
  }
}
