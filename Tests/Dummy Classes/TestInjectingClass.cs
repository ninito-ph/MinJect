using Ninito.MinJect.Injection;

namespace Ninito.MinJect.Tests
{
    public sealed class TestInjectingClass : IInjectable
    {
        [InjectField]
        private TestClass _testClass;

        [InjectField]
        private ITestInterface _otherTestClass;

        public TestClass GetTestClass() => _testClass;

        public ITestInterface GetTestClassFromInterface() => _otherTestClass;
    }
}