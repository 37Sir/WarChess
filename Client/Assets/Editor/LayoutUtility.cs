using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 自定义面板时会用到的通用方法
/// </summary>
public static class LayoutUtility
{

    private static readonly string[] _labels = { "Data", "ScriptableObject" };

    /// <summary>
    /// 通过菜单创建EffectPack
    /// </summary>
    [MenuItem("Assets/Create/ProjectX/EffectPack")]
    public static void CreateAsset()
    {
        ScriptableObject asset = ScriptableObject.CreateInstance("EffectPack");
        if (asset)
        {
            SaveAsset(asset);
        }
        else
        {
            Debugger.LogDebug("Create EffectPack Faild!");
        }
    }

    /// <summary>
    /// 通过菜单创建TweenPack
    /// </summary>
    [MenuItem("Assets/Create/ProjectX/TweenPack")]
    public static void CreateTweenPackAsset()
    { 
        ScriptableObject asset = ScriptableObject.CreateInstance("TweenPack");
        string path = "Assets/" + Config.TweenPacksRoot + "/" + asset.GetType().Name + ".asset";
        if (asset)
        {
            SaveAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
        }
        else
        {
            Debugger.LogDebug("Create TweenPack Faild!");
        }
    }

    public static void CreateAsset<T>() where T : ScriptableObject
    {
        SaveAsset(ScriptableObject.CreateInstance<T>());
    }

    private static void SaveAsset(ScriptableObject asset)
    {
        SaveAsset(asset, GetUniquePath(asset));
    }

    /// <summary>
    /// 得到唯一的路径（防止重名）
    /// </summary>
    /// <param name="asset">待创建的资源</param>
    /// <returns></returns>
    private static string GetUniquePath(ScriptableObject asset)
    {
        string path = "Assets/" + Config.EffectPacksRoot + "/" + asset.GetType().Name + ".asset";
        return AssetDatabase.GenerateUniqueAssetPath(path);
    }


    private static void SaveAsset(ScriptableObject asset, string path)
    {
        AssetDatabase.SetLabels(asset, _labels);
        Debugger.LogDebug(path);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    public static string PrefabFiled(string fieldName, GameObject obj)
    {
        GameObject prefabObj = EditorGUILayout.ObjectField(fieldName, obj, typeof(GameObject), true) as GameObject;
        if (prefabObj != null && PrefabUtility.GetPrefabType(prefabObj) == PrefabType.Prefab)
        {
            return prefabObj.name;
        }
        return null;
    }

    public static string AudioFiled(string fieldName, GameObject obj)
    {
        //EditorGUILayout.
        AudioClip audioClip = EditorGUILayout.ObjectField(fieldName, obj, typeof(AudioClip), true) as AudioClip;
        if (audioClip != null)
        {
            return audioClip.name;
        }
        return null;
    }

    public static void AssignWithUndo<T>(ref T oldObj, T newObj, string name, UnityEngine.Object undoObj)
    {
        if (oldObj == null)
        {
            if (newObj == null)
                return;
        }
        else
        {
            if (oldObj.Equals(newObj))
                return;
        }

        UnityEditor.Undo.RecordObject(undoObj, name);
        oldObj = newObj;
        EditorUtility.SetDirty(undoObj);
    }

    public static bool Foldout(string header)
    {
        bool state = EditorPrefs.GetBool(header, true);

        GUILayout.Space(3f);
        if (!state)
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);

        string text = "<b><size=11>" + header + "</size></b>";
        if (state)
            text = "\u25B2 " + header;
        else
            text = "\u25BC " + header;
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f)))
        {
            state = !state;
            EditorPrefs.SetBool(header, state);
        }

        GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!state)
        {
            GUILayout.Space(3f);
        }
        return state;
    }

    public static void BeginContents()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    public static void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }
}
