using System;

using AnatomyNext.WebGLApp.Www.API_Data;
using System.Collections.Generic;

using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Structs
{
    [Serializable]
    public class Preparation : API_Struct
    {
        public string title;
        public string iconUrl;
        public ListItem properties;

        /// <summary>
        /// ListID's mapped with ListID's. Key: listID, Value: assetID
        /// </summary>
        public Dictionary<int, int> list_asset_map;

        [SerializeField]
        private List<file_generic> preparationFiles;
        public List<file_generic> PreparationFiles
        {
            get
            {
                preparationFiles = new List<file_generic>();

                foreach (int material in materials)
                {
                    try
                    {
                        preparationFiles.Add(Models.AssetManager.MaterialsTable[material]);
                    }
                    catch
                    {
                        CustomDebug.Log(string.Format("Error preparing materials list. \nPreparation: {0}; Material: {1}", properties.ID, material));
                    }
                }

                foreach (int asset in assets)
                {
                    try
                    {
                        preparationFiles.Add(Models.AssetManager.AssetsTable[asset]);
                    }
                    catch
                    {
                        CustomDebug.Log(string.Format("Error preparing assets list. \nPreparation: {0}; Asset: {1}", properties.ID, asset));
                    }
                }

                return preparationFiles;
            }
        }

        public Preparation(ListItem properties)
        {
            this.properties = properties;
            this.title = properties.Title;
            iconUrl = properties.IconURL;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(properties.IconURL))
            {
                System.Diagnostics.StackFrame stack = new System.Diagnostics.StackFrame(1, true);
                int lineNumber = stack.GetFileLineNumber();
                string fileName = stack.GetFileName();
                Debug.Log("File " + fileName + " at line " + lineNumber + " says: " + properties.IconURL);
            }
#endif
            // Link materials with preperations
            foreach (int matID in properties.Materials)
            {
                AddFile(matID, FileType.Material);
            }

            list_asset_map = new Dictionary<int, int>();
        }

#region void MapAssets
        /// <summary>
        /// Map ListID with AssetID belonging to this preparation
        /// </summary>
        /// <param name="listID"></param>
        /// <param name="assetID"></param>
        public void MapAssets(int listID, int assetID)
        {
            if (!list_asset_map.ContainsKey(listID))
                list_asset_map.Add(listID, assetID);
        }

        /// <summary>
        /// Map ListID with AssetID belonging to this preparation
        /// </summary>
        public void MapAssets(ListItem listItem)
        {
            MapAssets(listItem.ID, listItem.AssetID);
        }
#endregion
    }
}
