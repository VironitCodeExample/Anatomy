using System;
using System.Collections.Generic;

using AnatomyNext.WebGLApp.Www.API_Data;
using AnatomyNext.WebGLApp.Models;

using UnityEngine;
using System.Text.RegularExpressions;

namespace AnatomyNext.WebGLApp.Www.API_Structs
{
    [Serializable]
    public class HeadNeck : API_Struct
    {
        public List<HeadNeckGroup> groups;
        public List<HeadNeckItem> items;

        #region void AddGroup
        /// <summary>
        /// Add group as root HN group or as subgroup (parses tree automatically)
        /// </summary>
        /// <param name="group">ListItem to be parsed as HN group</param>
        public void AddGroup(ListItem group)
        {
            AddGroup(new HeadNeckGroup(group));

            //foreach (int mat in group.Materials)
            //    AddFile(mat, FileType.Material);
        }

        /// <summary>
        /// Add group as root HN group or as subgroup (parses tree automatically)
        /// </summary>
        /// <param name="group">HN group</param>
        public void AddGroup(HeadNeckGroup group)
        {
            if (groups == null)
                groups = new List<HeadNeckGroup>();

            foreach (int mat in group.properties.Materials)
                AddFile(mat, FileType.Material);

            // if is directly under root, add as one of root group's
            if (AssetManager.RootItems.ContainsKey(group.properties.ParentID))
            {
                // double-check for duplicates
                foreach (HeadNeckGroup temp in groups)
                {
                    if (group.properties.ID == temp.properties.ID)
                        return;
                }

                groups.Add(group);
                return;
            }
            else
            {
                // try to add this as subgroup
                foreach (HeadNeckGroup temp in groups)
                {
                    // search group to parent under
                    HeadNeckGroup sg = TryGetGroupByID(group.properties.ParentID);
                    if (sg != null)
                    {
                        sg.AddGroup(group);
                        return;
                    }
                }
            }
        }
        #endregion

        #region void AddItem
        /// <summary>
        /// Add HN item in subgroup or root, if subroup not found(parses tree automatically)
        /// </summary>
        /// <param name="group">ListItem to be parsed as HN item</param>
        public void AddItem(ListItem hnItem)
        {
            AddItem(new HeadNeckItem(hnItem));
        }

