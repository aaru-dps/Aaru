using System.Text;
using NUnit.Framework;

namespace Aaru.Tests
{
    [SetUpFixture]
    public class Encoding
    {
        [OneTimeSetUp]
        public void EnableEncodings() => System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}