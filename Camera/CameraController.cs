using UnityEngine;
using UnityEngine.EventSystems;

using AnatomyNext.WebGLApp.UI;
using AnatomyNext.WebGLApp.Models;
using System;

namespace AnatomyNext.Camera
{
    [AddComponentMenu("Anatomy Next/Camera/Camera Controller")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraController : GenericSingletonClass<CameraController>
    {
        #region Instance
        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;
        }
        #endregion

        [Header("General Settings")]
        public Vector2 referenceScreenResolution;
        [SerializeField]
        private Transform meshRoot;
        public static Transform MeshRoot { get { return Instance.meshRoot; } }

        private Quaternion initialMeshFacing;
        //private Vector3 meshCenterPivotOffset = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || (!UNITY_IOS && !UNITY_ANDROID)
        [Range(0.001f, 1f)]
        public float pcModifier = 0.2f;
#endif

#if UNITY_EDITOR || UNITY_IOS
        [Range(0.001f, 1f)]
        public float iosModifier = 0.2f;
#endif
        private UnityEngine.Camera mainCamera;
        public static UnityEngine.Camera MainCamera { get { return Instance.mainCamera; } }
        private Vector2 screenScale;
        public static Vector2 ScreenScale { get { return Instance.screenScale; } }

        [Header("Rotation Settings")]
        [SerializeField]
        private bool canRotate = true;
        [Range(0.001f, 5f)]
        public float rotSpeed = 1.5f;
        [Range(0.001f, 5f)]
        public float rotationDeltaModifier = 2f;
        [Tooltip("Higher is slower decreese speed")]
        [Range(0.001f, 1f)]
        public float speedDecreeseRate = 0.8f;
        [Range(0.001f, 1f)]
        public float speedTrap = 0.009f;

        public float MaxY = 89f;
        public float MinY = -89f;
        public float MaxX = float.PositiveInfinity;
        public float MinX = float.NegativeInfinity;

        private Vector2 rotationDelta = Vector2.zero;
        private Vector2 rotationAmount = Vector2.zero;
        private Transform rotationPoint;
        public static Transform RotationPivot { get { return Instance.rotationPoint; } }
        private Transform offsetPointer;
        public static Transform OffsetPointer
        {
            get
            {
                if (Instance.offsetPointer == null)
                {
                    GameObject go = new GameObject("OffsetPointer");
                    go.transform.parent = Instance.meshRoot;
                    Instance.offsetPointer = go.transform;
                }

                return Instance.offsetPointer;
            }
        }

        private float tempRotSpeed;

        [Header("Zoom Properties")]
        [SerializeField]
        private bool canZoom = true;
        [Range(0.001f, 5f)]
        public float zoomSpeed = 1.5f;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        public float zoomModifier = 0.005f;
#else
        [Range(1f, 20f)]
        public float zoomModifier = 5f;
#endif
        public float NearZoom = 5f;
        public float FarZoom = 60f;

        private float originalCameraDistance;

        [Header("Panning Properties")]
        [SerializeField]
        private bool canPan = true;
        [Range(0.5f, 3f)]
        public float panModifier = 2f;

        private Transform panPoint;

        // touch settings
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        private Touch touch1;
        private Touch touch2;
        private float touchDirection;
#else
        private Vector3 touch1;
        private Vector3 touch2;
        private Vector3 lastTouch1;
        private Vector3 lastTouch2;
#endif
        
        // Use this for initialization
        new void Start()
        {
            mainCamera = GetComponent<UnityEngine.Camera>();
            originalCameraDistance = mainCamera.transform.position.z;

            rotationPoint = RotationPoint.RotationPivot;
            panPoint = RotationPoint.ZoomPivot;

            screenScale = new Vector2(referenceScreenResolution.x / (float)Screen.width, referenceScreenResolution.y / (float)Screen.height);

            tempRotSpeed = rotSpeed;

            initialMeshFacing = meshRoot.localRotation;

            UpdateRotationPointPosition();
        }

        private void Update()
        {
            if (UiManager.Instance)
            {
                UiManager.IsOverUI = false;
            }

            // read fingers
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR

            if (Input.touchCount > 0)
            {
                if (EventSystem.current.IsPointerOverGameObject(0))
                {
                    if (UiManager.Instance)
                    {
                        UiManager.IsOverUI = true;
                    }
                }

                touch1 = Input.GetTouch(0);
            }


            if (Input.touchCount >= 2)
            {
                touch2 = Input.GetTouch(1);
                touchDirection = Vector2.Dot(touch1.deltaPosition.normalized, touch2.deltaPosition.normalized);
            }
#else
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (UiManager.Instance)
                {
                    UiManager.IsOverUI = true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                lastTouch1 = Input.mousePosition;
            }

            if (Input.GetMouseButton(0)) // left click
                touch1 = Input.mousePosition;
            else
                touch1 = lastTouch1;

            bool altPan = false;
            if (Input.GetKey(KeyCode.RightAlt)
                || Input.GetKey(KeyCode.LeftAlt)
                || Input.GetKey(KeyCode.AltGr) && Input.GetMouseButton(0))
            {
                altPan = true;
            }

            if (Input.GetMouseButton(2) || altPan) // middle click
                touch2 = Input.mousePosition;
            else
                touch2 = lastTouch2;
#endif
            // do actual work
            if (canRotate && !altPan)
                DoRotation();

            if (canZoom)
                DoZoom();

            if (canPan)
                DoPan();

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#else
            lastTouch1 = touch1;
            lastTouch2 = Input.mousePosition;
#endif
        }

        #region Pan
        private void DoPan()
        {
            if (UiManager.Instance && (UiManager.IsLoading || UiManager.IsScrolling || UiManager.IsOverUI))
                return;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (Input.touchCount != 2)
                return;
            Vector2 tempPan = (touch1.position + touch2.position) / panModifier - (touch1.position - touch1.deltaPosition + touch2.position - touch2.deltaPosition) / panModifier;
#else // end of Mobile Only code

            bool altPan = false;
            if ((Input.GetKey(KeyCode.RightAlt)
                || Input.GetKey(KeyCode.LeftAlt)
                 || Input.GetKey(KeyCode.AltGr)) && Input.GetMouseButton(0))
            {
                altPan = true;
            }

            Vector2 tempPan = Vector2.zero;
            if (Input.GetMouseButton(2) || altPan)
            {
                tempPan = (touch2 - lastTouch2) / panModifier;
            }
            else
                return;
#endif

            float zoomLevel = Conversions.NormalizeValue(mainCamera.transform.position.z, NearZoom, FarZoom);
            Vector2 pan = tempPan + tempPan * zoomLevel * screenScale.x;
#if UNITY_IOS
            pan *= iosModifier;
#elif UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            pan *= pcModifier;
#endif // end of iOS Only Code

            float z = mainCamera.transform.position.z;
            mainCamera.transform.Translate(new Vector3(-pan.x, -pan.y, 0f) * Time.deltaTime, Space.Self);
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, z);

            //panPoint.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, panPoint.position.z);
        }
        #endregion

