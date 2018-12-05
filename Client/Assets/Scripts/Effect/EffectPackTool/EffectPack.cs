using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 特效包
/// </summary>
public sealed class EffectPack : ScriptableObject
{
    [System.Serializable]
    public class Attribute
    {
        public string EffectName = null;                        //特效资源文件名
        public string EffectPath = null;                        //特效资源文件路径
        public string Anchor = "root";                          //跟随的物体的名字
        public float DelayTime = 0.0f;                          //延迟播放的时间
        public bool IsAttach = true;                            //是否跟随
        public bool IsFixedTranslate = false;                   //是否固定
        public bool IsFixedRotation = false;                    //是否固定角度
        public bool IsFixedScale = false;                       //是否固定缩放
        public Vector3 Offset = Vector3.zero;                   //偏移量

        public void Copy(Attribute attribute)
        {
            if ((attribute == this) || (attribute == null))
                return;

            EffectName = attribute.EffectName;
            Anchor = attribute.Anchor;
            DelayTime = attribute.DelayTime;
            IsAttach = attribute.IsAttach;
            IsFixedTranslate = attribute.IsFixedTranslate;
            IsFixedRotation = attribute.IsFixedRotation;
            IsFixedScale = attribute.IsFixedScale;
            Offset = attribute.Offset;
        }
    }

    [System.Serializable]
    public class AudioAttr
    {
        public string AudioName = null;                         //音效资源文件名
        public string AudioType = "mp3";                        //音效资源类型
        public string AudioPath = null;                         //音效资源文件路径
        public float DelayTime = 0.0f;                          //延迟播放的时间

        public void Copy(AudioAttr audioAttr)
        {
            if ((audioAttr == this) || (audioAttr == null))
                return;

            AudioName = audioAttr.AudioName;
            AudioPath = audioAttr.AudioPath;
            DelayTime = audioAttr.DelayTime;
            AudioType = audioAttr.AudioType;
        }
    }

    #region Public Field

    public float LifeTime = 1.0f;      //播放时长
    public bool IsLoop = false;        //循环
    public bool IsFadeOut = false;     //渐出
    public bool IsPlaySync = false;    //同步播放

    [HideInInspector]
    public List<Attribute> Attributes = new List<Attribute>();

    [HideInInspector]
    public List<AudioAttr> AudioAttrs = new List<AudioAttr>();

    #endregion Public Field

    #region Private Field

    /// <summary>
    /// 比较延迟时间
    /// </summary>
    private static System.Comparison<Attribute> _comparsion = ((a1, a2) => a1.DelayTime.CompareTo(a2.DelayTime));

    #endregion Private Field


    #region Public Method

    /// <summary>
    /// 根据延迟时间排序
    /// </summary>
    public void SortByDelayTime()
    {
        Attributes.Sort(_comparsion);
    }

    #endregion Public Method
}
