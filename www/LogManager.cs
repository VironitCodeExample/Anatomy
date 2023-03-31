using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnatomyNext.WebGLApp.Www
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/Log Manager")]
    [DisallowMultipleComponent]
    public class LogManager : GenericSingletonClass<LogManager>
    {

        #region Instance
        //Awake is always called before any Start functions
        new void Awake()
        {
            base.Awake();
            if (!enabled) return;
        }
        #endregion

        /// <summary>
        /// Send message to Logging server
        /// </summary>
        public static void LogOnServer(string msg)
        {
            if (!Application.isEditor)
                Instance.StartCoroutine(Instance.SendToAPI(msg));
        }

        private IEnumerator SendToAPI(string msg)
        {
            // Create a form object for sending data to the server
            WWWForm wwwForm = new WWWForm();
            foreach (WwwManager.postData pd in WwwManager.ApiData)
            {
                wwwForm.AddField(pd.fieldName, pd.data);
            }

            wwwForm.AddField("message", msg);

            // Add POST data headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers = wwwForm.headers;
            headers.Add("Access-Control-Allow-Credentials", "true");
            headers.Add("Access-Control-Allow-Headers", "Accept");
            headers.Add("Access-Control-Allow-Methods", "POST");
            headers.Add("Access-Control-Allow-Origin", "*");

            WWW upload = new WWW(WwwManager.LogAPI, wwwForm.data, headers);

            // Wait until the upload is done
            yield return upload;

            if (upload.error != null)
            {
                CustomDebug.Log("Error sending message to server");
            }
            {
                CustomDebug.Log("Error successfully sent to server!");
            }
        }
    }
}