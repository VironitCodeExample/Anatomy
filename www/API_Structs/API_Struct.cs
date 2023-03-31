using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Structs
{
    public enum FileType { Asset, Material }

    [Serializable]
    public class API_Struct
    {
        [SerializeField]
        private GameObject linkedUiObject;
        public GameObject LinkedUIObject { get { return linkedUiObject; } }

        [SerializeField]
        private List<int> _materials;
        [SerializeField]
        private List<int> _assets;
        public List<int> materials
        {
            get
            {
                if (_materials == null)
                    _materials = new List<int>();

                return _materials;
            }
        }
        public List<int> assets
        {
            get
            {
                if (_assets == null)
                    _assets = new List<int>();

                return _assets;
            }
        }

        /// <summary>
        /// Returns total count of files used by this preparation
        /// </summary>
        public int TotalFilesCount { get { return materials.Count + assets.Count; } }

        /// <summary>
        /// Add file used by preparation
        /// </summary>
        /// <param name="fileID">File ID</param>
        /// <param name="fileType">File type - Asset or Material</param>
        public void AddFile(int fileID, FileType fileType)
        {

            foreach (int tempID in ((fileType == FileType.Asset) ? assets : materials)) // select list based on file type
            {
                if (tempID == fileID)
                    return;
            }

            // select list based on file type
            ((fileType == FileType.Asset) ? assets : materials).Add(fileID);
        }

        public void LinkUIGameObject(GameObject go)
        {
            linkedUiObject = go;
        }
    }
}
