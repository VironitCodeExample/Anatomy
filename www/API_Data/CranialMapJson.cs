using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CranialMapJson
{
    public string nerve_id;
    public string nerve_title;
    public string map_url;
    public string map_level_count;
    public string youtube_url;
    public string paid;

    public static CranialMapJson CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<CranialMapJson>(jsonString);
    }
}
