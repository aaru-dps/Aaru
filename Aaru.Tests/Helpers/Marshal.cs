using NUnit.Framework;

namespace Aaru.Tests.Helpers;

[TestFixture]
public class Marshal
{
    readonly string[] _testStrings =
    {
        "275048534431364760011a77d2014701", "0235800001000000", "0xbabeface", "0xcefaadde"
    };

    readonly byte[][] _resultBytes =
    {
        new byte[]
        {
            0x27, 0x50, 0x48, 0x53, 0x44, 0x31, 0x36, 0x47, 0x60, 0x01, 0x1a, 0x77, 0xd2, 0x01, 0x47, 0x01
        },
        new byte[] { 0x02, 0x35, 0x80, 0x00, 0x01, 0x00, 0x00, 0x00 }, new byte[] { 0xba, 0xbe, 0xfa, 0xce },
        new byte[] { 0xce, 0xfa, 0xad, 0xde }
    };

    [Test]
    public void ConvertFromHexAscii()
    {
        for(var i = 0; i < _testStrings.Length; i++)
        {
            int count = Aaru.Helpers.Marshal.ConvertFromHexAscii(_testStrings[i], out byte[] buf);

            Assert.AreEqual(_resultBytes[i].Length, buf.Length);
            Assert.AreEqual(_resultBytes[i].Length, count);
            Assert.AreEqual(_resultBytes[i],        buf);
        }
    }
}