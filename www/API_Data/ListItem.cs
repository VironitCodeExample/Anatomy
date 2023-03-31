using System;
using UnityEngine;
using System.Collections.Generic;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class ListItem
    {
        public int ID;
        public string Title;
        public int ParentID;
        public Constants.JsonContentType Type;
        public int AssetID;
        public int[] Materials;
        /// <summary>
        /// Use for Preperations
        /// </summary>
        public Camera.CameraController.CameraView CameraView;
        public string IconURL;
        public bool Visible;
        public bool Enabled;
        public Vector3 CenterCoord;
        /// <summary>
        /// Create C# object from parsed JSON string
        /// </summary>
        public ListItem(ListItemJson jsonItem)
        {
            ID = int.Parse(jsonItem.id);
            Title = jsonItem.title.Trim();
            ParentID = int.Parse(jsonItem.parent_id);
            Type = (Constants.JsonContentType)int.Parse(jsonItem.type);
            AssetID = int.Parse(jsonItem.item_id);
            IconURL = jsonItem.icon;

            Visible = Conversions.StringToBool(jsonItem.visible);
            Enabled = Conversions.StringToBool(jsonItem.enabled);
            // materials
            string[] tempMats = jsonItem.materials.Split(',');
            List<int> mats = new List<int>();
            foreach (string mat in tempMats)
            {
                string m = mat.Trim();
                if (!string.IsNullOrEmpty(m))
                    mats.Add(int.Parse(m));
            }
            Materials = mats.ToArray();

            //3D vectors
            try
            {
                jsonItem.view = jsonItem.view.Replace('\'', '\"');
                CameraView = JsonUtility.FromJson<Camera.CameraController.CameraView>(jsonItem.view);
            }
            catch { CameraView = null; }
            try
            {
                jsonItem.center_coord = jsonItem.center_coord.Replace('\'', '\"');
                CenterCoord = JsonUtility.FromJson<Vector3>(jsonItem.center_coord);
            }
            catch { CenterCoord = Vector3.zero; }
        }

        /// <summary>
        /// <para>Create C# object from raw JSON string.</para>
        /// <para>Parses string automatically!</para>
        /// </summary>
        public ListItem(string jsonString) : this(JsonUtility.FromJson<ListItemJson>(jsonString)) { }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ListItem() { }

        public List<GameObject> LinkedObjects;
        public void AddLinkedObject(GameObject objectToLink)
        {
            if (LinkedObjects == null)
                LinkedObjects = new List<GameObject>();

            foreach(GameObject go in LinkedObjects)
            {
                if (go == objectToLink)
                    return;
            }

            LinkedObjects.Add(objectToLink);
        }
        public GameObject LinkedObject {
            get
            {
                if (LinkedObjects == null)
                    LinkedObjects = new List<GameObject>();

                if (LinkedObjects.Count > 0)
                    return LinkedObjects[0];
                else
                    return null;
            }
        }

        public override string ToString()
        {
            return string.Format("ID: {0}; Title: {1}",ID, Title);
        }
    }
}