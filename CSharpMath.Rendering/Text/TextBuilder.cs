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
    public static bool NoEnhancedColors { get; set; }
    public static Result<TextAtom> Build(string latex) {
      if (string.IsNullOrEmpty(latex)) return new TextAtom.List(Array.Empty<TextAtom>(), 0);
      bool? displayMath = null;
      StringBuilder mathLaTeX = null;
      bool backslashEscape = false;
      bool afterCommand = false; //ignore spaces after command
      int dollarCount = 0;
      var atoms = new TextAtomListBuilder();
      var breaker = new CustomBreaker { BreakNumberAfterText = true, ThrowIfCharOutOfRange = false };
      var breakList = new List<BreakAtInfo>();
      breaker.BreakWords(latex);
      breaker.LoadBreakAtList(breakList);
      Result CheckDollarCount() {
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
      (int startAt, int endAt, char endingChar, WordKind wordKind) ObtainRange(int i) =>
        (i == 0 ? 0 : breakList[i - 1].breakAt, breakList[i].breakAt, latex[breakList[i].breakAt - 1], breakList[i].wordKind);
      for (var i = 0; i < breakList.Count; i++) {
        var (startAt, endAt, endingChar, wordKind) = ObtainRange(i);
        bool SetNextRange() {
          bool success = ++i < breakList.Count;
          if(success) (startAt, endAt, endingChar, wordKind) = ObtainRange(i);
          return success;
        }
        Result<string> ReadArgument() {
          afterCommand = false;
          if (!SetNextRange()) return Err("Missing argument");
          if (endingChar != '{') {
            var toReturn = latex[startAt].ToString();
#warning Not one char only, should skip spaces then read next char, and it is a possible command
            //range contains one char only
            if (startAt == endAt)
              _ = SetNextRange(); //reaching the end does not affect validity of argument
            else
              startAt += 1;
            return Ok(toReturn);
          }
          int endingIndex = -1;
          //startAt + 1 to not start at the { we started at
          for (int j = startAt + 1, bracketDepth = 0; j < latex.Length; j++) {
            if (latex[j] == '{') bracketDepth++;
            else if (latex[j] == '}')
              if (bracketDepth > 0) bracketDepth--;
              else { endingIndex = j; break; }
          }
          if (endingIndex == -1) return Err("Missing }");
          var resultText = latex.Substring(endAt, endingIndex - endAt);
          while (startAt < endingIndex)
            _ = SetNextRange(); //this never fails because the above check
          return Ok(resultText);
        }
        atoms.TextLength = startAt;
        if (endingChar == '$') {
          if (backslashEscape)
            if (displayMath != null) mathLaTeX.Append(@"\$");
            else atoms.Add("$");
          else {
            dollarCount++;
            continue;
          }
          backslashEscape = false;
        } else {
          { if (CheckDollarCount().Error is string error) return error; }

          //Normal unescaped text section, could be in display/inline math mode
          if (!backslashEscape) {
            var textSection = latex.Substring(startAt, endAt - startAt);
            switch (endingChar) {
              case '$':
                throw new InvalidCodePathException("The $ case should have been accounted for.");
              case '\\':
                backslashEscape = true;
                continue;
              case var sp when wordKind == WordKind.Whitespace || wordKind == WordKind.NewLine:
                //Collpase spaces
                //Consume newlines after commands
                if (displayMath == null)
                  if (afterCommand) continue;
                  else atoms.Add();
                else mathLaTeX.Append(textSection);
                break;
              case var punc when displayMath == null && wordKind == WordKind.Punc && atoms.Last is TextAtom.Text t:
                //Append punctuation to text
                t.Append(textSection);
                break;
              default: //Just ordinary text
                if (displayMath == null) atoms.Add(textSection);
                else mathLaTeX.Append(textSection);
                break;
            }
            afterCommand = false;
            continue;
          }

          //Escaped text section but in inline/display math mode
          if (displayMath != null) {
            switch (endingChar) {
              case '$':
                throw new InvalidCodePathException("The $ case should have been accounted for.");
              case '(':
                switch (displayMath) {
                  case true:
                    return "Cannot open inline math mode in display math mode";
                  case false:
                    return "Cannot open inline math mode in inline math mode";
                  default:
                    throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                }
              case ')':
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
              case '[':
                switch (displayMath) {
                  case true:
                    return "Cannot open display math mode in display math mode";
                  case false:
                    return "Cannot open display math mode in inline math mode";
                  default:
                    throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                }
              case ']':
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
                mathLaTeX.Append($@"\{latex.Substring(startAt, endAt - startAt)}");
                break;
            }
            backslashEscape = false;
            continue;
          }

          //Escaped text section and not in inline/display math mode
          afterCommand = true;
          switch (latex.Substring(startAt, endAt - startAt)) {
            case "(":
              mathLaTeX = new StringBuilder();
              displayMath = false;
              break;
            case ")":
              return "Cannot close inline math mode outside of math mode";
            case "[":
              mathLaTeX = new StringBuilder();
              displayMath = true;
              break;
            case "]":
              return "Cannot close display math mode outside of math mode";
            case @"\":
              atoms.Break(1);
              break;
            case ",":
              atoms.Add(Space.ShortSpace, 1);
              break;
            case var _ when wordKind == WordKind.Whitespace: //control space
              atoms.Add();
              break;
            case "par":
              atoms.Break(3);
#warning Should the newline and space occupy the same range?
              atoms.TextLength -= 3;
              atoms.Add(Space.ParagraphIndent, 3);
              break;
            case "fontsize": {
                if (ReadArgument().Bind(fontSize =>
                    float.TryParse(fontSize, System.Globalization.NumberStyles.AllowDecimalPoint |
                                             System.Globalization.NumberStyles.AllowLeadingWhite |
                                             System.Globalization.NumberStyles.AllowTrailingWhite,
                                             System.Globalization.CultureInfo.InvariantCulture,
                                             out var parsedResult) ?
                    Ok(parsedResult) :
                    Err("Invalid font size")
                  ).Bind(
                    ReadArgument().Bind(Build),
                    (fontSize, resizedContent) =>
                      atoms.Add(resizedContent, fontSize, "fontsize".Length)
                  ).Error is string error
                ) return error;
                break;
              }
            case "color": {
                if (ReadArgument().Bind(color =>
                    Color.Create(color, !NoEnhancedColors) is Color value ?
                    Ok(value) :
                    Err("Invalid color")
                  ).Bind(
                    ReadArgument().Bind(Build),
                    (color, coloredContent) =>
                      atoms.Add(coloredContent, color, "color".Length)
                  ).Error is string error
                ) return error;
                break;
              }
            //case "red", "yellow", ...
            case var shortColor when !NoEnhancedColors && Color.PredefinedColors.Contains(shortColor): {
                if (Ok(Color.Create(shortColor, !NoEnhancedColors) ??
                      throw new InvalidCodePathException(
                        "This case's condition should have checked the validity of shortColor.")
                  ).Bind(
                    ReadArgument().Bind(Build),
                    (color, coloredContent) =>
                      atoms.Add(coloredContent, color, shortColor.Length)
                  ).Error is string error
                ) return error;
                break;
              }
            //case "textbf", "textit", ...
            case var textStyle when !textStyle.Contains("math") && FontStyleExtensions.FontStyles.TryGetByFirst(textStyle.Replace("text", "math"), out var fontStyle): {
                if (ReadArgument()
                  .Bind(Build)
                  .Bind(builtContent => atoms.Add(builtContent, fontStyle, textStyle.Length))
                  .Error is string error)
                  return error;
                break;
              }
            //case "^", "\"", ...
            case var textAccent when TextAtoms.PredefinedAccents.TryGetByFirst(textAccent, out var accent): {
                if (ReadArgument()
                  .Bind(Build)
                  .Bind(builtContent => atoms.Add(builtContent, accent, textAccent.Length))
                  .Error is string error)
                  return error;
                break;
              }
            //case "textasciicircum", "textless", ...
            case var textSymbol when TextAtoms.PredefinedTextSymbols.TryGetValue(textSymbol, out var replaceResult):
              atoms.Add(replaceResult);
              break;
            case var command:
              if (displayMath != null) mathLaTeX.Append(command); //don't eat the command when parsing math
              else return @"Unknown command \" + command;
              break;
          }
          backslashEscape = false;
        }
      }
      { if (CheckDollarCount().Error is string error) return error; }
      if (backslashEscape) return @"Unknown command \";
      if (displayMath != null) return "Math mode was not terminated";
      return atoms.Build();
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