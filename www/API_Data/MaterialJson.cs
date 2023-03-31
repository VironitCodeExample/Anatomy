using System;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class MaterialJson
    {
        // public properties with EXACTLY SAME name as in JSON string
        public string id;
        public string title;
        public string path_to_bundle;
        public string filename;

        /// <summary>
        /// Parse JSON string
        /// </summary>
        public static MaterialJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<MaterialJson>(jsonString);
        }
    }
}