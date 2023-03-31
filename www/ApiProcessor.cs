using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

using AnatomyNext.WebGLApp.Models;
using System.Diagnostics;
using System.Linq;
using AnatomyNext.WebGLApp.Www.API_Structs;

namespace AnatomyNext.WebGLApp.Www
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/API Processor")]
    [DisallowMultipleComponent]
    public class ApiProcessor : GenericSingletonClass<ApiProcessor>
    {
        #region Instance
        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;
        }
        #endregion

        [Space(10)]
        [SerializeField]
        private float ApiLoadingTimeout = 10f;
        public static float APILoadingTimeout { get { return Instance.ApiLoadingTimeout; } }

        #region debug bools
#if UNITY_EDITOR
#pragma warning disable 0414
        [Header("Debug only!")]
        [SerializeField]
        private bool processConfigurationAPI = true;
        [SerializeField]
        private bool processMaterialsAPI = true;
        [SerializeField]
        private bool processAssetsAPI = true;
        [SerializeField]
        private bool processListAPI = true;
        [SerializeField]
        private bool processTermsAPI = true;
        [SerializeField]
        private bool processMapsAPI = true;
#pragma warning restore 0414
#endif
        #endregion

        #region Generic APIs Processing
        /// <summary>
        /// Process API
        /// </summary>
        public IEnumerator ProcessAPI(string URL, bool postParameters, Action<string> onFail, Action<string, bool> onSuccess, Dictionary<string,string> additionalPostData=null)
        {
            #region POST header
            // Create a form object for sending data to the server
            WWWForm wwwForm = new WWWForm();

            if(additionalPostData != null)
            {
                foreach (KeyValuePair<string, string> pair in additionalPostData)
                    wwwForm.AddField(pair.Key, pair.Value);
            }

            if (postParameters)
            {
                string viewParameter = string.Empty;
                switch (IntegrationManager.ModelView)
                {
                    case IntegrationManager.Model.HeadNeck:
                        viewParameter = "headneck";
                        break;
                    case IntegrationManager.Model.Nerves:
                        viewParameter = "preparations";
                        break;
                    case IntegrationManager.Model.Bones:
                        viewParameter = "scull";
                        break;
                }

                if (!string.IsNullOrEmpty(viewParameter))
                {
                    wwwForm.AddField("view", viewParameter);
                }
            }

            var userToken = WwwManager.UserToken;

            // Add POST data headers
            Dictionary<string, string> headers = wwwForm.headers;
            headers.Add("user_token", userToken);

            foreach (WwwManager.postData pd in WwwManager.ApiData)
                headers.Add(pd.fieldName, pd.data);

            //headers.Add("Access-Control-Allow-Credentials", "true");
            //headers.Add("Access-Control-Allow-Headers", "Accept");
            //headers.Add("Access-Control-Allow-Methods", "POST");
            //headers.Add("Access-Control-Allow-Origin", "*");

            #endregion
            byte[] postData = (wwwForm.data != null && wwwForm.data.Length > 0) ? wwwForm.data : null;
            using (WWW www = new WWW(URL, postData, headers))
            {
                float timer = 0;
                bool timeoutReached = false;

                while (!www.isDone)
                {
                    if (timer > ApiLoadingTimeout && www.progress == 0) // if timeout reached and still no progress
                    {
                        timeoutReached = true;
                        break;
                    }
                    timer += Time.deltaTime;

                    yield return null;
                }

                if (!string.IsNullOrEmpty(www.error) || timeoutReached)
                {
                    string msg = string.Empty;
                    if (www.error != null)
                        msg = "WWW error: " + www.error + " URL: " + www.url;
                    else if (timeoutReached)
                        msg = "WWW error: Timeout reached. URL: " + www.url;

                    onFail(msg);
                }
                else
                {
                    //string jsonText = Conversions.RepairJSON(www.text);
                    string jsonText = www.text;
                    if (jsonText != "[]")
                        onSuccess(jsonText, true);
                }
            }
        }

        public void OnFail(string msg)
        {
            throw new CustomException(msg);
        }
        #endregion

        /// <summary>
        /// Main processing function
        /// </summary>
        public static void StartApiProcessing(Action OnApiProcessed = null)
        {
            Instance.StartCoroutine(Instance.StartApiProcessingRoutine(OnApiProcessed));
        }

        /// <summary>
        /// Main processing function
        /// </summary>
        private IEnumerator StartApiProcessingRoutine(Action OnApiProcessed = null)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int progress = 0;
            int allSteps = IntegrationManager.ModelView == IntegrationManager.Model.Nerves ? 6 : 5;


            //yield return StartCoroutine(ProcessAPI(WwwManager.SubscriptionCheckAPI,
                                                   //false, OnFail,
                                                   //UI.UiManager.Instance.OnSubscriptionCheckResult));

