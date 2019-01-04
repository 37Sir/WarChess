using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 动画片段播放
/// </summary>
public class Tween
{
    #region Private Field

    private GameObject m_owner;                         //该动画对应的物体
    private Tweener m_tweener;                          //该动画对应的Tweener
    private List<Tweener> m_tweeners = new List<Tweener>();
    private float m_duration;                           //持续时间

    private Vector3 m_to = Vector3.zero;                //目标位置、目标大小、目标旋转
    private float m_fade = 1;                           //透明度
    private Color m_color = Color.white;                //颜色
    private String m_toText = null;

    private Vector3 m_fromPos = Vector3.zero;              //初始位置、大小、旋转
    private float m_fromFade = 1;
    private Color m_fromColor = Color.white;

    private TweenType m_tweenType = TweenType.Position; //动画种类
    private Ease m_easeType = Ease.Linear;              //动画函数类型
    private float m_delayTime = 0f;                     //延迟时间
    private int m_loop = 0;                             //循环次数
    private LoopType m_loopType = LoopType.Restart;     //循环类型

    #endregion Private Field

    #region Public Field

    public GameObject Owner
    {
        get
        {
            return m_owner;
        }
        set
        {
            m_owner = value;
        }
    }

    public bool isLast = false;
    public bool isNeedFrom = false;
    public delegate void TweenPackCompleteDelegate();
    private TweenPackCompleteDelegate m_tweenPackOnComplete = null;

    public delegate void TweenClipCompleteDelegate(object[] body);
    private TweenClipCompleteDelegate m_onComplete = null;
    private object[] m_object = null;
    #endregion Public Field

    #region Public Method

    /// <summary>
    /// 设置动画函数
    /// </summary>
    /// <param name="easeType"></param>
    public void SetEaseType(Ease easeType)
    {
        m_easeType = easeType;
    }

    /// <summary>
    /// 设置持续时间
    /// </summary>
    /// <param name="duration"></param>
    public void SetDuration(float duration)
    {
        m_duration = duration;
    }

    /// <summary>
    /// 设置延迟时间
    /// </summary>
    /// <param name="delayTime"></param>
    public void SetDelayTime(float delayTime)
    {
        m_delayTime = delayTime;
    }

    /// <summary>
    /// 设置循环次数
    /// </summary>
    /// <param name="loop"></param>
    public void SetLoop(int loop)
    {
        m_loop = loop;
    }

    /// <summary>
    /// 设置目标位置、大小、旋转
    /// </summary>
    /// <param name="to"></param>
    public void SetTo(Vector3 to)
    {
        m_to = to;
    }

    /// <summary>
    /// 设置动画类型
    /// </summary>
    /// <param name="type"></param>
    public void SetTweenType(int type)
    {
        m_tweenType = (TweenType)type;
    }

    /// <summary>
    /// 设置动画类型
    /// </summary>
    /// <param name="type"></param>
    public void SetTweenType(TweenType type)
    {
        m_tweenType = type;
    }

    /// <summary>
    /// 设置循环类型
    /// </summary>
    /// <param name="type"></param>
    public void SetLoopType(LoopType type)
    {
        m_loopType = type;
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    /// <param name="fade"></param>
    public void SetFade(float fade)
    {
        m_fade = fade;
    }

    /// <summary>
    /// 设置颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        m_color = color;
    }

    /// <summary>
    /// 设置目标文本内容
    /// </summary>
    /// <param name="text"></param>
    public void SetToText(string text)
    {
        m_toText = text;
    }

    /// <summary>
    /// 设置起始位置
    /// </summary>
    /// <param name="vector"></param>
    public void SetPosFrom(Vector3 vector)
    {
        m_fromPos = vector;
    }

    /// <summary>
    /// 设置起始透明度
    /// </summary>
    /// <param name="fade"></param>
    public void SetFadeFrom(float fade)
    {
        m_fromFade = fade;
    }

    /// <summary>
    /// 设置初始颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetColorFrom(Color color)
    {
        m_fromColor = color;
    }

