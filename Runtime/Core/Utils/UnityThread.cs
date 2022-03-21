using System.Threading;
using UnityEngine;

namespace AUE
{
    public static class UnityThread
    {
        public static Thread thread = Thread.CurrentThread;

        public static bool allowsAPI => Thread.CurrentThread == thread;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void RuntimeInitialize()
        {
            thread = Thread.CurrentThread;
        }
    }
}
