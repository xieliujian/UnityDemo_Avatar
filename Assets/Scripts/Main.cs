using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class AvatarRes
{
    public string mName;
    public GameObject mSkeleton;
    public List<GameObject> mEyesList = new List<GameObject>();
    public List<GameObject> mFaceList = new List<GameObject>();
    public List<GameObject> mHairList = new List<GameObject>();
    public List<GameObject> mPantsList = new List<GameObject>();
    public List<GameObject> mShoesList = new List<GameObject>();
    public List<GameObject> mTopList = new List<GameObject>();

    public int mEyesIdx = 0;
    public int mFaceIdx = 0;
    public int mHairIdx = 0;
    public int mPantsIdx = 0;
    public int mShoesIdx = 0;
    public int mTopIdx = 0;
    
    public void Reset()
    {
        mEyesIdx = 0;
        mFaceIdx = 0;
        mHairIdx = 0;
        mPantsIdx = 0;
        mShoesIdx = 0;
        mTopIdx = 0;
    }

    public void AddIndex(int type)
    {
        if (type == (int)EPart.EP_Eyes)
        {
            mEyesIdx++;
            if (mEyesIdx >= mEyesList.Count)
                mEyesIdx = 0;
        }
        else if (type == (int)EPart.EP_Face)
        {
            mFaceIdx++;
            if (mFaceIdx >= mFaceList.Count)
                mFaceIdx = 0;
        }
        else if (type == (int)EPart.EP_Hair)
        {
            mHairIdx++;
            if (mHairIdx >= mHairList.Count)
                mHairIdx = 0;
        }
        else if (type == (int)EPart.EP_Pants)
        {
            mPantsIdx++;
            if (mPantsIdx >= mPantsList.Count)
                mPantsIdx = 0;
        }
        else if (type == (int)EPart.EP_Shoes)
        {
            mShoesIdx++;
            if (mShoesIdx >= mShoesList.Count)
                mShoesIdx = 0;
        }
        else if (type == (int)EPart.EP_Top)
        {
            mTopIdx++;
            if (mTopIdx >= mTopList.Count)
                mTopIdx = 0;
        }
    }

    public void ReduceIndex(int type)
    {
        if (type == (int)EPart.EP_Eyes)
        {
            mEyesIdx--;
            if (mEyesIdx < 0)
                mEyesIdx = mEyesList.Count - 1;
        }
        else if (type == (int)EPart.EP_Face)
        {
            mFaceIdx--;
            if (mFaceIdx < 0)
                mFaceIdx = mFaceList.Count - 1;
        }
        else if (type == (int)EPart.EP_Hair)
        {
            mHairIdx--;
            if (mHairIdx < 0)
                mHairIdx = mHairList.Count - 1;
        }
        else if (type == (int)EPart.EP_Pants)
        {
            mPantsIdx--;
            if (mPantsIdx < 0)
                mPantsIdx = mPantsList.Count - 1;
        }
        else if (type == (int)EPart.EP_Shoes)
        {
            mShoesIdx--;
            if (mShoesIdx < 0)
                mShoesIdx = mShoesList.Count - 1;
        }
        else if (type == (int)EPart.EP_Top)
        {
            mTopIdx--;
            if (mTopIdx < 0)
                mTopIdx = mTopList.Count - 1;
        }
    }
}

public enum EPart
{
    EP_Eyes,
    EP_Face,
    EP_Hair,
    EP_Pants,
    EP_Shoes,
    EP_Top,
}

public class Main : MonoBehaviour
{
    #region 常量

    private const string SkeletonName = "skeleton";
    private const string EyesName = "eyes";
    private const string FaceName = "face";
    private const string HairName = "hair";
    private const string PantsName = "pants";
    private const string ShoesName = "shoes";
    private const string TopName = "top";

    private const int typeWidth = 240;
    private const int typeheight = 100;
    private const int buttonWidth = 60;

    #endregion

    #region 变量

    private List<AvatarRes> mAvatarResList = new List<AvatarRes>(); 
    private AvatarRes mAvatarRes = null;
    private int mAvatarResIdx = 0;