    /// <summary>
    /// 设置回调函数
    /// </summary>
    /// <param name="super"></param>
    /// <param name="call"></param>
    public void SetOnComplete(TweenClipCompleteDelegate onComplete, object[] body)
    {
        m_onComplete = onComplete;
        m_object = body;
    }

    /// <summary>
    /// 移除回调函数
    /// </summary>
    public void RemoveOnComplete()
    {
        //if (onComplete != null)
        //{
        //    onComplete.Release();
        //}
        //onComplete = null;
    }

    /// <summary>
    /// 播放
    /// </summary>
    public void Play()
    {
        if(m_tweener == null)
        {
            switch (m_tweenType)
            {
                case TweenType.Position:
                    if (isNeedFrom)
                    {
                        m_owner.transform.position = m_fromPos;
                    }
                    m_tweener = m_owner.transform.DOMove(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)                        
                        .OnComplete(OnComplete);
                    break;
                case TweenType.Rotation:
                    if (isNeedFrom)
                    {
                        m_owner.transform.rotation = Quaternion.Euler(m_fromPos);
                    }
                    m_tweener = m_owner.transform.DORotate(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    break;
                case TweenType.LocalScale:
                    if (isNeedFrom)
                    {
                        m_owner.transform.localScale = m_fromPos;
                    }
                    m_tweener = m_owner.transform.DOScale(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    break;
                case TweenType.Fade:
                    foreach(Material material in m_owner.GetComponent<Renderer>().materials)
                    {
                        if (isNeedFrom)
                        {
                            material.color = new Color(material.color.r, material.color.g, material.color.b, m_fromFade);
                        }
                        material.DOFade(m_fade, m_duration)
                                .SetDelay(m_delayTime)
                                .SetEase(m_easeType)
                                .SetLoops(m_loop, m_loopType)
                                .OnComplete(OnComplete);
                    }                
                    break;
                case TweenType.Color:
                    foreach (var renderer in m_owner.GetComponentsInChildren<Renderer>())
                    {
                        foreach(var material in renderer.materials)
                        {
                            if (isNeedFrom)
                            {
                                material.color = m_fromColor;
                            }
                            var tweener = material.DOColor(m_color, m_duration)
                                    .SetDelay(m_delayTime)
                                    .SetEase(m_easeType)
                                    .SetLoops(m_loop, m_loopType)
                                    .OnComplete(OnComplete);
                            m_tweeners.Add(tweener);
                        }
                    }
                    break;
                case TweenType.UIColor:
                    Text textColor = m_owner.GetComponent<Text>();
                    Image imageColor = m_owner.GetComponent<Image>();
                    if (textColor != null)
                    {
                        if (isNeedFrom)
                        {
                            textColor.color = m_fromColor;
                        }
                        textColor.DOBlendableColor(m_color, m_duration)
                            .SetDelay(m_delayTime)
                            .SetEase(m_easeType)
                            .SetLoops(m_loop, m_loopType)
                            .OnComplete(OnComplete);
                    }
                    if (imageColor != null)
                    {
                        if (isNeedFrom)
                        {
                            imageColor.color = m_fromColor;
                        }
                        imageColor.DOBlendableColor(m_color, m_duration)
                             .SetDelay(m_delayTime)
                             .SetEase(m_easeType)
                             .SetLoops(m_loop, m_loopType)
                             .OnComplete(OnComplete);
                    }
                    break;
                case TweenType.UIFade:
                    CanvasGroup canvasGroup = m_owner.GetComponent<CanvasGroup>();
                    if(canvasGroup == null)
                    {
                        Text text = m_owner.GetComponent<Text>();
                        Image image = m_owner.GetComponent<Image>();
                        if(text != null)
                        {
                            if (isNeedFrom)
                            {
                                text.color = new Color(text.color.r, text.color.g, text.color.b, m_fromFade);
                            }
                            text.DOFade(m_fade, m_duration)
                                .SetDelay(m_delayTime)
                                .SetEase(m_easeType)
                                .SetLoops(m_loop, m_loopType)
                                .OnComplete(OnComplete);
                        }
                        if (image != null)
                        {
                            if (isNeedFrom)
                            {
                                image.color = new Color(image.color.r, image.color.g, image.color.b, m_fromFade);
                            }
                            image.DOFade(m_fade, m_duration)
                                 .SetDelay(m_delayTime)
                                 .SetEase(m_easeType)
                                 .SetLoops(m_loop, m_loopType)
                                 .OnComplete(OnComplete);
                        }
                    }
                    else
                    {
                        if (isNeedFrom)
                        {
                            canvasGroup.alpha = m_fromFade;
                        }
                        canvasGroup.DOFade(m_fade, m_duration)
                                   .SetDelay(m_delayTime)
                                   .SetEase(m_easeType)
                                   .SetLoops(m_loop, m_loopType)
                                   .OnComplete(OnComplete);
                    }                
                    break;
                case TweenType.UIPosition:
                    var rectTransform = m_owner.GetComponent<RectTransform>();
                    if (isNeedFrom)
                    {
                        rectTransform.anchoredPosition3D = m_fromPos;
                    }
                    m_tweener = rectTransform.DOAnchorPos3D(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    break;
                case TweenType.LocalPosition:
                    if (isNeedFrom)
                    {
                        m_owner.transform.localPosition = m_fromPos;
                    }
                    m_tweener = m_owner.transform.DOLocalMove(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    //m_tweener = m_owner.transform.DOBlendableLocalMoveBy(m_to, m_duration)
                    //    .SetDelay(m_delayTime)
                    //    .SetEase(m_easeType)
                    //    .SetLoops(m_loop, m_loopType)
                    //    .OnComplete(OnComplete);
                    break;
                case TweenType.LocalRotation:
                    if (isNeedFrom)
                    {
                        m_owner.transform.localRotation = Quaternion.Euler(m_fromPos);
                    }
                    m_tweener = m_owner.transform.DOLocalRotate(m_to, m_duration, RotateMode.Fast)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    //m_tweener = m_owner.transform.DOBlendableLocalRotateBy(m_to, m_duration, RotateMode.FastBeyond360)
                    //    .SetDelay(m_delayTime)
                    //    .SetEase(m_easeType)
                    //    .SetLoops(m_loop, m_loopType)
                    //    .OnComplete(OnComplete);
                    break;
                case TweenType.BlendableScale:
                    if (isNeedFrom)
                    {
                        m_owner.transform.localScale = m_fromPos;
                    }
                    m_tweener = m_owner.transform.DOBlendableScaleBy(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    break;
                case TweenType.UIText:
                    Text uiText = m_owner.GetComponent<Text>();
                    if(uiText != null)
                    {
                        m_tweener = uiText.DOText(m_toText, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    }
                    break;
                case TweenType.BlendablePosition:
                    if (isNeedFrom)
                    {
                        m_owner.transform.localPosition = m_fromPos;
                    }
                    m_tweener = m_owner.transform.DOBlendableLocalMoveBy(m_to, m_duration)
                        .SetDelay(m_delayTime)
                        .SetEase(m_easeType)
                        .SetLoops(m_loop, m_loopType)
                        .OnComplete(OnComplete);
                    break;
            }
        }
        else
        {
            m_tweener.Play();
        }
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        if(m_tweener != null)
        {
            m_tweener.Pause();
        }       
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        if (m_tweener != null)
        {
            m_tweener.Kill();
            m_tweener = null;
        }
        foreach(var tweener in m_tweeners)
        {
            tweener.Kill();
        }
        m_tweeners.Clear();
    }

    /// <summary>
    /// 恢复播放
    /// </summary>
    public void Resume()
    {
        if (m_tweener != null)
        {
            m_tweener.Play();
        }
    }

    public void SetTweenPackCompleteDelegate(TweenPackCompleteDelegate tweenPackCompleteDelegate)
    {
        m_tweenPackOnComplete = tweenPackCompleteDelegate;
    }

    #endregion Public Method

    /// <summary>
    /// 动画播完回调
    /// </summary>
    protected void OnComplete()
    {
        //if(isLast == true)
        //{
        //    tweenPackOnComplete();
        //}

        if (m_onComplete != null)
        {
            m_onComplete.Invoke(m_object);
        }
    }
}
