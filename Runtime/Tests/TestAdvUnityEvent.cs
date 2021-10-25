using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AUE
{
    public class TestAdvUnityEvent : MonoBehaviour, ISerializationCallbackReceiver
    {
        [System.Serializable]
        public class Test
        {
            [ReadOnly]
            public float lol;
        }


        public class PathTest
        {
            public void LogStuff()
            {
                Debug.Log("PathTest.LogStuff");
            }
        }

        [System.Serializable]
        public class FloatAdvUnityEvent : AUEEvent<float> { }

        [SerializeField]
        private AUEEvent _simpleEvent;

        [SerializeField]
        private AUEEvent<int> _intEvent;

        [SerializeField]
        private AUEEvent<int, float, string> _argEvent;

        [SerializeField]
        private AUEGet<int, int> _intGetter;

        [System.Serializable]
        public class FloatUnityEvent : UnityEvent<float> { }

        [System.Serializable]
        public class FloatIntUnityEvent : UnityEvent<float, int> { }

        [SerializeField]
        private FloatUnityEvent _fltUnityEvent;

        [SerializeField]
        private AUEEvent<float> _fltAUE;

        [SerializeField]
        private FloatIntUnityEvent _fltIntUnityEvent;

        [SerializeField]
        private AUEEvent<float, int> _fltIntAUE;

        [SerializeField]
        private UnityEvent _unityEvent;

        [SerializeField]
        private AUEEvent _aueEvent;


        [SerializeField]
        private FloatAdvUnityEvent _floatEvent;

        public string HelloWorld => "Hello World";

        public PathTest Path = new PathTest();


        void OnEnable()
        {
            _simpleEvent.Invoke();
            _intEvent.Invoke(42);
            _floatEvent.Invoke(38.5f);

            Debug.Log(_intGetter.Invoke(42));
        }

        [Button]
        public void TriggerFloatEvent(float value)
        {
            _floatEvent.Invoke(value);
        }

        public void LogStuff()
        {
            Debug.Log("log stuff");
        }
        
        public void LogStuff(Test t)
        {
        }

        public void DoWithObject(int intValue, float floatValue, string strValue, bool boolValue, Vector2 vector2Value, Vector3 vector3Value, Transform transValue, UnityEngine.Object objValue, MonoBehaviour mb)
        {
            Debug.Log("Execute DoWithObject");
            Debug.Log(intValue);
            Debug.Log(floatValue);
            Debug.Log(strValue);
            Debug.Log(boolValue);
            Debug.Log(vector2Value);
            Debug.Log(vector3Value);
            Debug.Log(transValue);
            Debug.Log(objValue);
            Debug.Log(mb);
        }

        public int Add(int v1, int v2)
        {
            return v1 + v2;
        }

        public void FloatAndInt(float f, int i) { }

        public void OnBeforeSerialize()
        {
            Upgrader.ToAUEEvent(_fltUnityEvent, _fltAUE);
            Upgrader.ToAUEEvent(_fltIntUnityEvent, _fltIntAUE);
            Upgrader.ToAUEEvent(_unityEvent, _aueEvent);
        }

        public void OnAfterDeserialize()
        {
        }

    }
}