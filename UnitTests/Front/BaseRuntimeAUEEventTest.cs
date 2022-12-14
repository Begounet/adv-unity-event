using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AUE.Test
{
    public class BaseRuntimeAUEEventTest
    {
        [Test]
        public void EventSubscription()
        {
            bool hasBeenCalled = false;
            var aueEvt = new AUEEvent();
            aueEvt.AddAction(() => hasBeenCalled = true);
            Assert.IsFalse(hasBeenCalled);
            aueEvt.Invoke();
            Assert.IsTrue(hasBeenCalled);
        }

        [Test]
        public void EventSubscriptionWithParameters()
        {
            bool hasBeenCalled = false;
            int intSample = 42;
            float floatSample = 12.5f;
            string stringSample = "hello";

            var aueEvt = new AUEEvent<int, float, string>();
            aueEvt.AddAction((intValue, floatValue, stringValue) =>
            {
                Assert.AreEqual(intSample, intValue);
                Assert.AreEqual(floatSample, floatValue);
                Assert.AreEqual(stringSample, stringValue);
                hasBeenCalled = true;
            });

            Assert.IsFalse(hasBeenCalled);
            aueEvt.Invoke(intSample, floatSample, stringSample);
            Assert.IsTrue(hasBeenCalled);
        }

        [Test]
        public void EventOrder()
        {
            string test = string.Empty;
            var aueEvt = new AUEEvent();
            aueEvt.AddAction(() => test += 'a');
            aueEvt.AddAction(() => test += 'b');
            aueEvt.Invoke();
            Assert.AreEqual("ab", test);
        }

        [Test]
        public void RuntimeAddAndRemoveEvents()
        {
            int num = 0;
            Action indent = () => ++num;
            var aueEvt = new AUEEvent();
            aueEvt.AddAction(indent);
            aueEvt.Invoke();
            Assert.AreEqual(1, num);

            num = 0;
            aueEvt.AddAction(indent);
            aueEvt.Invoke();
            Assert.AreEqual(2, num);

            num = 0;
            aueEvt.RemoveAction(indent);
            aueEvt.Invoke();
            Assert.AreEqual(1, num);
        }

        [Test]
        public void BoundState()
        {
            var aueEvt = new AUEEvent();
            Assert.IsFalse(aueEvt.IsBound);
            aueEvt.AddAction(() => { });
            Assert.IsTrue(aueEvt.IsBound);
        }
    }
}
