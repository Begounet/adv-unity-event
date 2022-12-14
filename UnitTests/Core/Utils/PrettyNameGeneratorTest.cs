using System;
using NUnit.Framework;

namespace AUE.Test
{
    public class PrettyNameGeneratorTest
    {
        [Test]
        public void GenerationNoParameter()
        {
            Action evt = EmptyAction;
            string prettyName = PrettyNameHelper.GeneratePrettyName(evt.GetInvocationList());
            Assert.AreEqual($"Void ({typeof(PrettyNameGeneratorTest).FullName}).{nameof(EmptyAction)}()", prettyName);

            evt = () => { };
            prettyName = PrettyNameHelper.GeneratePrettyName(evt.GetInvocationList());
            Assert.AreEqual($"Void ({typeof(PrettyNameGeneratorTest).FullName}+<>c).<{nameof(GenerationNoParameter)}>b__0_0()", prettyName);

            evt = EmptyAction;
            evt += EmptyAction2;
            prettyName = PrettyNameHelper.GeneratePrettyName(evt.GetInvocationList());
            Assert.AreEqual($"Void ({typeof(PrettyNameGeneratorTest).FullName}).{nameof(EmptyAction)}() | Void ({typeof(PrettyNameGeneratorTest).FullName}).{nameof(EmptyAction2)}()", prettyName);
        }

        [Test]
        public void GenerationTwoParameters()
        {
            Action<int, float> evt = EmptyAction3;
            string prettyName = PrettyNameHelper.GeneratePrettyName(evt.GetInvocationList());
            Assert.AreEqual($"Void ({typeof(PrettyNameGeneratorTest).FullName}).{nameof(EmptyAction3)}({typeof(int).Name} a, {typeof(float).Name} b)", prettyName);
        }

        private void EmptyAction() { }
        private void EmptyAction2() { }
        private void EmptyAction3(int a, float b) { }
    }
}
