using System;
using CSharpMath.Atom;

namespace CSharpMath.Display {
  using FrontEnd;
  public static class UnicodeFontChanger {
    private const char UnicodeGreekLowerStart = 'α'; // 03B1
    private const char UnicodeGreekLowerEnd = 'ω'; // 03C9
    private const char UnicodeGreekUpperStart = 'Α'; // 0391
    private const char UnicodeGreekUpperEnd = 'Ω'; // 03A9

    private const int UnicodeMathCapitalBoldStart = 0x1D400;
    private const int UnicodeMathLowerBoldStart = 0x1D41A;
    private const int UnicodeGreekCapitalBoldStart = 0x1D6A8;
    private const int UnicodeGreekLowerBoldStart = 0x1D6C2;
    private const int UnicodeGreekSymbolBoldStart = 0x1D6DC;
    private const int UnicodeNumberBoldStart = 0x1D7CE;

    private const int UnicodeMathCapitalItalicStart = 0x1D434;
    private const int UnicodeMathLowerItalicStart = 0x1D44E;
    private const int UnicodeGreekCapitalItalicStart = 0x1D6E2;
    private const int UnicodeGreekLowerItalicStart = 0x1D6FC;
    private const int UnicodeGreekSymbolItalicStart = 0x1D716;

    private const int UnicodeMathCapitalBoldItalicStart = 0x1D468;
    private const int UnicodeMathLowerBoldItalicStart = 0x1D482;
    private const int UnicodeGreekCapitalBoldItalicStart = 0x1D71C;
    private const int UnicodeGreekLowerBoldItalicStart = 0x1D736;
    private const int UnicodeGreekSymbolBoldItalicStart = 0x1D750;

    private const int UnicodeMathCapitalTTStart = 0x1D670;
    private const int UnicodeMathLowerTTStart = 0x1D68A;
    private const int UnicodeNumberTTStart = 0x1D7F6;

    private const int UnicodeMathCapitalSansSerifStart = 0x1D5A0;
    private const int UnicodeMathLowerSansSerifStart = 0x1D5BA;
    private const int UnicodeNumberSansSerifStart = 0x1D7E2;

    private const int UnicodeMathCapitalFrakturStart = 0x1D504;
    private const int UnicodeMathLowerFrakturStart = 0x1D51E;

    private const int UnicodeMathCapitalBlackboardStart = 0x1D538;
    private const int UnicodeMathLowerBlackboardStart = 0x1D552;
    private const int UnicodeNumberBlackboardStart = 0x1D7D8;

    private const int UnicodeMathCapitalScriptStart = 0x1D49C;
    private static bool IsLowerEn(char c) => c >= 'a' && c <= 'z';
    private static bool IsUpperEn(char c) => c >= 'A' && c <= 'Z';
    private static bool IsNumber(char c) => c >= '0' && c <= '9';
    private static bool IsLowerGreek(char c) => c >= UnicodeGreekLowerStart && c <= UnicodeGreekLowerEnd;
    private static bool IsUpperGreek(char c) => c >= UnicodeGreekUpperStart && c <= UnicodeGreekUpperEnd;

    static readonly char[] greekSymbols = {
      'ϵ', // 03F5
      'ϑ', // 03D1
      'ϰ', // 03F0
      'ϕ', // 03D5
      'ϱ', // 03F1
      'ϖ' // 03D6
    };
    private static int GreekSymbolOrder(char c) =>
      // These greek symbols that always appear in unicode in this particular order
      // after the alphabet. 
      // The symbols are epsilon, vartheta, varkappa, phi, varrho, and varpi.
      Array.IndexOf(greekSymbols, c);
    private static bool IsGreekSymbol(char c) => GreekSymbolOrder(c) != -1;
    private static int GetDefaultStyle(char c) =>
      IsLowerEn(c) || IsUpperEn(c) || IsLowerGreek(c) || IsGreekSymbol(c) ? GetItalicized(c) : c;
    private static int GetItalicized(char c) =>
      c == 'h' ? 0x210E // Plank's Constant
      : IsUpperEn(c) ? UnicodeMathCapitalItalicStart + c - 'A'
      : IsLowerEn(c) ? UnicodeMathLowerItalicStart + c - 'a'
      : IsLowerGreek(c) ? UnicodeGreekLowerItalicStart + c - UnicodeGreekLowerStart
      : IsUpperGreek(c) ? UnicodeGreekCapitalItalicStart + c - UnicodeGreekUpperStart
      : IsGreekSymbol(c) ? UnicodeGreekSymbolItalicStart + GreekSymbolOrder(c)
      : c;
    private static int GetBold(char c) =>
      IsUpperEn(c) ? UnicodeMathCapitalBoldStart + c - 'A'
      : IsLowerEn(c) ? UnicodeMathLowerBoldStart + c - 'a'
      : IsUpperGreek(c) ? UnicodeGreekCapitalBoldStart + c - UnicodeGreekUpperStart
      : IsLowerGreek(c) ? UnicodeGreekLowerBoldStart + c - UnicodeGreekLowerStart
      : IsGreekSymbol(c) ? UnicodeGreekSymbolBoldStart + GreekSymbolOrder(c)
      : IsNumber(c) ? UnicodeNumberBoldStart + c - '0'
      : c;