    private Character mCharacter = null;

    private static bool mCombine = false;

    #endregion

    #region 内置函数

    // Use this for initialization
    void Start ()
    {
        CreateAllAvatarRes();
        InitCharacter();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnGUI()
    {
        GUI.skin.box.fontSize = 50;
        GUI.skin.button.fontSize = 50;

        GUILayout.BeginArea(new Rect(10, 10, typeWidth + 2 * buttonWidth + 8, 1000));

        // Buttons for changing the active character.
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            ReduceAvatarRes();
            mCharacter.Generate(mAvatarRes, mCombine);
        }

        GUILayout.Box("Character", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            AddAvatarRes();
            mCharacter.Generate(mAvatarRes, mCombine);
        }

        GUILayout.EndHorizontal();

        // Buttons for changing character elements.
        AddCategory((int)EPart.EP_Face, "Head", null);
        AddCategory((int)EPart.EP_Eyes, "Eyes", null);
        AddCategory((int)EPart.EP_Hair, "Hair", null);
        AddCategory((int)EPart.EP_Top, "Body", "item_shirt");
        AddCategory((int)EPart.EP_Pants, "Legs", "item_pants");
        AddCategory((int)EPart.EP_Shoes, "Feet", "item_boots");

        GUILayout.EndArea();
    }

    // Draws buttons for configuring a specific category of items, like pants or shoes.
    void AddCategory(int parttype, string displayName, string anim)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.ReduceIndex(parttype);

            if (!mCombine)
                mCharacter.ChangeEquipUnCombine(parttype, mAvatarRes);
            else
                mCharacter.Generate(mAvatarRes, mCombine);
        }

        GUILayout.Box(displayName, GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.AddIndex(parttype);

            if (!mCombine)
                mCharacter.ChangeEquipUnCombine(parttype, mAvatarRes);
            else
                mCharacter.Generate(mAvatarRes, mCombine);
        }

        GUILayout.EndHorizontal();
    }

    #endregion

    #region 函数

    private void InitCharacter()
    {
        GameObject go = new GameObject();
        mCharacter = go.AddComponent<Character>();
        mCharacter.SetName("Character");

        mAvatarRes = mAvatarResList[mAvatarResIdx];
        mCharacter.Generate(mAvatarRes, mCombine);
    }

    private void CreateAllAvatarRes()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/");
        foreach(var subdir in dir.GetDirectories())
        {
            string[] splits = subdir.Name.Split('/');
            string dirname = splits[splits.Length - 1];

            GameObject [] golist = Resources.LoadAll<GameObject>(dirname);

            AvatarRes avatarres = new AvatarRes();
            mAvatarResList.Add(avatarres);

            avatarres.mName = dirname;
            avatarres.mSkeleton = FindRes(golist, SkeletonName)[0];
            avatarres.mEyesList = FindRes(golist, EyesName);
            avatarres.mFaceList = FindRes(golist, FaceName);
            avatarres.mHairList = FindRes(golist, HairName);
            avatarres.mPantsList = FindRes(golist, PantsName);
            avatarres.mShoesList = FindRes(golist, ShoesName);
            avatarres.mTopList = FindRes(golist, TopName);
        }
    }

    private List<GameObject> FindRes(GameObject []golist, string findname)
    {
        List<GameObject> findlist = new List<GameObject>();
        foreach (var go in golist)
        {
            if (go.name.Contains(findname))
            {
                findlist.Add(go);
            }
        }

        return findlist;
    }

    private void AddAvatarRes()
    {
        mAvatarResIdx++;
        if (mAvatarResIdx >= mAvatarResList.Count)
            mAvatarResIdx = 0;

        mAvatarRes = mAvatarResList[mAvatarResIdx];
    }

    private void ReduceAvatarRes()
    {
        mAvatarResIdx--;
        if (mAvatarResIdx < 0)
            mAvatarResIdx = mAvatarResList.Count - 1;

        mAvatarRes = mAvatarResList[mAvatarResIdx];
    }

    #endregion
}
