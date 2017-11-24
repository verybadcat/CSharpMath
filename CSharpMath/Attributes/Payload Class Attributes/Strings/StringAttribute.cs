using System;

namespace CSharpMath
{
  public class StringAttribute: Attribute<string>
  {
    private const string EscapedPercent = "%25";
    private const string EscapedComma = "%2C";
    private const string EscapedRightBrace = "%7D";
    /// <summary>When decoding, use shouldUnescape = true.  When passing in a literal display string, use false.</summary>
    public StringAttribute(string displayString) : base(displayString)
    {
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

    public override string ToString() {
      return "@" + this.PayloadString();
    }
    public override bool PayloadBool(bool defaultValue)
    {
      string pay = this.Payload;
      if (pay == null) {
        return defaultValue;
      }
      return BoolExtensions.TryParse(pay);
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
