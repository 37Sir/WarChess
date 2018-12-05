using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 自定义EffectPack的预览窗
/// </summary>
[CustomEditor(typeof(EffectPack), true), CanEditMultipleObjects]
public class EffectPackInspector : Editor
{
    private string[] _effectAnchors = null;                                                 //跟随物体可选列表
    private EffectPack.Attribute m_attribute = new EffectPack.Attribute();                  //特效包的参数
    private EffectPack.AudioAttr m_audioAttr = new EffectPack.AudioAttr();                  //音效的参数
    #region Unity Method

    public override void OnInspectorGUI()
    {
        Time.timeScale = EditorGUILayout.Slider("Time Scale", Time.timeScale, 0.0f, 2.0f);  //时间
        if (GUILayout.Button("Sort By Delay Time", GUILayout.Width(130f)))                  //按钮:根据延迟时间排序
        {
            for (int j = 0; j < targets.Length; ++j)
            {
                (targets[j] as EffectPack).SortByDelayTime();
            }
        }

        base.OnInspectorGUI();

        if (LayoutUtility.Foldout("Effects"))
        {
            LayoutUtility.BeginContents();
            DrawEffectList(targets);
            LayoutUtility.EndContents();
        }

        if (LayoutUtility.Foldout("Audios"))
        {
            LayoutUtility.BeginContents();
            DrawAudioList(targets);
            LayoutUtility.EndContents();
        }
    }

    private void OnEnable()
    {
        _effectAnchors = EffectConfig.Instance.Anchors.ToArray();

        for (int i = 0; i < targets.Length; ++i)
        {
            (targets[i] as EffectPack).SortByDelayTime();
        }
    }

    #endregion Unity Method

