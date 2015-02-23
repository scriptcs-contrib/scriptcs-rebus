using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Idioms;

namespace ScriptCs.Rebus.Test
{
    [TestFixture]
    public abstract class TestBase<T> where T : class
    {
        protected static IFixture Fixture;
        protected GuardClauseAssertion GuardClauseAssertion;
        private bool _methodVerificationDisabled;
        
        [SetUp]
        public void Setup()
        {
            Fixture = new Fixture().Customize(new CompositeCustomization(new AutoMoqCustomization(), new MultipleCustomization()));

            GuardClauseAssertion = new GuardClauseAssertion(Fixture);

            DoSetUp();
        }

        protected virtual void DoSetUp()
        {

        }

        [DebuggerStepThrough]
        protected virtual T SUT()
        {
            return Fixture.Create<T>();
        }

        ///<summary>
        /// Guard Clause verification.
        ///</summary>
        [Test]
        public void GuardClauseVerification()
        {
            // Arrange
            var methods =
                typeof(T).GetMethods(BindingFlags.DeclaredOnly |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance);

            // Act
            // Assert
            GuardClauseAssertion.Verify(typeof(T).GetConstructors());

            if (!_methodVerificationDisabled)
            {
                GuardClauseAssertion.Verify(methods.Where(info => !info.Name.StartsWith("set_")));
            }
        }

        protected void DisableGuardClauseVerificationOnMethods()
        {
            _methodVerificationDisabled = true;
        }
    }
}
