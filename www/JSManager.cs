using System;
using System.Runtime.InteropServices;
using UnityEngine;

using AnatomyNext.WebGLApp.Models;
using AnatomyNext.WebGLApp.Www.API_Structs;
using AnatomyNext.WebGLApp.Www.API_Data;
using AnatomyNext.WebGLApp.UI;

namespace AnatomyNext.WebGLApp.Www
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/JS Manager")]
    [DisallowMultipleComponent]
    public class JSManager : GenericSingletonClass<JSManager>
    {
        #region Instance
        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;
        }
        #endregion

        [Serializable]
        private class ViewState
        {
            public bool preperation;
            public string groupID;
            public string listID;

            public ViewState() { }

            public ViewState(bool preperation)
            {
                this.preperation = preperation;
                groupID = "-1";
                listID = "-1";
            }

            public ViewState(bool preperation, string groupID, string listID)
            {
                this.preperation = preperation;
                this.groupID = groupID;
                this.listID = listID;
            }
        }

        #region Receive from external JS
        /// <summary>
        /// Highlight 3D model from JavaScript.
        /// </summary>
        public void Highlight3DModel(int listID)
        {
            if (IntegrationManager.ModelView == IntegrationManager.Model.Nerves)
            {
                #region cranial nerves
                Preparation currentPrep = AssetManager.CranialNerves.TryGetPreparationByID(ModelManager.CurrentPreparationID);
                if (currentPrep != null)
                {
                    if (currentPrep.list_asset_map.ContainsKey(listID))
                    {
                        ModelManager.HighlightModel(AssetManager.ListItemsTable[listID]);
                    }
                    else
                    {
                        CustomDebug.Log("Asset in current preparation not found");
                    }
                }
                else
                {
                    ModelManager.DisableModelsHighlight();
                    JSManager.Current3DObject(-1);
                }
                #endregion
            }
            else if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                #region Head & neck
                try
                { 
                    ModelManager.HighlightModel(AssetManager.ListItemsTable[listID]);
                }
                catch
                {
                    CustomDebug.Log("Asset not found in Head & Neck");
                    ModelManager.DisableModelsHighlight();
                    JSManager.Current3DObject(-1);
                }
                #endregion
            }
        }

        /// <summary>
        /// Switch preparation click from JavaScript. 
        /// </summary>
        public void SwitchPreparation(int listID)
        {
            Preparation prep = AssetManager.CranialNerves.TryGetPreparationByID(listID);
            if(prep != null)
                DownloadManager.StartLoadPreparation(prep);
        }

        /// <summary>
        /// Call tooltip from JavaScript. Passing invalid ID clears tooltip visibility
        /// </summary>
        /// <param name="termID"></param>
        public void ShowTooltip(int termID)
        {
            ModelManager.ShowTooltip(termID);
        }

        /// <summary>
        /// Reset Camera and rotation pivot point's position
        /// </summary>
        public void Reset3DView()
        {
            ModelManager.ResetCamera();
        }

        /// <summary>
        /// Clear scene from 3D models
        /// </summary>
        public void Clear3DScene()
        {
            Constants.ClearTransform(AssetManager.AssetsRoot);
        }

        #region system visibility
        public void SetVisibility(string parameters)// id,1/0 (true/false)
        {
            string[] parametersArr = parameters.Split(',');
            SetGroupVisibility(int.Parse(parametersArr[0]), Conversions.StringToBool(parametersArr[1]));
        }

        /// <summary>
        /// Change Model visibility from JS
        /// </summary>
        private void SetGroupVisibility(int listID, bool isVisible)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                #region Head & neck
                try
                {
                    AssetManager.ListItemsTable[listID].Visible = isVisible;
                    ModelManager.RenderModel(AssetManager.ListItemsTable[listID], isVisible);
                }
                catch
                {
                    CustomDebug.Log("Asset not found in Head & Neck");
                }
                #endregion
            }
        }
        #endregion

        #region asset hiding functions
        /// <summary>
        /// Hide selected asset from web
        /// </summary>
        public void AssetHide(int listID)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                ModelManager.ShowHideAsset(listID, false, true);
            }
        }

        /// <summary>
        /// Show hided asset from web
        /// </summary>
        public void AssetShow(int listID)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                ModelManager.ShowHideAsset(listID, true, true);
            }
        }

        /// <summary>
        /// Hide all assets (except selected one) from web
        /// </summary>
        public void AssetHideOthers(int ignoredListID)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                ModelManager.HideOtherAssets(ignoredListID);
            }
        }
        #endregion

        #region asset fadeing functions
        /// <summary>
        /// Fade selected asset from web
        /// </summary>
        public void AssetFade(int listID)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                ModelManager.FadeUnfadeAsset(listID, false, true);
            }
        }

        /// <summary>
        /// Unfade asset from web
        /// </summary>
        public void AssetUnfade(int listID)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                ModelManager.FadeUnfadeAsset(listID, true, true);
            }
        }

        /// <summary>
        /// Fade all assets (except selected one) from web
        /// </summary>
        public void AssetFadeOthers(int ignoredListID)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                ModelManager.FadeOtherAssets(ignoredListID);
            }
        }
        #endregion

        #endregion

        #region Call in external JS

        /// <summary>
        /// Signal Web page that Unity container is ready for use
        /// </summary>
        public static void UnityLoaded()
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                UnityLoadedJS();

            CustomDebug.Log("Unity Loaded & API processed");
        }

        /// <summary>
        /// Pass 3D viewer state to external JS
        /// </summary>
        public static void SendCurrent3DViewToJS(bool preperation = false, string groupID = "-1", string listID = "-1")
        {
            ViewState vs = new ViewState(preperation, groupID, listID);
            string jsonString = JsonUtility.ToJson(vs);

            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Current3DViewJS(jsonString);
            }
        }

        /// <summary>
        /// Tell web about currently selected asset (list id)
        /// </summary>
        public static void Current3DObject(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                Current3DObjectJS(listID.ToString());

            //CustomDebug.Log("Current ID:", listID);
        }

        #region loading signalers

        #region  void PreparationLoadProgress
        /// <summary>
        /// Inform external JS about preperation load progress
        /// </summary>
        public static void PreparationLoadProgress(int listID, int loadPercentage)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                PreperationLoadProgressJS(listID.ToString(), loadPercentage.ToString());
        }

        /// <summary>
        /// Inform external JS about preperation load progress
        /// </summary>
        public static void PreparationLoadProgress(int listID, float normalizedLoadPercentage)
        {
            int percentage = (int)(normalizedLoadPercentage * 100);
            PreparationLoadProgress(listID, percentage);
        }
        #endregion

        #region void HNLoadProgress
        /// <summary>
        /// Inform external JS about preperation load progress
        /// </summary>
        public static void HNLoadProgress(int loadPercentage)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                HNLoadProgressJS(loadPercentage.ToString());
        }

        /// <summary>
        /// Inform external JS about preperation load progress
        /// </summary>
        public static void HNLoadProgress(float normalizedLoadPercentage)
        {
            int percentage = (int)(normalizedLoadPercentage * 100);
            HNLoadProgress(percentage);
        }
        #endregion

        /// <summary>
        /// Inform external JS about loaded preperation
        /// </summary>
        public static void PreparationLoaded(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                PreperationLoadedJS(listID.ToString());
        }

        /// <summary>
        /// Inform external JS about loaded (downloaded & displayed) ListID
        /// </summary>
        public static void ListIDLoaded(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                ListItemLoadedJS(listID.ToString());
        }
        #endregion

        #region asset hiding functions
        public static void AssetHided(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                AssetHidedJS(listID.ToString());

            CustomDebug.Log("Hided: ", listID);
        }

        public static void AssetHided(string listIdArray)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                AssetHidedJS(listIdArray);

            CustomDebug.Log("Hided: ", listIdArray);
        }

        public static void AssetShowed(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                AssetShowedJS(listID.ToString());

            CustomDebug.Log("Unhided: ", listID);
        }
        #endregion

        #region asset fadeing functions
        public static void AssetFaded(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                AssetFadedJS(listID.ToString());

            CustomDebug.Log("Faded: ", listID);
        }

        public static void AssetFaded(string listIdArray)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                AssetFadedJS(listIdArray);

            CustomDebug.Log("Faded: ", listIdArray);
        }

        public static void AssetUnfaded(int listID)
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
                AssetUnfadedJS(listID.ToString());

            CustomDebug.Log("Unfaded: ", listID);
        }
        #endregion

        #endregion

        #region JS imports
        [DllImport("__Internal")]
        private static extern void Current3DObjectJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void Current3DViewJS(string jsonString); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void UnityLoadedJS(); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void PreperationLoadedJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void PreperationLoadProgressJS(string listID, string percentage); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void HNLoadProgressJS(string percentage); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void AssetHidedJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void AssetShowedJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void AssetFadedJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void AssetUnfadedJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        [DllImport("__Internal")]
        private static extern void ListItemLoadedJS(string listID); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib

        #endregion
    }
}