using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FunctionUtil
{
    public static void Reset(this GameObject go, GameObject parent)
    {
        go.transform.parent = parent.transform;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }
}

public class Character : MonoBehaviour
{
    #region 变量

    private GameObject mSkeleton;
    private GameObject mEyes;
    private GameObject mFace;
    private GameObject mHair;
    private GameObject mPants;
    private GameObject mShoes;
    private GameObject mTop;
    private Animation mAnim;

    /// <summary>
    /// 是否组合
    /// </summary>
    private bool mCombine = false;

    #endregion

    #region 内置函数

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #endregion

    #region 函数

    public void SetName(string name)
    {
        gameObject.name = name;
    }

    public void Generate(AvatarRes avatarres, bool combine = false)
    {
        if (!combine)
            GenerateUnCombine(avatarres);
        else
            GenerateCombine(avatarres);
    }

    private void DestroyAll()
    {
        if (mSkeleton != null)
            GameObject.DestroyImmediate(mSkeleton);

        mEyes = null;
        mFace = null;
        mHair = null;
        mPants = null;
        mShoes = null;
        mTop = null;
    }

    private void GenerateUnCombine(AvatarRes avatarres)
    {
        DestroyAll();

        mSkeleton = GameObject.Instantiate(avatarres.mSkeleton);
        mSkeleton.Reset(gameObject);
        mSkeleton.name = avatarres.mSkeleton.name;

        mAnim = mSkeleton.GetComponent<Animation>();

        ChangeEquipUnCombine((int)EPart.EP_Eyes, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Face, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Hair, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Pants, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Shoes, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Top, avatarres);

        if (mAnim != null)
        {
            mAnim.wrapMode = WrapMode.Loop;
            mAnim.Play("walk");
        }
    }

    private void GenerateCombine(AvatarRes avatarres)
    {
        if (mSkeleton != null)
        {
            bool iscontain = mSkeleton.name.Equals(avatarres.mSkeleton.name);
            if (!iscontain)
            {
                GameObject.DestroyImmediate(mSkeleton);
            }
        }

        if (mSkeleton == null)
        {
            mSkeleton = GameObject.Instantiate(avatarres.mSkeleton);
            mSkeleton.Reset(gameObject);
            mSkeleton.name = avatarres.mSkeleton.name;
        }

        mAnim = mSkeleton.GetComponent<Animation>();

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<Transform> bones = new List<Transform>();
        ChangeEquipCombine((int)EPart.EP_Eyes, avatarres, ref combineInstances, ref materials, ref bones);
        ChangeEquipCombine((int)EPart.EP_Face, avatarres, ref combineInstances, ref materials, ref bones);
        ChangeEquipCombine((int)EPart.EP_Hair, avatarres, ref combineInstances, ref materials, ref bones);
        ChangeEquipCombine((int)EPart.EP_Pants, avatarres, ref combineInstances, ref materials, ref bones);
        ChangeEquipCombine((int)EPart.EP_Shoes, avatarres, ref combineInstances, ref materials, ref bones);
        ChangeEquipCombine((int)EPart.EP_Top, avatarres, ref combineInstances, ref materials, ref bones);

        // Obtain and configure the SkinnedMeshRenderer attached to
        // the character base.
        SkinnedMeshRenderer r = mSkeleton.GetComponent<SkinnedMeshRenderer>();
        if (r != null)
        {
            GameObject.DestroyImmediate(r);
        }

        r = mSkeleton.AddComponent<SkinnedMeshRenderer>();
        r.sharedMesh = new Mesh();
        r.sharedMesh.CombineMeshes(combineInstances.ToArray(), false, false);
        r.bones = bones.ToArray();
        r.materials = materials.ToArray();

        if (mAnim != null)
        {
            if (!mAnim.IsPlaying("walk"))
            {
                mAnim.wrapMode = WrapMode.Loop;
                mAnim.Play("walk");
            }
        }
    }

