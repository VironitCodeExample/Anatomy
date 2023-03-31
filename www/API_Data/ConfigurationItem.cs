using UnityEngine;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    public class ConfigurationItem
    {
        public Color GlobalColor;
        private float aopacity;

        public bool isEditor { get; private set; }

        /// <summary>
        /// Create C# object from parsed JSON string
        /// </summary>
        public ConfigurationItem(ConfigurationItemJson jsonItem)
        {
            // parse color
            string rgbString = jsonItem.acolor.Remove(jsonItem.acolor.Length - 1, 1);
            rgbString = rgbString.Remove(0, 4);
            string[] rgbValueStrings = rgbString.Split(',');

            int[] RGBs = new int[] {
                int.Parse(rgbValueStrings[0]),
                int.Parse(rgbValueStrings[1]),
                int.Parse(rgbValueStrings[2]) };

            GlobalColor = (!Models.ModelManager.HighlightWithAlpha)? new Color(
                    Conversions.NormalizeValue(RGBs[0], 255),
                    Conversions.NormalizeValue(RGBs[1], 255),
                    Conversions.NormalizeValue(RGBs[2], 255)):
                new Color(
                    Conversions.NormalizeValue(RGBs[0], 255),
                    Conversions.NormalizeValue(RGBs[1], 255),
                    Conversions.NormalizeValue(RGBs[2], 255),
                    float.Parse(jsonItem.aopacity));

            isEditor = Conversions.StringToBool(jsonItem.editor);
        }

        /// <summary>
        /// <para>Create C# object from raw JSON string.</para>
        /// <para>Parses string automatically!</para>
        /// </summary>
        public ConfigurationItem(string jsonString) : this(JsonUtility.FromJson<ConfigurationItemJson>(jsonString)) { }

    }
}