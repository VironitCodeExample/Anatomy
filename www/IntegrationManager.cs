using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/Integration Manager")]
    [DisallowMultipleComponent]
    public class IntegrationManager : GenericSingletonClass<IntegrationManager>
    {
        #region Instance
        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;
        }
        #endregion

        #region URL processing
        public enum Model { Nerves, HeadNeck, Bones, None }
        public enum Downloaders { WWW, WebRequest }

        [Header("URL parameters")]
        [SerializeField]
        private Model modelView = Model.None;
        public static Model ModelView { get { return Instance.modelView; } }

        [SerializeField]
        private Downloaders usedDownloader = Downloaders.WWW;
        public static Downloaders UsedDownloader { get { return Instance.usedDownloader; } }

        const string urlParam_UI = "ui=";
        const string urlParam_View = "view=";
        const string urlParam_DL = "dl=";

        const string viewValue_Headneck = "headneck";
        const string viewValue_HN = "hn";

        const string viewValue_Preperation = "preperation";
        const string viewValue_Preparation = "preparation";
        const string viewValue_Nerves = "nerves";
        const string viewValue_Cn = "cn";

        const string viewValue_Bones = "bones";
        const string viewValue_Skull = "skull";
        const string viewValue_Scull = "scull";

        const string dlValue_WWW = "www";
        const string dlValue_Req = "req";

        /// <summary>
        /// Read integration URL parameters
        /// </summary>
        public void ProcessURLIntegrationParameters(string url)
        {
            try
            {
                #region show/hide ui
                if (url.Contains(urlParam_UI))
                {
                    string paramValue = GetParameterValueFromUrl(url, urlParam_UI);
                    try
                    {
                        switch (int.Parse(paramValue))
                        {
                            case 0:
                                uiEnabled = false;
                                break;
                            default:
                                uiEnabled = true;
                                break;
                        }
                    }
                    catch (Exception ex) { uiEnabled = true; CustomDebug.Log(ex); }
                }
                else
                {
                    uiEnabled = true;
                }
                #endregion

                #region load model
                if (url.Contains(urlParam_View))
                {
                    string paramValue = GetParameterValueFromUrl(url, urlParam_View);
                    try
                    {
                        switch (paramValue)
                        {
                            case viewValue_Headneck:
                            case viewValue_HN:
                                modelView = Model.HeadNeck;
                                break;
                            case viewValue_Preperation:
                            case viewValue_Preparation:
                            case viewValue_Nerves:
                            case viewValue_Cn:
                                modelView = Model.Nerves;
                                break;
                            case viewValue_Bones:
                            case viewValue_Skull:
                            case viewValue_Scull:
                                modelView = Model.Bones;
                                break;
                            default:
                                modelView = Model.None;
                                break;
                        }
                    }
                    catch (Exception ex) { modelView = Model.None; CustomDebug.Log(ex); }
                }
                else
                {
                    modelView = Model.None;
                }
                #endregion

                #region downloader type
                if (url.Contains(urlParam_DL))
                {
                    string paramValue = GetParameterValueFromUrl(url, urlParam_DL);
                    try
                    {
                        switch (paramValue)
                        {
                            case dlValue_Req:
                                usedDownloader = Downloaders.WebRequest;
                                break;
                            case dlValue_WWW:
                                usedDownloader = Downloaders.WWW;
                                break;
                            default:
                                usedDownloader = Downloaders.WWW;
                                break;
                        }
                    }
                    catch (Exception ex) { usedDownloader = Downloaders.WWW; CustomDebug.Log(ex); }
                }
                else
                {
                    usedDownloader = Downloaders.WWW;
                }
                #endregion
            }
            catch (Exception e) { CustomDebug.Log(e); }

            ConfigureUI();
        }

        /// <summary>
        /// Retrieve parameter value from URL.
        /// </summary>
        /// <returns>Returns as string. Manually parse and check for string.Empty</returns>
        private string GetParameterValueFromUrl(string url,string parameterToSearch)
        {
            string result = string.Empty;
            int valueStartIndex = url.IndexOf(parameterToSearch) + parameterToSearch.Length;

            if (valueStartIndex < 0) return result;

            if (url.Substring(valueStartIndex).Contains("&"))
                result = url.Substring(valueStartIndex, url.IndexOf('&', valueStartIndex) - valueStartIndex);
            else
                result = url.Substring(valueStartIndex);

            return result;
        }

        #endregion

        #region UI Config

        [Header("Integration UI")]
        [SerializeField]
        private bool uiEnabled = true;
        public static bool UI_Enabled { get { return Instance.uiEnabled; } }
        [SerializeField]
        private List<Canvas> uiCanvas;

        private void ConfigureUI()
        {
            foreach (Canvas go in uiCanvas)
                go.enabled = uiEnabled;
        }


        #endregion
    }
}