using System;
using System.Collections.Generic;

namespace CSharpMath.Atom {
  /// <summary>The lexical categories shared by the math and text LaTeX readers.</summary>
  internal enum LaTeXTokenKind {
    RawText,
    Whitespace,
    ControlWord,
    ControlSymbol,
    GroupOpen,
    GroupClose,
    InlineMathDelimiter,
    DisplayMathDelimiter,
    InvalidDollarRun,
  }

  /// <summary>A source token. Start and Length are UTF-16 code-unit offsets.</summary>
  internal readonly struct LaTeXToken {
    internal LaTeXToken(LaTeXTokenKind kind, int start, int length, string source) {
      Kind = kind;
      Start = start;
      Length = length;
      Source = source;
    }
    internal LaTeXTokenKind Kind { get; }
    internal int Start { get; }
    internal int Length { get; }
    internal int End => Start + Length;
    internal string Source { get; }
    internal string Text => Source.Substring(Start, Length);
    public override string ToString() => $"{Kind}({Start}, {Length}): {Text}";
  }

  /// <summary>Tokenizes LaTeX without interpreting commands or consuming terminators.</summary>
  internal static class LaTeXTokenizer {
    internal static int ReadCommandLength(ReadOnlySpan<char> source) {
      if (source.IsEmpty || source[0] != '\\') throw new ArgumentException("A command must start with \\", nameof(source));
      var end = 1;
      if (end == source.Length) return end;
      if (IsAsciiLetter(source[end])) {
        do end++; while (end < source.Length && IsAsciiLetter(source[end]));
      } else end++;
      return end;
    }
    internal static LaTeXToken ReadAt(string source, int offset) {
      if (source is null) throw new ArgumentNullException(nameof(source));
      if (offset < 0 || offset >= source.Length) throw new ArgumentOutOfRangeException(nameof(offset));
      var ch = source[offset]; var end = offset + 1;
      if (ch == '\\') {
        var length = ReadCommandLength(source.AsSpan(offset));
        return new LaTeXToken(length > 1 && IsAsciiLetter(source[offset + 1]) ? LaTeXTokenKind.ControlWord : LaTeXTokenKind.ControlSymbol, offset, length, source);
      }
      if (ch == '$') {
        while (end < source.Length && source[end] == '$') end++;
        var count = end - offset;
        return new LaTeXToken(count == 1 ? LaTeXTokenKind.InlineMathDelimiter : count == 2 ? LaTeXTokenKind.DisplayMathDelimiter : LaTeXTokenKind.InvalidDollarRun, offset, count, source);
      }
      if (ch == '{') return new LaTeXToken(LaTeXTokenKind.GroupOpen, offset, 1, source);
      if (ch == '}') return new LaTeXToken(LaTeXTokenKind.GroupClose, offset, 1, source);
      if (char.IsWhiteSpace(ch)) { while (end < source.Length && char.IsWhiteSpace(source[end])) end++; return new LaTeXToken(LaTeXTokenKind.Whitespace, offset, end - offset, source); }
      while (end < source.Length && !IsSpecial(source[end])) end++;
      return new LaTeXToken(LaTeXTokenKind.RawText, offset, end - offset, source);
    }
    internal static IReadOnlyList<LaTeXToken> Tokenize(string source) {
      if (source is null) throw new ArgumentNullException(nameof(source));
      var result = new List<LaTeXToken>();
      for (var i = 0; i < source.Length;) { var token = ReadAt(source, i); result.Add(token); i = token.End; }
      return result;
    }

    private static bool IsAsciiLetter(char c) => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
    private static bool IsSpecial(char c) => c == '\\' || c == '$' || c == '{' || c == '}' || char.IsWhiteSpace(c);
  }

  internal enum LaTeXMode { Text, InlineMath, DisplayMath }

  internal enum LaTeXModeBoundary {
    InlineDollar,
    DisplayDollar,
    InlineCommandOpen,
    InlineCommandClose,
    DisplayCommandOpen,
    DisplayCommandClose,
  }

  /// <summary>Defines the mode changes shared by text-mode math delimiters.</summary>
  internal static class LaTeXModeTransition {
    internal static string? TryTransition(LaTeXMode current, LaTeXModeBoundary boundary, out LaTeXMode next) {
      next = current;
      switch (current, boundary) {
        case (LaTeXMode.Text, LaTeXModeBoundary.InlineDollar):
        case (LaTeXMode.Text, LaTeXModeBoundary.InlineCommandOpen):
          next = LaTeXMode.InlineMath;
          return null;
        case (LaTeXMode.InlineMath, LaTeXModeBoundary.InlineDollar):
        case (LaTeXMode.InlineMath, LaTeXModeBoundary.InlineCommandClose):
          next = LaTeXMode.Text;
          return null;
        case (LaTeXMode.Text, LaTeXModeBoundary.DisplayDollar):
        case (LaTeXMode.Text, LaTeXModeBoundary.DisplayCommandOpen):
          next = LaTeXMode.DisplayMath;
          return null;
        case (LaTeXMode.DisplayMath, LaTeXModeBoundary.DisplayDollar):
        case (LaTeXMode.DisplayMath, LaTeXModeBoundary.DisplayCommandClose):
          next = LaTeXMode.Text;
          return null;
        case (LaTeXMode.DisplayMath, LaTeXModeBoundary.InlineDollar):
          return "Cannot close display math mode with $";
        case (LaTeXMode.InlineMath, LaTeXModeBoundary.DisplayDollar):
          return "Cannot close inline math mode with $$";
        case (LaTeXMode.InlineMath, LaTeXModeBoundary.InlineCommandOpen):
          return "Cannot open inline math mode in inline math mode";
        case (LaTeXMode.DisplayMath, LaTeXModeBoundary.InlineCommandOpen):
          return "Cannot open inline math mode in display math mode";
        case (LaTeXMode.Text, LaTeXModeBoundary.InlineCommandClose):
          return "Cannot close inline math mode outside of math mode";
        case (LaTeXMode.DisplayMath, LaTeXModeBoundary.InlineCommandClose):
          return "Cannot close inline math mode in display math mode";
        case (LaTeXMode.InlineMath, LaTeXModeBoundary.DisplayCommandOpen):
          return "Cannot open display math mode in inline math mode";
        case (LaTeXMode.DisplayMath, LaTeXModeBoundary.DisplayCommandOpen):
          return "Cannot open display math mode in display math mode";
        case (LaTeXMode.Text, LaTeXModeBoundary.DisplayCommandClose):
          return "Cannot close display math mode outside of math mode";
        case (LaTeXMode.InlineMath, LaTeXModeBoundary.DisplayCommandClose):
          return "Cannot close display math mode in inline math mode";
        default:
          throw new ArgumentOutOfRangeException(nameof(boundary));
      }
    }
  }
}
