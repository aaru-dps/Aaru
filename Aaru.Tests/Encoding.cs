namespace Aaru.Tests;

using System.Text;
using NUnit.Framework;

[SetUpFixture]
public class Tests
{
    [OneTimeSetUp]
    public void EnableEncodings() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}