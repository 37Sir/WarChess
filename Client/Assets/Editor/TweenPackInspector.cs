using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TweenPack), true), CanEditMultipleObjects]
public class TweenPackInspector : Editor
{
    private TweenPack.Attribute m_attribute = new TweenPack.Attribute();

    #region Unity Method

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Sort By Delay Time", GUILayout.Width(130f))){
            for(int j = 0; j < targets.Length; j++)
            {
                (targets[j] as TweenPack).SortByDelayTime();
            }
        }
        base.OnInspectorGUI();

        if (LayoutUtility.Foldout("Tweens"))
        {
            LayoutUtility.BeginContents();
            DrawTweenList(targets);
            LayoutUtility.EndContents();
        }
    }

    #endregion Unity Method

    #region Private Method

    private void DrawTweenList(UnityEngine.Object[] targets)
    {
        TweenPack tweenPack = targets[0] as TweenPack;
        for (int i = 0; i < tweenPack.Attributes.Count;)
        {
            bool isEqual = true;
            for (int j = 1; j < targets.Length; j++)
            {
                if (!tweenPack.Attributes[i].Equals((targets[j] as TweenPack).Attributes[j]))
                {
                    isEqual = false;
                    break;
                }
            }

            if (isEqual)
            {
                m_attribute.Copy(tweenPack.Attributes[i]);
                string header = m_attribute.TweenName;
                if (LayoutUtility.Foldout(header))
                {
                    LayoutUtility.BeginContents();
                    if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f)))
                    {
                        UnityEditor.Undo.RecordObjects(targets, "Remove Tween Attribute");
                        for (int j = 0; j < targets.Length; ++j)
                        {
                            (targets[j] as TweenPack).Attributes.RemoveAt(i);
                            EditorUtility.SetDirty(targets[j]);
                        }
                    }
                    else
                    {
                        DrawTweenAttribute(m_attribute);
                        if (!m_attribute.Equals(tweenPack.Attributes[i]))
                        {
                            UnityEditor.Undo.RecordObjects(targets, "Modify Tween Attribute");
                            for (int j = 0; j < targets.Length; ++j)
                            {
                                (targets[j] as TweenPack).Attributes[i].Copy(m_attribute);
                                EditorUtility.SetDirty(targets[j]);
                            }
                        }
                    }
                    LayoutUtility.EndContents();
                }
            }
            ++i;
        }

        //加入新的Tween
        if (LayoutUtility.Foldout("Add Tween Attribute"))
        {
            LayoutUtility.BeginContents();
            TweenType type = (TweenType)EditorGUILayout.EnumPopup("Tween Type", TweenType.None);
            if (type != TweenType.None)
            {
                UnityEditor.Undo.RecordObjects(targets, "Add Tween Attribute");
                for (int j = 0; j < targets.Length; ++j)
                {
                    TweenPack.Attribute attribute = new TweenPack.Attribute();
                    attribute.TweenType = type;
                    switch (type)
                    {
                        case TweenType.Position:
                            attribute.TweenName = "Position Tween";
                            break;
                        case TweenType.Rotation:
                            attribute.TweenName = "Rotation Tween";
                            break;
                        case TweenType.BlendableScale:
                            attribute.TweenName = "Scale Tween";
                            break;
                        case TweenType.Fade:
                            attribute.TweenName = "Fade Tween";
                            break;
                        case TweenType.Color:
                            attribute.TweenName = "Color Tween";
                            break;
                        case TweenType.UIColor:
                            attribute.TweenName = "UIFade Tween";
                            break;
                        case TweenType.UIFade:
                            attribute.TweenName = "UIFade Tween";
                            break;
                        case TweenType.UIPosition:
                            attribute.TweenName = "UIPosition Tween";
                            break;
                        case TweenType.LocalPosition:
                            attribute.TweenName = "LocalPosition Tween";
                            break;
                        case TweenType.LocalRotation:
                            attribute.TweenName = "LocalRotation Tween";
                            break;
                        case TweenType.LocalScale:
                            attribute.TweenName = "LocalScale Tween";
                            break;
                        case TweenType.UIText:
                            attribute.TweenName = "UIText Tween";
                            break;
                    }
                    (targets[j] as TweenPack).Attributes.Add(attribute);
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
    private void DrawTweenAttribute(TweenPack.Attribute attribute)
    {
        attribute.TweenName = EditorGUILayout.TextField("Tween Name", attribute.TweenName);
        attribute.TweenType = (TweenType)EditorGUILayout.EnumPopup("Tween Type", attribute.TweenType);
        attribute.DelayTime = EditorGUILayout.FloatField("Delay Time", attribute.DelayTime);
        attribute.EaseType = (Ease)EditorGUILayout.EnumPopup("Ease Type", attribute.EaseType);
        attribute.isNeedFrom = EditorGUILayout.Toggle("IsNeedFrom", attribute.isNeedFrom);
        switch (attribute.TweenType)
        {
            case TweenType.Position:              
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.Rotation:           
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.BlendableScale:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.Fade:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.Fade = EditorGUILayout.FloatField("To", attribute.Fade);
                break;
            case TweenType.Color:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromColor = EditorGUILayout.ColorField("From", attribute.FromColor);
                }
                attribute.Color = EditorGUILayout.ColorField("To", attribute.Color);
                break;
            case TweenType.UIColor:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromColor = EditorGUILayout.ColorField("From", attribute.FromColor);
                }
                attribute.Color = EditorGUILayout.ColorField("To", attribute.Color);
                break;
            case TweenType.UIFade:               
                if (attribute.isNeedFrom)
                {
                    attribute.FromFade = EditorGUILayout.FloatField("From", attribute.FromFade);
                }
                attribute.Fade = EditorGUILayout.FloatField("To", attribute.Fade);
                break;
            case TweenType.UIPosition:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.LocalPosition:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.LocalRotation:                
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.LocalScale:
                if (attribute.isNeedFrom)
                {
                    attribute.FromPos = EditorGUILayout.Vector3Field("From", attribute.FromPos);
                }
                attribute.To = EditorGUILayout.Vector3Field("To", attribute.To);
                break;
            case TweenType.UIText:
                attribute.ToText = EditorGUILayout.TextField("To Text", attribute.ToText);
                break;
        }
        
        attribute.Duration = EditorGUILayout.FloatField("Duration", attribute.Duration);
        attribute.Loop = EditorGUILayout.IntField("Loop", attribute.Loop);
        attribute.LoopType = (LoopType)EditorGUILayout.EnumFlagsField("Loop Type", attribute.LoopType);
    }

    #endregion Private Method
    }
