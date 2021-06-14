using Xunit;

namespace Jint.Tests.Test262.BuiltIns
{
    public class SharedArrayBufferTests : Test262Test
    {
        [Theory(DisplayName = "built-ins\\SharedArrayBuffer")]
        [MemberData(nameof(SourceFiles), "built-ins\\SharedArrayBuffer", false)]
        [MemberData(nameof(SourceFiles), "built-ins\\SharedArrayBuffer", true, Skip = "Skipped")]
        protected void RunTest(SourceFile sourceFile)
        {
            RunTestInternal(sourceFile);
        }
    }
}