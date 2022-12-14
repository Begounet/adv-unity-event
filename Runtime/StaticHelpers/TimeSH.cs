using UnityEngine;

namespace AUE.StaticHelpers
{
    public static class TimeSH
    {
        public static float DeltaTime => Time.deltaTime;
        public static float FixedDeltaTime => Time.fixedDeltaTime;
    }
}
