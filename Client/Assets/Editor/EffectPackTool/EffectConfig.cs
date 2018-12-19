using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

///EffectPack配置
public sealed class EffectConfig : ScriptableObject
{
    #region Public Field

    public string EffectPackPath = "EffectPack";                //保存EffectPack的路径
    public string EffectPath = "Effects";                        //Effect Prefab保存的路径
    public string AudioClipPath = "AudioClip";                  //音效资源路径
    public List<string> Anchors = new List<string>();           //anchors列表
    public static readonly string DefaultAnchor = "Root";       //默认的anchor

    #endregion Public Field

    #region Private Field

    private static EffectConfig _instance;                      
    private static object _lock = new object();
    private static bool applicationIsQuitting = false;
    #endregion Private Field

    #region Public Property

    public static EffectConfig Instance
    {
        get
        {
            if(applicationIsQuitting)
            {
                Debugger.LogError("Singleton Instance already destroyed");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<EffectConfig>(Config.EffectConfigPath);
                }
            }
            return _instance;
        }
    }

    #endregion Public Property
    private EffectConfig()
    {
        Anchors.Add(DefaultAnchor);
    }

    private void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}
