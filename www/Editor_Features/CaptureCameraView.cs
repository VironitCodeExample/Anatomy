using UnityEngine;

using AnatomyNext.Camera;
using AnatomyNext.WebGLApp.Www.API_Structs;
using AnatomyNext.WebGLApp.Models;
using System;

namespace AnatomyNext.WebGLApp.Www.EditorFeatures
{
    [AddComponentMenu("Anatomy Next/WebGL App/WWW/Editor Features/Capture Camera View")]
    [DisallowMultipleComponent]
    public class CaptureCameraView : EditingCore
    {
        public void CaptureView()
        {
            if (ModelManager.CurrentListItem != null)
            {
                try
                {
                    CameraController.CameraView view = CameraController.GetCurrentView();
                    string viewString = view.ToString();
                    CustomDebug.Log(viewString);

                    #region set data for API
                    // set id of preparation to link camera with
                    WwwManager.postData idField = new WwwManager.postData();
                    idField.fieldName = "id";
                    idField.data = ModelManager.CurrentListItem.ID.ToString();

                    // save view as string
                    WwwManager.postData viewField = new WwwManager.postData();
                    viewField.fieldName = "view";
                    viewField.data = viewString;
                    #endregion

                    // start sending data to server
                    StartCoroutine(
                        SendToAPI(
                            () => onSuccess(ModelManager.CurrentListItem.ID, view),
                            () => onFail(ModelManager.CurrentListItem.ID), false,
                            idField, viewField)
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

        private void onFail(int listID)
        {
            string str = "Error updating listItems's view: " + listID;
            CustomDebug.Log(str);

            if (LogManager.Instance)
                LogManager.LogOnServer(str);
        }

        private void onSuccess(int listID, CameraController.CameraView view)
        {
            if (IntegrationManager.ModelView == IntegrationManager.Model.Nerves)
            {
                Preparation prep = AssetManager.CranialNerves.TryGetPreparationByID(listID);
                if (prep != null)
                    prep.properties.CameraView = view;
            }

            CustomDebug.Log("View for ", listID, " saved!");
        }

    }
}
