using System.Text;
using NUnit.Framework;

namespace Aaru.Tests;

[SetUpFixture]
public class Tests
{
    [OneTimeSetUp]
    public void EnableEncodings() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}