    private static int GetBoldItalic(char c) =>
      IsLowerEn(c) ? UnicodeMathLowerBoldItalicStart + c - 'a'
      : IsUpperEn(c) ? UnicodeMathCapitalBoldItalicStart + c - 'A'
      : IsUpperGreek(c) ? UnicodeGreekCapitalBoldItalicStart + c - UnicodeGreekUpperStart
      : IsLowerGreek(c) ? UnicodeGreekLowerBoldItalicStart + c - UnicodeGreekLowerStart
      : IsGreekSymbol(c) ? UnicodeGreekSymbolBoldItalicStart + GreekSymbolOrder(c)
      // no bold italic for numbers, so we just bold them.
      : IsNumber(c) ? GetBold(c) : c;

    private static int GetCaligraphic(char c) =>
      c switch
      {
        // Caligraphic has lots of exceptions:
        'B' => 0x212C, // Script B (bernoulli)
        'E' => 0x2130, // Script E (emf)
        'F' => 0x2131, // Script F (fourier)
        'H' => 0x210B, // Script H (hamiltonian)
        'I' => 0x2110, // Script I
        'L' => 0x2112, // Script L (laplace)
        'M' => 0x2133, // Script M (M-matrix)
        'R' => 0x211B, // Script R (Riemann integral)
        // Latin modern math doesn't have lower case caligraphic characters
        'e' => 0x212F, // Script e (Natural exponent)
        'g' => 0x210A, // Script g (real number)
        'o' => 0x2134, // Script o (order)
        _ when IsUpperEn(c) => UnicodeMathCapitalScriptStart + c - 'A',
        // Latin modern math doesn't have lower case caligraphic characters
        _ when IsLowerEn(c) => GetDefaultStyle(c),
        // doesn't exist for greek or numbers.
        _ => GetDefaultStyle(c)
      };

    // mathsf
    private static int GetSansSerif(char c) =>
      IsUpperEn(c) ? UnicodeMathCapitalSansSerifStart + c - 'A'
      : IsLowerEn(c) ? UnicodeMathLowerSansSerifStart + c - 'a'
      : IsNumber(c) ? UnicodeNumberSansSerifStart + c - '0'
      // SansSerif doesn't exist for greek
      : GetDefaultStyle(c);

    // mathfrak
    private static int GetFraktur(char c) =>
      // Fraktur has exceptions:
      c switch
      {
        'C' => 0x212D, // C Fraktur
        'H' => 0x210C, // Hilbert space
        'I' => 0x2111, // Imaginary
        'R' => 0x211C, // Real
        'Z' => 0x2128, // Z Fraktur
        _ when IsUpperEn(c) => UnicodeMathCapitalFrakturStart + c - 'A',
        _ when IsLowerEn(c) => UnicodeMathLowerFrakturStart + c - 'a',
        _ => GetDefaultStyle(c),
      };

    // mathtt
    private static int GetTypewriter(char c) =>
      IsUpperEn(c) ? UnicodeMathCapitalTTStart + c - 'A'
      : IsLowerEn(c) ? UnicodeMathLowerTTStart + c - 'a'
      : IsNumber(c) ? UnicodeNumberTTStart + c - '0'
      // monospace doesn't exist for Greek, so use the default treatment
      : GetDefaultStyle(c);

    private static int GetBlackboard(char c) =>
      // Blackboard has lots of exceptions:
      c switch
      {
        'C' => 0x2102, // Complex numbers
        'H' => 0x210D, // Quarternions
        'N' => 0x2115, // Natural numbers
        'P' => 0x2119, // Primes
        'Q' => 0x211A, // Rationals
        'R' => 0x211D, // Reals
        'Z' => 0x2124, // Integers
        var _ when IsUpperEn(c) => UnicodeMathCapitalBlackboardStart + c - 'A',
        var _ when IsLowerEn(c) => UnicodeMathLowerBlackboardStart + c - 'a',
        var _ when IsNumber(c) => UnicodeNumberBlackboardStart + c - '0',
        _ => GetDefaultStyle(c),
      };
    public static int StyleCharacter(char c, FontStyle fontStyle) =>
      fontStyle switch
      {
        FontStyle.Default => GetDefaultStyle(c),
        FontStyle.Roman => c,
        FontStyle.Bold => GetBold(c),
        FontStyle.Italic => GetItalicized(c),
        FontStyle.BoldItalic => GetBoldItalic(c),
        FontStyle.Caligraphic => GetCaligraphic(c),
        FontStyle.Typewriter => GetTypewriter(c),
        FontStyle.SansSerif => GetSansSerif(c),
        FontStyle.Fraktur => GetFraktur(c),
        FontStyle.Blackboard => GetBlackboard(c),
        _ => throw new NotImplementedException("Unknown font style " + fontStyle),
      };
    public static string ChangeFont(string inputString, FontStyle outputFontStyle) {
      var builder = new System.Text.StringBuilder();
      foreach (var c in inputString) {
        int unicode = StyleCharacter(c, outputFontStyle);
        builder.Append(char.IsSurrogate(c)
          ? ((char)unicode).ToStringInvariant() : char.ConvertFromUtf32(unicode));
      }
      return builder.ToString();
    }
  }
}