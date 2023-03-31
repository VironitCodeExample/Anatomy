using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    public class Material:file_generic
    {
        //public int ID;
        //public string Title;
        //public string Filename;
        //public string FileURL;

        /// <summary>
        /// Create C# object from parsed JSON string
        /// </summary>
        public Material(MaterialJson jsonItem)
        {
            ID = int.Parse(jsonItem.id);
            Title = jsonItem.title;

            if (jsonItem.path_to_bundle.Length > 0)
            {
                if (jsonItem.path_to_bundle[jsonItem.path_to_bundle.Length - 1] == '/' ||
                    jsonItem.path_to_bundle[jsonItem.path_to_bundle.Length - 1] == '\\')
                    jsonItem.path_to_bundle.Remove(jsonItem.path_to_bundle.Length - 1, 1);
            }
            FileURL = string.Format("{0}/{1}{2}", jsonItem.path_to_bundle, jsonItem.filename, Constants.BundleExtension);
            FileURL = WwwManager.PrepareURL(FileURL).Trim();
            Filename = string.Format("{0}{1}", jsonItem.filename, Constants.BundleExtension);
        }

        /// <summary>
        /// <para>Create C# object from raw JSON string.</para>
        /// <para>Parses string automatically!</para>
        /// </summary>
        public Material(string jsonString) : this(JsonUtility.FromJson<MaterialJson>(jsonString)) { }

    }
}
