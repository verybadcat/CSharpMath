using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;

namespace CSharpMath.Rendering {
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
    public static (TextAtom atom, string error) Build(string text, bool enhancedColors) {
#warning Use a new struct called Result<>
      string error = null;
      bool? displayMath = null;
      StringBuilder mathLaTeX = null;
      bool backslashEscape = false;
      int dollarCount = 0;
      var atoms = new TextAtomListBuilder();
      var breaker = new CustomBreaker();
      var breakList = new List<BreakAtInfo> { new BreakAtInfo(0, WordKind.Unknown) };
      breaker.BreakWords(text, false);
      breaker.LoadBreakAtList(breakList);
      (int startAt, int endAt, char endingChar) ObtainRange(int i) =>
        (breakList[i].breakAt, i == breakList.Count - 1 ? text.Length : breakList[i + 1].breakAt, i == breakList.Count - 1 ? '\0' : text[breakList[i + 1].breakAt - 1]);
      for (int i = 0; i < breakList.Count; i++) {
#warning No more \0 workarounds
        var (startAt, endAt, endingChar) = ObtainRange(i);
        void SetNextRange() => (startAt, endAt, endingChar) = ObtainRange(++i);
        string ReadArgument() {
          SetNextRange();
          if (endingChar != '{') {
            if (startAt == text.Length) { error = "Missing argument"; return null; }
            var toReturn = text[startAt].ToString();
            //range contains one char only
            if (startAt == endAt)
              SetNextRange();
            else
              startAt += 1;
            return toReturn;
          }
          int bracketDepth = 0;
          int endingIndex = -1;
          //startAt + 1 to not start at the { we started at
          for (int j = startAt + 1; j < text.Length; j++) { if (text[j] == '{') bracketDepth++; else if (text[j] == '}') if (bracketDepth > 0) bracketDepth--; else { endingIndex = j; break; } }
          if (endingIndex == -1) { error = "Missing }"; return null; }
          var resultText = text.Substring(endAt, endingIndex - endAt);
          while (startAt < endingIndex) SetNextRange();
          return resultText;
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
          switch (dollarCount) {
            case 0:
              break;
            case 1:
              dollarCount = 0;
              switch (displayMath) {
                case true:
                  return (null, "Cannot close display math mode with $");
                case false:
                  if (atoms.Add(mathLaTeX.ToString(), false) is string mathError)
                    return (null, "[Math mode error] " + mathError);
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
                  if (atoms.Add(mathLaTeX.ToString(), true) is string mathError)
                    return (null, "[Math mode error] " + mathError);
                  mathLaTeX = null;
                  displayMath = null;
                  break;
                case false:
                  return (null, "Cannot close inline math mode with $$");
                case null:
                  mathLaTeX = new StringBuilder();
                  displayMath = true;
                  break;
              }
              break;
            default:
              return (null, "Invalid number of $: " + dollarCount);
          }
          if (!backslashEscape) //Normal unescaped text section, could be in display/inline math mode
            switch (endingChar) {
              case '$':
                throw new InvalidCodePathException("The $ case should have been accounted for.");
              case '\0':
                continue; //Don't do anything with the null char
              case '\\':
                backslashEscape = true;
                continue;
              case ' ': //Collpase spaces
                var textSection = " ";
                goto default;
              case var c: //Just ordinary text
                textSection = text.Substring(startAt, endAt - startAt);
                goto default;
              default:
                if (displayMath == null) atoms.Add(textSection);
                else mathLaTeX.Append(textSection);
                break;
            } else {
            if (displayMath != null) //Escaped text section but in inline/display math mode
              switch (endingChar) {
                case '$':
                  throw new InvalidCodePathException("The $ case should have been accounted for.");
                case '\0':
                  continue; //Don't do anything with the null char
                case '(':
                  switch (displayMath) {
                    case true:
                      return (null, "Cannot open inline math mode in display math mode");
                    case false:
                      return (null, "Cannot open inline math mode in inline math mode");
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                case ')':
                  switch (displayMath) {
                    case true:
                      return (null, "Cannot close inline math mode in display math mode");
                    case false:
                      if (atoms.Add(mathLaTeX.ToString(), false) is string mathError)
                        return (null, "[Math mode error] " + mathError);
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
                      return (null, "Cannot open display math mode in display math mode");
                    case false:
                      return (null, "Cannot open display math mode in inline math mode");
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                case ']':
                  switch (displayMath) {
                    case true:
                      if (atoms.Add(mathLaTeX.ToString(), true) is string mathError)
                        return (null, "[Math mode error] " + mathError);
                      mathLaTeX = null;
                      displayMath = null;
                      break;
                    case false:
                      return (null, "Cannot close display math mode in inline math mode");
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                  break;
                default:
                  mathLaTeX.Append($@"\{text.Substring(startAt, endAt - startAt)}");
                  break;
              } else //Escaped text section and not in inline/display math mode
              switch (text.Substring(startAt, endAt - startAt)) {
                case "(":
                  mathLaTeX = new StringBuilder();
                  displayMath = false;
                  break;
                case ")":
                  return (null, "Cannot close inline math mode outside of math mode");
                case "[":
                  mathLaTeX = new StringBuilder();
                  displayMath = true;
                  break;
                case "]":
                  return (null, "Cannot close display math mode outside of math mode");
                case @"\":
                  if (displayMath != null) mathLaTeX.Append('\\'); //don't eat the backslash when parsing math
                  else atoms.Break(1);
                  break;
                case "backslash":
                  atoms.Add(@"\");
                  break;
                case "par":
                  atoms.Break(3);
#warning Should the newline and space occupy the same range?
                  atoms.TextLength -= 3;
                  //1.25em is a rough estimate of the indentation by \par using my eyes - Happypig375
                  atoms.Add(new Structures.Space(1.25f * 18f, true), 3);
                  break;
#warning Refactor fontsize, color and shortColor, too much repitition
                case "fontsize":
                  var fontSize = ReadArgument();
                  if (fontSize == null) return (null, error);
                  if (!float.TryParse(fontSize, System.Globalization.NumberStyles.AllowDecimalPoint |
                                                System.Globalization.NumberStyles.AllowLeadingWhite |
                                                System.Globalization.NumberStyles.AllowTrailingWhite,
                                                System.Globalization.CultureInfo.InvariantCulture, out var parsedResult))
                    return (null, "Invalid font size");
                  var resizedContent = ReadArgument();
                  if (resizedContent == null) return (null, error);
                  var resizedResult = Build(resizedContent, enhancedColors);
                  if (resizedResult.error != null) return (null, resizedResult.error);
                  if (resizedContent != string.Empty) atoms.Add(resizedResult.atom, parsedResult, "fontsize".Length);
                  break;
                case "color":
                  var colorString = ReadArgument();
                  if (colorString == null) return (null, error);
                  var color = Structures.Color.Create(colorString, enhancedColors);
                  if (color == null) return (null, "Invalid color");
                  var toColorize = ReadArgument();
                  if (toColorize == null) return (null, error);
                  var colorized = Build(toColorize, enhancedColors);
                  if (colorized.error != null) return (null, colorized.error);
                  if (toColorize != string.Empty) atoms.Add(colorized.atom, color.Value, "color".Length);
                  break;
                case var shortColor when enhancedColors && Structures.Color.PredefinedColors.TryGetByFirst(shortColor, out var _):
                  var toColorizeShort = ReadArgument();
                  if (toColorizeShort == null) return (null, error);
                  var colour = Structures.Color.Create(shortColor, enhancedColors) ?? throw new InvalidCodePathException("This case's condition should have checked the validity of shortColor.");
                  var colorizedShort = Build(toColorizeShort, enhancedColors);
                  if (colorizedShort.error != null) return (null, colorizedShort.error);
                  if (toColorizeShort != string.Empty) atoms.Add(colorizedShort.atom, colour, "color".Length);
                  break;
                //case "textbf", "textit", ...
                case var command when !command.Contains("math") && FontStyleExtensions.FontStyles.TryGetByFirst(command.Replace("text", "math"), out var fontStyle):
                  var content = ReadArgument();
                  if (content == null) return (null, error);
                  var builtResult = Build(content, enhancedColors);
                  if (builtResult.error != null) return (null, builtResult.error);
                  if (content != string.Empty) atoms.Add(builtResult.atom, fontStyle, command.Length);
                  break;
                case var command:
                  if (displayMath != null) mathLaTeX.Append(command); //don't eat the command when parsing math
                  else return (null, @"Unknown command \" + command);
                  break;
              }
            backslashEscape = false;
          }
        }
      }
      if (displayMath != null) return (null, "Math mode was not terminated");
      return (atoms.Build(), error);
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
          throw new InvalidCodePathException("TextAtoms should never be null.");
        case var a:
          throw new InvalidCodePathException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
      }
    }
  }
}