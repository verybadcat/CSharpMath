using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CSharpMath.Rendering.Text.Tests {
  public class TextLaTeXParserTests {
    TextAtom Parse(string latex) =>
      TextLaTeXParser.TextAtomFromLaTeX(latex)
      .Match(atom => atom, error => throw new Xunit.Sdk.XunitException(error));
    private Action<TextAtom> CheckAtom<T>
      (Action<T>? action = null) where T : TextAtom =>
      atom => {
        var actualAtom = Assert.IsType<T>(atom);
        action?.Invoke(actualAtom);
      };
    [Theory]
    [InlineData("123")]
    [InlineData("123.456")]
    [InlineData("abc")]
    [InlineData("abc", "123")]
    [InlineData("123", "abc")]
    [InlineData("12", "a.m.")]
    [InlineData("1,", "2,", "3")]
    [InlineData("1,,", "2,,", "3")]
    [InlineData("1,,,")]
    [InlineData("1,,,", "2")]
    [InlineData("1()", "a")]
    [InlineData("a,", "b,", "c")]
    [InlineData("a,.", "b,.", "c")]
    [InlineData("a,,", "b,,", "c")]
    [InlineData("1/", "2/", "3")]
    [InlineData("!@*()")]
    [InlineData("ก", "ข", "ฃ")]
    [InlineData("中", "文")]
    [InlineData("😀", "Text")]
    [InlineData("Chinese", "中", "文")]
    [InlineData("Chinese", "中", "12345", "文", "😄")]
    public void Text(params string[] text) {
      var input = string.Concat(text);
      var atom = Parse(input);
      Assert.Equal(
        text.Length == 1
        ? (TextAtom)new TextAtom.Text(text[0])
        : new TextAtom.List(text.Select(t => new TextAtom.Text(t)).ToArray()), atom);
      Assert.Equal(input, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }
    [Theory]
    [InlineData(@"a\alpha a", @"a\alpha a", "a", "α", "a")]
    [InlineData(@"<\textless <", @"\textless \textless \textless ", "<", "<", "<")]
    [InlineData(@"\textbar   |\textbar   ", @"\textbar \textbar \textbar ", "|", "|", "|")]
    public void Command(string input, string output, params string[] text) {
      var atom = Parse(input);
      Assert.Equal(
        text.Length == 1
        ? (TextAtom)new TextAtom.Text(text[0])
        : new TextAtom.List(text.Select(t => new TextAtom.Text(t)).ToArray()), atom);
      Assert.Equal(output, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }
    [Theory]
    [InlineData(@"\color{red}␣a", "a", null, @"\color{red}{a}")]
    [InlineData(@"\color{red}␣{a}", "a", null, @"\color{red}{a}")]
    [InlineData(@"\color{red}␣😀", "😀", null, @"\color{red}{😀}")]
    [InlineData(@"\color{red}␣{😀}", "😀", null, @"\color{red}{😀}")]
    [InlineData(@"\color{red}␣\textbar", "|", null, @"\color{red}{\textbar }")]
    [InlineData(@"\color{red}␣{\textbar}", "|", null, @"\color{red}{\textbar }")]
    [InlineData(@"\color{red}␣aa", "a", "a", @"\color{red}{a}a")]
    [InlineData(@"\color{red}␣{a}a", "a", "a", @"\color{red}{a}a")]
    [InlineData(@"\color{red}␣a😄", "a", "😄", @"\color{red}{a}😄")]
    [InlineData(@"\color{red}␣{a}😄", "a", "😄", @"\color{red}{a}😄")]
    [InlineData(@"\color{red}␣😀a", "😀", "a", @"\color{red}{😀}a")]
    [InlineData(@"\color{red}␣{😀}a", "😀", "a", @"\color{red}{😀}a")]
    [InlineData(@"\color{red}␣😀😄", "😀", "😄", @"\color{red}{😀}😄")]
    [InlineData(@"\color{red}␣{😀}😄", "😀", "😄", @"\color{red}{😀}😄")]
    [InlineData(@"\color{red}␣\textbar a", "|", "a", @"\color{red}{\textbar }a")]
    [InlineData(@"\color{red}␣{\textbar}a", "|", "a", @"\color{red}{\textbar }a")]
    [InlineData(@"\color{red}␣a\textbar", "a", "|", @"\color{red}{a}\textbar ")]
    [InlineData(@"\color{red}␣{a}\textbar", "a", "|", @"\color{red}{a}\textbar ")]
    [InlineData(@"\color{red}␣\textbar\textbar", "|", "|", @"\color{red}{\textbar }\textbar ")]
    [InlineData(@"\color{red}␣{\textbar}\textbar", "|", "|", @"\color{red}{\textbar }\textbar ")]
    [InlineData(@"\color{red}␣\textbar😄", "|", "😄", @"\color{red}{\textbar }😄")]
    [InlineData(@"\color{red}␣{\textbar}😄", "|", "😄", @"\color{red}{\textbar }😄")]
    [InlineData(@"\color{red}␣😀\textbar", "😀", "|", @"\color{red}{😀}\textbar ")]
    [InlineData(@"\color{red}␣{😀}\textbar", "😀", "|", @"\color{red}{😀}\textbar ")]
    public void CommandArguments(string input, string colored, string? after, string output) {
      void Test(string input) {
        var atom = Parse(input);
        var list = new List<TextAtom> {
          new TextAtom.Colored(new TextAtom.Text(colored), Atom.LaTeXSettings.PredefinedColors.FirstToSecond["red"])
        };
        if (after != null) list.Add(new TextAtom.Text(after));
        Assert.Equal(list.Count == 1 ? list[0] : new TextAtom.List(list), atom);
        Assert.Equal(output, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
      }
      Test(input.Replace("␣", ""));
      Test(input.Replace("␣", " "));
      Test(input.Replace("␣", "  "));
      Test(input.Replace("␣", "\r"));
      Test(input.Replace("␣", "\n"));
      Test(input.Replace("␣", "\r\n"));
      Test(input.Replace("␣", " \r "));
      Test(input.Replace("␣", " \n "));
      Test(input.Replace("␣", " \r\n "));
    }
    [Theory]
    [InlineData(@"\textbb", Atom.FontStyle.Blackboard)]
    [InlineData(@"\textbf", Atom.FontStyle.Bold)]
    [InlineData(@"\textrm", Atom.FontStyle.Default)] // Default is converted to Roman 
    [InlineData(@"\textrm", Atom.FontStyle.Roman)]
    [InlineData(@"\textit", Atom.FontStyle.Italic)]
    public void Style(string command, Atom.FontStyle style) {
      var input = command + "{a}bc";
      var atom = Parse(input);
      Assert.Equal(new TextAtom.List(new TextAtom[] {
        new TextAtom.Style(new TextAtom.Text("a"), style),
        new TextAtom.Text("bc"),
      }), atom);
      Assert.Equal(input, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }
    [Fact]
    public void StyleDefault() {
      var input = @"\textnormal abc";
      var atom = Parse(input);
      Assert.Equal(new TextAtom.List(new TextAtom[] {
        new TextAtom.Style(new TextAtom.Text("a"), Atom.FontStyle.Roman),
        new TextAtom.Text("bc"),
      }), atom);
      Assert.Equal(@"\textrm{a}bc", TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }
    [Theory]
    [InlineData(@"\! ", -3, true)]
    [InlineData(@"\, ", 3, true)]
    [InlineData(@"\: ", 4, true)]
    [InlineData(@"\> ", 4, true, @"\: ")]
    [InlineData(@"\; ", 5, true)]
    [InlineData(@"\enspace ", 9, true)]
    [InlineData(@"\hspace{1ex}", 9, true, @"\enspace ")]
    [InlineData(@"\hspace{0.8333333333333333333em}", 15, true, @"\hspace{0.83333em}")]
    [InlineData(@"\quad ", 18, true)]
    [InlineData(@"\qquad ", 36, true)]
    [InlineData(@"\hspace{3pt}", 3, false, @"\hspace{3.0pt}")]
    [InlineData(@"\hspace{12.3pt}", 12.3, false)]
    public void Space(string command, float length, bool isMu, string? outCommand = null) {
      outCommand ??= command;
      var input = "a" + command + "b";
      var atom = Parse(input);
      Assert.Equal(new TextAtom.List(new TextAtom[] {
        new TextAtom.Text("a"),
        new TextAtom.Space((isMu ? Atom.Length.MathUnit : Atom.Length.Point) * length),
        new TextAtom.Text("b"),
      }), atom);
      Assert.Equal(@$"a{outCommand}b", TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    // Sync with CSharpMath.CoreTests LaTeXParserTests
    [Theory]
    [InlineData("#FFFFFF", "white", 0xFF, 0xFF, 0xFF)]
    [InlineData("#ffff00", "yellow", 0xFF, 0xFF, 0x00)]
    [InlineData("#ff0000ff", "blue", 0x00, 0x00, 0xFF)]
    [InlineData("#FF00FF00", "lime", 0x00, 0xFF, 0x00)]
    [InlineData("#008000", "green", 0x00, 0x80, 0x00)]
    [InlineData("#d3D3d3", "lightgray", 0xD3, 0xD3, 0xD3)]
    [InlineData("#Ff000000", "black", 0x00, 0x00, 0x00)]
    [InlineData("#fFa9A9a9", "gray", 0xA9, 0xA9, 0xA9)]
    [InlineData("cyan", "cyan", 0x00, 0xFF, 0xFF)]
    [InlineData("BROWN", "brown", 0x96, 0x4B, 0x00)]
    [InlineData("oLIve", "olive", 0x80, 0x80, 0x00)]
    [InlineData("#12345678", "#12345678", 0x34, 0x56, 0x78, 0x12)]
    [InlineData("#fedcba98", "#FEDCBA98", 0xDC, 0xBA, 0x98, 0xFE)]
    public void Color(string inColor, string outColor, byte r, byte g, byte b, byte a = 0xFF) {
      var atom = Parse($@"\color{{{inColor}}}ab");
      CheckAtom<TextAtom.List>(l =>
        Assert.Collection(l.Content,
          CheckAtom<TextAtom.Colored>(colored => {
            Assert.Equal(r, colored.Colour.R);
            Assert.Equal(g, colored.Colour.G);
            Assert.Equal(b, colored.Colour.B);
            Assert.Equal(a, colored.Colour.A);
            CheckAtom<TextAtom.Text>(t => Assert.Equal("a", t.Content))(colored.Content);
          }),
          CheckAtom<TextAtom.Text>(t => Assert.Equal("b", t.Content))
        )
      )(atom);
      Assert.Equal($@"\color{{{outColor}}}{{a}}b", TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    [Theory]
    [InlineData("`", "\u0300")] //grave
    [InlineData("'", "\u0301")] //acute
    [InlineData("^", "\u0302")] //circumflex
    [InlineData("\"", "\u0308")] //umlaut, trema or dieresis
    [InlineData("H", "\u030B")] //long Hungarian umlaut (double acute)
    [InlineData("c", "\u00B8")] //cedilla //\u0327 is not in Latin Modern Math, so forced to use \u00B8, but still no problems
    [InlineData("k", "\u02DB")] //ogonek //\u0328 is not in Latin Modern Math, so forced to use \u02DB, but still no problems
    [InlineData("=", "\u0304")] //macron accent (a bar over the letter)
    [InlineData("b", "\u0331")] //macron accent below (a bar under the letter)
    [InlineData("~", "\u0303")] //tilde
    [InlineData(".", "\u0307")] //dot over the letter
    [InlineData("d", "\u0323")] //dot under the letter
    [InlineData("r", "\u030A")] //ring over the letter
    [InlineData("u", "\u0306")] //breve over the letter
    [InlineData("v", "\u030C")] //caron/háček ("v") over the letter
    [InlineData("t", "\u23DC")] //"tie" (inverted u) over the two letters
    public void Accent(string command, string accent) {
      var atom = Parse($@"\{command} ab");
      CheckAtom<TextAtom.List>(l =>
        Assert.Collection(l.Content,
          CheckAtom<TextAtom.Accent>(a => {
            Assert.Equal(accent, a.AccentChar);
            CheckAtom<TextAtom.Text>(t => Assert.Equal("a", t.Content))(a.Content);
          }),
          CheckAtom<TextAtom.Text>(t => Assert.Equal("b", t.Content))
        )
      )(atom);
      Assert.Equal($@"\{command}{{a}}b", TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    [Theory]
    [InlineData(@"$1$", null, @"1", false, null, @"\(1\)")]
    [InlineData(@"\(1$", null, @"1", false, null, @"\(1\)")]
    [InlineData(@"$1\)", null, @"1", false, null, @"\(1\)")]
    [InlineData(@"\(1\)", null, @"1", false, null, @"\(1\)")]
    [InlineData(@"$$1$$", null, @"1", true, null, @"\[1\]")]
    [InlineData(@"\[1$$", null, @"1", true, null, @"\[1\]")]
    [InlineData(@"$$1\]", null, @"1", true, null, @"\[1\]")]
    [InlineData(@"\[1\]", null, @"1", true, null, @"\[1\]")]

    [InlineData(@"a$1$", "a", @"1", false, null, @"a\(1\)")]
    [InlineData(@"a\(1$", "a", @"1", false, null, @"a\(1\)")]
    [InlineData(@"a$1\)", "a", @"1", false, null, @"a\(1\)")]
    [InlineData(@"a\(1\)", "a", @"1", false, null, @"a\(1\)")]
    [InlineData(@"a$$1$$", "a", @"1", true, null, @"a\[1\]")]
    [InlineData(@"a\[1$$", "a", @"1", true, null, @"a\[1\]")]
    [InlineData(@"a$$1\]", "a", @"1", true, null, @"a\[1\]")]
    [InlineData(@"a\[1\]", "a", @"1", true, null, @"a\[1\]")]

    [InlineData(@"$1$b", null, @"1", false, "b", @"\(1\)b")]
    [InlineData(@"\(1$b", null, @"1", false, "b", @"\(1\)b")]
    [InlineData(@"$1\)b", null, @"1", false, "b", @"\(1\)b")]
    [InlineData(@"\(1\)b", null, @"1", false, "b", @"\(1\)b")]
    [InlineData(@"$$1$$b", null, @"1", true, "b", @"\[1\]b")]
    [InlineData(@"\[1$$b", null, @"1", true, "b", @"\[1\]b")]
    [InlineData(@"$$1\]b", null, @"1", true, "b", @"\[1\]b")]
    [InlineData(@"\[1\]b", null, @"1", true, "b", @"\[1\]b")]

    [InlineData(@"a$1$b", "a", @"1", false, "b", @"a\(1\)b")]
    [InlineData(@"a\(1$b", "a", @"1", false, "b", @"a\(1\)b")]
    [InlineData(@"a$1\)b", "a", @"1", false, "b", @"a\(1\)b")]
    [InlineData(@"a\(1\)b", "a", @"1", false, "b", @"a\(1\)b")]
    [InlineData(@"a$$1$$b", "a", @"1", true, "b", @"a\[1\]b")]
    [InlineData(@"a\[1$$b", "a", @"1", true, "b", @"a\[1\]b")]
    [InlineData(@"a$$1\]b", "a", @"1", true, "b", @"a\[1\]b")]
    [InlineData(@"a\[1\]b", "a", @"1", true, "b", @"a\[1\]b")]

    [InlineData(@"\color{red}1$\color{yellow}2$\color{blue}3", @"\color{red}1", @"\color{yellow}2", false, @"\color{blue}3", @"\color{red}{1}\(\color{yellow}{2}\)\color{blue}{3}")]
    [InlineData(@"\color{red}1\(\color{yellow}2$\color{blue}3", @"\color{red}1", @"\color{yellow}2", false, @"\color{blue}3", @"\color{red}{1}\(\color{yellow}{2}\)\color{blue}{3}")]
    [InlineData(@"\color{red}1$\color{yellow}2\)\color{blue}3", @"\color{red}1", @"\color{yellow}2", false, @"\color{blue}3", @"\color{red}{1}\(\color{yellow}{2}\)\color{blue}{3}")]
    [InlineData(@"\color{red}1\(\color{yellow}2\)\color{blue}3", @"\color{red}1", @"\color{yellow}2", false, @"\color{blue}3", @"\color{red}{1}\(\color{yellow}{2}\)\color{blue}{3}")]
    [InlineData(@"\color{red}1$$\color{yellow}2$$\color{blue}3", @"\color{red}1", @"\color{yellow}2", true, @"\color{blue}3", @"\color{red}{1}\[\color{yellow}{2}\]\color{blue}{3}")]
    [InlineData(@"\color{red}1\[\color{yellow}2$$\color{blue}3", @"\color{red}1", @"\color{yellow}2", true, @"\color{blue}3", @"\color{red}{1}\[\color{yellow}{2}\]\color{blue}{3}")]
    [InlineData(@"\color{red}1$$\color{yellow}2\]\color{blue}3", @"\color{red}1", @"\color{yellow}2", true, @"\color{blue}3", @"\color{red}{1}\[\color{yellow}{2}\]\color{blue}{3}")]
    [InlineData(@"\color{red}1\[\color{yellow}2\]\color{blue}3", @"\color{red}1", @"\color{yellow}2", true, @"\color{blue}3", @"\color{red}{1}\[\color{yellow}{2}\]\color{blue}{3}")]

    [InlineData(@"a$\$$b", "a", @"\$", false, "b", @"a\(\$ \)b")]
    [InlineData(@"a\(\$$b", "a", @"\$", false, "b", @"a\(\$ \)b")]
    [InlineData(@"a$\$\)b", "a", @"\$", false, "b", @"a\(\$ \)b")]
    [InlineData(@"a\(\$\)b", "a", @"\$", false, "b", @"a\(\$ \)b")]
    [InlineData(@"a$$\$$$b", "a", @"\$", true, "b", @"a\[\$ \]b")]
    [InlineData(@"a\[\$$$b", "a", @"\$", true, "b", @"a\[\$ \]b")]
    [InlineData(@"a$$\$\]b", "a", @"\$", true, "b", @"a\[\$ \]b")]
    [InlineData(@"a\[\$\]b", "a", @"\$", true, "b", @"a\[\$ \]b")]

    [InlineData(@"\$$\$$\$", @"\$", @"\$", false, @"\$", @"\$\(\$ \)\$")]
    [InlineData(@"\$\(\$$\$", @"\$", @"\$", false, @"\$", @"\$\(\$ \)\$")]
    [InlineData(@"\$$\$\)\$", @"\$", @"\$", false, @"\$", @"\$\(\$ \)\$")]
    [InlineData(@"\$\(\$\)\$", @"\$", @"\$", false, @"\$", @"\$\(\$ \)\$")]
    [InlineData(@"\$$$\$$$\$", @"\$", @"\$", true, @"\$", @"\$\[\$ \]\$")]
    [InlineData(@"\$\[\$$$\$", @"\$", @"\$", true, @"\$", @"\$\[\$ \]\$")]
    [InlineData(@"\$$$\$\]\$", @"\$", @"\$", true, @"\$", @"\$\[\$ \]\$")]
    [InlineData(@"\$\[\$\]\$", @"\$", @"\$", true, @"\$", @"\$\[\$ \]\$")]

    // https://github.com/verybadcat/CSharpMath/issues/113
    [InlineData(@",$,$,", @",", @",,", false, null, @",\(,,\)")]
    [InlineData(@",\(,$,", @",", @",,", false, null, @",\(,,\)")]
    [InlineData(@",$,\),", @",", @",,", false, null, @",\(,,\)")]
    [InlineData(@",\(,\),", @",", @",,", false, null, @",\(,,\)")]
    [InlineData(@",$$,$$,", @",", @",", true, @",", @",\[,\],")]
    [InlineData(@",\[,$$,", @",", @",", true, @",", @",\[,\],")]
    [InlineData(@",$$,\],", @",", @",", true, @",", @",\[,\],")]
    [InlineData(@",\[,\],", @",", @",", true, @",", @",\[,\],")]
    public void Math(string input, string? textBefore, string math, bool display, string? textAfter, string output) {
      var atom = Parse(input);
      var list = new List<TextAtom>();
      if (textBefore is { } before)
        list.Add(Parse(before));
      var inner = Atom.LaTeXParser.MathListFromLaTeX(math).Match(m => m, e => throw new Xunit.Sdk.XunitException(e));
      list.Add(new TextAtom.Math(inner, display));
      if (textAfter is { } after)
        list.Add(Parse(after));
      Assert.Equal(list.Count == 1 ? list[0] : new TextAtom.List(list), atom);
      Assert.Equal(output, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    [Theory]
    [InlineData("$", @"\$")]
    [InlineData("#", @"\#")]
    [InlineData("%", @"\%")]
    [InlineData("&", @"\&")]
    [InlineData("{", @"\{")]
    [InlineData("}", @"\}")]
    [InlineData("^", @"\textasciicircum ")]
    [InlineData("_", @"\_")]
    [InlineData(@"\", @"\backslash ")]
    public void Escape(string ch, string command) {
      if (ch != "%") Assert.Throws<Xunit.Sdk.XunitException>(() => Parse(ch));
      var atom = Parse(command);
      Assert.Equal(new TextAtom.Text(ch), atom);
      Assert.Equal(command, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }

    [Theory]
    [InlineData("{}", "")]
    [InlineData("{{}}", "")]
    [InlineData("{}{}", "")]
    [InlineData(@"\{", @"\{")]
    [InlineData(@"\}", @"\}")]
    [InlineData(@"\{\{", @"\{\{")]
    [InlineData(@"\}\}", @"\}\}")]
    [InlineData(@"\{\}", @"\{\}")]
    [InlineData(@"{\{}\}", @"\{\}")]
    [InlineData(@"\{{\}}", @"\{\}")]
    [InlineData(@"{\{\}}", @"\{\}")]
    [InlineData(@",{\{\}}", @",\{\}")]
    [InlineData(@"{,\{\}}", @",\{\}")]
    [InlineData(@"{\{,\}}", @"\{,\}")]
    [InlineData(@"{\{\},}", @"\{\},")]
    [InlineData(@"{\{\}},", @"\{\},")]
    [InlineData(@"\color{red}\{", @"\color{red}{\{}")]
    public void Braces(string input, string output) {
      var atom = Parse(input);
      Assert.Equal(output, TextLaTeXParser.TextAtomToLaTeX(atom).ToString());
    }
    [Theory,
      InlineData(@"\", @"Error: Invalid command \
\
↑ (pos 1)"),
      InlineData(@"\notacommand", @"Error: Invalid command \notacommand
\notacommand
           ↑ (pos 12)"),
      InlineData(@"\(", @"Error: Math mode was not terminated
\(
 ↑ (pos 2)"),
      InlineData(@"\[", @"Error: Math mode was not terminated
\[
 ↑ (pos 2)"),
      InlineData(@"\)", @"Error: Cannot close inline math mode outside of math mode
\)
 ↑ (pos 2)"),
      InlineData(@"\]", @"Error: Cannot close display math mode outside of math mode
\]
 ↑ (pos 2)"),
      InlineData(@"{", @"Error: Expected }, unbalanced braces
{
↑ (pos 1)"),
      InlineData(@"}", @"Error: Missing opening brace
}
↑ (pos 1)"),
      InlineData(@"#", @"Error: Unexpected command argument reference character # outside of new command definition (currently unsupported)
#
↑ (pos 1)"),
      InlineData(@"^", @"Error: Unexpected script indicator ^ outside of math mode
^
↑ (pos 1)"),
      InlineData(@"_", @"Error: Unexpected script indicator _ outside of math mode
_
↑ (pos 1)"),
      InlineData(@"^_", @"Error: Unexpected script indicator ^ outside of math mode
^_
↑ (pos 1)"),
      InlineData(@"_^", @"Error: Unexpected script indicator _ outside of math mode
_^
↑ (pos 1)"),
      InlineData(@"&", @"Error: Unexpected alignment tab character & outside of table environments
&
↑ (pos 1)"),
      InlineData(@"{%}", @"Error: Expected }, unbalanced braces
{%}
  ↑ (pos 3)"),
      InlineData(@"rewgfrh}e", @"Error: Missing opening brace
rewgfrh}e
       ↑ (pos 8)"),
      InlineData(@"\(\(", @"Error: Cannot open inline math mode in inline math mode
\(\(
   ↑ (pos 4)"),
      InlineData(@"\(\[", @"Error: Cannot open display math mode in inline math mode
\(\[
   ↑ (pos 4)"),
      InlineData(@"\(\]", @"Error: Cannot close display math mode in inline math mode
\(\]
   ↑ (pos 4)"),
      InlineData(@"\($$", @"Error: Cannot close inline math mode with $$
\($$
   ↑ (pos 4)"),
      InlineData(@"$ $$", @"Error: Cannot close inline math mode with $$
$ $$
   ↑ (pos 4)"),
      InlineData(@"\[\(", @"Error: Cannot open inline math mode in display math mode
\[\(
   ↑ (pos 4)"),
      InlineData(@"\[\[", @"Error: Cannot open display math mode in display math mode
\[\[
   ↑ (pos 4)"),
      InlineData(@"\[\)", @"Error: Cannot close inline math mode in display math mode
\[\)
   ↑ (pos 4)"),
      InlineData(@"\[$", @"Error: Cannot close display math mode with $
\[$
  ↑ (pos 3)"),
      InlineData(@"$$ $", @"Error: Cannot close display math mode with $
$$ $
   ↑ (pos 4)"),
      InlineData(@"$$$", @"Error: Invalid number of $: 3
$$$
  ↑ (pos 3)"),
      InlineData(@"$$$$", @"Error: Invalid number of $: 4
$$$$
   ↑ (pos 4)"),
      InlineData(@"\(\notacommand \frac12\(", @"Error: Cannot open inline math mode in inline math mode
···notacommand \frac12\(
                       ↑ (pos 24)"),
      InlineData(@"\(\notacommand \frac12\)", @"Error: [Math] Invalid command \notacommand
\(\notacommand \frac12\)
  ↑ (pos 3)"),
      InlineData(@"\(\notacommand \frac12\[", @"Error: Cannot open display math mode in inline math mode
···notacommand \frac12\[
                       ↑ (pos 24)"),
      InlineData(@"\(\notacommand \frac12\]", @"Error: Cannot close display math mode in inline math mode
···notacommand \frac12\]
                       ↑ (pos 24)"),
      InlineData(@"\(\notacommand \frac12$", @"Error: [Math] Invalid command \notacommand
\(\notacommand \frac12$
  ↑ (pos 3)"),
      InlineData(@"\(\notacommand \frac12$$", @"Error: Cannot close inline math mode with $$
···notacommand \frac12$$
                       ↑ (pos 24)"),
      InlineData(@"\[\notacommand \frac12\(", @"Error: Cannot open inline math mode in display math mode
···notacommand \frac12\(
                       ↑ (pos 24)"),
      InlineData(@"\[\notacommand \frac12\)", @"Error: Cannot close inline math mode in display math mode
···notacommand \frac12\)
                       ↑ (pos 24)"),
      InlineData(@"\[\notacommand \frac12\[", @"Error: Cannot open display math mode in display math mode
···notacommand \frac12\[
                       ↑ (pos 24)"),
      InlineData(@"\[\notacommand \frac12\]", @"Error: [Math] Invalid command \notacommand
\[\notacommand \frac12\]
  ↑ (pos 3)"),
      InlineData(@"\[\notacommand \frac12$", @"Error: Cannot close display math mode with $
···\notacommand \frac12$
                       ↑ (pos 23)"),
      InlineData(@"\[\notacommand \frac12$$", @"Error: [Math] Invalid command \notacommand
\[\notacommand \frac12$$
  ↑ (pos 3)"),
      InlineData(@"\color", @"Error: Missing {
\color
     ↑ (pos 6)"),
      InlineData(@"\color{", @"Error: Missing }
\color{
      ↑ (pos 7)"),
      InlineData(@"\color{notacolor}", @"Error: Invalid color: notacolor
\color{notacolor}
                ↑ (pos 17)"),
      InlineData(@"\color{red}{", @"Error: Expected }, unbalanced braces
\color{red}{
           ↑ (pos 12)"),
      InlineData(@"\color{notacolor}a", @"Error: Invalid color: notacolor
\color{notacolor}a
                ↑ (pos 17)"),
      InlineData(@"\color{#12345}a", @"Error: Invalid color: #12345
\color{#12345}a
             ↑ (pos 14)"),
      InlineData(@"\fontsize", @"Error: Missing {
\fontsize
        ↑ (pos 9)"),
      InlineData(@"\fontsize{", @"Error: Missing }
\fontsize{
         ↑ (pos 10)"),
      InlineData(@"\fontsize{p15}", @"Error: Invalid font size
\fontsize{p15}
             ↑ (pos 14)"),
      InlineData(@"\fontsize{15p}", @"Error: Invalid font size
\fontsize{15p}
             ↑ (pos 14)"),
      InlineData(@"\fontsize{15}{", @"Error: Expected }, unbalanced braces
\fontsize{15}{
             ↑ (pos 14)"),
      InlineData(@"\fontsize{15p}a", @"Error: Invalid font size
\fontsize{15p}a
             ↑ (pos 14)"),
    ]
    public void Error(string badInput, string expected) {
      var (atom, actual) = TextLaTeXParser.TextAtomFromLaTeX(badInput);
      Assert.Null(atom);
      Assert.Equal(expected.Replace("\r", null), actual);
    }
  }
}