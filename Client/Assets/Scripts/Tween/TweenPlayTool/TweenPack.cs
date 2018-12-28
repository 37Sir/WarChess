using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum TweenType
{
    None = 0,
    Position = 1,
    Rotation = 2,
    BlendableScale = 3,
    Color = 4,
    Fade = 5,
    UIPosition = 6,
    UIColor = 7,
    UIFade = 8,
    LocalPosition = 9,
    LocalRotation = 10,
    LocalScale = 11,
    UIText = 12,
    BlendablePosition = 13,
}



/// <summary>
/// 动画包
/// </summary>
public sealed class TweenPack : ScriptableObject
{
    [System.Serializable]
    public class Attribute
    {
        public string TweenName = null;                         //动画的名字
        public TweenType TweenType = TweenType.Position;        //动画的类型
        public Ease EaseType = Ease.Linear;                     //动画函数类型

        public Vector3 To = Vector3.zero;                       //目的坐标
        public Color Color = Color.white;
        public float Fade = 1f;
        public string ToText = null;                              //目标字符串

        public Vector3 FromPos = Vector3.zero;
        public Color FromColor = Color.white;
        public float FromFade = 1f;

        public float Duration = 1;                              //持续时间
        public float DelayTime = 0.0f;                          //延迟播放的时间
        public int Loop = 0;                                    //循环的次数
        public LoopType LoopType = LoopType.Restart;            //循环播放类型
        public bool isNeedFrom = false;

        public void Copy(Attribute attribute)
        {
            if ((attribute == this) || (attribute == null))
                return;

            TweenName = attribute.TweenName;
            EaseType = attribute.EaseType;

            To = attribute.To;
            Color = attribute.Color;
            Fade = attribute.Fade;
            ToText = attribute.ToText;

            FromPos = attribute.FromPos;
            FromColor = attribute.FromColor;
            FromFade = attribute.FromFade;

            Duration = attribute.Duration;
            DelayTime = attribute.DelayTime;
            Loop = attribute.Loop;
            LoopType = attribute.LoopType;
            TweenType = attribute.TweenType;
            isNeedFrom = attribute.isNeedFrom;
        }
    }

    [HideInInspector]
    public List<Attribute> Attributes = new List<Attribute>();

    /// <summary>
    /// 比较延迟时间
    /// </summary>
    private static System.Comparison<Attribute> m_comparsion = ((a1, a2) => a1.DelayTime.CompareTo(a2.DelayTime));


    /// <summary>
    /// 根据延迟时间排序
    /// </summary>
    public void SortByDelayTime()
    {
        Attributes.Sort(m_comparsion);
    }
}
