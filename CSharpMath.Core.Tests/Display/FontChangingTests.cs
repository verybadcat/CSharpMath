namespace CSharpMath.Core.Tests {
  using Xunit;
  using Atom;
  public class FontChangingTests {
    // Tests are ordered by numeric value of the Atom.FontStyle enumeration
    void Test(string input, string output, string command) {
      var displays = TypesetterTests.ParseLaTeXToDisplay(@"\" + command + "{" + input + "}").Displays;
      if (input is "")
        Assert.Empty(displays);
      else {
        var display =
          Assert.IsType<Display.Displays.TextLineDisplay<BackEnd.TestFont, System.Text.Rune>>(Assert.Single(displays));
        var run = Assert.Single(display.Runs).Run;
        Assert.Equal(output, run.Text.ToString());
        Assert.Equal(output, string.Concat(run.Glyphs));
        Assert.Equal(output, string.Concat(display.Text));
        Assert.All(display.Atoms, atom => Assert.Equal(LaTeXSettings.FontStyles.FirstToSecond[command], atom.FontStyle));
      }
      Assert.Equal(output, Display.UnicodeFontChanger.ChangeFont(input, LaTeXSettings.FontStyles.FirstToSecond[command]));
    }
    // Variables become italic but Captial Greek stay upright
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "1")]
    [InlineData("a", "𝑎")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "1234567890")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝑎𝑏𝑐𝑑𝑒𝑓𝑔ℎ𝑖𝑗𝑘𝑙𝑚𝑛𝑜𝑝𝑞𝑟𝑠𝑡𝑢𝑣𝑥𝑦𝑧")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝐴𝐵𝐶𝐷𝐸𝐹𝐺𝐻𝐼𝐽𝐾𝐿𝑀𝑁𝑂𝑃𝑄𝑅𝑆𝑇𝑈𝑉𝑊𝑋𝑌𝑍")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")]
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")]
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!2|𝑎@𝐴<𝛽$Δ`𝜛|")]
    public void Default(string input, string output) => Test(input, output, "mathnormal");
    // The default appearance for characters is Roman
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "1")]
    [InlineData("a", "a")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "1234567890")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "abcdefghijklmnopqrstuvxyz")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ")]
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")]
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!2|a@A<β$Δ`ϖ|")]
    public void Roman(string input, string output) => Test(input, output, "mathrm");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "𝟏")]
    [InlineData("a", "𝐚")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "𝟏𝟐𝟑𝟒𝟓𝟔𝟕𝟖𝟗𝟎")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝐚𝐛𝐜𝐝𝐞𝐟𝐠𝐡𝐢𝐣𝐤𝐥𝐦𝐧𝐨𝐩𝐪𝐫𝐬𝐭𝐮𝐯𝐱𝐲𝐳")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝐀𝐁𝐂𝐃𝐄𝐅𝐆𝐇𝐈𝐉𝐊𝐋𝐌𝐍𝐎𝐏𝐐𝐑𝐒𝐓𝐔𝐕𝐖𝐗𝐘𝐙")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛂𝛃𝛄𝛅𝛆𝛇𝛈𝛉𝛊𝛋𝛌𝛍𝛎𝛏𝛐𝛑𝛒𝛓𝛔𝛕𝛖𝛗𝛘𝛙𝛚∂𝛜𝛝𝛞𝛟𝛠𝛡")]
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "𝚨𝚩𝚪𝚫𝚬𝚭𝚮𝚯𝚰𝚱𝚲𝚳𝚴𝚵𝚶𝚷𝚸𝚺𝚻𝚼𝚽𝚾𝚿𝛀")]
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!𝟐|𝐚@𝐀<𝛃$𝚫`𝛡|")]
    public void Bold(string input, string output) => Test(input, output, "mathbf");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "1")]
    [InlineData("a", "𝑎")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "1234567890")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝑎𝑏𝑐𝑑ℯ𝑓ℊℎ𝑖𝑗𝑘𝑙𝑚𝑛ℴ𝑝𝑞𝑟𝑠𝑡𝑢𝑣𝑥𝑦𝑧")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝒜ℬ𝒞𝒟ℰℱ𝒢ℋℐ𝒥𝒦ℒℳ𝒩𝒪𝒫𝒬ℛ𝒮𝒯𝒰𝒱𝒲𝒳𝒴𝒵")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")] // Default font
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")] // Default font
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!2|𝑎@𝒜<𝛽$Δ`𝜛|")]
    public void Caligraphic(string input, string output) => Test(input, output, "mathcal");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "𝟷")]
    [InlineData("a", "𝚊")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "𝟷𝟸𝟹𝟺𝟻𝟼𝟽𝟾𝟿𝟶")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝚊𝚋𝚌𝚍𝚎𝚏𝚐𝚑𝚒𝚓𝚔𝚕𝚖𝚗𝚘𝚙𝚚𝚛𝚜𝚝𝚞𝚟𝚡𝚢𝚣")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝙰𝙱𝙲𝙳𝙴𝙵𝙶𝙷𝙸𝙹𝙺𝙻𝙼𝙽𝙾𝙿𝚀𝚁𝚂𝚃𝚄𝚅𝚆𝚇𝚈𝚉")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")] // Default font
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")] // Default font
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!𝟸|𝚊@𝙰<𝛽$Δ`𝜛|")]
    public void Typewriter(string input, string output) => Test(input, output, "mathtt");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "1")]
    [InlineData("a", "𝑎")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "1234567890")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝑎𝑏𝑐𝑑𝑒𝑓𝑔ℎ𝑖𝑗𝑘𝑙𝑚𝑛𝑜𝑝𝑞𝑟𝑠𝑡𝑢𝑣𝑥𝑦𝑧")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝐴𝐵𝐶𝐷𝐸𝐹𝐺𝐻𝐼𝐽𝐾𝐿𝑀𝑁𝑂𝑃𝑄𝑅𝑆𝑇𝑈𝑉𝑊𝑋𝑌𝑍")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")]
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "𝛢𝛣𝛤𝛥𝛦𝛧𝛨𝛩𝛪𝛫𝛬𝛭𝛮𝛯𝛰𝛱𝛲𝛴𝛵𝛶𝛷𝛸𝛹𝛺")] // Unlike Default font
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!2|𝑎@𝐴<𝛽$𝛥`𝜛|")]
    public void Italic(string input, string output) => Test(input, output, "mathit");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "𝟣")]
    [InlineData("a", "𝖺")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "𝟣𝟤𝟥𝟦𝟧𝟨𝟩𝟪𝟫𝟢")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝖺𝖻𝖼𝖽𝖾𝖿𝗀𝗁𝗂𝗃𝗄𝗅𝗆𝗇𝗈𝗉𝗊𝗋𝗌𝗍𝗎𝗏𝗑𝗒𝗓")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝖠𝖡𝖢𝖣𝖤𝖥𝖦𝖧𝖨𝖩𝖪𝖫𝖬𝖭𝖮𝖯𝖰𝖱𝖲𝖳𝖴𝖵𝖶𝖷𝖸𝖹")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")] // Default font
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")] // Default font
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!𝟤|𝖺@𝖠<𝛽$Δ`𝜛|")]
    public void SansSerif(string input, string output) => Test(input, output, "mathsf");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "1")]
    [InlineData("a", "𝔞")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "1234567890")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝔞𝔟𝔠𝔡𝔢𝔣𝔤𝔥𝔦𝔧𝔨𝔩𝔪𝔫𝔬𝔭𝔮𝔯𝔰𝔱𝔲𝔳𝔵𝔶𝔷")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝔄𝔅ℭ𝔇𝔈𝔉𝔊ℌℑ𝔍𝔎𝔏𝔐𝔑𝔒𝔓𝔔ℜ𝔖𝔗𝔘𝔙𝔚𝔛𝔜ℨ")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")] // Default font
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")] // Default font
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!2|𝔞@𝔄<𝛽$Δ`𝜛|")]
    public void Fraktur(string input, string output) => Test(input, output, "mathfrak");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "𝟙")]
    [InlineData("a", "𝕒")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "𝟙𝟚𝟛𝟜𝟝𝟞𝟟𝟠𝟡𝟘")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝕒𝕓𝕔𝕕𝕖𝕗𝕘𝕙𝕚𝕛𝕜𝕝𝕞𝕟𝕠𝕡𝕢𝕣𝕤𝕥𝕦𝕧𝕩𝕪𝕫")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝔸𝔹ℂ𝔻𝔼𝔽𝔾ℍ𝕀𝕁𝕂𝕃𝕄ℕ𝕆ℙℚℝ𝕊𝕋𝕌𝕍𝕎𝕏𝕐ℤ")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝛼𝛽𝛾𝛿𝜀𝜁𝜂𝜃𝜄𝜅𝜆𝜇𝜈𝜉𝜊𝜋𝜌𝜍𝜎𝜏𝜐𝜑𝜒𝜓𝜔∂𝜖𝜗𝜘𝜙𝜚𝜛")] // Default font
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ")] // Default font
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!𝟚|𝕒@𝔸<𝛽$Δ`𝜛|")]
    public void Blackboard(string input, string output) => Test(input, output, "mathbb");
    [Theory]
    [InlineData("", "")]
    [InlineData("1", "𝟏")]
    [InlineData("a", "𝒂")]
    [InlineData("!", "!")]
    [InlineData("1234567890", "𝟏𝟐𝟑𝟒𝟓𝟔𝟕𝟖𝟗𝟎")]
    [InlineData("abcdefghijklmnopqrstuvxyz", "𝒂𝒃𝒄𝒅𝒆𝒇𝒈𝒉𝒊𝒋𝒌𝒍𝒎𝒏𝒐𝒑𝒒𝒓𝒔𝒕𝒖𝒗𝒙𝒚𝒛")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "𝑨𝑩𝑪𝑫𝑬𝑭𝑮𝑯𝑰𝑱𝑲𝑳𝑴𝑵𝑶𝑷𝑸𝑹𝑺𝑻𝑼𝑽𝑾𝑿𝒀𝒁")]
    [InlineData("αβγδεζηθικλμνξοπρςστυφχψω∂ϵϑϰϕϱϖ", "𝜶𝜷𝜸𝜹𝜺𝜻𝜼𝜽𝜾𝜿𝝀𝝁𝝂𝝃𝝄𝝅𝝆𝝇𝝈𝝉𝝊𝝋𝝌𝝍𝝎∂𝝐𝝑𝝒𝝓𝝔𝝕")]
    [InlineData("ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ", "𝜜𝜝𝜞𝜟𝜠𝜡𝜢𝜣𝜤𝜥𝜦𝜧𝜨𝜩𝜪𝜫𝜬𝜮𝜯𝜰𝜱𝜲𝜳𝜴")]
    [InlineData("~!@<$`|=*();+", "~!@<$`|=*();+")]
    [InlineData("!2|a@A<β$Δ`ϖ|", "!𝟐|𝒂@𝑨<𝜷$𝜟`𝝕|")]
    public void BoldItalic(string input, string output) => Test(input, output, "mathbfit");
  }
}
