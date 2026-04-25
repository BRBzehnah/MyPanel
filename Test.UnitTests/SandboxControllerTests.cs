using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MyPanel.APIs.SandboxieAPI;
using Xunit;

namespace Test.UnitTests
{
    class TestSandboxController : SandboxController
    {
        private readonly bool _shouldThrow;

        public TestSandboxController(bool shouldThrow) : base(true)
        {
            _shouldThrow = shouldThrow;
        }

        protected override Process StartProcess(ProcessStartInfo psi)
        {
            if (_shouldThrow)
                throw new InvalidOperationException("Simulated start failure");

            // Do not actually start a process during tests
            return null;
        }
    }

    public class SandboxControllerTests
    {
        [Fact]
        public async Task RunBox_ReturnsTrue_WhenProcessStartsSuccessfully()
        {
            var sut = new TestSandboxController(false);

            var result = await sut.RunBox("/some-args");

            Assert.True(result);
        }

        [Fact]
        public async Task RunBox_ReturnsFalse_WhenStartProcessThrows()
        {
            var sut = new TestSandboxController(true);

            var result = await sut.RunBox("/some-args");

            Assert.False(result);
        }
    }
}
