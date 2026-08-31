using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Atom {
  using Atoms;
  using static Result;
  using Space = Atoms.Space;
  public class LaTeXParser {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces",
      Justification = "This is a marker interface to enable compile-time type checking")]
#pragma warning disable CA1034 // Nested types should not be visible
    // Justification: Implementation details exposed for extensibility
    public interface IEnvironment { }
    public class TableEnvironment : IEnvironment {
      public TableEnvironment(string? name) => Name = name;
      public string? Name { get; set; }
      public bool Ended { get; set; }
      public int NRows { get; set; }
      /// <summary>The raw {…} argument of environments that take one
      /// (alignedat's {n}, array's column spec).</summary>
      public string? Argument { get; set; }
      /// <summary>Per-row-boundary \hline counts (array only), length rows+1.</summary>
      public List<int> HorizontalLines { get; } = new List<int>();
      public string? ArrayAlignments { get; set; }
    }
    public class InnerEnvironment : IEnvironment {
      public Boundary? RightBoundary { get; set; }
    }
#pragma warning restore CA1034 // Nested types should not be visible
    public string Chars { get; }
    public int NextChar { get; private set; }
    public bool TextMode { get; set; } //_spacesAllowed in iosMath
    public FontStyle CurrentFontStyle { get; set; }
    public Stack<IEnvironment> Environments { get; } = new Stack<IEnvironment>();
    /// <summary>Maximum recursion depth for BuildInternal: comfortably deeper than any
    /// realistic human-authored expression yet far below the frames needed to overflow the stack.</summary>
    internal const int MaxRecursionDepth = 150;
    private int _recursionDepth;
    internal int MacroExpansionDepth { get; set; }
    /// <summary>Set by a TeX group-transformation command (\over, \atop, …) that fired
    /// inside a `{…}` group; the group must then NOT be wrapped as a Group atom because
    /// the resulting fraction replaces it. Read-and-clear: the `{` handler consumes it,
    /// so an inner transform never leaks into an enclosing group's decision.</summary>
    private sealed class BuildFrame {
      public bool GroupWasTransformedByStopCommand { get; set; }
    }
    private readonly Stack<BuildFrame> _buildFrames = new Stack<BuildFrame>();
    private bool _lastCompletedBuildWasTransformed;
    /// <summary>When true (built-in macro templates only), `#N` reads a macro parameter
    /// instead of being an invalid character.</summary>

    /// <summary>Whether a group-transformation command (\over/\atop/…) fired inside the
    /// group currently being read. Read-and-clear.</summary>
    internal bool GroupWasTransformedByStopCommand {
      get {
        var value = _lastCompletedBuildWasTransformed;
        _lastCompletedBuildWasTransformed = false;
        return value;
      }
      set {
        if (_buildFrames.Count == 0)
          throw new InvalidCodePathException("No active parser frame for group transformation");
        _buildFrames.Peek().GroupWasTransformedByStopCommand = value;
      }
    }

    /// <summary>Returns the character at the read position without consuming it.</summary>
    public char PeekChar() => Chars[NextChar];

    /// <summary>Reads a `\`-command name (letters only) or a single non-letter character
    /// after the backslash. The backslash must already be consumed.</summary>
    public string ReadCommandName() {
      if (!HasCharacters) return string.Empty;
      var ch = ReadChar();
      static bool IsAsciiLetter(char c) =>
        (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
      if (!IsAsciiLetter(ch)) {
        // Single non-letter character command (e.g. \,, \;)
        return ch.ToStringInvariant();
      }
      var builder = new StringBuilder().Append(ch);
      while (HasCharacters && IsAsciiLetter(Chars[NextChar])) {
        builder.Append(ReadChar());
      }
      return builder.ToString();
    }

    public LaTeXParser(string str) {
      Chars = str;
      CurrentFontStyle = FontStyle.Default;
    }
    public Result<MathList> Build() {
      _recursionDepth = 0;
      _buildFrames.Clear();
      _lastCompletedBuildWasTransformed = false;
      return BuildInternal(false);
    }
    public char ReadChar() => Chars[NextChar++];
    public void UndoReadChar() =>
      _ = NextChar == 0
      ? throw new InvalidCodePathException("Can't unlook below character 0")
      : NextChar--;
    public bool HasCharacters => NextChar < Chars.Length;
    public Result<MathList> ReadArgument(MathList? appendTo = null) => BuildInternal(true, r: appendTo);
    /// <summary>Reads one macro argument as source text. Braces are retained and
    /// escaped braces do not affect nesting; this is intentionally not a parsed AST.</summary>
    internal Result<string> ReadRawArgument() {
      SkipSpaces();
      if (!HasCharacters) return Result.Err("Missing required argument at end of input");
      if (Chars[NextChar] is '}' or '^' or '_' or '&')
        return Result.Err($"Missing required argument before '{Chars[NextChar]}'");
      if (Chars[NextChar] == '\\') {
        var commandStart = NextChar;
        NextChar++;
        var command = ReadCommandName();
        UndoTo(commandStart);
        if (LaTeXSettings.IsStopCommand(command) && !LaTeXSettings.IsBuiltinMacro(command))
          return Result.Err($@"Missing required argument before \{command}");
      }
      var first = ReadChar();
      if (first == '\\') {
        if (!HasCharacters) return Result.Err("Trailing \\ in a macro argument");
        return Result.Ok("\\" + ReadCommandName());
      }
      if (first != '{') {
        if (char.IsHighSurrogate(first) && HasCharacters && char.IsLowSurrogate(Chars[NextChar]))
          return Result.Ok(new string(new[] { first, ReadChar() }));
        return Result.Ok(first.ToString());
      }
      var body = new System.Text.StringBuilder();
      var depth = 0;
      while (HasCharacters) {
        var c = ReadChar();
        if (c == '\\') {
          if (!HasCharacters) return Result.Err("Trailing \\ in a macro argument");
          body.Append(c).Append(ReadChar());
          continue;
        }
        if (c == '}') {
          if (depth == 0) return Result.Ok(body.ToString());
          depth--;
        } else if (c == '{') depth++;
        body.Append(c);
      }
      return Result.Err("Unmatched { in a macro argument");
    }
    public Result<MathList?> ReadArgumentOptional(MathList? appendTo = null) =>
      ReadCharIfAvailable('[')
      ? BuildInternal(false, ']', r: appendTo).Bind(mathList => (MathList?)mathList)
      : (MathList?)null;
    public Result<MathList> ReadUntil(char stopChar, MathList? appendTo = null) =>
      BuildInternal(false, stopChar, r: appendTo);
    // TODO: Example
    //https://phabricator.wikimedia.org/T99369
    //https://phab.wmfusercontent.org/file/data/xsimlcnvo42siudvwuzk/PHID-FILE-bdcqexocj5b57tj2oezn/math_rendering.png
    //dt, \text{d}t, \partial t, \nabla\psi \\ \underline\overline{dy/dx, \text{d}y/\text{d}x, \frac{dy}{dx}, \frac{\text{d}y}{\text{d}x}, \frac{\partial^2}{\partial x_1\partial x_2}y} \\ \prime,
    /// <summary>True while the innermost BuildInternal call is filling a single-character
    /// / single-field slot (^{…}, _{…}, \frac{…}, command arguments): a brace there *is*
    /// the field, so it must be flattened rather than wrapped as a Group.</summary>
    public bool IsReadingOneCharField { get; private set; }
    private Result<MathList> BuildInternal(bool oneCharOnly, char stopChar = '\0', MathList? r = null) {
      if (oneCharOnly && stopChar > '\0') {
        throw new InvalidCodePathException("Cannot set both oneCharOnly and stopChar");
      }
      if (_recursionDepth >= MaxRecursionDepth) {
        return "LaTeX nesting too deep";
      }
      _recursionDepth++;
      var frame = new BuildFrame();
      _buildFrames.Push(frame);
      var outerOneChar = IsReadingOneCharField;
      // Only a one-char field (^{…}, _{…}, \frac{…}, command arguments) makes braces
      // inside it fields. A stopChar read (a `{…}` group body, an environment body)
      // is NOT a field: nested braces there are genuine groups (iosMath 086d345).
      IsReadingOneCharField = oneCharOnly;
      try {
        return BuildInternalInner(oneCharOnly, stopChar, r);
      } finally {
        _buildFrames.Pop();
        _lastCompletedBuildWasTransformed = frame.GroupWasTransformedByStopCommand;
        IsReadingOneCharField = outerOneChar;
        _recursionDepth--;
      }
    }

    private Result<MathList> BuildInternalInner(bool oneCharOnly, char stopChar, MathList? r) {
      r ??= new MathList();
      MathAtom? prevAtom = null;
      while (HasCharacters) {
        MathAtom? atom = null;
        if (Chars[NextChar] == stopChar && stopChar > '\0') {
          NextChar++;
          return r;
        }
        if (Chars[NextChar] == '\\') {
          // Macros first: one Macro atom then flows through the shared tail below.
          var saveChar = NextChar;
          NextChar++; // consume the backslash
          var commandName = ReadCommandName();
          if (LaTeXSettings.IsBuiltinMacro(commandName)) {
            var macroResult = LaTeXSettings.MacroAtomForCommand(this, commandName);
            if (macroResult.Error is string macroError) return macroError;
            if (macroResult._value.Atom is { } theMacro) {
              theMacro.FontStyle = CurrentFontStyle;
              r.Add(theMacro);
              if (oneCharOnly) return r;
              continue;
            }
          }
          // Not a macro: rewind and continue with normal command dispatch.
          UndoTo(saveChar);
          // Explicit fixed-size delimiters are single atoms; unlike \left/\right,
          // they never pair and retain their TeX math class for spacing.
          NextChar++;
          var largeName = ReadCommandName();
          if (TryLargeDelimiter(largeName, out var largeSize, out var largeClass)) {
            var (boundary, boundaryError) = ReadDelimiter(largeName);
            if (boundaryError != null) return boundaryError;
            var largeNucleus = boundary.Nucleus switch {
              "\u2329" => "\u27E8",
              "\u232A" => "\u27E9",
              _ => boundary.Nucleus ?? string.Empty
            };
            var large = new Atoms.LargeDelimiter(largeNucleus, largeSize, largeClass);
            r.Add(large);
            if (oneCharOnly) return r;
            continue;
          }
          UndoTo(saveChar);
        }
        var ((handler, splitIndex), error) = LaTeXSettings.Commands.TryLookup(Chars.AsSpan(NextChar));
        if (error != null) {
          NextChar++; // Point to the start of the erroneous command
          return error;
        }
        NextChar += splitIndex;

        (MathAtom?, MathList?) handlerResult;
        (handlerResult, error) = handler(this, r, stopChar);
        if (error != null) return error;

        switch (handlerResult) {
          case ( { } /* dummy */, { } atoms): // Atoms producer (pre-styled)
            r.Append(atoms);
            prevAtom = r.Atoms.LastOrDefault();
            if (oneCharOnly)
              return r;
            else continue;
          case (null, { } @return): // Environment ender
            return @return;
          case (null, null): // Atom modifier
            continue;
          case ( { } resultAtom, null): // Atom producer
            atom = resultAtom;
            break;
        }
        atom.FontStyle = CurrentFontStyle;
        r.Add(atom);
        prevAtom = atom;
        if (oneCharOnly) {
          return r; // we consumed our character.
        }
      }
      return stopChar switch {
        '\0' => r,
        '}' => "Missing closing brace",
        _ => "Expected character not found: " + stopChar.ToStringInvariant(),
      };
    }

    private static bool TryLargeDelimiter(string name, out Atoms.LargeDelimiter.DelimiterSize size,
      out System.Type mathClass) {
      var suffix = name.Length > 0 && name[name.Length - 1] is 'l' or 'r' or 'm'
        ? name[name.Length - 1] : '\0';
      var prefix = suffix == '\0' ? name : name.Substring(0, name.Length - 1);
      size = prefix switch {
        "big" => Atoms.LargeDelimiter.DelimiterSize.Size1,
        "Big" => Atoms.LargeDelimiter.DelimiterSize.Size2,
        "bigg" => Atoms.LargeDelimiter.DelimiterSize.Size3,
        "Bigg" => Atoms.LargeDelimiter.DelimiterSize.Size4,
        _ => default
      };
      mathClass = suffix switch {
        'l' => typeof(Atoms.Open),
        'r' => typeof(Atoms.Close),
        'm' => typeof(Atoms.Relation),
        _ => typeof(Atoms.Ordinary)
      };
      return prefix is "big" or "Big" or "bigg" or "Bigg";
    }

    public string ReadString() {
      var builder = new StringBuilder();
      while (HasCharacters) {
        var ch = ReadChar();
        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
          builder.Append(ch.ToStringInvariant());
        } else {
          UndoReadChar();
          break;
        }
      }
      return builder.ToString();
    }

    public Result<Color> ReadColor() {
      if (!ReadCharIfAvailable('{')) {
        return "Missing {";
      }
      SkipSpaces();
      // Read the entire token up to the closing brace so invalid inputs produce a
      // clear "Invalid color" error instead of a confusing "Missing }".
      var index = NextChar;
      var length = 0;
      while (HasCharacters) {
        var ch = ReadChar();
        if (ch == '}') {
          UndoReadChar();
          break;
        }
        length++;
      }
      var str = Chars.Substring(index, length);
      if (ParseColorStrict(str) is not Color color) {
        return "Invalid color: " + str;
      }
      SkipSpaces();
      if (!ReadCharIfAvailable('}')) {
        return "Missing }";
      }
      return color;
    }

    /// <summary>Validates a color token: '#' followed by exactly 3, 6 or 8 hex digits
    /// (#RGB expands like CSS), or one of the predefined named colors. Everything
    /// else is an error rather than a silent no-op.</summary>
    static Color? ParseColorStrict(string str) =>
      LaTeXSettings.ParseColor(str);

    public void SkipSpaces() {
      while (HasCharacters) {
        var ch = ReadChar();
        if (char.IsWhiteSpace(ch) || char.IsControl(ch)) {
          continue;
        } else {
          UndoReadChar();
          return;
        }
      }
    }

    private static void AssertNotSpace(char ch) {
      if (char.IsWhiteSpace(ch) || char.IsControl(ch)) {
        //throw since this is not normal
        throw new InvalidOperationException("Expected non space character; found " + ch);
      }
    }

    /// <summary>Advances <see cref="NextChar"/> if <paramref name="ch"/> is available.</summary>
    /// <returns>Whether the char was read.</returns>
    public bool ReadCharIfAvailable(char ch) {
      AssertNotSpace(ch);
      SkipSpaces();
      if (HasCharacters) {
        var c = ReadChar();
        AssertNotSpace(c);
        if (c == ch) {
          return true;
        } else {
          UndoReadChar();
          return false;
        }
      }
      return false;
    }

    /// <summary>Restores the read position, so the caller can dispatch without consuming.</summary>
    public void UndoTo(int position) => NextChar = position;

    public Result<string> ReadEnvironment() {
      if (!ReadCharIfAvailable('{')) {
        return Err("Missing {");
      }
      SkipSpaces();
      var env = ReadString();
      SkipSpaces();
      if (!ReadCharIfAvailable('}')) {
        return Err("Missing }");
      }
      return Ok(env);
    }
    public Result<Atom.Length> ReadSpace() {
      SkipSpaces();
      var sb = new StringBuilder();
      while (HasCharacters) {
        var ch = ReadChar();
        if (char.IsDigit(ch) || ch == '.' || ch == '-' || ch == '+') {
          sb.Append(ch);
        } else {
          UndoReadChar();
          break;
        }
      }
      var length = sb.ToString();
      if (string.IsNullOrEmpty(length)) {
        return "Expected length value";
      }
      SkipSpaces();
      var unit = new char[2];
      for (int i = 0; i < 2 && HasCharacters; i++) {
        unit[i] = ReadChar();
      }
      if (!HasCharacters && unit[1] == default) {
        // The input ended inside the two-character unit.
        return "Expected two-character length unit";
      }
      return Atom.Length.Create(length, new string(unit), TextMode);
    }

    /// <summary>The fraction macro table: command name → (hasRule, styleOverride,
    /// delimiters, continued, acceptsAlign). Shared by \frac \binom \dfrac \tfrac
    /// \dbinom \tbinom \cfrac.</summary>
    private static readonly Dictionary<string,
      (bool hasRule, Atoms.FractionStyle style, string? leftDelim, string? rightDelim,
       bool continued, bool acceptsAlign)> FractionMacros =
      new[] {
        ("frac",   (true,  Atoms.FractionStyle.Auto,    (string?)null, null, false, false)),
        ("binom",  (false, Atoms.FractionStyle.Auto,    "(",           ")",  false, false)),
        ("dfrac",  (true,  Atoms.FractionStyle.Display, (string?)null, null, false, false)),
        ("tfrac",  (true,  Atoms.FractionStyle.Text,    (string?)null, null, false, false)),
        ("dbinom", (false, Atoms.FractionStyle.Display, "(",           ")",  false, false)),
        ("tbinom", (false, Atoms.FractionStyle.Text,    "(",           ")",  false, false)),
        ("cfrac",  (true,  Atoms.FractionStyle.Display, (string?)null, null, true,  true)),
      }.ToDictionary(t => t.Item1, t => t.Item2);

    internal static Result<(MathAtom?, MathList?)> FractionMacro(LaTeXParser parser, string command) {
      var spec = FractionMacros[command];
      var fraction = new Atoms.Fraction(new MathList(), new MathList(), spec.hasRule) {
        StyleOverride = spec.style
      };
      if (spec.acceptsAlign && parser.HasCharacters) {
        // Optional [l|c|r] alignment argument for \cfrac.
        var saveChar = parser.NextChar;
        if (parser.ReadCharIfAvailable('[')) {
          if (!parser.HasCharacters) {
            return @"Unterminated optional alignment for \cfrac";
          }
          var letter = parser.ReadChar();
          switch (letter) {
            case 'l':
              fraction.NumeratorAlignment = Atoms.FractionAlignment.Left;
              break;
            case 'c':
              fraction.NumeratorAlignment = Atoms.FractionAlignment.Center;
              break;
            case 'r':
              fraction.NumeratorAlignment = Atoms.FractionAlignment.Right;
              break;
            default:
              return $@"Invalid alignment for \cfrac: '{letter}' (expected l, c, or r)";
          }
          if (!parser.ReadCharIfAvailable(']')) {
            return @"Unterminated optional alignment for \cfrac";
          }
        } else {
          parser.UndoTo(saveChar);
        }
      }
      if (spec.continued) {
        fraction.IsContinuedFraction = true;
      }
      var (numerator, error) = parser.ReadArgument();
      if (error != null) return error;
      var (denominator, error2) = parser.ReadArgument();
      if (error2 != null) return error2;
      fraction.Numerator.Clear();
      fraction.Numerator.Append(numerator);
      fraction.Denominator.Clear();
      fraction.Denominator.Append(denominator);
      if (spec.leftDelim != null) {
        fraction.LeftDelimiter = new Boundary(spec.leftDelim);
        fraction.RightDelimiter = new Boundary(spec.rightDelim!);
      }
      return LaTeXSettings.Ok(fraction);
    }

    /// <summary>The over/under stack commands: static extensible rows plus the four
    /// MathList-row commands. argRoles lists which arguments are read in order.</summary>
    private enum StackArgRole { Base, Over, Under }

    private sealed class StackCommandSpec {
      public StackConstruction? OverConstruction;
      public StackConstruction? UnderConstruction;
      public System.Type DisplayClassType = typeof(Ordinary);
      public bool InheritsClass;
      public StackArgRole[] ArgRoles = { StackArgRole.Base };
    }

    private static readonly Dictionary<string, StackCommandSpec>
      StackCommands = BuildStackCommands();

    private static Dictionary<string, StackCommandSpec>
        BuildStackCommands() {
      var specs = new System.Collections.Generic.Dictionary<string, StackCommandSpec> {
        ["overrightarrow"] = new StackCommandSpec {
          OverConstruction = new StackConstruction.Extensible("→")
        },
        ["overleftarrow"] = new StackCommandSpec {
          OverConstruction = new StackConstruction.Extensible("←")
        },
        ["overleftrightarrow"] = new StackCommandSpec {
          OverConstruction = new StackConstruction.Extensible("↔")
        },
        ["underrightarrow"] = new StackCommandSpec {
          UnderConstruction = new StackConstruction.Extensible("→")
        },
        ["underleftarrow"] = new StackCommandSpec {
          UnderConstruction = new StackConstruction.Extensible("←")
        },
        ["underleftrightarrow"] = new StackCommandSpec {
          UnderConstruction = new StackConstruction.Extensible("↔")
        },
        ["overbrace"] = new StackCommandSpec {
          OverConstruction = new StackConstruction.Extensible("⏞")
        },
        // \underbrace keeps its pre-port UnderAnnotation registration (its _
        // attaches an under-list), so it is deliberately absent here.
        ["overset"] = new StackCommandSpec {
          InheritsClass = true,
          ArgRoles = new[] { StackArgRole.Over, StackArgRole.Base }
        },
        ["underset"] = new StackCommandSpec {
          InheritsClass = true,
          ArgRoles = new[] { StackArgRole.Under, StackArgRole.Base }
        },
        ["stackrel"] = new StackCommandSpec {
          DisplayClassType = typeof(Relation),
          ArgRoles = new[] { StackArgRole.Over, StackArgRole.Base }
        },
        ["stackbin"] = new StackCommandSpec {
          DisplayClassType = typeof(BinaryOperator),
          ArgRoles = new[] { StackArgRole.Over, StackArgRole.Base }
        },
      };
      return specs;
    }

    /// <summary>\overset/\underset inherit a lone Bin/Rel base atom's intrinsic class
    /// (read before finalize's Bin→Ord reclassification, matching amsmath \binrel@).</summary>
    private static System.Type InheritedDisplayClassForBase(MathList baseList) =>
      baseList.Count == 1 && baseList[0] is var only
      && (only is BinaryOperator || only is Relation)
        ? only.GetType()
        : typeof(Ordinary);

    internal static Result<(MathAtom?, MathList?)> StackAtomForCommand(LaTeXParser parser, string command) {
      if (!StackCommands.TryGetValue(command, out var spec)) {
        return LaTeXSettings.Ok(null);
      }
      var stack = new Atoms.Stack {
        Over = spec.OverConstruction?.Clone(false),
        Under = spec.UnderConstruction?.Clone(false)
      };
      foreach (var role in spec.ArgRoles) {
        var (arg, error) = parser.ReadArgument();
        if (error != null) return error;
        switch (role) {
          case StackArgRole.Base:
            stack.InnerList = arg;
            break;
          case StackArgRole.Over:
            stack.Over = new StackConstruction.MathListRow(arg);
            break;
          case StackArgRole.Under:
            stack.Under = new StackConstruction.MathListRow(arg);
            break;
        }
      }
      stack.DisplayClassType =
        spec.InheritsClass ? InheritedDisplayClassForBase(stack.InnerList) : spec.DisplayClassType;
      return LaTeXSettings.Ok(stack);
    }

    /// <summary>The box commands: phantom/smash/lap plus the cancel family.
    /// Tuple: keepWidth, keepHeight, keepDepth, drawChild, hAlign, acceptsTB, synthParen.</summary>
    private static readonly Dictionary<string,
      (bool kW, bool kH, bool kD, bool draw, BoxHAlign hAlign, bool acceptsTB, bool synthParen)>
      BoxCommands = new[] {
        ("phantom",   (true,  true,  true,  false, BoxHAlign.Left,   false, false)),
        ("hphantom",  (true,  false, false, false, BoxHAlign.Left,   false, false)),
        ("vphantom",  (false, true,  true,  false, BoxHAlign.Left,   false, false)),
        ("mathstrut", (false, true,  true,  false, BoxHAlign.Left,   false, true)),
        ("smash",     (true,  false, false, true,  BoxHAlign.Left,   true,  false)),
        ("llap",      (false, true,  true,  true,  BoxHAlign.Right,  false, false)),
        ("rlap",      (false, true,  true,  true,  BoxHAlign.Left,   false, false)),
        ("clap",      (false, true,  true,  true,  BoxHAlign.Center, false, false)),
        ("mathllap",  (false, true,  true,  true,  BoxHAlign.Right,  false, false)),
        ("mathrlap",  (false, true,  true,  true,  BoxHAlign.Left,   false, false)),
        ("mathclap",  (false, true,  true,  true,  BoxHAlign.Center, false, false)),
        ("cancel",    (true,  true,  true,  true,  BoxHAlign.Left,   false, false)),
        ("bcancel",   (true,  true,  true,  true,  BoxHAlign.Left,   false, false)),
        ("xcancel",   (true,  true,  true,  true,  BoxHAlign.Left,   false, false)),
        ("sout",      (true,  true,  true,  true,  BoxHAlign.Left,   false, false)),
      }.ToDictionary(t => t.Item1, t => t.Item2);

    private static readonly Dictionary<string, StrikeStyle>
      CancelStyles = new Dictionary<string, StrikeStyle> {
        ["cancel"] = StrikeStyle.Forward,
        ["bcancel"] = StrikeStyle.Backward,
        ["xcancel"] = StrikeStyle.Cross,
        ["sout"] = StrikeStyle.Horizontal,
      };

    internal static Result<(MathAtom?, MathList?)> BoxAtomForCommand(LaTeXParser parser, string command) {
      if (!BoxCommands.TryGetValue(command, out var spec)) {
        return LaTeXSettings.Ok(null);
      }
      var box = new Atoms.Box {
        KeepWidth = spec.kW,
        KeepHeight = spec.kH,
        KeepDepth = spec.kD,
        DrawChild = spec.draw,
        HAlign = spec.hAlign
      };
      if (CancelStyles.TryGetValue(command, out var strike)) {
        box.StrikeStyle = strike;
      }
      if (spec.synthParen) {
        // \mathstrut: no argument; synthetic inner list with a single open paren.
        box.InnerList.Add(new Open("("));
        return LaTeXSettings.Ok(box);
      }
      if (spec.acceptsTB && parser.HasCharacters) {
        // \smash[t]/[b]: optional [t]/[b] before the {X} argument.
        var saveChar = parser.NextChar;
        if (parser.ReadCharIfAvailable('[')) {
          var opt = new StringBuilder();
          var foundClose = false;
          while (parser.HasCharacters) {
            var c = parser.ReadChar();
            if (c == ']') { foundClose = true; break; }
            opt.Append(c);
          }
          if (!foundClose) {
            return "Expected character not found: ]";
          }
          switch (opt.ToString().Trim()) {
            case "t":
              box.KeepHeight = false;
              box.KeepDepth = true;
              break;
            case "b":
              box.KeepHeight = true;
              box.KeepDepth = false;
              break;
              // any other value: ignore bracket, smash both, no crash
          }
        } else {
          parser.UndoTo(saveChar);
        }
      }
      var (innerList, error) = parser.ReadArgument();
      if (error != null) return error;
      box.InnerList.Append(innerList);
      return LaTeXSettings.Ok(box);
    }
    public Result<Boundary> ReadDelimiter(string commandName) {
      if (!HasCharacters) {
        return @"Missing delimiter for \" + commandName;
      }
      SkipSpaces();
      var ((result, splitIndex), error) = LaTeXSettings.BoundaryDelimiters.TryLookup(Chars.AsSpan(NextChar));
      if (error != null) {
        NextChar++; // Point to the start of the erroneous command
        return error;
      }
      NextChar += splitIndex;
      return result;
    }

    private static readonly Dictionary<string, (string left, string right)?> _matrixEnvironments =
      new Dictionary<string, (string left, string right)?> {
        { "matrix",  null } ,
        { "pmatrix", ("(", ")") } ,
        { "bmatrix", ("[", "]") },
        { "Bmatrix", ("{", "}") },
        { "vmatrix", ("|", "|") },
        { "Vmatrix", ("‖", "‖") }
      };
    /// <summary>Environments that take a mandatory raw `{…}` argument after \begin{env}
    /// (alignedat's {n}, array's column spec).</summary>
    private static readonly HashSet<string> _environmentsTakingArgument =
      new HashSet<string> { "alignedat", "array" };
    /// <summary>Environments that permit \hline row-boundary markers.</summary>
    private static readonly HashSet<string> _environmentsAllowingHorizontalLines =
      new HashSet<string> { "array" };
    public Result<MathAtom> ReadTable
      (string? name, MathList? firstList, bool isRow, char stopChar) {
      var environment = new TableEnvironment(name);
      Environments.Push(environment);
      int currentRow = 0;
      int currentColumn = 0;
      var rows = new List<List<MathList>> { new List<MathList>() };
      if (firstList != null) {
        rows[currentRow].Add(firstList);
        if (isRow) {
          environment.NRows++;
          currentRow++;
          rows.Add(new List<MathList>());
        } else {
          currentColumn++;
        }
      }
      if (environment.Name != null && _environmentsTakingArgument.Contains(environment.Name)) {
        // Raw {…} argument after \begin{env}: the {n} of alignedat or the column
        // spec of array. Any character up to the matching } is captured raw.
        if (!ReadCharIfAvailable('{')) {
          return environment.Name == "array"
            ? "Missing array alignment"
            : $@"{environment.Name} requires a numeric argument, e.g. \begin{{{environment.Name}}}{{2}}";
        }
        var builder = new StringBuilder();
        var done = false;
        while (HasCharacters && !done) {
          var ch = ReadChar();
          if (ch == '}') done = true;
          else builder.Append(ch);
        }
        if (!done) {
          return "Missing }";
        }
        environment.ArrayAlignments = builder.ToString();
        if (environment.Name == "array") {
          var alignmentCount = 0;
          foreach (var specChar in environment.ArrayAlignments) {
            if (specChar is 'l' or 'c' or 'r') alignmentCount++;
            else if (specChar != '|' && !char.IsWhiteSpace(specChar))
              return $"Invalid array alignment character '{specChar}'";
          }
          if (alignmentCount == 0) return "Array alignment must contain at least one column";
        }
      }
      // Record \hline at the current row boundary. Emits no atom; only valid in array.
      void RecordHorizontalLine() {
        while (environment.HorizontalLines.Count <= environment.NRows)
          environment.HorizontalLines.Add(0);
        environment.HorizontalLines[environment.NRows]++;
      }
      while (HasCharacters && !environment.Ended) {
        // \hline may appear at any row/cell boundary (and repeatedly); it emits no
        // atom and only counts the boundary. Skip whitespace before peeking so
        // "\hline a" and "\\ \hline b" both work.
        var saveChar = NextChar;
        SkipSpaces();
        if (HasCharacters && Chars[NextChar] == '\\') {
          NextChar++;
          var command = ReadCommandName();
          if (command == "hline") {
            if (!_environmentsAllowingHorizontalLines.Contains(environment.Name ?? string.Empty)) {
              return @"\hline is only valid inside an array environment";
            }
            RecordHorizontalLine();
            continue;
          }
          UndoTo(saveChar);
        } else {
          UndoTo(saveChar);
        }
        var (list, error) = BuildInternal(false, stopChar);
        if (error != null) return error;
        rows[currentRow].Add(list);
        currentColumn++;
        if (environment.NRows > currentRow) {
          currentRow = environment.NRows;
          rows.Add(new List<MathList>());
          currentColumn = 0;
        }
        // The } in \begin{matrix} is not stopChar so this line is not written in the while-condition
        if (stopChar != '\0' && Chars[NextChar - 1] == stopChar) break;
      }
      if (environment.Name != null && !environment.Ended) {
        return $@"Missing \end for \begin{{{environment.Name}}}";
      }

      if (environment.Name == "array" && rows.Count > 0
        && rows[rows.Count - 1].All(cell => cell.Count == 0)) {
        // A trailing \\ opens the boundary needed for a bottom \hline. If no
        // content follows it, discard that synthetic row while retaining the rule.
        rows.RemoveAt(rows.Count - 1);
      }

      // We have finished parsing the table, now interpret the environment
      name = environment.Name;
      var arrayAlignments = environment.ArrayAlignments;
      var horizontalLines = environment.HorizontalLines;
      // Table environments with { Name: null } may have been popped by \right
      if (Environments.PeekOrDefault() == environment)
        Environments.Pop();

      var table = new Table(name, rows);
      switch (name) {
        case null:
          table.InterRowAdditionalSpacing = 1;
          for (int i = 0; i < table.NColumns; i++) {
            table.SetAlignment(ColumnAlignment.Left, i);
          }
          return table;
        case var _ when _matrixEnvironments.TryGetValue(name, out var delimiters):
          table.Environment = "matrix"; // TableEnvironment is set to matrix as delimiters are converted to latex outside the table.
          table.InterColumnSpacing = 18;
          for (var r = 0; r < table.Cells.Count; r++) {
            var logicalColumn = 0;
            for (var c = 0; c < table.Cells[r].Count; c++, logicalColumn++)
              if (table.Cells[r][c].Count == 1 && table.Cells[r][c][0] is MulticolumnAtom multi) {
                table.Cells[r][c] = multi.Content;
                table.SetColumnSpan(r, c, multi.Span, multi.Alignment, multi.Specification);
                logicalColumn += multi.Span - 1;
              }
          }
          // All the cells render in textstyle, stored on the table rather than per-cell.
          table.CellStyle = LineStyle.Text;
          return delimiters switch {
            (var left, var right) => new Inner(
              new Boundary(left),
              new MathList(table),
              new Boundary(right)
            ),
            null => table
          };
        case "array": {
            if (arrayAlignments is null)
              throw new InvalidCodePathException("arrayAlignments is null despite array environment");
            table.InterRowAdditionalSpacing = 0;
            table.InterColumnSpacing = 18;
            // Cells render in textstyle via the table (post-#245).
            table.CellStyle = LineStyle.Text;
            // Interpret the column spec: l/c/r append an alignment and open a boundary
            // slot; | increments the current (rightmost) boundary slot.
            var vLines = new List<int> { 0 };
            foreach (var specChar in arrayAlignments) {
              switch (specChar) {
                case 'l':
                  table.SetAlignment(ColumnAlignment.Left, table.Alignments.Count);
                  vLines.Add(0);
                  break;
                case 'c':
                  table.SetAlignment(ColumnAlignment.Center, table.Alignments.Count);
                  vLines.Add(0);
                  break;
                case 'r':
                  table.SetAlignment(ColumnAlignment.Right, table.Alignments.Count);
                  vLines.Add(0);
                  break;
                case '|':
                  vLines[vLines.Count - 1]++;
                  break;
              }
            }
            var declaredColumns = table.Alignments.Count;
            for (var r = 0; r < table.Cells.Count; r++) {
              var logicalColumn = 0;
              for (var c = 0; c < table.Cells[r].Count; c++, logicalColumn++) {
                if (table.Cells[r][c].Count != 1 || table.Cells[r][c][0] is not MulticolumnAtom multi) continue;
                if (multi.Span > declaredColumns - logicalColumn)
                  return @"\multicolumn span exceeds the array column count or overlaps another cell";
                table.Cells[r][c] = multi.Content;
                table.SetColumnSpan(r, c, multi.Span, multi.Alignment, multi.Specification);
                logicalColumn += multi.Span - 1;
              }
              // Preserve the historical array behavior: ordinary cells beyond
              // the declared specification are tolerated and dropped by the
              // renderer.  Multicolumn spans remain strictly validated above.
            }
            // Note: rows may declare fewer/more cells than the spec; extra cells are
            // dropped and missing ones render empty, matching pre-port behavior.
            while (vLines.Count < table.NColumns + 1) vLines.Add(0);
            table.VerticalLines = vLines;
            while (horizontalLines.Count < table.NRows + 1) horizontalLines.Add(0);
            table.HorizontalLines = horizontalLines;
            return table;
          }
        case "smallmatrix":
          // Compact inline matrix: script-style cells, no delimiters, center default,
          // \thickspace = 5mu inter-column gap scaled to the Script cell style at layout.
          table.InterRowAdditionalSpacing = 0;
          table.InterColumnSpacing = 5;
          table.CellStyle = LineStyle.Script;
          return table;
        case "eqalign":
        case "split":
        case "aligned":
          if (table.NColumns != 2) {
            return name + " environment can only have 2 columns";
          } else {
            // add a spacer before each of the second column elements, in order to create the correct spacing for "=" and other relations.
            var spacer = new Ordinary(string.Empty);
            foreach (var row in table.Cells) {
              if (row.Count > 1) {
                row[1].Insert(0, spacer.Clone(false));
              }
            }
            table.InterRowAdditionalSpacing = 1;
            table.SetAlignment(ColumnAlignment.Right, 0);
            table.SetAlignment(ColumnAlignment.Left, 1);
            return table;
          }
        case "alignedat": {
            // Generalization of aligned to n alignment pairs (2n columns). The raw
            // argument is the declared pair count; require a positive integer and a
            // column count that matches it.
            var argument = (arrayAlignments ?? "").Trim();
            bool numeric = argument.Length > 0;
            foreach (var c in argument) numeric &= c >= '0' && c <= '9';
            if (!numeric || !int.TryParse(argument, out var pairs) || pairs < 1) {
              return @"alignedat requires a numeric argument, e.g. \begin{alignedat}{2}";
            }
            if (table.NColumns != 2 * pairs) {
              return $@"alignedat declares {{{pairs}}} ({2 * pairs} columns) but a row has {table.NColumns} columns";
            }
            // Relation spacer before each odd column for correct = / relation spacing.
            var spacer = new Ordinary(string.Empty);
            foreach (var row in table.Cells) {
              for (int j = 1; j < row.Count; j += 2) {
                row[j].Insert(0, spacer.Clone(false));
              }
            }
            table.InterRowAdditionalSpacing = 1;
            table.InterColumnSpacing = 0;
            for (int j = 0; j < table.NColumns; j++) {
              table.SetAlignment(
                j % 2 == 0 ? ColumnAlignment.Right : ColumnAlignment.Left, j);
            }
            return table;
          }
        case "displaylines":
        case "gather":
        case "gathered":
          if (table.NColumns != 1) {
            return name + " environment can only have 1 column";
          }
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 0;
          table.SetAlignment(ColumnAlignment.Center, 0);
          return table;
        case "eqnarray":
          if (table.NColumns != 3) {
            return name + " must have exactly 3 columns";
          } else {
            table.InterRowAdditionalSpacing = 1;
            table.InterColumnSpacing = 18;
            table.SetAlignment(ColumnAlignment.Right, 0);
            table.SetAlignment(ColumnAlignment.Center, 1);
            table.SetAlignment(ColumnAlignment.Left, 2);
            return table;
          }
        case "cases":
          if (table.NColumns < 1 || table.NColumns > 2) {
            return "cases environment must have 1 to 2 columns";
          } else {
            table.Environment = "array";
            table.InterRowAdditionalSpacing = 1;
            table.InterColumnSpacing = 18;
            table.CellStyle = LineStyle.Text;
            table.SetAlignment(ColumnAlignment.Left, 0);
            if (table.NColumns == 2) table.SetAlignment(ColumnAlignment.Left, 1);
            // The evaluator's piecewise reader keys on per-cell \textstyle atoms, so
            // cases (unlike matrix) keeps the injected style atom alongside CellStyle.
            var textStyle = new Style(LineStyle.Text);
            foreach (var row in table.Cells) {
              foreach (var cell in row) {
                cell.Insert(0, textStyle);
              }
            }
            // add delimiters
            return new Inner(
              new Boundary("{"),
              new MathList(new Atoms.Space(Atom.Length.ShortSpace), table),
              Boundary.Empty
            );
          }
        default:
          return "Unknown environment " + name;
      }
    }

    public static Result<MathList> MathListFromLaTeX(string str) {
      var builder = new LaTeXParser(str);
      return builder.Build().Match(Ok,
        error => Err(HelpfulErrorMessage(error, builder.Chars, builder.NextChar)));
    }

    public static string HelpfulErrorMessage(string error, string source, int right) {
      if (right <= 0) right = 1;
      // Just like Xunit's helpful error message in Assert.Equal(string, string)
      const string dots = "···";
      const int lookbehind = 20;
      const int lookahead = 41;
      var sb = new StringBuilder("Error: ").Append(error);
      sb.Append('\n');
      var left = right - 1;
      var startIsFarAway = left > lookbehind;
      if (startIsFarAway)
        sb.Append(dots).Append(source, left - lookbehind, lookbehind);
      else sb.Append(source, 0, left);
      var endIsFarAway = left < source.Length - lookahead;
      if (endIsFarAway)
        sb.Append(source, left, lookahead).Append(dots);
      else sb.Append(source, left, source.Length - left);
      sb.Append('\n');
      if (startIsFarAway)
        sb.Append(' ', lookbehind + dots.Length);
      else sb.Append(' ', left);
      sb.Append("↑ (pos ").Append(right).Append(')');
      return sb.ToString();
    }

    // ^ LaTeX -> Math atoms
    // v Math atoms -> LaTeX

    public static string EscapeAsLaTeX(string literal) =>
      new StringBuilder(literal)
      .Replace("{", @"\{")
      .Replace("}", @"\}")
      .Replace(@"\", @"\backslash ")
      .Replace("#", @"\#")
      .Replace("$", @"\$")
      .Replace("%", @"\%")
      .Replace("&", @"\&")
      .Replace("^", @"\textasciicircum ")
      .Replace("_", @"\_")
      .Replace("~", @"\textasciitilde ")
      .ToString();

    static string BoundaryToLaTeX(Boundary delimiter) =>
      delimiter.Nucleus switch {
        "\u27E8" => "<",
        "\u27E9" => ">",
        _ => LaTeXSettings.BoundaryDelimitersReverse.TryGetValue(delimiter, out var command)
          ? command : delimiter.Nucleus ?? ""
      };

    /// <summary>Closest LaTeX command for a box variant (cancel / phantom / lap / smash),
    /// picked from the flag matrix.</summary>
    static string BoxCommandName(Atoms.Box box) {
      if (box.StrikeStyle != Atoms.StrikeStyle.None) {
        return box.StrikeStyle switch {
          Atoms.StrikeStyle.Forward => "cancel",
          Atoms.StrikeStyle.Backward => "bcancel",
          Atoms.StrikeStyle.Cross => "xcancel",
          Atoms.StrikeStyle.Horizontal => "sout",
          _ => throw new InvalidCodePathException("Unknown strike style"),
        };
      }
      if (!box.DrawChild) {
        // phantom family
        return
          box.KeepWidth && box.KeepHeight && box.KeepDepth ? "phantom" :
          box.KeepWidth ? "hphantom" : "vphantom";
      }
      if (!box.KeepWidth) {
        // lap family
        return box.HAlign switch {
          Atoms.BoxHAlign.Right => "llap",
          Atoms.BoxHAlign.Center => "clap",
          _ => "rlap"
        };
      }
      // smash family
      return
        !box.KeepHeight && !box.KeepDepth ? "smash" :
        box.KeepDepth ? @"smash[t]" : @"smash[b]";
    }

    private static void MathListToLaTeX
      (MathList mathList, StringBuilder builder, FontStyle outerFontStyle) {
      static bool MathAtomToLaTeX(MathAtom atom, StringBuilder builder,
#if !NETSTANDARD2_0 && !NET45
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
#endif
        out string? command) {
        if (LaTeXSettings.CommandForAtom(atom) is string name) {
          command = name;
          builder.Append(name);
          if (name.AsSpan().StartsWithInvariant(@"\"))
            builder.Append(' ');
          return true;
        }
        command = null;
        return false;
      }

      if (mathList is null) throw new ArgumentNullException(nameof(mathList));
      if (mathList.IsEmpty()) return;
      var currentFontStyle = outerFontStyle;
      foreach (var atom in mathList) {
        if (currentFontStyle != atom.FontStyle) {
          if (currentFontStyle != outerFontStyle) {
            // close the previous font style
            builder.Append('}');
          }
          if (atom.FontStyle != outerFontStyle) {
            // open a new font style
            builder.Append('\\').Append(LaTeXSettings.FontStyles.SecondToFirst[atom.FontStyle]).Append('{');
          }
        }
        currentFontStyle = atom.FontStyle;
        switch (atom) {
          case Comment { Nucleus: var comment }:
            builder.Append('%').Append(comment).Append('\n');
            break;
          case Fraction fraction: {
              // Style overrides serialize as \displaystyle/\textstyle wrapping each
              // operand rather than emitting \dfrac directly (lossy round trip).
              static string Wrap(MathList operand, Atoms.FractionStyle style) {
                var inner = new StringBuilder();
                MathListToLaTeX(operand, inner, FontStyle.Default);
                return style switch {
                  Atoms.FractionStyle.Display => @"\displaystyle{" + inner + "}",
                  Atoms.FractionStyle.Text => @"\textstyle{" + inner + "}",
                  _ => inner.ToString()
                };
              }
              if (fraction.HasRule) {
                builder.Append(@"\frac{")
                  .Append(Wrap(fraction.Numerator, fraction.StyleOverride))
                  .Append("}{")
                  .Append(Wrap(fraction.Denominator, fraction.StyleOverride))
                  .Append('}');
              } else {
                builder.Append('{');
                MathListToLaTeX(fraction.Numerator, builder, currentFontStyle);
                builder.Append(@" \").Append(
                  (fraction.LeftDelimiter, fraction.RightDelimiter) switch {
                    ( { Nucleus: null }, { Nucleus: null }) => "atop",
                    ( { Nucleus: "(" }, { Nucleus: ")" }) => "choose",
                    ( { Nucleus: "{" }, { Nucleus: "}" }) => "brace",
                    ( { Nucleus: "[" }, { Nucleus: "]" }) => "brack",
                    (var left, var right) => $"atopwithdelims{BoundaryToLaTeX(left)}{BoundaryToLaTeX(right)}",
                  }).Append(' ');
                MathListToLaTeX(fraction.Denominator, builder, currentFontStyle);
                builder.Append('}');
              }
              break;
            }
          case Radical radical:
            builder.Append(@"\sqrt");
            if (radical.Degree.IsNonEmpty()) {
              builder.Append('[');
              MathListToLaTeX(radical.Degree, builder, currentFontStyle);
              builder.Append(']');
            }
            builder.Append('{');
            MathListToLaTeX(radical.Radicand, builder, currentFontStyle);
            builder.Append('}');
            break;
          case Inner { LeftBoundary: { Nucleus: null }, InnerList: var list, RightBoundary: { Nucleus: null } }:
            MathListToLaTeX(list, builder, currentFontStyle);
            break;
          case Inner { LeftBoundary: { Nucleus: "〈" }, InnerList: var list, RightBoundary: { Nucleus: "|" } }:
            builder.Append(@"\Bra{");
            MathListToLaTeX(list, builder, currentFontStyle);
            builder.Append('}');
            break;
          case Inner { LeftBoundary: { Nucleus: "|" }, InnerList: var list, RightBoundary: { Nucleus: "〉" } }:
            builder.Append(@"\Ket{");
            MathListToLaTeX(list, builder, currentFontStyle);
            builder.Append('}');
            break;
          case Inner { LeftBoundary: var left, InnerList: var list, RightBoundary: var right }:
            builder.Append(@"\left").Append(BoundaryToLaTeX(left)).Append(' ');
            MathListToLaTeX(list, builder, currentFontStyle);
            builder.Append(@"\right").Append(BoundaryToLaTeX(right)).Append(' ');
            break;
          case Atoms.Stack stack: {
              // MathList-row stacks emit \overset/\underset/\stackrel/\stackbin;
              // extensible (stretchy) stacks emit their canonical command; a stack
              // with both MathList rows emits nested \underset{…}{\overset{…}{…}}.
              static string? StackCommandName(Atoms.Stack s) {
                bool overML = s.Over is StackConstruction.MathListRow;
                bool underML = s.Under is StackConstruction.MathListRow;
                if (overML && underML) return null;
                if (underML) return "underset";
                if (overML) return s.DisplayClassType == typeof(Relation) ? "stackrel"
                  : s.DisplayClassType == typeof(BinaryOperator) ? "stackbin" : "overset";
                if (s.Over is StackConstruction.Extensible o) return ExtensibleCommand(o.Glyph, over: true);
                if (s.Under is StackConstruction.Extensible u) return ExtensibleCommand(u.Glyph, over: false);
                return null;
              }
              static string? ExtensibleCommand(string glyph, bool over) => (glyph, over) switch {
                ("→", true) => "overrightarrow",
                ("←", true) => "overleftarrow",
                ("↔", true) => "overleftrightarrow",
                ("⏞", true) => "overbrace",
                ("→", false) => "underrightarrow",
                ("←", false) => "underleftarrow",
                ("↔", false) => "underleftrightarrow",
                ("⏟", false) => "underbrace",
                _ => null
              };
              var name = StackCommandName(stack);
              if (stack.Over is StackConstruction.MathListRow bothOver
                && stack.Under is StackConstruction.MathListRow bothUnder) {
                builder.Append(@"\underset{");
                MathListToLaTeX(bothUnder.List, builder, currentFontStyle);
                builder.Append(@"}{\overset{");
                MathListToLaTeX(bothOver.List, builder, currentFontStyle);
                builder.Append("}{");
                MathListToLaTeX(stack.InnerList, builder, currentFontStyle);
                builder.Append("}}");
              } else if (name == null) {
                // Programmatically-built stack with non-canonical rows: emit only the inner list.
                MathListToLaTeX(stack.InnerList, builder, currentFontStyle);
              } else if (stack.Over is StackConstruction.MathListRow overRow) {
                builder.Append('\\').Append(name).Append('{');
                MathListToLaTeX(overRow.List, builder, currentFontStyle);
                builder.Append("}{");
                MathListToLaTeX(stack.InnerList, builder, currentFontStyle);
                builder.Append('}');
              } else if (stack.Under is StackConstruction.MathListRow underRow) {
                builder.Append('\\').Append(name).Append('{');
                MathListToLaTeX(underRow.List, builder, currentFontStyle);
                builder.Append("}{");
                MathListToLaTeX(stack.InnerList, builder, currentFontStyle);
                builder.Append('}');
              } else {
                builder.Append('\\').Append(name).Append('{');
                MathListToLaTeX(stack.InnerList, builder, currentFontStyle);
                builder.Append('}');
              }
              break;
            }
          case Atoms.Box box: {
              builder.Append('\\').Append(BoxCommandName(box)).Append('{');
              MathListToLaTeX(box.InnerList, builder, currentFontStyle);
              builder.Append('}');
              break;
            }
          case Atoms.Group group:
            // Always emit the braces: a Group is an Ord subformula, and dropping
            // them would make serialization non-idempotent (iosMath 086d345).
            builder.Append('{');
            MathListToLaTeX(group.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case Atoms.Macro macro:
            builder.Append('\\').Append(macro.Command);
            if (macro.Arguments.Count == 0) {
              // Nothing would terminate the command name otherwise.
              builder.Append(' ');
            }
            foreach (var argument in macro.Arguments) {
              builder.Append('{').Append(argument).Append('}');
            }
            break;
          case Atoms.LargeDelimiter large:
            var prefix = large.Size switch {
              Atoms.LargeDelimiter.DelimiterSize.Size1 => "big",
              Atoms.LargeDelimiter.DelimiterSize.Size2 => "Big",
              Atoms.LargeDelimiter.DelimiterSize.Size3 => "bigg",
              _ => "Bigg"
            };
            var suffix = large.MathClass == typeof(Atoms.Open) ? "l" :
              large.MathClass == typeof(Atoms.Close) ? "r" :
              large.MathClass == typeof(Atoms.Relation) ? "m" : "";
            builder.Append('\\').Append(prefix).Append(suffix)
              .Append(large.Nucleus.Length == 0 ? "." : BoundaryToLaTeX(new Boundary(large.Nucleus)));
            break;
          case Table table:
            if (table.Environment != null) {
              builder.Append(@"\begin{" + table.Environment + "}");
              if (table.Environment == "alignedat") {
                builder.Append('{').Append(table.NColumns / 2).Append('}');
              } else if (table.Environment == "array") {
                // Reconstruct the column spec: |count then l/c/r per column.
                builder.Append('{');
                for (int i = 0; i <= table.NColumns; i++) {
                  for (int k = i < table.VerticalLines.Count ? table.VerticalLines[i] : 0; k > 0; k--)
                    builder.Append('|');
                  if (i < table.NColumns && i < table.Alignments.Count) {
                    builder.Append(table.Alignments[i] switch {
                      ColumnAlignment.Left => 'l',
                      ColumnAlignment.Right => 'r',
                      _ => 'c'
                    });
                  }
                }
                builder.Append('}');
              }
            }
            for (int i = 0; i < table.NRows; i++) {
              var row = table.Cells[i];
              if (table.Environment == "array" && i < table.HorizontalLines.Count) {
                for (int k = table.HorizontalLines[i]; k > 0; k--)
                  builder.Append(@"\hline ");
              }
              for (int j = 0; j < row.Count; j++) {
                var cell = row[j];
                var span = table.GetColumnSpan(i, j);
                if (span > 1) {
                  var spec = table.SpanSpecifications.Count > i && table.SpanSpecifications[i].Count > j
                    ? table.SpanSpecifications[i][j] : null;
                  builder.Append(@"\multicolumn{").Append(span).Append("}{")
                    .Append(spec ?? table.GetAlignment(j).ToString().ToLowerInvariant()).Append("}{");
                }
                if (table.Environment == "matrix"
                    && cell.Count >= 1
                    && cell[0] is Style) {
                  // remove the first atom.
                  cell = cell.Slice(1, cell.Count - 1);
                }
                if (table.Environment switch {
                  "eqalign" => true,
                  "aligned" => true,
                  "split" => true,
                  _ => false
                }
                    && j == 1
                    && cell.Count >= 1
                    && cell[0] is Ordinary ord
                    && string.IsNullOrEmpty(ord.Nucleus)) {
                  // empty nucleus added for spacing. Remove it.
                  cell = cell.Slice(1, cell.Count - 1);
                }
                if (table.Environment == "alignedat"
                    && j % 2 == 1
                    && cell.Count >= 1
                    && cell[0] is Ordinary ordAt
                    && string.IsNullOrEmpty(ordAt.Nucleus)) {
                  // empty nucleus added for spacing. Remove it.
                  cell = cell.Slice(1, cell.Count - 1);
                }
                MathListToLaTeX(cell, builder, currentFontStyle);
                if (span > 1) builder.Append('}');
                if (j < row.Count - 1) {
                  builder.Append('&');
                }
              }
              bool lastRow = i == table.NRows - 1;
              int bottomHLines = table.Environment == "array"
                && lastRow && table.NRows < table.HorizontalLines.Count
                ? table.HorizontalLines[table.NRows] : 0;
              if (!lastRow) {
                builder.Append(@"\\ ");
              } else if (bottomHLines > 0) {
                // A bottom \hline needs a row terminator before it.
                builder.Append(@"\\ ");
              }
            }
            if (table.Environment == "array"
                && table.NRows < table.HorizontalLines.Count) {
              for (int k = table.HorizontalLines[table.NRows]; k > 0; k--)
                builder.Append(@"\hline ");
            }
            if (table.Environment != null) {
              builder.Append(@"\end{")
                .Append(table.Environment)
                .Append('}');
            }
            break;
          case Overline over:
            builder.Append(@"\overline{");
            MathListToLaTeX(over.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case Underline under:
            builder.Append(@"\underline{");
            MathListToLaTeX(under.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case UnderAnnotation underAnotation:
            MathAtomToLaTeX(underAnotation, builder, out _);
            builder.Append('{');
            MathListToLaTeX(underAnotation.InnerList, builder, currentFontStyle);
            builder.Append('}');

            if (underAnotation.UnderList is { Count: > 0 }) {
              builder.Append("_{");
              MathListToLaTeX(underAnotation.UnderList, builder, currentFontStyle);
              builder.Append('}');
            }
            break;
          case Accent accent:
            MathAtomToLaTeX(accent, builder, out _);
            builder.Append('{');
            MathListToLaTeX(accent.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case LargeOperator op:
            if (MathAtomToLaTeX(op, builder, out var command)) {
              if (!(LaTeXSettings.AtomForCommand(command!) is LargeOperator originalOperator))
                throw new InvalidCodePathException("original operator not found!");
              if (originalOperator.Limits == op.Limits)
                break;
            } else {
              builder.Append($@"\operatorname{{{op.Nucleus}}} ");
            }
            switch (op.Limits) {
              case true:
                builder.Append(@"\limits ");
                break;
              case false:
                if (!op.ForceNoLimits) builder.Append(@"\nolimits ");
                break;
              case null:
                break;
            }
            break;
          case Colored colored:
            builder.Append(@"\color{");
            LaTeXSettings.ColorToString(colored.Color, builder)
              .Append("}{");
            MathListToLaTeX(colored.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case ColorBox colorBox:
            builder.Append(@"\colorbox{");
            LaTeXSettings.ColorToString(colorBox.Color, builder)
              .Append("}{");
            MathListToLaTeX(colorBox.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case RaiseBox r:
            builder.Append(@"\raisebox{")
              .Append(r.Raise.Amount.ToStringInvariant("0.0####"))
              .Append(r.Raise.IsMu ? "mu" : "pt")
              .Append("}{");
            MathListToLaTeX(r.InnerList, builder, currentFontStyle);
            builder.Append('}');
            break;
          case var _ when MathAtomToLaTeX(atom, builder, out _):
            break;
          case Atoms.Space space:
            var intSpace = (int)space.Length;
            if (space.IsMu)
              builder.Append(@"\mkern")
                .Append(space.Length.ToStringInvariant("0.0####"))
                .Append("mu");
            else
              builder.Append(@"\kern")
                .Append(space.Length.ToStringInvariant("0.0####"))
                .Append("pt");
            break;
          case { Nucleus: null }:
          case { Nucleus: "" }:
            builder.Append("{}");
            break;
          case { Nucleus: "\u2236" }:
            builder.Append(':');
            break;
          case { Nucleus: "\u2212" }:
            builder.Append('-');
            break;
          case { Nucleus: var aNucleus }:
            builder.Append(aNucleus);
            break;
        }
        static void AppendScript
          (StringBuilder builder, MathList script, char scriptChar, FontStyle currentFontStyle) {
          if (script.IsNonEmpty()) {
            builder.Append(scriptChar).Append('{');
            var lengthBeforeScript = builder.Length;
            MathListToLaTeX(script, builder, currentFontStyle);
            if (lengthBeforeScript + 1 == builder.Length)
              builder.Remove(lengthBeforeScript - 1, 1); // Remove { if script is only 1 char
            else
              builder.Append('}');
          }
        }
        AppendScript(builder, atom.Subscript, '_', currentFontStyle);
        AppendScript(builder, atom.Superscript, '^', currentFontStyle);
      }
      if (currentFontStyle != outerFontStyle) {
        builder.Append('}');
      }
    }
    public static StringBuilder MathListToLaTeX(MathList mathList, StringBuilder? sb = null) {
      sb ??= new StringBuilder();
      MathListToLaTeX(mathList, sb, FontStyle.Default);
      return sb;
    }
  }
}
