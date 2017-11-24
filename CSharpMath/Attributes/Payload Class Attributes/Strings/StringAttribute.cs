using System;
using CSharpMath.Debugging;
using CSharpMath.Maths;

namespace CSharpMath
{
  public class StringAttribute: Attribute<string>
  {
    private const string EscapedPercent = "%25";
    private const string EscapedComma = "%2C";
    private const string EscapedRightBrace = "%7D";
    private static string Escape(string s) {
      return CustomEscapings.Attribute.Escape(s);
    }
    private static string Unescape(string s) {
      return CustomEscapings.Attribute.Unescape(s);
    }
    /// <summary>When decoding, use shouldUnescape = true.  When passing in a literal display string, use false.</summary>
    public StringAttribute(string displayString, bool shouldUnescape = false) : base(shouldUnescape ? StringAttribute.Unescape(displayString) : displayString)
    {
    }
    public override string ToSQLiteString() {
      return this.Payload;
    }
    public override string Encoding {
      get {
        string r;
        string payload = this.Payload;
        if (payload == null) {
          r = "";
        } else {
          r = StringAttribute.Escape(payload);
        }
        return r;
      }
    }
   
    public override bool PayloadsAreEqual(string payload1, string payload2) {
      bool r = (payload1 == payload2);
      return r;
    }
    public override GenericAttribute CloneAttribute() {
      return new StringAttribute(this.Payload);
    }
    public override string PayloadString(string defaultValue = null) {
      return this.Payload;
    }
    public override string PayloadString(Func<string> makeDefaultValue) {
      return this.Payload;
    }
    public override DateTime PayloadDateTime(DateTime defaultValue)
    {
      // DateTime is currently encoded as ticks, which in turn gets decoded to a long,
      // which ends up in a StringAttribute. So we treat this as ticks.
      long defaultTicks = defaultValue.Ticks;
      long myTicks = LongAdditions.TryParse(this.Payload, defaultTicks);
      DateTime r = new DateTime(myTicks);
      return r;
    }
    public override string ToString() {
      return StringConstants.AtString + this.PayloadString();
    }
    public override FuzzyBool PayloadBoolQ()
    {
      string pay = this.Payload;
      FuzzyBool r = FuzzyBoolAdditions.TryParseQ(pay);
      return r;
    }
    public override GenericAttribute CloneAttributeButReplaceString(string replaceMe, string replacement) {
      string s = this.Payload;
      string replaceS = s.Replace(replaceMe, replacement);
      StringAttribute r = new StringAttribute(replaceS);
      return r;
    }
    public override TResult Accept<TResult>(IResultingAttributeVisitor<TResult> visitor) {
      TResult r = visitor.Visit(this);
      return r;
    }
  }
}
