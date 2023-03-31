using UnityEngine;

namespace AnatomyNext.Camera
{
    [AddComponentMenu("Anatomy Next/Camera/Rotation Point")]
    [DisallowMultipleComponent]
    public class RotationPoint : GenericSingletonClass<RotationPoint>
    {
        #region Instance
        private Vector3 falsePositive;

        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;

            falsePositive = new Vector3(-1, -1, -1);

            tr = GetComponent<Transform>();
            originalPosition = zoomPivot.position = tr.position;
        }
        #endregion

        private Vector3 originalPosition;
        private Transform tr;
        public static Transform RotationPivot { get { return Instance.tr; } }

        [SerializeField]
        private Transform zoomPivot;
        public static Transform ZoomPivot { get { return Instance.zoomPivot; } }

        /// <summary>
        /// Reset rotation pivots position
        /// </summary>
        public static void ResetPosition()
        {
            Instance.tr.position = Instance.originalPosition;
        }

        /// <summary>
        /// Move pivot point to position
        /// </summary>
        public static void MoveTo(Vector3 position)
        {
            if (position == Instance.falsePositive)
                ResetPosition();
            else
                Instance.tr.position = position;
        }
    }
}