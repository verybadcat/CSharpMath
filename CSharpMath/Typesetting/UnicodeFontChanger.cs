using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;
using TLongGlyph = System.Int32;

namespace CSharpMath
{
  public class UnicodeFontChanger : IFontChanger
  {

    private const TGlyph UnicodeGreekLowerStart = 0x03B1;
    private const TGlyph UnicodeGreekLowerEnd = 0x03C9;
    private const TGlyph UnicodeGreekUpperStart = 0x0391;
    private const TGlyph UnicodeGreekUpperEnd = 0x03A9;

    private const TLongGlyph UnicodeMathCapitalBoldStart = 0x1D400;
    private const TLongGlyph UnicodeMathLowerBoldStart = 0x1D41A;
    private const TLongGlyph UnicodeGreekCapitalBoldStart = 0x1D6A8;
    private const TLongGlyph UnicodeGreekLowerBoldStart = 0x1D6C2;
    private const TLongGlyph UnicodeGreekSymbolBoldStart = 0x1D6DC;
    private const TLongGlyph UnicodeNumberBoldStart = 0x1D7CE;

    private const TGlyph kMTUnicodePlanksConstant = 0x210e;
    private const TLongGlyph UnicodeMathCapitalItalicStart = 0x1D434;
    private const TLongGlyph UnicodeMathLowerItalicStart = 0x1D44E;
    private const TLongGlyph UnicodeGreekCapitalItalicStart = 0x1D6E2;
    private const TLongGlyph UnicodeGreekLowerItalicStart = 0x1D6FC;
    private const TLongGlyph UnicodeGreekSymbolItalicStart = 0x1D716;

    private const TLongGlyph UnicodeMathCapitalBoldItalicStart = 0x1D468;
    private const TLongGlyph UnicodeMathLowerBoldItalicStart = 0x1D482;
    private const TLongGlyph UnicodeGreekCapitalBoldItalicStart = 0x1D71C;
    private const TLongGlyph UnicodeGreekLowerBoldItalicStart = 0x1D736;
    private const TLongGlyph UnicodeGreekSymbolBoldItalicStart = 0x1D750;


    private const TLongGlyph UnicodeMathCapitalTTStart = 0x1D670;
    private const TLongGlyph UnicodeMathLowerTTStart = 0x1D68A;
    private const TLongGlyph UnicodeNumberTTStart = 0x1D7F6;


    private const TLongGlyph UnicodeMathCapitalSansSerifStart = 0x1D5A0;
    private const TLongGlyph UnicodeMathLowerSansSerifStart = 0x1D5BA;
    private const TLongGlyph UnicodeNumberSansSerifStart = 0x1D7E2;

    private const TLongGlyph UnicodeMathCapitalFrakturStart = 0x1D504;
    private const TLongGlyph UnicodeMathLowerFrakturStart = 0x1D51E;

    private const TLongGlyph UnicodeMathCapitalBlackboardStart = 0x1D538;
    private const TLongGlyph UnicodeMathLowerBlackboardStart = 0x1D552;
    private const TLongGlyph UnicodeNumberBlackboardStart = 0x1D7D8;

    private const TLongGlyph UnicodeMathCapitalScriptStart = 0x1D49C;

    public UnicodeFontChanger()
    {
    }

    private bool IsLowerEn(TGlyph glyph)
      => glyph >= 'a' && glyph <= 'z';

    private bool IsUpperEn(TGlyph glyph)
      => glyph >= 'A' && glyph <= 'Z';

    private bool IsNumber(TGlyph glyph)
      => glyph >= '0' && glyph <= '9';

    private bool IsLowerGreek(TGlyph glyph)
      => glyph >= UnicodeGreekLowerStart && glyph <= UnicodeGreekLowerEnd;

    private bool IsUpperGreek(TGlyph glyph)
      => glyph >= UnicodeGreekUpperStart && glyph <= UnicodeGreekUpperEnd;

    private int GreekSymbolOrder(TGlyph glyph)
    {
      // These greek symbols that always appear in unicode in this particular order
      // after the alphabet. 
      // The symbols are epsilon, vartheta, varkappa, phi, varrho, and varpi.
      TGlyph[] greekSymbols = { 0x03F5, 0x03D1, 0x03F0, 0x03F1, 0x03D6 };
      return Array.IndexOf(greekSymbols, glyph);
    }

