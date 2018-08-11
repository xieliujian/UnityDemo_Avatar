using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

public static class FunctionUtil
{
    public static void Reset(this GameObject go, GameObject parent)
    {
        go.transform.parent = parent.transform;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }

    public static List<T> CollectAll<T>(string path) where T : UnityEngine.Object
    {
        List<T> l = new List<T>();
        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            if (file.Contains(".meta")) continue;
            T asset = (T)AssetDatabase.LoadAssetAtPath(file, typeof(T));
            if (asset == null) throw new Exception("Asset is not " + typeof(T) + ": " + file);
            l.Add(asset);
        }
        return l;
    }
}
