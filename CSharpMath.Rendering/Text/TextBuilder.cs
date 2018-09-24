using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;

namespace CSharpMath.Rendering {
  using Enumerations;
  using Structures;
  using static Structures.Result;
  public static class TextBuilder {
    /* //Paste this into the C# Interactive, fill <username> yourself
#r "C:/Users/<username>/source/repos/CSharpMath/Typography/Build/NetStandard/Typography.TextBreak/bin/Debug/netstandard1.3/Typography.TextBreak.dll"
using Typography.TextBreak;
(int, WordKind, char)[] BreakText(string text) {
var breaker = new CustomBreaker();
var breakList = new List<BreakAtInfo>();
breaker.BreakWords(text);
breaker.LoadBreakAtList(breakList);
//index is after the boundary -> last one will be out of range
return breakList.Select(i => (i.breakAt, i.wordKind, text.ElementAtOrDefault(i.breakAt))).ToArray();
}
BreakText(@"Here are some text $1 + 12 \frac23 \sqrt4$ $$Display$$ text")
     */
    /* //Version 2
#r "C:/Users/<username>/source/repos/CSharpMath/Typography/Build/NetStandard/Typography.TextBreak/bin/Debug/netstandard1.3/Typography.TextBreak.dll"
using Typography.TextBreak;
string BreakText(string text, string seperator = "|")
{
  var breaker = new CustomBreaker();
  var breakList = new List<BreakAtInfo>();
  breaker.BreakWords(text);
  breaker.LoadBreakAtList(breakList);
  //reverse to ensure earlier inserts do not affect later ones
  foreach (var @break in breakList.Select(i => i.breakAt).Reverse())
      text = text.Insert(@break, seperator);
  return text;
}
BreakText(@"Here are some text $1 + 12 \frac23 \sqrt4$ $$Display$$ text")
     */
    public const int StringArgumentLimit = 25;
    public static bool NoEnhancedColors { get; set; }
    private static CustomBreaker breaker = new CustomBreaker { BreakNumberAfterText = true, ThrowIfCharOutOfRange = false };
    private const string SpecialChars = @"#$%&\^_{}~";
    public static Result<TextAtom> Build(ReadOnlySpan<char> latexSource) {
      if (latexSource.IsEmpty) return new TextAtom.List(Array.Empty<TextAtom>(), 0);
      bool? displayMath = null;
      StringBuilder mathLaTeX = null;
      bool backslashEscape = false;
      bool afterCommand = false; //ignore spaces after command
      bool afterNewline = false;
      int dollarCount = 0;
      var globalAtoms = new TextAtomListBuilder();
      var breakList = new List<BreakAtInfo>();
      breaker.BreakWords(latexSource, breakList);
      Result CheckDollarCount(TextAtomListBuilder atoms) {
        switch (dollarCount) {
          case 0:
            break;
          case 1:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                return "Cannot close display math mode with $";
              case false:
                if (atoms.Add(mathLaTeX.ToString(), false).Error is string mathError)
                  return "[Math mode error] " + mathError;
                mathLaTeX = null;
                displayMath = null;
                break;
              case null:
                mathLaTeX = new StringBuilder();
                displayMath = false;
                break;
            }
            break;
          case 2:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                if (atoms.Add(mathLaTeX.ToString(), true).Error is string mathError)
                  return "[Math mode error] " + mathError;
                mathLaTeX = null;
                displayMath = null;
                break;
              case false:
                return "Cannot close inline math mode with $$";
              case null:
                mathLaTeX = new StringBuilder();
                displayMath = true;
                break;
            }
            break;
          default:
            return "Invalid number of $: " + dollarCount;
        }
        return Ok();
      }
      Result<int> BuildBreakList(ReadOnlySpan<char> latex, TextAtomListBuilder atoms, int i, bool oneCharOnly, char stopChar) {
        void ParagraphBreak() {
          atoms.Break(3);
#warning Should the newline and space occupy the same range?
          atoms.TextLength -= 3;
          atoms.Add(Space.ParagraphIndent, 3);
        }
        for (; i < breakList.Count; i++) {
          void ObtainRange(ReadOnlySpan<char> latexInput, int index, out int start, out int end, out ReadOnlySpan<char> section, out WordKind kind) {
            (start, end) = (index == 0 ? 0 : breakList[index - 1].breakAt, breakList[index].breakAt);
            section = latexInput.Slice(start, end - start);
            kind = breakList[index].wordKind;
          }
          ObtainRange(latex, i, out var startAt, out var endAt, out var textSection, out var wordKind);
          bool SetPrevRange(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            bool success = i-- > 0;
            if (success) ObtainRange(latexInput, i, out startAt, out endAt, out section, out wordKind);
            return success;
          }
          bool SetNextRange(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            bool success = ++i < breakList.Count;
            if (success) ObtainRange(latexInput, i, out startAt, out endAt, out section, out wordKind);
            return success;
          }
          Result<TextAtom> ReadArgumentAtom(ReadOnlySpan<char> latexInput) {
            backslashEscape = false;
            var argAtoms = new TextAtomListBuilder();
            if(BuildBreakList(latexInput, argAtoms, ++i, true, '\0').Bind(index => i = index).Error is string error) return error;
            return argAtoms.Build();
          }
          SpanResult<char> ReadArgumentString(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            afterCommand = false;
            if (!SetNextRange(latexInput, ref section)) return Err("Missing argument");
            if (section.IsNot('{')) return Err("Missing {");
            int endingIndex = -1;
            //startAt + 1 to not start at the { we started at
            bool isEscape = false;
            for (int j = startAt + 1, bracketDepth = 0; j < latexInput.Length; j++) {
              if (latexInput[j] == '\\')
                isEscape = true;
              else if (latexInput[j] == '{' && !isEscape) bracketDepth++;
              else if (latexInput[j] == '}' && !isEscape)
                if (bracketDepth > 0) bracketDepth--;
                else { endingIndex = j; break; } else isEscape = false;
            }
            if (endingIndex == -1) return Err("Missing }");
            var resultText = latexInput.Slice(endAt, endingIndex - endAt);
            while (startAt < endingIndex)
              _ = SetNextRange(latexInput, ref section); //this never fails because the above check
            return Ok(resultText);
          }
          ReadOnlySpan<char> LookAheadForPunc(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            int start = endAt;
            while (SetNextRange(latexInput, ref section))
              if (wordKind != WordKind.Punc || SpecialChars.Contains(section[0])) {
                //We have overlooked by one
                SetPrevRange(latexInput, ref section);
                break;
              }
            return latexInput.Slice(start, endAt - start);
          }
          //Nothing should be before dollar sign checking -- dollar sign checking uses continue;
          atoms.TextLength = startAt;
          if (textSection.Is('$')) {
            if (backslashEscape)
              if (displayMath != null) mathLaTeX.Append(@"\$");
              else atoms.Add("$", LookAheadForPunc(latex, ref textSection));
            else {
              dollarCount++;
              continue;
            }
            backslashEscape = false;
          } else {
            { if (CheckDollarCount(atoms).Error is string error) return error; }
            if (stopChar > 0 && textSection[0] == stopChar) return Ok(i);
            if (!backslashEscape) {
            //Unescaped text section, inside display/inline math mode
              if(displayMath != null)
                switch (textSection) {
                  case var _ when textSection.Is('$'):
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case var _ when textSection.Is('\\'):
                    backslashEscape = true;
                    continue;
                  default:
                    mathLaTeX.Append(textSection);
                    break;
                }
              //Unescaped text section, not inside display/inline math mode
              else switch (textSection) {
                  case var _ when textSection.Is('$'):
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case var _ when textSection.Is('\\'):
                    backslashEscape = true;
                    continue;
                  case var _ when textSection.Is('#'):
                    return "Unexpected command argument reference character # outside of new command definition (currently unsupported)";
                  case var _ when textSection.Is('^'):
                  case var _ when textSection.Is('_'):
                    return $"Unexpected script indicator {textSection[0]} outside of math mode";
                  case var _ when textSection.Is('&'):
                    return $"Unexpected alignment tab character & outside of table environments";
                  case var _ when textSection.Is('~'):
                    atoms.Add();
                    break;
                  case var _ when textSection.Is('%'):
                    var comment = new StringBuilder();
                    while (SetNextRange(latex, ref textSection) && wordKind != WordKind.NewLine) comment.Append(textSection);
                    atoms.Comment(comment.ToString());
                    break;
                  case var _ when textSection.Is('{'):
                    if(BuildBreakList(latex, atoms, ++i, false, '}').Bind(index => i = index).Error is string error) return error;
                    break;
                  case var _ when textSection.Is('}'):
                    return "Unexpected }, unbalanced braces";
                  case var _ when wordKind == WordKind.NewLine:
                    //Consume newlines after commands
                    //Double newline == paragraph break
                    if (afterNewline) {
                      ParagraphBreak();
                      afterNewline = false;
                      break;
                    } else {
                      atoms.Add();
                      afterNewline = true;
                      continue;
                    }
                  case var _ when wordKind == WordKind.Whitespace:
                    //Collpase spaces
                    if (afterCommand) continue;
                    else atoms.Add();
                    break;
                  default: //Just ordinary text
                    if (oneCharOnly) {
                      if (startAt + 1 < endAt) { //Only re-read if current break span is more than 1 long
                        i--;
                        breakList[i] = new BreakAtInfo(breakList[i].breakAt + 1, breakList[i].wordKind);
                      }
                      //Need to allocate in the end :(
                      atoms.Add(textSection[0].ToString(), LookAheadForPunc(latex, ref textSection));
                    } else atoms.Add(textSection.ToString(), LookAheadForPunc(latex, ref textSection));
                    break;
                }
              afterCommand = false;
            }

            //Escaped text section but in inline/display math mode
            else if (displayMath != null) {
              switch (textSection) {
                case var _ when textSection.Is('$'):
                  throw new InvalidCodePathException("The $ case should have been accounted for.");
                case var _ when textSection.Is('('):
                  switch (displayMath) {
                    case true:
                      return "Cannot open inline math mode in display math mode";
                    case false:
                      return "Cannot open inline math mode in inline math mode";
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                case var _ when textSection.Is(')'):
                  switch (displayMath) {
                    case true:
                      return "Cannot close inline math mode in display math mode";
                    case false:
                      if (atoms.Add(mathLaTeX.ToString(), false).Error is string mathError)
                        return "[Math mode error] " + mathError;
                      mathLaTeX = null;
                      displayMath = null;
                      break;
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                  break;
                case var _ when textSection.Is('['):
                  switch (displayMath) {
                    case true:
                      return "Cannot open display math mode in display math mode";
                    case false:
                      return "Cannot open display math mode in inline math mode";
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                case var _ when textSection.Is(']'):
                  switch (displayMath) {
                    case true:
                      if (atoms.Add(mathLaTeX.ToString(), true).Error is string mathError)
                        return "[Math mode error] " + mathError;
                      mathLaTeX = null;
                      displayMath = null;
                      break;
                    case false:
                      return "Cannot close display math mode in inline math mode";
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                  break;
                default:
                  mathLaTeX.Append('\\').Append(textSection);
                  break;
              }
              backslashEscape = false;
            } else {
              //Escaped text section and not in inline/display math mode
              afterCommand = true;
              switch (textSection) {
                case var _ when textSection.Is('('):
                  mathLaTeX = new StringBuilder();
                  displayMath = false;
                  break;
                case var _ when textSection.Is(')'):
                  return "Cannot close inline math mode outside of math mode";
                case var _ when textSection.Is('['):
                  mathLaTeX = new StringBuilder();
                  displayMath = true;
                  break;
                case var _ when textSection.Is(']'):
                  return "Cannot close display math mode outside of math mode";
                case var _ when textSection.Is('\\'):
                  atoms.Break(1);
                  break;
                case var _ when textSection.Is(','):
                  atoms.Add(Space.ShortSpace, 1);
                  break;
                case var _ when wordKind == WordKind.Whitespace: //control space
                  atoms.Add();
                  break;
                case var _ when textSection.Is("par"):
                  ParagraphBreak();
                  break;
                case var _ when textSection.Is("fontsize"): {
                    if (ReadArgumentString(latex, ref textSection).Bind(fontSize => {
                      if (fontSize.Length > StringArgumentLimit)
                        return Err($"Length of font size has over {StringArgumentLimit} characters. Please shorten it.");
                      Span<byte> charBytes = stackalloc byte[fontSize.Length];
                      for (int j = 0; j < fontSize.Length; j++) {
                        if (fontSize[j] > 127) return Err("Invalid font size");
                        charBytes[j] = (byte)fontSize[j];
                      }
                      return System.Buffers.Text.Utf8Parser.TryParse(charBytes, out float parsedResult, out _, 'f') ?
                        Ok(parsedResult) :
                        Err("Invalid font size");
                    }).Bind(
                      ReadArgumentAtom(latex),
                      (fontSize, resizedContent) =>
                        atoms.Add(resizedContent, fontSize, "fontsize".Length)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                case var _ when textSection.Is("color"): {
                    if (ReadArgumentString(latex, ref textSection).Bind(color =>
                        color.Length > StringArgumentLimit ?
                          Err($"Length of color has over {StringArgumentLimit} characters. Please shorten it.") :
                        Color.Create(color, !NoEnhancedColors) is Color value ?
                          Ok(value) :
                        Err("Invalid color")
                      ).Bind(
                        ReadArgumentAtom(latex),
                        (color, coloredContent) =>
                          atoms.Add(coloredContent, color, "color".Length)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                //case "red", "yellow", ...
                case var shortColor when !NoEnhancedColors && shortColor.TryAccessDictionary(Color.PredefinedColors, out var color): {
                    int tmp_commandLength = shortColor.Length;
                    if (ReadArgumentAtom(latex).Bind(
                        coloredContent => atoms.Add(coloredContent, color, tmp_commandLength)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                //case "textbf", "textit", ...
                bool ValidTextStyle(ReadOnlySpan<char> textStyle, out FontStyle fontStyle) {
                    fontStyle = default;
                    if (textStyle.Length > 3 &&
                        textStyle[0] == 'm'  &&
                        textStyle[1] == 'a'  &&
                        textStyle[2] == 't'  &&
                        textStyle[3] == 'h') return false;
                    Span<char> copy = stackalloc char[textStyle.Length];
                    textStyle.CopyTo(copy);
                    if (textStyle.Length > 3 &&
                        textStyle[0] == 't' &&
                        textStyle[1] == 'e' &&
                        textStyle[2] == 'x' &&
                        textStyle[3] == 't') {
                      copy[0] = 'm';
                      copy[1] = 'a';
                      copy[2] = 't';
                      copy[3] = 'h';
                    }
                    foreach (var item in FontStyleExtensions.FontStyles)
                      if(textStyle.Is(item.Key)) {
                        fontStyle = item.Value;
                        return true;
                      }
                    return false;
                }
                case var textStyle when ValidTextStyle(textStyle, out var fontStyle): {
                    int tmp_commandLength = textStyle.Length;
                    if (ReadArgumentAtom(latex)
                      .Bind(builtContent => atoms.Add(builtContent, fontStyle, tmp_commandLength))
                      .Error is string error)
                      return error;
                    break;
                  }
                //case "^", "\"", ...
                case var textAccent when textAccent.TryAccessDictionary(TextAtoms.PredefinedAccents, out var accent): {
                    int tmp_commandLength = textAccent.Length;
                    if (ReadArgumentAtom(latex)
                      .Bind(builtContent => atoms.Add(builtContent, accent, tmp_commandLength))
                      .Error is string error)
                      return error;
                    break;
                  }
                //case "textasciicircum", "textless", ...
                case var textSymbol when textSymbol.TryAccessDictionary(TextAtoms.PredefinedTextSymbols, out var replaceResult):
                  atoms.Add(replaceResult, LookAheadForPunc(latex, ref textSection));
                  break;
                case var command:
                  if (displayMath != null) mathLaTeX.Append(command); //don't eat the command when parsing math
                  else return $@"Unknown command \{command.ToString()}";
                  break;
              }
              backslashEscape = false;
            }
          }
          afterNewline = false;
          if (oneCharOnly) return Ok(i);
        }
        if (backslashEscape) return @"Unknown command \";
        if (stopChar > 0) return stopChar == '}' ? "Expected }, unbalanced braces" : $@"Expected {stopChar}";
        return Ok(i);
      }
      { if (BuildBreakList(latexSource, globalAtoms, 0, false, '\0').Error is string error) return error; }
      { if (CheckDollarCount(globalAtoms).Error is string error) return error; }
      if (displayMath != null) return "Math mode was not terminated";
      return globalAtoms.Build();
    }
    public static StringBuilder Unbuild(TextAtom atom, StringBuilder b) {
      switch (atom) {
        case TextAtom.Text t:
          return b.Append(t.Content);
        case TextAtom.Newline n:
          return b.Append(@"\\");
        case TextAtom.Math m:
          return b.Append('\\').Append(m.DisplayStyle ? '[' : '(').Append(Atoms.MathListBuilder.MathListToString(m.Content)).Append('\\').Append(m.DisplayStyle ? ']' : ')');
        case TextAtom.Space s:
          return b.Append(@"\hspace").AppendInBraces(s.Content.Length.ToStringInvariant(), NullHandling.EmptyContent);
        case TextAtom.ControlSpace c:
          return b.Append(@"\ ");
        case TextAtom.Style t:
          return b.Append('\\').Append(t.FontStyle.FontName()).AppendInBraces(Unbuild(t.Content, new StringBuilder()).ToString(), NullHandling.None);
        case TextAtom.Size z:
          return b.Append(@"\fontsize").AppendInBraces(z.PointSize.ToStringInvariant(), NullHandling.EmptyContent)
                                       .AppendInBraces(Unbuild(z.Content, new StringBuilder()).ToString(), NullHandling.None);
        case TextAtom.List l:
          foreach (var a in l.Content) {
            b.Append(Unbuild(a, b));
          }
          return b;
        case null:
          throw new ArgumentNullException(nameof(atom), "TextAtoms should never be null. You must have sneaked one in.");
        case var a:
          throw new InvalidCodePathException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
      }
    }
  }
}