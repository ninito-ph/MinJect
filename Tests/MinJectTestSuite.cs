using FluentAssertions;
using Ninito.MinJect.Injection;
using Ninito.MinJect.Injection.Exceptions;
using Ninito.MinJect.Reflection;
using NUnit.Framework;

namespace Ninito.MinJect.Tests
{
    public sealed class MinJectTestSuite
    {
        [TestFixture]
        private sealed class BindingTests
        {
            private Injector _injector;
            private TestClass _testClass;

            [SetUp]
            public void SetUp()
            {
                _injector = new Injector();
                _testClass = new TestClass();
            }
            
            [Test]
            public void GetInstance_OfUnboundClass_ShouldThrow()
            {
                _injector.Invoking(injector => injector.GetInstance<TestClass>()).Should().Throw<InjectorException>();
            }
            
            [Test]
            public void GetInstance_OfBoundClass_ShouldNotThrow()
            {
                _injector.Bind(_testClass);
                _injector.Invoking(injector => injector.GetInstance<TestClass>()).Should().NotThrow();
            }

            [Test]
            public void GetInstance_OfBoundClass_ShouldReturnBoundInstance()
            {
                _injector.Bind(_testClass);
                _injector.GetInstance<TestClass>().Should().Be(_testClass);
            }

            [Test]
            public void GetInstance_OfUnboundInterface_ShouldThrow()
            {
                _injector.Invoking(injector => injector.GetInstance<ITestInterface>()).Should().Throw<InjectorException>();
            }

            [Test]
            public void GetInstance_OfBoundInterfaceThroughClass_ShouldThrow()
            {
                _injector.Bind(new TestClassFromInterface());

                _injector.Invoking(injector => injector.GetInstance<ITestInterface>()).Should()
                    .Throw<InjectorException>();
            }
            
            [Test]
            public void GetInstance_OfBoundInterfaceThroughInterface_ShouldNotThrow()
            {
                _injector.Bind(new TestClassFromInterface());

                _injector.Invoking(injector => injector.GetInstance<TestClassFromInterface>()).Should()
                    .NotThrow();
            }

            [Test]
            public void GetInstance_OfBoundInterface_ShouldReturnBoundInstance()
            {
                TestClassFromInterface testClass = new TestClassFromInterface();

                _injector.Bind<ITestInterface>(testClass);

                _injector.GetInstance<ITestInterface>().Should().Be(testClass);
            }
        }

        [TestFixture]
        public sealed class ParentTests
        {
            private Injector _injector;
            private Injector _parentInjector;
            private TestClass _testClass;

            [SetUp]
            public void SetUp()
            {
                _parentInjector = new Injector();
                _injector = new Injector(_parentInjector);
                _testClass = new TestClass();
            }
            
            [Test]
            public void GetInstance_OfClassBoundToParent_ShouldNotThrow()
            {
                _parentInjector.Bind(_testClass);
                
                _injector.Invoking(injector => injector.GetInstance<TestClass>()).Should().NotThrow();
            }
            
            [Test]
            public void GetInstance_OfClassBoundToParent_ShouldReturnBoundInstance()
            {
                _parentInjector.Bind(_testClass);
                
                _injector.GetInstance<TestClass>().Should().Be(_testClass);
            }

            [Test]
            public void GetInstance_OfClassBoundToAncestor_ShouldNotThrow()
            {
                _parentInjector.Bind(_testClass);

                Injector injector = null;

                for (int i = 0; i < 2; i++)
                {
                    injector = new Injector(_parentInjector);
                    _parentInjector = injector;
                }

                injector.Invoking(subject => subject.GetInstance<TestClass>()).Should().NotThrow();
            }
            
            [Test]
            public void GetInstance_OfClassBoundToAncestor_ShouldReturnBoundInstance()
            {
                _parentInjector.Bind(_testClass);

                Injector injector = null;

                for (int i = 0; i < 2; i++)
                {
                    injector = new Injector(_parentInjector);
                    _parentInjector = injector;
                }

                // ReSharper disable once PossibleNullReferenceException
                injector.GetInstance<TestClass>().Should().Be(_testClass);
            }

            [Test]
            public void GetInstance_OfClassBoundInChildAndParent_ShouldReturnInstanceBoundToChild()
            {
                TestClass testClassInChild = new TestClass();
                
                _parentInjector.Bind(_testClass);
                _injector.Bind(testClassInChild);

                _injector.GetInstance<TestClass>().Should().Be(testClassInChild);
            }
        }

        [TestFixture]
        private sealed class InjectionTests
        {
            private Injector _injector;
            private TestClass _testClass;
            private TestClassFromInterface _testClassFromInterface;
            private TestInjectingClass _testInjectingClass;

            [SetUp]
            public void SetUp()
            {
                _injector = new Injector();
                _testClass = new TestClass();
                _testClassFromInterface = new TestClassFromInterface();
                _testInjectingClass = new TestInjectingClass();
                
                _injector.Bind(_testClass);
                _injector.Bind<ITestInterface>(_testClassFromInterface);
            }
            
            [Test]
            public void InjectDependenciesOf_InjectableClass_ShouldInjectClassDependencies()
            {
                _injector.InjectDependenciesOf(_testInjectingClass);

                _testInjectingClass.GetTestClass().Should().Be(_testClass);
            }

            [Test]
            public void InjectDependenciesOf_InjectableClass_ShouldInjectInterfaceDependencies()
            {
                _injector.InjectDependenciesOf(_testInjectingClass);

                _testInjectingClass.GetTestClassFromInterface().Should().Be(_testClassFromInterface);
            }

            [Test]
            public void InjectDependenciesOnBindings_ShouldInjectClassDependencies()
            {
                _injector.Bind(_testInjectingClass);
                _injector.InjectDependenciesOnBindings();

                _testInjectingClass.GetTestClass().Should().Be(_testClass);
            }
            
            [Test]
            public void InjectDependenciesOnBindings_ShouldInjectInterfaceDependencies()
            {
                _injector.Bind(_testInjectingClass);
                _injector.InjectDependenciesOnBindings();

                _testInjectingClass.GetTestClassFromInterface().Should().Be(_testClassFromInterface);
            }
        }

        [TestFixture]
        private sealed class ReflectorTests
        {
            [Test]
            public void GetInjectableFieldsOf_TestInjectingClass_ShouldReturnTwoFields()
            {
                Reflector.GetInjectableFieldsOf(typeof(TestInjectingClass)).Length.Should().Be(2);
            }
            
            [Test]
            public void GetInjectableFieldsOf_TestInjectingClass_ShouldNotReturnNulls()
            {
                Reflector.GetInjectableFieldsOf(typeof(TestInjectingClass)).Should().NotContainNulls();
            }
        }
    }
}