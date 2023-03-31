using System;
using System.Collections.Generic;

using AnatomyNext.WebGLApp.Www.API_Data;
using AnatomyNext.WebGLApp.Models;

namespace AnatomyNext.WebGLApp.Www.API_Structs
{

    [Serializable]
    public class CranialNerves
    {
        public List<CranialNerve> cranialNerves;

        #region void AddNerve
        /// <summary>
        /// Add nerve as root Cranial nerve or as subnerve (parses tree automatically)
        /// </summary>
        /// <param name="nerve">ListItem to be parsed as CN</param>
        public void AddNerve(ListItem nerve)
        {
            AddNerve(new CranialNerve(nerve));
        }

        /// <summary>
        /// Add nerve as root Cranial nerve or as subnerve (parses tree automatically)
        /// </summary>
        /// <param name="nerve">Cranial nerve</param>
        public void AddNerve(CranialNerve nerve)
        {
            if (cranialNerves == null)
                cranialNerves = new List<CranialNerve>();

            // if is directly under root, add as one of root CN's
            if (AssetManager.RootItems.ContainsKey(nerve.properties.ParentID))
            {
                // double-check for duplicates
                foreach (CranialNerve temp in cranialNerves)
                {
                    if (nerve.properties.ID == temp.properties.ID)
                        return;
                }

                cranialNerves.Add(nerve);
                return;
            }
            else
            {
                // try to add this as subnerve
                foreach (CranialNerve temp in cranialNerves)
                {
                    // search nerve to parent under
                    CranialNerve sn = TryGetNerveByID(nerve.properties.ParentID);
                    if (sn != null)
                    {
                        sn.AddNerve(nerve);
                        return;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Try to get nerve (including subnerve) in whole CN tree. Returns null if CN with that ID is not found
        /// </summary>
        /// <param name="nerveID">Nerve ID (from API) to search for</param>
        /// <returns>Returns CN or null if not found</returns>
        public CranialNerve TryGetNerveByID(int nerveID)
        {
            // search in all root nerves. Will return root itself, if needed
            foreach (CranialNerve cn in cranialNerves)
            {
                CranialNerve temp = cn.TryGetNerveByID(nerveID);
                if (temp != null)
                    return temp;
            }

            // this is needed if CN list is empty
            return null;
        }

        /// <summary>
        /// Try to get preparation in whole CN tree. Returns null if preparation with that ID is not found
        /// </summary>
        /// <param name="preparationID">Preparation ID (from API) to search for</param>
        /// <returns>Returns preparation or null if not found</returns>
        public Preparation TryGetPreparationByID(int preparationID)
        {
            // search in all root nerves. Will return root itself, if needed
            foreach (CranialNerve cn in cranialNerves)
            {
                Preparation temp = cn.TryGetPreparationByID(preparationID);
                if (temp != null)
                    return temp;
            }

            // this is needed if CN list is empty
            return null;
        }
    }

    [Serializable]
    public class CranialNerve : API_Struct
    {
        public string title;
        public ListItem properties;
        public bool paid;
        public List<CranialNerve> subnerves;
        public List<Preparation> preparations;

        public CranialNerve(ListItem properties)
        {
            this.properties = properties;
            this.title = properties.Title;
            subnerves = new List<CranialNerve>();
            preparations = new List<Preparation>();
        }

        /// <summary>
        /// Add new CN as subnerve
        /// </summary>
        /// <param name="nerve">Cranial nerve</param>
        public void AddNerve(CranialNerve nerve)
        {
            // double-check for duplicates
            foreach (CranialNerve temp in subnerves)
            {
                if (temp.properties.ID == nerve.properties.ID)
                    return;
            }

            subnerves.Add(nerve);
        }

        #region void AddPreparation
        /// <summary>
        /// Add preparation to CN
        /// </summary>
        /// <param name="nerve">ListItem to be parsed as preparation</param>
        public void AddPreparation(ListItem preparation)
        {
            AddPreparation(new Preparation(preparation));
        }

        /// <summary>
        /// Add preparation to CN
        /// </summary>
        /// <param name="nerve">Preparation</param>
        public void AddPreparation(Preparation preparation)
        {
            // double-check for duplicates
            foreach (Preparation temp in preparations)
            {
                if (temp.properties.ID == preparation.properties.ID)
                    return;
            }

            preparations.Add(preparation);
        }
        #endregion

        /// <summary>
        /// Try to get nerve (including subnerve) in whole CN tree. Returns null if CN with that ID is not found
        /// </summary>
        /// <param name="nerveID">Nerve ID (from API) to search for</param>
        /// <returns>Returns CN or null if not found</returns>
        public CranialNerve TryGetNerveByID(int nerveID)
        {
            if (properties.ID == nerveID)
                return this;

            // search in all root nerves. Will return root itself, if needed
            foreach (CranialNerve cn in subnerves)
            {
                if (cn.properties.ID == nerveID)
                    return cn;

                if (cn.subnerves.Count > 0)
                {
                    CranialNerve temp = cn.TryGetNerveByID(nerveID);
                    if (temp != null)
                        return temp;
                }
            }

            // this is needed if CN list is empty
            return null;
        }

        /// <summary>
        /// Try to get preparation in whole CN tree. Returns null if preparation with that ID is not found
        /// </summary>
        /// <param name="preparationID">Preparation ID (from API) to search for</param>
        /// <returns>Returns preparation or null if not found</returns>
        public Preparation TryGetPreparationByID(int preparationID)
        {
            foreach(Preparation prep in preparations)
            {
                if (prep.properties.ID == preparationID)
                    return prep;
            }

            foreach(CranialNerve sn in subnerves)
            {
                Preparation temp = sn.TryGetPreparationByID(preparationID);
                if (temp != null)
                    return temp;
            }

            // this is needed if CN list is empty
            return null;
        }

    }
}
