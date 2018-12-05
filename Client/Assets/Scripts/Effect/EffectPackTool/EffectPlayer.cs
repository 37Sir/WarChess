using Framework;
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// EffectPack的播放和停止
/// </summary>
[AddComponentMenu("Effect/EffectPlayer")]
public class EffectPlayer : MonoBehaviour
{
    [Serializable]
    public class Event : UnityEvent<EffectPlayer>
    {

    }

    #region Public Field

    public EffectPack EffectPack    // 对应的特效包
    {
        get
        {
            return m_effectPack;
        }
        set
        {
            m_effectPack = value;
        }
    }

    public EffectList EffectList    // 对应的特效列表
    {
        get
        {
            return m_effectList;
        }
        set
        {
            m_effectList = value;
        }
    }

    public bool IsPlaying           //是否正在播
    {
        get
        {
            return (m_playIndex > 0);
        }
    }

    public float PlayedTime         // 已经播放的时间
    {
        get;
        private set;
    }

    public float Scale
    {
        get
        {
            return m_scale;
        }
        set
        {
            if (value > 0.0f)
            {
                if (m_effectList != null)
                {
                    for (int i = 0; i < m_playIndex; ++i)
                    {
                        if (m_effectList.Effects[i].EffectObject)
                        {
                            m_effectList.Effects[i].SetScale(value);
                        }
                    }
                }
                m_scale = value;
            }
        }
    }

    public Quaternion Rotation
    {
        get
        {
            return m_rotation;
        }
        set
        {
            if (value != null)
            {
                if (m_effectList != null)
                {
                    for (int i = 0; i < m_playIndex; ++i)
                    {
                        if (m_effectList.Effects[i].EffectObject)
                        {
                            m_effectList.Effects[i].SetEffectRotation(value);
                        }
                    }
                }
                m_rotation = value;
            }
        }
    }

    public Quaternion LocalRotation
    {
        get
        {
            return m_localRotation;
        }
        set
        {
            if (value != null)
            {
                if (m_effectList != null)
                {
                    for (int i = 0; i < m_playIndex; ++i)
                    {
                        if (m_effectList.Effects[i].EffectObject)
                        {
                            m_effectList.Effects[i].SetEffectLocalRotation(value);
                        }
                    }
                }
                m_localRotation = value;
            }
        }
    }

    public float PlaybackSpeed      //播放的速率
    {
        get
        {
            return m_playbackSpeed;
        }
        set
        {
            if(value > 0.0f)
            {
                if(m_effectList != null)
                {
                    for(int i = 0; i < m_playIndex; i++)
                    {
                        if (m_effectList.Effects[i].EffectObject)
                        {
                            m_effectList.Effects[i].SetPlaybackSpeed(value);
                        }
                    }
                }
                m_playbackSpeed = value;
            }
        }
    }

    public int Layer
    {
        get
        {
            return m_layer;
        }
        set
        {
            if (value >= 0)
            {
                if(m_effectList != null)
                {
                    for(int i =0; i < m_playIndex; i++)
                    {
                        if (m_effectList.Effects[i].EffectObject)
                        {
                            m_effectList.Effects[i].SetLayer(value);
                        }
                    }
                }
                m_layer = value;
            }
        }
    }

    public bool IsOnce = false;               // 是否一次性(播完即销)
    public bool IsObjectNeedDestroy = false;  // 对应的Object是否需要销毁
    public Vector3 Offset = Vector3.zero;     // 相对于anchor的偏移量
    public EffectPack m_effectPack;

    #endregion Public Field

    #region Private Field


    private EffectList m_effectList;
    private int m_playIndex = 0;             //当前播放序号
    private int m_layer = -1;                //层 
    private float m_playbackSpeed = 1.0f;    //播放的速率
    private float m_scale = 1.0f;            //缩放大小
    private Quaternion m_rotation = new Quaternion(0, 0, 0, 0);
    private Quaternion m_localRotation = new Quaternion(0, 0, 0, 0);

    [SerializeField]
    private Event m_onStart = null;          //开始播放时触发的事件

    [SerializeField]
    private Event m_onStop = null;           //停止播放时触发的事件

    #endregion Private Field

    #region Public Method

    /// <summary>
    /// 添加开始播放时触发的事件
    /// </summary>
    /// <param name="onStart"></param>
    public void AddOnStart(UnityAction<EffectPlayer> onStart)
    {
        if(m_onStart == null)
        {
            m_onStart = new Event();
        }
        m_onStart.AddListener(onStart);       
    }

    /// <summary>
    /// 添加停止播放时触发的事件
    /// </summary>
    /// <param name="onStop"></param>
    public void AddOnStop(UnityAction<EffectPlayer> onStop)
    {
        if (m_onStop == null)
        {
            m_onStop = new Event();
        }
        m_onStop.AddListener(onStop);
    }

    #endregion Public Method

    #region MonoBehaviour Method

    private void Awake()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        if(m_effectList == null)
        {
            if (m_effectPack)
            {
                m_effectList = App.EffectManager.PreLoadEffects(m_effectPack);
            }
        }
    }

    private void OnDisable()
    {
        if (m_effectList != null)
        {
            Stop();
            if (IsOnce)
            {
                Destroy(this);
            }
        }
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    private void Stop()
    {
        if (m_onStop != null)
        {
            m_onStop.Invoke(this);
        }
        m_playIndex = 0;
        PlayedTime = 0.0f;
        for (int i = 0; i < m_effectList.Effects.Count; ++i)
        {
            m_effectList.Effects[i].Stop(m_effectPack.IsFadeOut);
        }
        if (App.EffectManager)
        {
            if (IsOnce && IsObjectNeedDestroy)
            {
                App.EffectManager.DestroyObject(gameObject);
            }          
            App.EffectManager.StopEffects(m_effectList);
        }
        m_effectList = null;
    }

    private void Update()
    {
        if (m_effectList != null)
        {
            PlayedTime += Time.deltaTime * m_playbackSpeed;

            if (m_effectPack.IsLoop || (PlayedTime < m_effectPack.LifeTime))
            {
                if (m_playIndex < m_effectPack.Attributes.Count)
                {
                    for (int i = m_playIndex; (i < m_effectPack.Attributes.Count) && (i < m_effectList.Effects.Count); ++i)
                    {
                        if (PlayedTime >= m_effectPack.Attributes[m_playIndex].DelayTime)
                        {
                            if ((m_playIndex == 0) && (m_onStart != null))
                            {
                                m_onStart.Invoke(this);
                            }
                            m_effectList.Effects[m_playIndex].Play(gameObject, Offset, m_scale, m_rotation, m_playbackSpeed, m_layer, m_effectPack.Attributes[m_playIndex]);
                            ++m_playIndex;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                enabled = false;
            }
        }
    }

    #endregion MonoBehaviour Method
}