    private bool IsGreekSymbol(TGlyph glyph)
      => GreekSymbolOrder(glyph) != -1;

    private TLongGlyph StyleCharacter(char c, FontStyle fontStyle)
    {
      switch (fontStyle)
      {
        case FontStyle.Default:
          return GetDefaultStyle(glyph);
        case FontStyle.Roman:
          return glyph;
        case FontStyle.Bold:
          return GetBold(glyph);
        case FontStyle.Italic:
          return GetItalicized(glyph);
        case FontStyle.BoldItalic:
          return GetBoldItalic(glyph);
        case FontStyle.Caligraphic:
          return GetCaligraphic(glyph);
        case FontStyle.Typewriter:
          return GetTypewriter(glyph);
        case FontStyle.SansSerif:
          return GetSansSerif(glyph);
        case FontStyle.Fraktur:
          return GetFraktur(glyph);
        case FontStyle.Blackboard:
          return GetBlackboard(glyph);
        default: throw new NotImplementedException("Unknown font style " + fontStyle);
      }
    }

    private TLongGlyph GetDefaultStyle(ushort glyph)
    {
      if (IsLowerEn(glyph) || IsUpperEn(glyph) || IsLowerGreek(glyph) || IsGreekSymbol(glyph))
      {
        return GetItalicized(glyph);
      }
      if (IsNumber(glyph) || IsUpperGreek(glyph))
      {
        return glyph;
      }
      if (glyph == '.')
      {
        return glyph;
      }
      else
      {
        throw new InvalidOperationException(@"Illegal character " + glyph);
      }
    }

    private TLongGlyph GetItalicized(TGlyph glyph)
    {
      TLongGlyph r = glyph;
      if (glyph == 'h')
      {
        r = kMTUnicodePlanksConstant;
      }
      else if (IsUpperEn(glyph))
      {
        r = UnicodeMathCapitalItalicStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        r = UnicodeMathLowerItalicStart + (TLongGlyph)(glyph - 'a');
      }
      else if (IsLowerGreek(glyph))
      {
        r = UnicodeGreekLowerItalicStart + (TLongGlyph)(glyph - UnicodeGreekLowerStart);
      }
      else if (IsUpperGreek(glyph))
      {
        r = UnicodeGreekCapitalItalicStart + (TLongGlyph)(glyph - UnicodeGreekUpperStart);
      }
      else if (IsGreekSymbol(glyph))
      {
        r = UnicodeGreekSymbolItalicStart + (TLongGlyph)GreekSymbolOrder(glyph);
      }
      return r;
    }

    private TLongGlyph GetBold(TGlyph glyph)
    {
      TLongGlyph r = glyph;
      if (IsUpperEn(glyph))
      {
        r = UnicodeMathCapitalBoldStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        r = UnicodeMathLowerBoldStart + (TLongGlyph)(glyph - 'a');
      }
      else if (IsUpperGreek(glyph))
      {
        r = UnicodeGreekCapitalBoldStart + (TLongGlyph)(glyph - UnicodeGreekUpperStart);
      }
      else if (IsLowerGreek(glyph))
      {
        r = UnicodeGreekLowerBoldStart + (TLongGlyph)(glyph - UnicodeGreekLowerStart);
      }
      else if (IsNumber(glyph))
      {
        r = UnicodeNumberBoldStart + (TLongGlyph)(glyph - '0');
      }
      return r;
    }

    private TLongGlyph GetBoldItalic(TGlyph glyph)
    {
      TLongGlyph r = glyph;
      if (IsLowerEn(glyph))
      {
        r = UnicodeMathLowerBoldItalicStart + (TLongGlyph)(glyph - 'a');
      }
      else if (IsUpperEn(glyph))
      {
        r = UnicodeMathCapitalBoldItalicStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsUpperGreek(glyph))
      {
        r = UnicodeGreekCapitalBoldItalicStart + (TLongGlyph)(glyph - UnicodeGreekUpperStart);
      }
      else if (IsLowerGreek(glyph))
      {
        r = UnicodeGreekLowerBoldItalicStart + (TLongGlyph)(glyph - UnicodeGreekLowerStart);
      }
      else if (IsGreekSymbol(glyph))
      {
        r = UnicodeGreekSymbolBoldItalicStart + (TLongGlyph)GreekSymbolOrder(glyph);
      }
      else if (IsNumber(glyph))
      {
        // no bold italic for numbers, so we just bold them.
        r = GetBold(glyph);
      }
      return r;
    }

