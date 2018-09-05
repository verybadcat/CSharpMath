using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public static class TextAtoms {
    public static Dictionary<string, string> PredefinedReplacementAliases { get; } = new Dictionary<string, string> {
      { "backslash", @"\" },
    };
    public static BiDictionary<string, string> PredefinedReplacements { get; } = new BiDictionary<string, string> {
      { "{", "{" },
      { "}", "}" },
      { "%", "%" },
      { "$", "$" },
      { "_", "_" },
      { "P", "¶" },
      { "ddag", "‡" },
      { "textbar", "|" },
      { "textgreater", ">" },
      { "textendash", "–" },
      { "texttrademark", "™" },
      { "textexclamdown", "¡" },
      //textsuperscript
      { "pounds", "£" },
      { "#", "#" },
      { "&", "&" },
      { "S", "§" },
      { "dag", "†" },
      { "textbackslash", @"\" },
      { "textless", "<" },
      { "textemdash", "—" },
      { "textregistered", "®" },
      { "textquestiondown", "¿" },
      //textcircled
      { "copyright", "©" },
    };
    public static BiDictionary<char, char> PredefinedAccents { get; } = new BiDictionary<char, char> {

                           { '`' , '\u0300' }, //grave
                     { '\'' , '\u0301' }, //acute
                     { '^' , '\u0302' },  //circumflex
                     { '"' , '\u0308' }, //umlaut, trema or dieresis
                     { '~' , '\u0303' }, //tilde
                     { '.' , '\u0307' }, //dot
                     /*{ 'bar' , '\u0304' },
                     { 'overbar', '\u0305' }, //not in iosMath
                     { 'breve' , '\u0306' },
                     { 'ovhook', '\u0309' }, //not in iosMath
                     { 'ocirc', '\u030A' }, //not in iosMath
                     { 'check' , '\u030C' },
                     { 'leftharpoonaccent', '\u20D0' }, //not in iosMath
                     { 'rightharpoonaccent', '\u20D1' }, //not in iosMath
                     { 'vertoverlay', '\u20D2' }, //not in iosMath
                     { 'vec' , '\u20D7' },
                     { 'dddot', '\u20DB' }, //not in iosMath
                     { 'ddddot', '\u20DC' }, //not in iosMath
                     { 'widebridgeabove', '\u20E9' }, //not in iosMath
                     { 'asteraccent', '\u20F0' }, //not in iosMath
                     { 'threeunderdot', '\u20E8' } //not in iosMath*/
    };

  }
}
