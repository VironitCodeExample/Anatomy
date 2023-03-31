using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using System;

namespace AnatomyNext.WebGLApp.Www.EditorFeatures
{
    [DisallowMultipleComponent]
    public class EditingCore : MonoBehaviour
    {
        protected IEnumerator SendToAPI(Action onSuccess, Action onFail, bool prodOnly = false, params WwwManager.postData[] postData)
        {
            // Create a form object for sending data to the server
            WWWForm wwwForm = new WWWForm();

            // required POST data
            foreach (WwwManager.postData pd in WwwManager.ApiData)
                wwwForm.AddField(pd.fieldName, pd.data);

            // actions specific data
            foreach (WwwManager.postData pd in postData)
                wwwForm.AddField(pd.fieldName, pd.data);

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers = wwwForm.headers;
            headers.Add("Access-Control-Allow-Credentials", "true");
            headers.Add("Access-Control-Allow-Headers", "Accept");
            headers.Add("Access-Control-Allow-Methods", "POST");
            headers.Add("Access-Control-Allow-Origin", "*");

            string apiUrl = WwwManager.EditDetailsAPI;
            if (prodOnly && WwwManager.Domain_DevDomain)
            {
                bool isDev = WwwManager.Domain_DevDomain;
                bool isHttps = WwwManager.Domain_IsHttps;
                bool isWww = WwwManager.Domain_HasWWW;

                WwwManager.Domain_DevDomain = false;
                WwwManager.Domain_IsHttps = true;
                WwwManager.Domain_HasWWW = true;

                apiUrl = WwwManager.PrepareURL(WwwManager.EditDetailsAPI);

                WwwManager.Domain_DevDomain = isDev;
                WwwManager.Domain_IsHttps = isHttps;
                WwwManager.Domain_HasWWW = isWww;
            }

            //CustomDebug.Log("EditAPI: ", apiUrl);
            WWW upload = new WWW(apiUrl, wwwForm.data, headers);

            //Wait until the upload is done
            yield return upload;

            if (upload.error != null)
            {
                if (onFail != null)
                    onFail();
            }
            else
            {
                CustomDebug.Log(upload.text);
                if (onSuccess != null)
                    onSuccess();
            }
        }

    }
}