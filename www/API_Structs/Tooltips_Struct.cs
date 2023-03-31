using System;
using UnityEngine;

using AnatomyNext.WebGLApp.Www.API_Data;
using System.Collections.Generic;
using AnatomyNext.WebGLApp.Models;

namespace AnatomyNext.WebGLApp.Www.API_Structs
{
    [Serializable]
    public class Tooltips_Struct// : API_Struct
    {
        public List<Terms> termsList;
        private Transform tooltipRoot;
        public Transform TooltipRoot
        {
            get
            {
                if(tooltipRoot == null)
                {
                    ModelManager.CurrentTermID = -1;
                    tooltipRoot = new GameObject("TooltipRoot").transform;
                    tooltipRoot.SetParent(AssetManager.AssetsRoot);
                    tooltipRoot.localPosition = new Vector3(0, 0, 0);
                    tooltipRoot.localRotation = Quaternion.Euler(0, 0, 0);
                }

                return tooltipRoot;
            }
        }

        /// <summary>
        /// Add new Term avoiding duplicates
        /// </summary>
        public void AddTerm(Terms term)
        {
            if (termsList == null)
                termsList = new List<Terms>();

            // double-check for duplicates
            foreach (Terms temp in termsList)
            {
                if (temp.ID == term.ID)
                    return;
            }

            termsList.Add(term);
        }

        /// <summary>
        /// Try to get Term by ID. Returns null if term that ID is not found
        /// </summary>
        /// <param name="termID">Term ID (from TermsAPI) to search for</param>
        /// <returns>Returns Term or null if not found</returns>
        public Terms TryGetTermByID(int termID)
        {
            // search in all root groups. Will return root itself, if needed
            foreach (Terms tr in termsList)
            {
                if (tr.ID == termID)
                    return tr;
            }

            // this is needed if groups list is empty
            return null;
        }

        /// <summary>
        /// Returns list of terms containing tooltips for specific preparation
        /// </summary>
        /// <param name="preparationID">Preparation ID you are searching for</param>
        /// <param name="oddTerms">Outputs second list with all other terms where tooltip is not found</param>
        public List<Terms> TryGetListOfTooltipsForPreparation(int preparationID, out List<Terms> oddTerms)
        {
            List<Terms> resultList = new List<Terms>();
            oddTerms = new List<Terms>();

            foreach(Terms tr in termsList)
            {
                if (tr.HasTooltipForPreparation(preparationID))
                    resultList.Add(tr);
                else
                    oddTerms.Add(tr);
            }

            return resultList;
        }

    }
}
