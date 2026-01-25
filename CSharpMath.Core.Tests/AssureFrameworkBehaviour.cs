using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CSharpMath.Core.Tests {
  public class AssureFrameworkBehaviour {
    [Fact]
    public void TestUnicodeEncodingPi() {
      var input = "\u03C0 is pi";
      var encoding = new UnicodeEncoding();
      Span<char> chars = stackalloc char[50];
      // Number of chars written to array.
      var written =
        encoding.GetDecoder().GetChars(new Span<byte>(encoding.GetBytes(input)), chars, false);
      // create a new string
      var output = new string(chars.Slice(0, written));
      Assert.Equal(input, output);
    }
    [Fact]
    public void TestBitConverter() {
      static IEnumerable<ushort> ToUintEnumerable(ReadOnlyMemory<byte> bytes) {
        for (int i = 0; i < bytes.Length; i += 2) {
          yield return i == bytes.Length - 1 ? bytes.Span[i] : BitConverter.ToUInt16(bytes.Span.Slice(i, 2));
        }
      }
      static ushort[] ToUintArray(ReadOnlyMemory<byte> bytes) => ToUintEnumerable(bytes).ToArray();
      static byte[] ToByteArray(ushort[] uints) {
        byte[] r = new byte[uints.Length * 2];
        for (int i = 0; i < uints.Length; i++) {
          byte[] localBytes = BitConverter.GetBytes(uints[i]);
          r[2 * i] = localBytes[0];
          r[1 + 2 * i] = localBytes[1];
        }
        return r;
      }
      var bytes = new ReadOnlyMemory<byte>(new byte[] { 10, 20, 30, 40, 50 });
      var shorts = ToUintArray(bytes);
      var bytes2 = ToByteArray(shorts);
      for (int i = 0; i < bytes.Length; i++) {
        Assert.Equal(bytes.Span[i], bytes2[i]);
      }
      if (bytes2.Length > bytes.Length) {
        Assert.Equal(0, bytes2.Last());
        Assert.Equal(bytes.Length + 1, bytes2.Length);
      }
    }

    [Fact]
    public void TestConvertFromUtf32() {
      var input = 0x0001D45A; // mathematical italic small m
      var chars = char.ConvertFromUtf32(input).ToCharArray();
      Assert.Equal(55349, chars[0]);
      Assert.Equal(56410, chars[1]);
    }
    [Fact]
    public void TestParseCombiningCharacters() {
      string foo = "\u0104\u0301Hello\u0104\u0304  world";
      Assert.Equal(16, foo.Length);
      Assert.Equal(2 * foo.Length, new UnicodeEncoding().GetBytes(foo).Length);
      int[] fooInfo = System.Globalization.StringInfo.ParseCombiningCharacters(foo);
      Assert.DoesNotContain(1, fooInfo);
      Assert.DoesNotContain(8, fooInfo);
    }
    [Fact]
    public void TestXunitNullIsNotObject() {
      Assert.IsNotType<object>(null);
    }
  }
}
