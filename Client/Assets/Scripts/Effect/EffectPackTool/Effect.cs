using Assets.Scripts.Utility;
using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Effect
{
    public GameObject EffectObject                       // 特效的object
    {
        get;
        private set;
    }

    protected float m_scaleChange = 1.0f;                //比例的变化量
    private GameObject m_refObject = null;               //EffectPlayer所在的object
    private GameObject m_anchorObject = null;            //要跟随的object
    private EffectPack.Attribute m_attribute = null;     //属性     
    private ParticleSystem[] m_particleSystems = null;   //粒子特效
    private float m_speedChange = 1.0f;                  //播放速率
    private GameObject m_anchor = null;                  //跟随的object
    private int m_layer;                                 //层

    /// <summary>
    /// 初始化Effect
    /// </summary>
    /// <param name="effectObject">特效的object</param>
    /// <param name="audio"></param>
    public void Init(GameObject effectObject)
    {
        EffectObject = effectObject;
        m_layer = EffectObject.layer;
    }

    /// <summary>
    /// 设置特效旋转角度
    /// </summary>
    /// <param name="rotation">旋转的角度</param>
    public void SetEffectRotation(Quaternion rotation)
    {
        EffectObject.transform.rotation = rotation;
    }

    public void SetEffectLocalRotation(Quaternion rotation)
    {
        EffectObject.transform.localRotation = rotation;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public virtual void Reset()
    {
        m_refObject = null;
        m_anchorObject = null;
        m_attribute = null;
        if (m_particleSystems != null)
        {
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                if (m_particleSystems[i])
                {
                    m_particleSystems[i].time = 0.0f;
                    m_particleSystems[i].Clear();
                }
            }
            m_particleSystems = null;
        }
        m_scaleChange = 1.0f;
        m_speedChange = 1.0f;
        m_anchor = null;
        EffectObject = null;
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    /// <param name="refObject"></param>
    /// <param name="offset">偏移量</param>
    /// <param name="scale">缩放</param>
    /// <param name="rotation">旋转角度</param>
    /// <param name="playbackSpeed">播放速率</param>
    /// <param name="layer">层</param>
    /// <param name="attribute">属性</param>
    public void Play(GameObject refObject, Vector3 offset, float scale, Quaternion rotation, float playbackSpeed, int layer, EffectPack.Attribute attribute)
    {
        m_refObject = refObject;
        m_attribute = attribute;
        if (EffectObject)
        {
            if (layer >= 0)
            {
                SetLayer(layer);
            }
            AttachAnchor(offset);
            EffectObject.SetActive(true);
            StartParticle();
            SetScale(scale);
            SetEffectRotation(rotation);
            SetPlaybackSpeed(playbackSpeed);
        }
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    /// <param name="isFadeOut">是否具有减出效果</param>
    public void Stop(bool isFadeOut)
    {
        if (EffectObject)
        {
            StopParticle();
            EffectObject.SetActive(false);
        }
    }

    /// <summary>
    /// 恢复
    /// </summary>
    public virtual void Restore()
    {
        if (EffectObject)
        {
            RestoreParticleScale();
            if (m_anchorObject)
            {
                UnityEngine.Object.Destroy(m_anchorObject);
            }
            //EffectObject.transform.localScale = Vector3.one;
            SetLayer(m_layer);
            EffectObject.SetActive(false);
        }
    }

    /// <summary>
    /// 设置隐藏   todo
    /// </summary>
    /// <param name="isHide"></param>
    public void SetHide(bool isHide)
    {

    }

    /// <summary>
    /// 设置缩放比例
    /// </summary>
    /// <param name="scale">比例</param>
    public virtual void SetScale(float scale)
    {
        if (m_scaleChange != 1.0f)
        {
            RestoreParticleScale();
        }

        m_scaleChange = scale;

        if (m_scaleChange != 1.0f)
        {
            SetParticleScale(scale);
        }
    }

    /// <summary>
    /// 设置层级
    /// </summary>
    /// <param name="layer">层级数</param>
    public void SetLayer(int layer)
    {
        if (layer != EffectObject.layer)
        {
            EffectObject.SetLayerRecursively(layer);
        }
    }

    /// <summary>
    /// 设置播放速率
    /// </summary>
    /// <param name="playbackSpeed"></param>
    public void SetPlaybackSpeed(float playbackSpeed)
    {
        if (m_speedChange != 1.0f)
        {
            RestoreParticleSpeed();
        }
        m_speedChange = playbackSpeed;
        if (m_speedChange != 1.0f)
        {
            SetParticleSpeed();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    private void AttachAnchor(Vector3 offset)
    {
        m_anchor = m_refObject.GetGameObjectByName(m_attribute.Anchor);

        if (m_anchor == null)
        {
            m_anchor = m_refObject;
        }

        EffectObject.transform.position = m_anchor.transform.position + m_anchor.transform.rotation * m_attribute.Offset + offset;
        EffectObject.transform.rotation = m_anchor.transform.rotation;

        if (m_attribute.IsAttach)
        {
            if (m_attribute.IsFixedTranslate || m_attribute.IsFixedRotation || m_attribute.IsFixedScale)
            {
                m_anchorObject = App.EffectManager.CreateObject(m_refObject.name + "-" + m_anchor.name);
                m_anchorObject.transform.position = m_anchor.transform.position;
                m_anchorObject.transform.rotation = m_anchor.transform.rotation;
                m_anchorObject.transform.parent = m_anchor.transform;

                Attacher attacher = m_anchorObject.AddComponent<Attacher>();
                attacher.IsFixedTranslate = m_attribute.IsFixedTranslate;
                attacher.IsFixedRotation = m_attribute.IsFixedRotation;
                attacher.IsFixedScale = m_attribute.IsFixedScale;

                EffectObject.transform.parent = m_anchorObject.transform;
            }
            else
            {
                EffectObject.transform.parent = m_anchor.transform;
            }
        }
    }

    /// <summary>
    /// 开启粒子特效
    /// </summary>
    private void StartParticle()
    {
        m_particleSystems = EffectObject.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < m_particleSystems.Length; ++i)
        {
            ParticleSystem particleSystem = m_particleSystems[i];
            if (particleSystem)
            {
                particleSystem.time = 0.0f;
                particleSystem.Clear();
                particleSystem.Play();
            }
        }
    }

    /// <summary>
    /// 停止粒子特效
    /// </summary>
    private void StopParticle()
    {
        if (m_particleSystems != null)
        {
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                if (m_particleSystems[i])
                {
                    m_particleSystems[i].Stop();
                }
            }
        }
    }

    /// <summary>
    /// 设置粒子特效的比例
    /// </summary>
    private void SetParticleScale(float scale)
    {
        if (m_particleSystems != null)
        {
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                if (m_particleSystems[i])
                {
                    m_particleSystems[i].gameObject.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }
    }

    /// <summary>
    /// 恢复粒子特效的比例
    /// </summary>
    private void RestoreParticleScale()
    {
        if (m_particleSystems != null)
        {
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                if (m_particleSystems[i])
                {
                    m_particleSystems[i].gameObject.transform.localScale = Vector3.one;
                }
            }
        }
    }

    /// <summary>
    /// 设置粒子特效的播放速率
    /// </summary>
    private void SetParticleSpeed()
    {
        if (m_particleSystems != null)
        {
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                if (m_particleSystems[i])
                {
                    ParticleSystem.MainModule main = m_particleSystems[i].main;
                    main.simulationSpeed = m_speedChange;
                }
            }
        }
    }

    /// <summary>
    /// 恢复粒子特效的播放速率
    /// </summary>
    private void RestoreParticleSpeed()
    {
        if (m_particleSystems != null)
        {
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                if (m_particleSystems[i])
                {
                    ParticleSystem.MainModule main = m_particleSystems[i].main;
                    main.simulationSpeed = 1.0f;
                }
            }
        }
    }
}