#if UNITY_EDITOR
            if (processConfigurationAPI)
#endif
            {
                progress++;
                //get global configuration
                yield return StartCoroutine(ProcessAPI(WwwManager.ConfigAPI, false, OnFail,
                                                       ApiParser.ParseConfigurationAPI));
                UI.UiManager.Instance.progress = Conversions.MapRanges(progress, 0, allSteps, 0.05f, 0.67f);
            }

            // get materials list
#if UNITY_EDITOR
            if (processMaterialsAPI)
#endif
            {
                progress++;
                yield return StartCoroutine(ProcessAPI(WwwManager.MaterialsAPI, true, OnFail,
                                                       ApiParser.ParseMaterialsAPI));
                UI.UiManager.Instance.progress = Conversions.MapRanges(progress, 0, allSteps, 0.05f, 0.67f);
            }

            //get assets list
#if UNITY_EDITOR
            if (processAssetsAPI)
#endif
            {
                progress++;
                yield return StartCoroutine(ProcessAPI(WwwManager.AssetsAPI, true, OnFail,
                                                       ApiParser.ParseAssetsAPI));
                UI.UiManager.Instance.progress = Conversions.MapRanges(progress, 0, allSteps, 0.05f, 0.67f);
            }

            // get UI list
#if UNITY_EDITOR
            if (processListAPI)
#endif
            {
                progress++;
                yield return StartCoroutine(ProcessAPI(WwwManager.UIListAPI, true, OnFail,
                                                       ApiParser.ParseListAPI));
                UI.UiManager.Instance.progress = Conversions.MapRanges(progress, 0, allSteps, 0.05f, 0.67f);
            }

            if (IntegrationManager.ModelView == IntegrationManager.Model.Nerves)
            {
                // get annotations list
#if UNITY_EDITOR
                if (processTermsAPI)
#endif
                {
                    progress++;
                    //Dictionary<string, string> addData = new Dictionary<string, string>();
                    //addData.Add("allTerms", "f");
                    yield return StartCoroutine(ProcessAPI(WwwManager.TermsAPI, true, OnFail,
                                                           ApiParser.ParseTermsAPI));//, addData));
                    UI.UiManager.Instance.progress = Conversions.MapRanges(progress, 0, allSteps, 0.05f, 0.67f);
                }

#if UNITY_EDITOR
                if (processMapsAPI)
#endif
                {
                    progress++;
                    yield return StartCoroutine(ProcessAPI(WwwManager.MapsAPI, true, OnFail,
                                                           ApiParser.ParseMapsAPI));
                    UI.UiManager.Instance.progress = Conversions.MapRanges(progress, 0, allSteps, 0.05f, 0.67f);
                }
            }

            stopWatch.Stop();
            CustomDebug.LogWarning("All API's Processed: ", Conversions.TimespanToString(stopWatch));

#if UNITY_EDITOR
            AssetManager.UpdateDebugData();
#endif

            if (OnApiProcessed != null)
                OnApiProcessed();
        }
    }
}