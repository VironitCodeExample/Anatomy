using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/WWW Manager")]
    [DisallowMultipleComponent]
    public class WwwManager : GenericSingletonClass<WwwManager>
    {
        [System.Serializable]
        public struct postData
        {
            public string fieldName;
            public string data;
        }

        public static string UserToken { get; private set; }

        [Header("Debug only!")]
        [SerializeField]
        private string currentEmbedURL;
        public static string CurrentEmbedURL { get { return Instance.currentEmbedURL; } }
#if UNITY_EDITOR
        [SerializeField]
        private bool processUrl;
#endif

        #region Instance
        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;

            Init();
        }

        public static void SetUserToken(string token)
        {
            // if assigned
            if (!string.IsNullOrEmpty(UserToken)) return;

            //Instance.apiData[0].data = token;
            UserToken = token;
        }

        private void Init()
        {
            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
            {
                currentEmbedURL = GetCurrentUrlString();
                ProcessURL(currentEmbedURL);
            }

#if UNITY_EDITOR
            if (processUrl)
                ProcessURL(currentEmbedURL);
#endif
            if(baseDomain.Length>0)
                baseDomain = PrepareURL(baseDomain);

            assetAPI = PrepareURL(assetAPI, fixApiUrl);
            materialsAPI = PrepareURL(materialsAPI, fixApiUrl);
            uiListAPI = PrepareURL(uiListAPI, fixApiUrl);

            subscriptionCheckApi = PrepareURL(subscriptionCheckApi, fixApiUrl);

            editDetailsAPI = PrepareURL(editDetailsAPI, fixApiUrl);
            logAPI = PrepareURL(logAPI, fixApiUrl);
            configurationAPI = PrepareURL(configurationAPI, fixApiUrl);
            termsAPI = PrepareURL(termsAPI, fixApiUrl);
            mapsAPI = PrepareURL(mapsAPI, fixApiUrl);

            IntegrationManager.Instance.ProcessURLIntegrationParameters(currentEmbedURL);
        }
        #endregion

        [Header("Asset URLs")]
#if UNITY_EDITOR
        [SerializeField]
        private bool fixApiUrl = true;
#else
        private bool fixApiUrl = true;
#endif
        [SerializeField]
        private string assetAPI;
        public static string AssetsAPI { get { return Instance.assetAPI; } }
        [SerializeField]
        private string materialsAPI;
        public static string MaterialsAPI { get { return Instance.materialsAPI; } }
        [SerializeField]
        private string uiListAPI;
        public static string UIListAPI { get { return Instance.uiListAPI; } }

        [Header("Other URLs")]
        [SerializeField]
        private string subscriptionCheckApi;
        public static string SubscriptionCheckAPI { get { return Instance.subscriptionCheckApi; } }
        [Space(6f)]
        [SerializeField]
        private string editDetailsAPI;
        public static string EditDetailsAPI { get { return Instance.editDetailsAPI; } }
        [SerializeField]
        private string logAPI;
        public static string LogAPI { get { return Instance.logAPI; } }
        [SerializeField]
        private string configurationAPI;
        public static string ConfigAPI { get { return Instance.configurationAPI; } }
        [SerializeField]
        private string termsAPI;
        public static string TermsAPI { get { return Instance.termsAPI; } }
        [SerializeField]
        private string mapsAPI;
        public static string MapsAPI { get { return Instance.mapsAPI; } }

        [Header("API Settings")]
        [SerializeField]
        private postData[] apiData;
        public static postData[] ApiData { get { return Instance.apiData; } }

        #region URL processing

        #region domain related
        [Header("Domain related Parameters")]
        [SerializeField]
        private string baseDomain;
        [SerializeField]
        private bool hasBaseDomain;

        [SerializeField]
        private bool domain_isHttps = false;
        public static bool Domain_IsHttps { get { return Instance.domain_isHttps; } set { Instance.domain_isHttps = value; } }
        [SerializeField]
        private bool domain_hasWWW = false;
        public static bool Domain_HasWWW { get { return Instance.domain_hasWWW; } set { Instance.domain_hasWWW = value; } }
        [SerializeField]
        private bool domain_devDomain = false;
        public static bool Domain_DevDomain { get { return Instance.domain_devDomain; } set { Instance.domain_devDomain = value; } }

        const string http = "http://";
        const string https = "https://";
        const string www = "://www.";
        const string slashes = "://";
        const string dev = "://dev.";        

        /// <summary>
        /// Get base domain from URL
        /// </summary>
        private static string getBaseDomain(string url, out bool urlProcessed)
        {
            urlProcessed = false;
            string result = url;
            if (result.Length == 0)
                return result;

            // find protocol slashes
            int protocolSlashesIndex = result.IndexOf(slashes);
            if (protocolSlashesIndex == -1) return result;
            protocolSlashesIndex += slashes.Length;

            // find tailing slashes
            int tailSlashIndex = result.IndexOf("/", protocolSlashesIndex);
            if (tailSlashIndex == -1) return result;
            int charRange = tailSlashIndex - protocolSlashesIndex;

            // crop out string
            result = result.Substring(protocolSlashesIndex, charRange);
            urlProcessed = result.Length>0;
            return result;
        }

        /// <summary>
        /// Determinate URL's protocol and if WWW subdomain is used
        /// </summary>
        private void ProcessURL(string url)
        {
            baseDomain = getBaseDomain(url,out Instance.hasBaseDomain);

#if !UNITY_EDITOR
            // get used protocol
            if (url.Contains(http))
                domain_isHttps = false;
            else if (url.Contains(https))
                domain_isHttps = true;
            else
                domain_isHttps = false;

            // has subdomain
            domain_hasWWW = (url.Contains(www)) ? true : false;
            domain_devDomain = (url.Contains(dev)) ? true : false;
#endif
        }

        /// <summary>
        /// Prepares URL for Http(s) communication
        /// </summary>
        public static string PrepareURL(string oldUrl, bool replaceBase = true)
        {
            string result = oldUrl;

            // fix http(s) protocols
            if (!result.Contains(http) && !result.Contains(https))
                result = http + result;

            result = (Instance.domain_isHttps) ? result.Replace(http, https) : result.Replace(https, http);

            // prepare domain

            if (!Instance.domain_devDomain) // prepare api for WWW domain
            {
                if (Instance.domain_hasWWW && result.Contains(dev))
                    result = result.Replace(dev, www);
                else if (Instance.domain_hasWWW && !result.Contains(www))
                    result = result.Replace(slashes, www);
                else if (!Instance.domain_hasWWW && result.Contains(www))
                    result = result.Replace(www, slashes);
            }
            else // prepare api for DEV domain
            {
                if (Instance.domain_devDomain && (!result.Contains(dev) && !result.Contains(www)))
                    result = result.Replace(slashes, dev);
                else if (Instance.domain_devDomain && (!result.Contains(dev) && result.Contains(www)))
                    result = result.Replace(www, dev);
            }

            bool tempUrlOk;
            string tempBase = getBaseDomain(result, out tempUrlOk);

            if (!replaceBase) return result;

            if (Instance.hasBaseDomain && !string.IsNullOrEmpty(tempBase))
                return result.Replace(tempBase, Instance.baseDomain);
            else
                return result;
        }
        #endregion

        #endregion

        #region Imports
        /// <summary>
        /// Get current URL (for WebGL) from internal JavaScript function
        /// </summary>
        [DllImport("__Internal")]
        private static extern string GetCurrentUrlString(); // Assets -> Plugins -> WebGL -> BrowserScripts.jslib
        #endregion
    }
}