using System;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class AssetJson
    {
        // public properties with EXACTLY SAME name as in JSON string
        public string id;
        public string title;
        public string path_to_bundle;
        public string filename;
        public string type;

        /// <summary>
        /// Parse JSON string
        /// </summary>
        public static AssetJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AssetJson>(jsonString);
        }
    }
}
