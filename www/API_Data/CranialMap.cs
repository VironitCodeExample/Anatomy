using AnatomyNext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AnatomyNext.WebGLApp.Models;

public class CranialMap
{
    public int ID;
    public string Title;
    public string URL;
    public int mapCount;
    public bool paid;
    public string VideoId;

    public CranialMap(CranialMapJson jsonItem)
    {
        ID = int.Parse(jsonItem.nerve_id);
        Title = jsonItem.nerve_title;
        URL = jsonItem.map_url;
        mapCount = int.Parse(jsonItem.map_level_count);
        paid = Conversions.StringToBool(jsonItem.paid);
        VideoId = jsonItem.youtube_url;
    }

    public CranialMap(string jsonString) : this(JsonUtility.FromJson<CranialMapJson>(jsonString)) { }
}
