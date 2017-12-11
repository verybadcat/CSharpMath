using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace CSharpMath.Tests {
  public class UnicodeTests {
    [Fact]
    public void TestPi() {
      var input = "\u03C0 is pi";
      var encoding = new UnicodeEncoding();
      var bytes = encoding.GetBytes(input);
      int nBytes = bytes.Count();
      var decoder = encoding.GetDecoder();
      var chars = new char[50];
      int index = 0;                   // Next character to write in array.
      int written = 0;                 // Number of chars written to array.
      written = decoder.GetChars(bytes, 0, nBytes, chars, index);
      index += written;
      // create a new string
      var output = new String(chars, 0, index);
      Assert.Equal(input, output);
    }

    private IEnumerable<UInt16> ToUintEnumerable(byte[] bytes) {
      for (int i=0; i<bytes.Length; i+=2) {
        if (i == bytes.Length - 1) {
          yield return bytes[i];
        } else {
          yield return BitConverter.ToUInt16(bytes, i);
        }
      }
    }

    public UInt16[] ToUintArray(byte[] bytes) {
      return ToUintEnumerable(bytes).ToArray();
    }

    public byte[] ToByteArray(UInt16[] uints) {
      byte[] r = new byte[uints.Length * 2];
      for (int i=0; i<uints.Length; i++) {
        byte[] localBytes = BitConverter.GetBytes(uints[i]);
        r[2 * i] = localBytes[0];
        r[1 + 2 * i] = localBytes[1];
      }
      return r;
    }

    [Fact]
    public void TestBitConverter() {
      var bytes = new byte[] { 10, 20, 30, 40, 50 };
      var shorts = ToUintArray(bytes);
      var bytes2 = ToByteArray(shorts);
      for (int i=0; i<bytes.Length; i++) {
        Assert.Equal(bytes[i], bytes2[i]);
      }
      if (bytes2.Length > bytes.Length) {
        Assert.Equal(0, bytes2.Last());
        Assert.Equal(bytes.Length + 1, bytes2.Length);
      }
    }

    [Fact]
    public void TestUnicodeGlyphFinder() {
      var input = "\u03C0 is pi";

      var encoder = new UnicodeGlyphFinder();
      var glyphs = encoder.FindGlyphs(input);
      var roundTrip = encoder.FindString(glyphs);
      Assert.Equal(input, roundTrip);
    }
  }
}
