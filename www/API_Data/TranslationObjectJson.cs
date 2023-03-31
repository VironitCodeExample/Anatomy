using System;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class TranslationObjectJson
    {
        // public properties with EXACTLY SAME name as in JSON string
        public string trans_term_title;
        public TranslationDetailsObjectJson[] translations;
    }
}
