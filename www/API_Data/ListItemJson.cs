using UnityEngine;
using System;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class ListItemJson
    {
        // public properties with EXACTLY SAME name as in JSON string
        public string id;
        public string title;
        public string parent_id;
        public string type;
        public string item_id;
        public string materials;
        public string view;
        //public TooltipElement[] position;
        public string icon;
        public string visible;
        public string enabled;
        public string center_coord;

        public string map_url;
        public string paid;
        /// <summary>
        /// Parse JSON string
        /// </summary>
        public static ListItemJson CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<ListItemJson>(jsonString);
        }
    }
}