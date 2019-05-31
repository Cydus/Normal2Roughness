using UnityEngine;

namespace Normal2Roughness
{
    /// <summary>
    /// Basic debug utility.
    /// </summary>
    public class EDebug
    {
        private static bool enabled = false;

        public static void Log(object obj)
        {
            if (enabled)
                Debug.Log(obj);
        }
    }
}