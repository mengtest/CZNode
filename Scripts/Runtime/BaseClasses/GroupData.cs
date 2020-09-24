using System;
using System.Collections.Generic;
using CZFramework.CZNode;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
[Serializable]
public class GroupData
{
    public string groupName = "Group";
    public Color headerColor;
    public List<NodeData> nodes = new List<NodeData>();

    public void Init()
    {
        headerColor = Color.HSVToRGB(Random.Range(0f, 1f), S: Random.Range(0.5f, 0.8f), V: Random.Range(0.5f, 0.8f));
        headerColor.a = 0.5f;
    }
}
#endif