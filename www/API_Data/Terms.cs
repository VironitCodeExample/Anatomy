using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

namespace AnatomyNext.WebGLApp.Www.API_Data
{
    [Serializable]
    public class Terms
    {
        public int ID;
        private TranslationObjectJson Titles;
        private TranslationObjectJson Description;
        private TranslationObjectJson LongDescription;
        public bool HasLongDescription { get; private set; }

        public int[] Maps;
        public bool Paid;
        [SerializeField]
        private TooltipElement[] tooltip_Elements;
        //public TooltipElement[] TooltipElements { get { return tooltip_Elements; } }

        /// <summary>
        /// Create C# object from parsed JSON string
        /// </summary>
        public Terms(TermsJson jsonItem)
        {
            ID = int.Parse(jsonItem.term_id);
            Titles = jsonItem.term_title;

            Description = jsonItem.term_description;
            LongDescription = jsonItem.term_long_description;

			if (LongDescription.translations != null)
			{
				HasLongDescription = LongDescription.translations.Any(tr => !string.IsNullOrEmpty(tr.text));
			}            

            Paid = Conversions.StringToBool(jsonItem.term_paid);
            Maps = new int[jsonItem.term_maps.Length];
            for (int i = 0; i < jsonItem.term_maps.Length; i++)
            {
                Maps[i] = int.Parse(jsonItem.term_maps[i]);
            }
            tooltip_Elements = jsonItem.position;
        }

        /// <summary>
        /// Tries to return title in required language. Returns empty string if no translation for required language. Default is EN
        /// </summary>
        /// <returns>Returns empty string if no translation for required language. Default is EN</returns>
        public string GetTitle(string language = "EN")
        {
            foreach(TranslationDetailsObjectJson translation in Titles.translations)
            {
                if (translation.lang.ToLower() == language.ToLower())
                    return translation.text.Trim();
            }

            return string.Empty;
        }

        public string GetShortDescription(string language = "EN")
        {
            foreach (TranslationDetailsObjectJson translation in Description.translations)
            {
                if (translation.lang.ToLower() == language.ToLower())
                    return translation.text.Trim()
                                      .Replace("&#10;", "\n")
                                      .Replace("&#13;", "")
                                      .Replace("&amp;#10;", "\n");
            }

            return string.Empty;
        }

        public string GetLongDescription(string language = "EN")
        {
            foreach (TranslationDetailsObjectJson translation in LongDescription.translations)
            {
                if (translation.lang.ToLower() == language.ToLower())
                    return translation.text.Trim()
                                      .Replace("&#10;", "\n")
                                      .Replace("&#13;", "")
                                      .Replace("&amp;#10;", "\n");
            }
            return string.Empty;
        }

        /// <summary>
        /// Try to get tooltip for passet preparation
        /// </summary>
        /// <param name="preparationID">Preparation ID you are searching for</param>
        /// <returns>Returns tooltip position in space or Constants.FalsePositiveTooltip if no tooltip found</returns>
        public Vector3 TryGetTooltipPosition(int preparationID)
        {
            if (tooltip_Elements != null && tooltip_Elements.Length > 0)
            {
                foreach(TooltipElement tt in tooltip_Elements)
                {
                    if (int.Parse(tt.preparation_id) == preparationID)
                        return tt.position;
                }

                return Constants.FalsePositiveTooltip;
            }
            else
                return Constants.FalsePositiveTooltip;
        }

        /// <summary>
        /// Save tooltips position after successful terms update
        /// </summary>
        public void SavePositionForPreparation(int preparationID)
        {
            foreach (TooltipElement tt in tooltip_Elements)
            {
                if (int.Parse(tt.preparation_id) == preparationID)
                {
                    tt.position = TooltipScript.transform.localPosition;
                }
            }

            List<TooltipElement> lst = new List<TooltipElement>(tooltip_Elements);

            TooltipElement temp = new TooltipElement();
            temp.preparation_id = preparationID.ToString();
            temp.position = TooltipScript.transform.localPosition;

            lst.Add(temp);

            tooltip_Elements = lst.ToArray();
        }

        /// <summary>
        /// Try to understand if term has tooltip for specific preparation
        /// </summary>
        /// <param name="preparationID">Preparation ID you are searching for</param>
        public bool HasTooltipForPreparation(int preparationID)
        {
            if (tooltip_Elements != null && tooltip_Elements.Length > 0)
            {
                foreach (TooltipElement tt in tooltip_Elements)
                {
                    if (int.Parse(tt.preparation_id) == preparationID)
                        return true;
                }

                return false;
            }
            else
                return false;
        }

        /// <summary>
        /// <para>Create C# object from raw JSON string.</para>
        /// <para>Parses string automatically!</para>
        /// </summary>
        public Terms(string jsonString) : this(JsonUtility.FromJson<TermsJson>(jsonString)) { }

        [SerializeField]
        private Tooltip3D.Tooltip3D ttScript;
        public Tooltip3D.Tooltip3D TooltipScript { get { return ttScript; } set { ttScript = value; } }
    }
}