        #region Zoom
        private void DoZoom()
        {
            if (UiManager.Instance && (UiManager.IsLoading || UiManager.IsScrolling || UiManager.IsOverUI))
                return;
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (Input.touchCount < 2 || touchDirection >= 0.5)
                return;
            float zoom = (((touch1.position + touch1.deltaPosition) - (touch2.position + touch2.deltaPosition)).magnitude - (touch1.position - touch2.position).magnitude);
#if UNITY_IOS
            zoom *= iosModifier;
#endif // end of iOS Only Code

#else // end of Mobile Only code
            float zoom = (Input.mouseScrollDelta.y / screenScale.x) * pcModifier;
#endif
            //float zoomLevel = Conversions.NormalizeValue(mainCamera.transform.position.z, NearZoom, FarZoom);
            float z = zoom * zoomSpeed * screenScale.x;

            Vector3 tempPos = mainCamera.transform.position;
            float timeModifier = Time.deltaTime * zoomModifier;
            mainCamera.transform.Translate(new Vector3(tempPos.x, tempPos.y, z) * timeModifier, Space.Self);

            float clampedZoom = Mathf.Clamp(mainCamera.transform.position.z, NearZoom, FarZoom);
            mainCamera.transform.position = new Vector3(tempPos.x, tempPos.y, clampedZoom);
        }
        #endregion

        #region Rotations
        private void DoRotation()
        {
            if (UiManager.Instance && (!UiManager.IsLoading && !UiManager.IsScrolling && !UiManager.IsOverUI))
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (Input.touchCount == 1 && touch1.phase != TouchPhase.Ended)
            {
                rotationDelta = Vector2.Scale(touch1.deltaPosition, screenScale) * rotationDeltaModifier;
#if UNITY_IOS
                rotationDelta *= iosModifier;
#endif
                tempRotSpeed = rotSpeed;
            }

#else
                if (Input.GetMouseButton(0))
                {
                    rotationDelta = Vector2.Scale((touch1 - lastTouch1), screenScale) * rotationDeltaModifier;
                    rotationDelta *= pcModifier;
                    tempRotSpeed = rotSpeed;
                }
#endif
            }

            rotationAmount += rotationDelta * tempRotSpeed;
            rotationAmount = new Vector2(Mathf.Clamp(rotationAmount.x, MinX, MaxX), Mathf.Clamp(rotationAmount.y, MinY, MaxY));
            Quaternion quaternion = Quaternion.Euler(-rotationAmount.y, -rotationAmount.x, 0f);

            rotationPoint.rotation = quaternion;

