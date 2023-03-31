using System;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class TermsJson
    {
        // public properties with EXACTLY SAME name as in JSON string
        public string term_id;
        public string term_paid;
        public string[] term_maps;
        public TranslationObjectJson term_description;
        public TranslationObjectJson term_long_description;
        public TranslationObjectJson term_title;
        public TooltipElement[] position;

        /// <summary>
        /// Parse JSON string
        /// </summary>
        public static TermsJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<TermsJson>(jsonString);
        }
    }
}