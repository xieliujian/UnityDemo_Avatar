using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class GenerateResEditor : Editor
{
    private const string ANIM_PATH = "Assets/Anims/";
    private const string MAT_PATH = "Assets/Materials/";
    private const string Prefab_PATH = "Assets/Resources/";
    private const string FBX_SUFFIX = ".fbx";
    private const string ANIM_SUFFIX = ".anim";
    private const string PREAFAB_SUFFIX = ".prefab";
    private const string INVALID_ANIM_NAME = "__preview__";

    [MenuItem("Avatar/Generate")]
    private static void Generate()
    {
        List<GameObject> objs = CollectObjs();

        if (objs.Count <= 0)
        {
            EditorUtility.DisplayDialog("Generator", "请选择UnuseRes文件夹", "Ok");
            return;
        }

        if (Directory.Exists(ANIM_PATH))
            Directory.Delete(ANIM_PATH, true);

        if (Directory.Exists(Prefab_PATH))
            Directory.Delete(Prefab_PATH, true);

        GenerateAllAnim(objs);
        GenerateAllPrefab(objs);
        GenerateAllSkeleton(objs);
    }

    private static List<GameObject> CollectObjs()
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (UnityEngine.Object o in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            GameObject obj = o as GameObject;
            if (!obj)
                continue;

            if (obj.name.Contains("@"))
                continue;

            objs.Add(obj);
        }

        return objs;
    }

    private static void GenerateAllSkeleton(List<GameObject> objs)
    {
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GenerateSkeleton(obj, dir, middir);
        }
    }

    private static void GenerateSkeleton(GameObject srcobj, string dir, string middir)
    {
        string prefabpath = Prefab_PATH + "/" + middir + "/";
        string animpath = ANIM_PATH + "/" + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        GameObject obj = GameObject.Instantiate(srcobj);
        obj.name = middir + "_skeleton";
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        foreach (SkinnedMeshRenderer smr in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            GameObject.DestroyImmediate(smr.gameObject);

        Animation anim = obj.GetComponent<Animation>();
        GameObject.DestroyImmediate(anim);

        anim = obj.AddComponent<Animation>();

        List<AnimationClip> clips = FunctionUtil.CollectAll<AnimationClip>(animpath);
        foreach(var clip in clips)
            anim.AddClip(clip, clip.name);

        string dstpath = prefabpath + obj.name.ToLower() + PREAFAB_SUFFIX;
        PrefabUtility.CreatePrefab(dstpath, obj);

        GameObject.DestroyImmediate(obj);
    }

    private static void GenerateAllPrefab(List<GameObject> objs)
    {
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GeneratePrefab(obj, dir, middir);
        }
    }

    private static void GeneratePrefab(GameObject srcobj, string dir, string middir)
    {
        string prefabpath = Prefab_PATH + "/" + middir + "/";
        string matpath = MAT_PATH + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        dirinfo = new DirectoryInfo(prefabpath);
        if (!dirinfo.Exists)
        {
            Directory.CreateDirectory(prefabpath);
        }

        List<Material> materials = FunctionUtil.CollectAll<Material>(matpath);
        foreach (SkinnedMeshRenderer smr in srcobj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(smr.gameObject);
            SkinnedMeshRenderer renderer = obj.GetComponent<SkinnedMeshRenderer>();
            GameObject rendererParent = obj.transform.parent.gameObject;
            foreach (SkinnedMeshRenderer tempsmr in rendererParent.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (tempsmr != renderer)
                    GameObject.DestroyImmediate(tempsmr.gameObject);
            }

            Animation anim = rendererParent.GetComponent<Animation>();
            GameObject.DestroyImmediate(anim);

            foreach (var material in materials)
            {
                if (material == null)
                    continue;

                if (!material.name.Contains(obj.name))
                    continue;

                GameObject newobj = GameObject.Instantiate(rendererParent);
                newobj.name = material.name;
                newobj.transform.position = Vector3.zero;
                newobj.transform.rotation = Quaternion.identity;
                newobj.transform.localScale = Vector3.one;

                SkinnedMeshRenderer newrenderer = newobj.GetComponentInChildren<SkinnedMeshRenderer>();
                newrenderer.material = material;

                string dstpath = prefabpath + newobj.name + PREAFAB_SUFFIX;
                PrefabUtility.CreatePrefab(dstpath, newobj);

                GameObject.DestroyImmediate(newobj);
            }

            GameObject.DestroyImmediate(rendererParent);
        }
    }

    private static void GenerateAllAnim(List<GameObject> objs)
    {
        foreach(var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GenerateAnim(dir, middir, filename);
        }
    }

    /// <summary>
    /// 生成一个动画资源
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    private static void GenerateAnim(string dir, string middir, string fbxname)
    {
        SplitAnimClip(dir, middir, fbxname);
    }

    private static void SplitAnimClip(string dir, string middir, string fbxname)
    {
        string clippath = ANIM_PATH + "/" + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        dirinfo = new DirectoryInfo(clippath);
        if (!dirinfo.Exists)
        {
            Directory.CreateDirectory(clippath);
        }

        var files = Directory.GetFiles(dir, "*.fbx");
        foreach (var file in files)
        {
            if (!file.Contains("@"))
                continue;

            UnityEngine.Object[] clipobjs = AssetDatabase.LoadAllAssetsAtPath(file);
            if (clipobjs.Length <= 0)
                continue;

            foreach (var clipobj in clipobjs)
            {
                AnimationClip srcclip = clipobj as AnimationClip;
                if (srcclip == null)
                    continue;

                if (srcclip.name.Contains(INVALID_ANIM_NAME))
                    continue;

                string dstclippath = clippath + srcclip.name + ANIM_SUFFIX;
                AnimationClip dstclip = AssetDatabase.LoadAssetAtPath(dstclippath, typeof(AnimationClip)) as AnimationClip;
                if (dstclip != null)
                    AssetDatabase.DeleteAsset(dstclippath);

                AnimationClip tempclip = new AnimationClip();
                EditorUtility.CopySerialized(srcclip, tempclip);
                AssetDatabase.CreateAsset(tempclip, dstclippath);
            }
        }
    }
}