        /// <summary>
        /// Add HN item in subgroup or root, if subroup not found(parses tree automatically)
        /// </summary>
        /// <param name="group">HN item</param>
        public void AddItem(HeadNeckItem hnItem)
        {
            if (items == null)
                items = new List<HeadNeckItem>();

            // if is directly under root, add as one of root items
            if (AssetManager.RootItems.ContainsKey(hnItem.properties.ParentID))
            {
                // double-check for duplicates
                foreach (HeadNeckItem temp in items)
                {
                    if (hnItem.properties.ID == temp.properties.ID)
                        return;
                }

                items.Add(hnItem);
                AddFile(hnItem.properties.AssetID, FileType.Asset);

                foreach (int mat in hnItem.properties.Materials)
                    AddFile(mat, FileType.Material);

                return;
            }
            else
            {
                // try to add this as item @ subgroup
                foreach (HeadNeckGroup temp in groups)
                {
                    // search group to parent under
                    HeadNeckGroup sg = TryGetGroupByID(hnItem.properties.ParentID);
                    if (sg != null)
                    {
                        sg.AddItem(hnItem);
                        return;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Try to get HN group (including subgroups) in whole HN tree. Returns null if HN group with that ID is not found
        /// </summary>
        /// <param name="groupID">Group ID (from API) to search for</param>
        /// <returns>Returns HN group or null if not found</returns>
        public HeadNeckGroup TryGetGroupByID(int groupID)
        {
            // search in all root groups. Will return root itself, if needed
            foreach (HeadNeckGroup gr in groups)
            {
                HeadNeckGroup temp = gr.TryGetGroupByID(groupID);
                if (temp != null)
                    return temp;
            }

            // this is needed if groups list is empty
            return null;
        }

        /// <summary>
        /// Try to get HN item in whole HN tree. Returns null if item with that ID is not found
        /// </summary>
        /// <param name="itemID">Item ID (from API) to search for</param>
        /// <returns>Returns item or null if not found</returns>
        public HeadNeckItem TryGetHeadNeckItemByID(int itemID)
        {
            foreach (HeadNeckItem item in items)
            {
                if (item.properties.ID == itemID)
                    return item;
            }

            // search in all groups. Will return root itself, if needed
            foreach (HeadNeckGroup grp in groups)
            {
                HeadNeckItem temp = grp.TryGetItemByID(itemID);
                if (temp != null)
                    return temp;
            }

            // this is needed if HN lists are empty
            return null;
        }

        /// <summary>
        /// Try to get Linked UI gameobject for specified item
        /// </summary>
        /// <param name="goForThisItem">Get UI gameobject related with this ListItem</param>
        public GameObject TryGetLinkedUIGameobject(ListItem goForThisItem)
        {
            foreach(HeadNeckItem hnItem in items)
            {
                if(hnItem.properties == goForThisItem)
                {
                    return hnItem.LinkedUIObject;
                }
            }

            foreach(HeadNeckGroup hnGroup in groups)
            {
                GameObject result = hnGroup.TryGetLinkedUIGameobject(goForThisItem);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Check if parent group is visible
        /// </summary>
        public bool IsParentVisible(ListItem currentItem)
        {
            return IsParentVisible(currentItem.ParentID);
        }

        /// <summary>
        /// Check if parent group is visible
        /// </summary>
        public bool IsParentVisible(int parentId)
        {
            if ((IntegrationManager.ModelView == IntegrationManager.Model.HeadNeck) ||
                (IntegrationManager.ModelView == IntegrationManager.Model.Bones))
            {
                HeadNeckGroup parent = TryGetGroupByID(parentId);
                if (parent == null)
                    return true;
                else
                    return parent.properties.Visible;
            }

            return true;
        }
    }

    [Serializable]
    public class HeadNeckGroup : API_Struct
    {
        public string title;
        public ListItem properties;
        public List<HeadNeckGroup> subgroups;
        public List<HeadNeckItem> items;

        public HeadNeckGroup(ListItem properties)
        {
            this.properties = properties;
            this.title = properties.Title;

            subgroups = new List<HeadNeckGroup>();
            items = new List<HeadNeckItem>();
        }

        #region void AddGroup
        /// <summary>
        /// Add new HN group as subgroup
        /// </summary>
        /// <param name="nerve">HN group</param>
        public void AddGroup(HeadNeckGroup group)
        {
            // double-check for duplicates
            foreach (HeadNeckGroup temp in subgroups)
            {
                if (temp.properties.ID == group.properties.ID)
                    return;
            }

            subgroups.Add(group);
        }

        /// <summary>
        /// Add HN item in subgroup or root, if subroup not found(parses tree automatically)
        /// </summary>
        /// <param name="group">HN item</param>
        public void AddItem(HeadNeckItem hnItem)
        {
            if (items == null)
                items = new List<HeadNeckItem>();

            // if is directly under root, add as one of root items
            if (properties.ID == hnItem.properties.ParentID)
            {
                // double-check for duplicates
                foreach (HeadNeckItem temp in items)
                {
                    if (hnItem.properties.ID == temp.properties.ID)
                        return;
                }

                items.Add(hnItem);
                AssetManager.HeadNeck.AddFile(hnItem.properties.AssetID, FileType.Asset);
                AddFile(hnItem.properties.AssetID, FileType.Asset);

                foreach (int mat in hnItem.properties.Materials)
                {
                    AssetManager.HeadNeck.AddFile(mat, FileType.Material);
                    AddFile(mat, FileType.Material);
                }

                return;
            }
            else
            {
                // try to add this as item @ subgroup
                foreach (HeadNeckGroup temp in subgroups)
                {
                    // search group to parent under
                    HeadNeckGroup sg = TryGetGroupByID(hnItem.properties.ParentID);
                    if (sg != null)
                    {
                        sg.AddItem(hnItem);
                        return;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Try to get HN group (including subgroups) in whole HN tree. Returns null if HN group with that ID is not found
        /// </summary>
        /// <param name="groupID">Group ID (from API) to search for</param>
        /// <returns>Returns HN group or null if not found</returns>
        public HeadNeckGroup TryGetGroupByID(int groupID)
        {
            if (properties.ID == groupID)
                return this;

            // search in all root nerves. Will return root itself, if needed
            foreach (HeadNeckGroup gr in subgroups)
            {
                if (gr.properties.ID == groupID)
                    return gr;

                if (gr.subgroups.Count > 0)
                {
                    HeadNeckGroup temp = gr.TryGetGroupByID(groupID);
                    if (temp != null)
                        return temp;
                }
            }

            // this is needed if CN list is empty
            return null;
        }

        /// <summary>
        /// Try to get HN item in whole HN tree. Returns null if item with that ID is not found
        /// </summary>
        /// <param name="itemID">Item ID (from API) to search for</param>
        /// <returns>Returns item or null if not found</returns>
        public HeadNeckItem TryGetItemByID(int itemID)
        {
            foreach (HeadNeckItem item in items)
            {
                if (item.properties.ID == itemID)
                    return item;
            }

            foreach (HeadNeckGroup grp in subgroups)
            {
                HeadNeckItem temp = grp.TryGetItemByID(itemID);
                if (temp != null)
                    return temp;
            }

            // this is needed if HN list are empty
            return null;
        }

        /// <summary>
        /// Try to get Linked UI gameobject for specified item
        /// </summary>
        /// <param name="goForThisItem">Get UI gameobject related with this ListItem</param>
        public GameObject TryGetLinkedUIGameobject(ListItem goForThisItem)
        {
            foreach (HeadNeckItem hnItem in items)
            {
                if (hnItem.properties == goForThisItem)
                {
                    return hnItem.LinkedUIObject;
                }
            }

            foreach (HeadNeckGroup hnGroup in subgroups)
            {
                GameObject result = hnGroup.TryGetLinkedUIGameobject(goForThisItem);
                if (result != null)
                    return result;
            }

            return null;
        }
    }

    [Serializable]
    public enum BoneSide
    {
        Center, Left, Right
    }
    public class HeadNeckItem : API_Struct
    {
        public string title;
        public ListItem properties;

        public string boneName;
        public BoneSide boneSide;

        public HeadNeckItem(ListItem properties)
        {
            this.properties = properties;
            this.title = properties.Title;

            this.boneName = Regex.Replace(title, @"\s* [l-rL-R]", "");
            var lastletter = title.Substring(title.Length - 1, 1);
            boneSide = lastletter == "L" ? BoneSide.Left : lastletter == "R" ? BoneSide.Right : BoneSide.Center;
        }
    }
}
