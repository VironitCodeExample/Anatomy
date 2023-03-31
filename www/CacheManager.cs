using UnityEngine;
using System;

namespace AnatomyNext.WebGLApp.Www
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/Cache Manager")]
    [DisallowMultipleComponent]
    public class CacheManager : MonoBehaviour
    {
        [Header("Cached Assets lifetime")]
        [Range(0, 150)] [SerializeField] private int Days = 7;
        [Range(0, 24)] [SerializeField] private int Hours = 0;
        [Range(0, 60)] [SerializeField] private int Minutes = 0;
        [Range(0, 60)] [SerializeField] private int Seconds = 0;

        private const int maxCacheTime = 12960000;

        // Awake is called when the script instance is being loaded
        public void Awake()
        {
            TimeSpan time = new TimeSpan(Days, Hours, Minutes, Seconds);

            if ((int)time.TotalSeconds > maxCacheTime)
                Caching.expirationDelay = maxCacheTime;
            else
                Caching.expirationDelay = (int)time.TotalSeconds;
        }
    }
}