    private void ChangeEquipCombine(int type, AvatarRes avatarres, ref List<CombineInstance> combineInstances, 
                        ref List<Material> materials, ref List<Transform> bones)
    {
        if (type == (int)EPart.EP_Eyes)
        {
            ChangeEquipCombine(avatarres.mEyesList[avatarres.mEyesIdx], ref combineInstances, ref materials, ref bones);
        }
        else if (type == (int)EPart.EP_Face)
        {
            ChangeEquipCombine(avatarres.mFaceList[avatarres.mFaceIdx], ref combineInstances, ref materials, ref bones);
        }
        else if (type == (int)EPart.EP_Hair)
        {
            ChangeEquipCombine(avatarres.mHairList[avatarres.mHairIdx], ref combineInstances, ref materials, ref bones);
        }
        else if (type == (int)EPart.EP_Pants)
        {
            ChangeEquipCombine(avatarres.mPantsList[avatarres.mPantsIdx], ref combineInstances, ref materials, ref bones);
        }
        else if (type == (int)EPart.EP_Shoes)
        {
            ChangeEquipCombine(avatarres.mShoesList[avatarres.mShoesIdx], ref combineInstances, ref materials, ref bones);
        }
        else if (type == (int)EPart.EP_Top)
        {
            ChangeEquipCombine(avatarres.mTopList[avatarres.mTopIdx], ref combineInstances, ref materials, ref bones);
        }
    }

    private void ChangeEquipCombine(GameObject resgo, ref List<CombineInstance> combineInstances,
                        ref List<Material> materials, ref List<Transform> bones)
    {
        Transform[] skettrans = mSkeleton.GetComponentsInChildren<Transform>();

        GameObject go = GameObject.Instantiate(resgo);
        SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

        materials.AddRange(smr.materials);
        for (int sub = 0; sub < smr.sharedMesh.subMeshCount; sub++)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            ci.subMeshIndex = sub;
            combineInstances.Add(ci);
        }

        // As the SkinnedMeshRenders are stored in assetbundles that do not
        // contain their bones (those are stored in the characterbase assetbundles)
        // we need to collect references to the bones we are using
        foreach (Transform bone in smr.bones)
        {
            string bonename = bone.name;
            foreach (Transform transform in skettrans)
            {
                if (transform.name != bonename)
                    continue;

                bones.Add(transform);
                break;
            }
        }

        GameObject.DestroyImmediate(go);
    }

    public void ChangeEquipUnCombine(int type, AvatarRes avatarres)
    {
        if (type == (int)EPart.EP_Eyes)
        {
            ChangeEquipUnCombine(ref mEyes, avatarres.mEyesList[avatarres.mEyesIdx]);
        }
        else if (type == (int)EPart.EP_Face)
        {
            ChangeEquipUnCombine(ref mFace, avatarres.mFaceList[avatarres.mFaceIdx]);
        }
        else if (type == (int)EPart.EP_Hair)
        {
            ChangeEquipUnCombine(ref mHair, avatarres.mHairList[avatarres.mHairIdx]);
        }
        else if (type == (int)EPart.EP_Pants)
        {
            ChangeEquipUnCombine(ref mPants, avatarres.mPantsList[avatarres.mPantsIdx]);
        }
        else if (type == (int)EPart.EP_Shoes)
        {
            ChangeEquipUnCombine(ref mShoes, avatarres.mShoesList[avatarres.mShoesIdx]);
        }
        else if (type == (int)EPart.EP_Top)
        {
            ChangeEquipUnCombine(ref mTop, avatarres.mTopList[avatarres.mTopIdx]);
        }
    }

    private void ChangeEquipUnCombine(ref GameObject go, GameObject resgo)
    {
        if (go != null)
        {
            GameObject.DestroyImmediate(go);
        }

        go = GameObject.Instantiate(resgo);
        go.Reset(mSkeleton);
        go.name = resgo.name;

        SkinnedMeshRenderer render = go.GetComponentInChildren<SkinnedMeshRenderer>();
        ShareSkeletonInstanceWith(render, mSkeleton);
    }

    // 共享骨骼
    public void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
    {
        Transform[] newBones = new Transform[selfSkin.bones.Length];
        for (int i = 0; i < selfSkin.bones.GetLength(0); ++i)
        {
            GameObject bone = selfSkin.bones[i].gameObject;
            
            // 目标的SkinnedMeshRenderer.bones保存的只是目标mesh相关的骨骼,要获得目标全部骨骼,可以通过查找的方式.
            newBones[i] = FindChildRecursion(target.transform, bone.name);
        }

        selfSkin.bones = newBones;
    }

    // 递归查找
    public Transform FindChildRecursion(Transform t, string name)
    {
        foreach (Transform child in t)
        {
            if (child.name == name)
            {
                return child;
            }
            else
            {
                Transform ret = FindChildRecursion(child, name);
                if (ret != null)
                    return ret;
            }
        }

        return null;
    }

    #endregion
}