    #region Private Method
    /// <summary>
    /// 绘制attribute的列表
    /// </summary>
    /// <param name="targets"></param>
    private void DrawEffectList(UnityEngine.Object[] targets)
    {
        EffectPack effectPack = targets[0] as EffectPack;

        for (int i = 0; i < effectPack.Attributes.Count;)
        {
            bool isEqual = true;

            for (int j = 1; j < targets.Length; ++j)
            {
                if (!effectPack.Attributes[i].Equals((targets[j] as EffectPack).Attributes[i]))
                {
                    isEqual = false;
                    break;
                }
            }

            if (isEqual)
            {
                m_attribute.Copy(effectPack.Attributes[i]);

                string header = ((m_attribute.EffectName != null) ? m_attribute.EffectName : string.Empty);

                if (LayoutUtility.Foldout(header))
                {
                    LayoutUtility.BeginContents();

                    if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f)))
                    {
                        UnityEditor.Undo.RecordObjects(targets, "Remove Effect Attribute");
                        for (int j = 0; j < targets.Length; ++j)
                        {
                            (targets[j] as EffectPack).Attributes.RemoveAt(i);
                            EditorUtility.SetDirty(targets[j]);
                        }
                    }
                    else
                    {
                        DrawEffectAttribute(m_attribute);
                        if (!m_attribute.Equals(effectPack.Attributes[i]))
                        {
                            UnityEditor.Undo.RecordObjects(targets, "Modify Effect Attribute");
                            for (int j = 0; j < targets.Length; ++j)
                            {
                                (targets[j] as EffectPack).Attributes[i].Copy(m_attribute);
                                EditorUtility.SetDirty(targets[j]);
                            }
                        }
                    }
                    LayoutUtility.EndContents();
                }
            }
            ++i;
        }

        // 绘制加入新的Effect的选项
        if (LayoutUtility.Foldout("Add Effect Attribute"))
        {
            LayoutUtility.BeginContents();
            string effectName = LayoutUtility.PrefabFiled("Effect", null);
            if (effectName != null)
            {
                UnityEditor.Undo.RecordObjects(targets, "Add Effect Attribute");

                for (int j = 0; j < targets.Length; ++j)
                {
                    EffectPack.Attribute attribute = new EffectPack.Attribute();
                    attribute.EffectName = effectName;
                    attribute.EffectPath = EffectConfig.Instance.EffectPath + "/" + effectName + ".prefab";
                    (targets[j] as EffectPack).Attributes.Add(attribute);
                    EditorUtility.SetDirty(targets[j]);
                }
            }
            LayoutUtility.EndContents();
        }
    }

    /// <summary>
    /// 绘制Audio列表
    /// </summary>
    /// <param name="targets"></param>
    private void DrawAudioList(UnityEngine.Object[] targets)
    {
        EffectPack effectPack = targets[0] as EffectPack;
        for (int i = 0; i < effectPack.AudioAttrs.Count;)
        {
            m_audioAttr.Copy(effectPack.AudioAttrs[i]);
            string header = ((m_audioAttr.AudioName != null) ? m_audioAttr.AudioName : string.Empty);//标题
            if (LayoutUtility.Foldout(header))
            {
                LayoutUtility.BeginContents();
                if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f)))
                {
                    UnityEditor.Undo.RecordObjects(targets, "Remove AudioClip");
                    for (int j = 0; j < targets.Length; ++j)
                    {
                        (targets[j] as EffectPack).AudioAttrs.RemoveAt(i);
                        EditorUtility.SetDirty(targets[j]);
                    }
                }
                else
                {
                    DrawAudioAttribute(m_audioAttr);
                    if (!m_audioAttr.Equals(effectPack.AudioAttrs[i]))
                    {
                        UnityEditor.Undo.RecordObjects(targets, "Modify AudioClip");
                        for (int j = 0; j < targets.Length; ++j)
                        {
                            (targets[j] as EffectPack).AudioAttrs[i].Copy(m_audioAttr);
                            EditorUtility.SetDirty(targets[j]);
                        }
                    }
                }
                LayoutUtility.EndContents();
            }
            ++i;
        }

        // 绘制加入新的Audio的选项
        if (LayoutUtility.Foldout("Add New AudioClip"))
        {
            LayoutUtility.BeginContents();
            string audioName = LayoutUtility.AudioFiled("Audio Clip", null);
            if (audioName != null)
            {
                UnityEditor.Undo.RecordObjects(targets, "Add Audio Clip");
                for (int j = 0; j < targets.Length; ++j)
                {
                    EffectPack.AudioAttr audioAttr = new EffectPack.AudioAttr();
                    audioAttr.AudioName = audioName;
                    audioAttr.AudioPath = EffectConfig.Instance.AudioClipPath + "/" + audioName;
                    (targets[j] as EffectPack).AudioAttrs.Add(audioAttr);
                    EditorUtility.SetDirty(targets[j]);
                }
            }
            LayoutUtility.EndContents();
        }
    }

    /// <summary>
    /// 绘制attribute面板
    /// </summary>
    /// <param name="attribute"></param>
    private void DrawEffectAttribute(EffectPack.Attribute attribute)
    {
        int anchorIndex = EffectConfig.Instance.Anchors.IndexOf(attribute.Anchor);
        if (anchorIndex < 0)
        {
            anchorIndex = EffectConfig.Instance.Anchors.Count - 1;
            _effectAnchors = EffectConfig.Instance.Anchors.ToArray();
        }

        attribute.Anchor = _effectAnchors[EditorGUILayout.Popup("Anchor", anchorIndex, _effectAnchors)];
        attribute.DelayTime = EditorGUILayout.FloatField("Delay Time", attribute.DelayTime);
        attribute.IsAttach = EditorGUILayout.Toggle("Is Attach", attribute.IsAttach);
        GUI.enabled = attribute.IsAttach;
        attribute.IsFixedTranslate = EditorGUILayout.Toggle("Is Fixed Translate", attribute.IsFixedTranslate);
        GUI.enabled = (attribute.IsAttach && !attribute.IsFixedTranslate);
        attribute.IsFixedRotation = EditorGUILayout.Toggle("Is Fixed Rotation", attribute.IsFixedRotation);
        GUI.enabled = true;
        attribute.IsFixedScale = EditorGUILayout.Toggle("Is Fixed Scale", attribute.IsFixedScale);
        attribute.Offset = EditorGUILayout.Vector3Field("Offset", attribute.Offset);
    }

    /// <summary>
    /// 绘制audioattr面板
    /// </summary>
    /// <param name="attribute"></param>
    private void DrawAudioAttribute(EffectPack.AudioAttr audioAttr)
    {
        audioAttr.DelayTime = EditorGUILayout.FloatField("Delay Time", audioAttr.DelayTime);
        audioAttr.AudioType = EditorGUILayout.TextField("Audio Type", audioAttr.AudioType);
        GUI.enabled = true;
    }
    #endregion Private Method
}