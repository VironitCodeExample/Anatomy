using UnityEngine;

using AnatomyNext.Camera;
using AnatomyNext.WebGLApp.Models;
using System;

namespace AnatomyNext.WebGLApp.Www.EditorFeatures
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/Editor Features/Save Center Coord")]
    [DisallowMultipleComponent]
    public class SaveCenterCoord : EditingCore
    {
        public void SaveCoord()
        {
            if (ModelManager.CurrentListItem != null)
            {
                try
                {
                    #region set data for API
                    // set id of preparation to link camera with
                    WwwManager.postData idField = new WwwManager.postData();
                    idField.fieldName = "id";
                    idField.data = ModelManager.CurrentListItem.ID.ToString();

                    // save view as string
                    WwwManager.postData center_coordField = new WwwManager.postData();
                    center_coordField.fieldName = "center_coord";
                    center_coordField.data = JsonUtility.ToJson(RotationPoint.RotationPivot.position);
                    #endregion

                    // start sending data to server
                    StartCoroutine(
                        SendToAPI(
                            () => onSuccess(ModelManager.CurrentListItem.ID),
                            () => onFail(ModelManager.CurrentListItem.ID), false,
                            idField, center_coordField)
                            );
                }
                catch (Exception ex)
                {
                    CustomDebug.Log(ex.ToString());
                    if (LogManager.Instance)
                        LogManager.LogOnServer(ex.ToString());
                }
            }
        }

        private void onFail(int listId)
        {
            string str = "Error updating center coord for ListItem: " + listId;
            CustomDebug.Log(str);

            if (LogManager.Instance)
                LogManager.LogOnServer(str);
        }

        private void onSuccess(int listId)
        {
            CustomDebug.Log("Center coord for ", listId, " saved!");
        }
    }
}