    private TLongGlyph GetCaligraphic(TGlyph glyph)
    {
      // Caligraphic has lots of exceptions:
      switch (glyph)
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
      TLongGlyph r;
      if (IsUpperEn(glyph))
      {
        r = UnicodeMathCapitalScriptStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        // Latin modern math doesn't have lower case caligraphic characters
        r = GetDefaultStyle(glyph);
      }
      else
      {
        // doesn't exist for greek or numbers.
        r = GetDefaultStyle(glyph);
      }
      return r;
    }

    // mathsf
    private TLongGlyph GetSansSerif(TGlyph glyph)
    {
      if (IsUpperEn(glyph))
      {
        return UnicodeMathCapitalSansSerifStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        return UnicodeMathLowerSansSerifStart + (TLongGlyph)(glyph - 'a');
      }
      else if (IsNumber(glyph))
      {
        return UnicodeNumberSansSerifStart + (TLongGlyph)(glyph - 0);
      }
      // SansSerif doesn't exist for greek
      return GetDefaultStyle(glyph);
    }

    // mathfrak
    private TLongGlyph GetFraktur(TGlyph glyph)
    {
      // Fraktur has exceptions:
      switch (glyph)
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
      if (IsUpperEn(glyph))
      {
        return UnicodeMathCapitalFrakturStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        return UnicodeMathLowerFrakturStart + (TLongGlyph)(glyph - 'a');
      }
      return GetDefaultStyle(glyph);
    }

    // mathtt
    private TLongGlyph GetTypewriter(TGlyph glyph)
    {
      if (IsUpperEn(glyph))
      {
        return UnicodeMathCapitalTTStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        return UnicodeMathLowerTTStart + (TLongGlyph)(glyph - 'a');
      }
      else if (IsNumber(glyph))
      {
        return UnicodeNumberTTStart + (TLongGlyph)(glyph - '0');
      }
      // monospace doesn't exist for Greek, so use the default treatment
      return GetDefaultStyle(glyph);
    }

    private TLongGlyph GetBlackboard(TGlyph glyph)
    {
      // Blackboard has lots of exceptions:
      switch (glyph)
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
      if (IsUpperEn(glyph))
      {
        return UnicodeMathCapitalBlackboardStart + (TLongGlyph)(glyph - 'A');
      }
      else if (IsLowerEn(glyph))
      {
        return UnicodeMathLowerBlackboardStart + (TLongGlyph)(glyph - 'a');
      }
      else if (IsNumber(glyph))
      {
        return UnicodeNumberBlackboardStart + (TLongGlyph)(glyph - '0');
      }
      return GetDefaultStyle(glyph);
    }

    private TLongGlyph ToLittleEndian(TLongGlyph glyph)
    {
      return glyph; // TODO: figure out and implement this. Most systems are little endian in which case it is already correct.
    }

    public string ChangeFont(char c, FontStyle outputFontStyle)
    {
      TLongGlyph unicode = StyleCharacter(c, outputFontStyle);
      unicode = ToLittleEndian(unicode);
      string utf32String = char.ConvertFromUtf32(unicode);
      Debug.WriteLine(c.ToString() + " =>");
      utf32String.LogCharacters();
      return utf32String;
    }

    public string ChangeFont(string inputString, FontStyle outputFontStyle)
    {
      StringBuilder builder = new StringBuilder();
      var encoding = new UnicodeEncoding();
      var chars = inputString.ToCharArray();
      foreach (TGlyph glyph in glyphs)
      {
        var changedGlyph = ChangeFont(glyph, outputFontStyle);
        builder.Append(changedGlyph);
      }
      var r = builder.ToString();
      r.LogCharacters();
      return r;
    }
  }
}
