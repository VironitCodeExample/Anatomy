using System;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class ConfigurationItemJson
    {
        // public properties with EXACTLY SAME name as in JSON string
        /// <summary>
        /// Transperency as float from 0 to 1
        /// </summary>
        public string aopacity = "0.3";
        /// <summary>
        /// RGB string: rgb(R, G, B) with ints in range from 0 to 255
        /// </summary>
        public string acolor = "rgb(255,0,0)";
        /// <summary>
        /// Enable editor mode? Parse as bool - string: t/f
        /// </summary>
        public string editor = "f";

        /// <summary>
        /// Parse JSON string
        /// </summary>
        public static ConfigurationItemJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<ConfigurationItemJson>(jsonString);
        }
    }
}