            tempRotSpeed *= speedDecreeseRate;
            if (tempRotSpeed >= speedTrap)
                return;
            tempRotSpeed = 0f;
        }

        private void LateUpdate()
        {
            if (!canRotate) return;

            var rotation = Quaternion.LookRotation(rotationPoint.forward, rotationPoint.up);
            rotation *= initialMeshFacing;
            meshRoot.rotation = rotation;
        }
        #endregion

        /// <summary>
        /// Compensate mesh center's offset from null point
        /// </summary>
        private static void FixObjectPositions()
        {
            MeshRoot.localPosition = OffsetPointer.localPosition;
            foreach (Transform child in MeshRoot)
            {
                if (child != OffsetPointer)
                    child.localPosition += MeshRoot.localPosition * -1;
            }
        }

        #region Helpers

        public static float GetAngleRotationCoef(float eulerAngle)
        {
            if (eulerAngle > 180)
                eulerAngle -= 360;

            //if (eulerAngle > 90)
            //    eulerAngle -= 90;
            //if (eulerAngle < -90)
            //    eulerAngle += 90;

            return eulerAngle.NormalizeValue(-180, 180);//.Remap(0, 1, -1, 1);
        }

        #region Camera view preset
        private CameraView currentView;

        [Serializable]
        public class CameraView
        {
            public Vector3 CameraPosition;
            public Vector2 RotationAmount;

            public CameraView() { }

            /// <param name="CameraPosition">MainCamera.transform.position</param>
            /// <param name="RotationAmount">Instance.rotationAmount</param>
            public CameraView(Vector3 CameraPosition, Vector2 RotationAmount)
            {
                this.CameraPosition = CameraPosition;
                this.RotationAmount = RotationAmount;
            }

            public override string ToString()
            {
                return JsonUtility.ToJson(this);//.Replace('\"', '\'');
            }

            public static CameraView FromString(string jsonString)
            {
                try
                {
                    CameraView view = JsonUtility.FromJson<CameraView>(jsonString);
                    return view;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static CameraView GetCurrentView()
        {
            return new CameraView(MainCamera.transform.position, Instance.rotationAmount);
        }

        public static void SetCurrentView(CameraView viewState)
        {
            if (viewState != null)
            {
                Instance.rotationAmount = viewState.RotationAmount;

                MainCamera.transform.position = viewState.CameraPosition;

                Instance.currentView = viewState;
            }
            else
            {
                Instance.currentView = null;
                ResetCamera();
            }
        }

        public static void SetCurrentView(string viewJson)
        {
            try
            {
                CameraView view = JsonUtility.FromJson<CameraView>(viewJson);
                SetCurrentView(view);
            }
            catch
            {
                Instance.currentView = null;
                ResetCamera();
            }
        }

        #endregion

        public static void ResetCamera()
        {
            if (Instance.currentView == null)
            {
                ResetRotations();

                OffsetPointer.position = Instance.panPoint.position = Instance.rotationPoint.position;
                Instance.mainCamera.transform.position = new Vector3(Instance.rotationPoint.position.x, Instance.rotationPoint.position.y, Instance.originalCameraDistance);
            }
            else
                SetCurrentView(Instance.currentView);
        }

        public static void ResetRotations()
        {
            Instance.rotationPoint.localRotation = Instance.meshRoot.localRotation = Instance.initialMeshFacing;
            Instance.rotationDelta = Instance.rotationAmount = new Vector2(0, 0);
        }

        public static void ResetPositions()
        {
            Instance.rotationPoint.localPosition = Instance.meshRoot.localPosition = Instance.panPoint.position = Vector3.zero; //Instance.meshCenterPivotOffset = Instance.panPoint.position = Vector3.zero;
            Instance.mainCamera.transform.position = new Vector3(Instance.rotationPoint.position.x, Instance.rotationPoint.position.y, Instance.originalCameraDistance);
        }

        public static void UpdateRotationPointPosition()
        {
            ResetRotations();
            ResetPositions();

            bool fixPositions = false;
            try
            {
                if (ModelManager.CurrentListItem != null && ModelManager.CurrentListItem.CenterCoord != Vector3.zero)
                {
                    RotationPoint.MoveTo(ModelManager.CurrentListItem.CenterCoord);
                }
                else
                {
                    Vector3 center = CenterFinder.GetCenterOfMeshGroup(Instance.meshRoot);
                    RotationPoint.MoveTo(center);
                    fixPositions = true;
                }
            }
            catch { }

            OffsetPointer.position = Instance.panPoint.position = Instance.rotationPoint.position;
            Instance.mainCamera.transform.position = new Vector3(Instance.rotationPoint.position.x, Instance.rotationPoint.position.y, Instance.originalCameraDistance);
            MeshRoot.localPosition = OffsetPointer.localPosition;

            if (fixPositions)
                FixObjectPositions();
        }

        #endregion
    }
}
