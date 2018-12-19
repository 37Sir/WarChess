using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 特效管理器
    /// </summary>
    public class EffectManager : Manager
    {

        #region Public Field

        /// <summary>
        /// EffectPlayer 的字典
        /// </summary>
        public Dictionary<string, EffectPlayer> m_effectPlayers = new Dictionary<string, EffectPlayer>();

        /// <summary>
        /// 屏幕适配比例
        /// </summary>
        public float ScreenFixedK
        {
            get
            {
                return (float)Screen.height / (float)Screen.width / ((float)UIManager.DefaultScreenHeight / (float)UIManager.DefaultScreenWidth);
            }
        }

        #endregion Public Field

        #region Private Field

        /// <summary>
        /// 特效包待处理列表
        /// </summary>
        private Queue<EffectList> m_loadingEffectList = new Queue<EffectList>();

        /// <summary>
        /// 待销毁列表
        /// </summary>
        private Queue<EffectList> m_destroyingEffectList = new Queue<EffectList>();

        [SerializeField]
        private const float m_fadeTime = 0.5f;
        private int m_maxCoroutineMilliseconds = 10;

        #endregion Private Field

        #region Public Method

        /// <summary>
        /// 初始化EffectManager
        /// </summary>
        public void Init()
        {
            StartCoroutine(AsyncCreateEffects());
            StartCoroutine(AsyncDestroyEffects());
        }

        /// <summary>
        /// 预加载EffectPack
        /// </summary>
        /// <param name="effectPack">要加载的EffectPack</param>
        /// <returns>EffectPack对应的EffectList</returns>
        public EffectList PreLoadEffects(EffectPack effectPack)
        {
            EffectList effectList = new EffectList(effectPack);
            m_loadingEffectList.Enqueue(effectList);
            return effectList;
        }

        /// <summary>
        /// 加载指定名字的EffectPack 并在refObject上创建EffectPlayer组件
        /// </summary>
        /// <param name="refObject">要挂EffectPlayer的物体</param>
        /// <param name="name">要加载EffectPack的名字</param>
        /// <returns></returns>
        public EffectPlayer LoadEffect(GameObject refObject, string name)
        {
            return RegisterEffectPlayer(refObject, name, false);
        }

        /// <summary>
        /// 加载指定名字的EffectPack 在指定位置创建物体 并为其添加EffectPlayer 
        /// </summary>
        /// <param name="position">创建物体的坐标</param>
        /// <param name="rotation">创建物体的角度</param>
        /// <param name="name">要加载EffectPack的名字</param>
        /// <returns></returns>
        public EffectPlayer LoadEffect(Vector3 position, Quaternion rotation, string name)
        {
            GameObject refObject = CreateObject(name);
            refObject.transform.position = position;
            refObject.transform.rotation = rotation;
            return RegisterEffectPlayer(refObject, name, true);
        }

        /// <summary>
        /// 创建EffectPlayer
        /// </summary>
        /// <param name="refObject">EffectPlayer跟随的物体</param>
        /// <param name="effectPack">存有多个特效</param>
        /// <returns></returns>
        public EffectPlayer CreatEffect(GameObject refObject, EffectPack effectPack)
        {
            return RegisterEffectPlayer(refObject, effectPack, false);
        }

        /// <summary>
        /// 指定位置创建EffectPlayer
        /// </summary>
        /// <param name="position">EffectPlayer的坐标</param>
        /// <param name="rotation">EffectPlayer的角度</param>
        /// <param name="effectPack">存有多个特效</param>
        /// <returns></returns>
        public EffectPlayer CreateEffect(Vector3 position, Quaternion rotation, EffectPack effectPack)
        {
            GameObject refObject = CreateObject(name);
            refObject.transform.position = position;
            refObject.transform.rotation = rotation;
            return RegisterEffectPlayer(refObject, effectPack, true);
        }

        /// <summary>
        /// 创建空object
        /// </summary>
        /// <param name="name">object的名字</param>
        /// <returns>创建的Object</returns>
        public GameObject CreateObject(string name)
        {
            GameObject refObject = new GameObject(name);
            refObject.transform.parent = transform;
            refObject.SetActive(true);
            return refObject;
        }

        /// <summary>
        /// 销毁object
        /// </summary>
        /// <param name="gameObject"></param>
        public void DestroyObject(GameObject gameObject)
        {
            if (enabled)
            {
                StartCoroutine(AsnycDestroyGameObject(gameObject));
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <param name="effectList">要停止播放的列表</param>
        public void StopEffects(EffectList effectList)
        {
            effectList.DestroyTime = Time.timeSinceLevelLoad;
            m_destroyingEffectList.Enqueue(effectList);
        }

        /// <summary>
        /// 获取当前object上的所有EffectPlayer
        /// </summary>
        /// <param name="refObject">目标object</param>
        /// <returns>EffectPlayer数组</returns>
        public EffectPlayer[] GetEffectPlayers(GameObject refObject)
        {
            return refObject.GetComponents<EffectPlayer>();
        }

        public EffectPlayer[] GetEffectPlayers(GameObject refObject, string packName)
        {
            EffectPlayer[] effectPlayers = refObject.GetComponents<EffectPlayer>();
            List<EffectPlayer> tempPlayers = new List<EffectPlayer>();
            for(int i = 0; i < effectPlayers.Length; i++)
            {
                if(effectPlayers[i].EffectPack.name == packName)
                {
                    tempPlayers.Add(effectPlayers[i]);
                }                
            }
            return tempPlayers.ToArray();
        }

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// 注册EffectPlayer
        /// </summary>
        /// <param name="refObject">EffectPlayer跟随的物体</param>
        /// <param name="effectPack">和EffectPlayer关联的EffectPack</param>
        /// <returns></returns>
        private EffectPlayer RegisterEffectPlayer(GameObject refObject, EffectPack effectPack, bool isNeedClean)
        {
            EffectPlayer effectPlayer = refObject.AddComponent<EffectPlayer>();
            if (effectPlayer)
            {
                effectPlayer.EffectPack = effectPack;
                effectPlayer.EffectList = PreLoadEffects(effectPack);
                effectPlayer.IsObjectNeedDestroy = isNeedClean;
                return effectPlayer;
            }
            return null;
        }

        /// <summary>
        /// 注册EffectPlayer
        /// </summary>
        /// <param name="name">EffectPack的名字</param>
        /// <param name="refObject">EffectPlayer跟随的物体</param>
        /// <returns></returns>
        private EffectPlayer RegisterEffectPlayer(GameObject refObject, string name, bool isNeedClean)
        {
            EffectPlayer effectPlayer = refObject.AddComponent<EffectPlayer>();
            if (effectPlayer)
            {
                string effectPackPath = Config.EffectPacksLoadRoot + "/" + name;
                App.ResourceManager.LoadResource<EffectPack>(effectPackPath, (EffectPack effectPack) =>
                {
                    if (effectPack != null)
                    {
                        effectPlayer.EffectPack = effectPack;
                        effectPlayer.EffectList = PreLoadEffects(effectPack);
                        effectPlayer.IsObjectNeedDestroy = isNeedClean;
                    }
                });
                return effectPlayer;
            }
            return null;
        }

        /// <summary>
        /// 创建Effect
        /// </summary>
        /// <returns></returns>
        private IEnumerator AsyncCreateEffects()
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            while (true)
            {
                yield return null;
                stopWatch.Reset();
                stopWatch.Start();
                while(m_loadingEffectList.Count > 0)
                {
                    EffectList effectList = m_loadingEffectList.Dequeue();
                    App.SoundManager.RegisterAudioAttrList(effectList.EffectPack.AudioAttrs);                       //播放音效
                    
                    for (int j = effectList.Effects.Count; j < effectList.EffectPack.Attributes.Count; j++)
                    {
                        Effect effect = EffectFactory.CreateEffect();                                               //创建Effect
                        App.ObjectPoolManager.RegisteObject(effectList.EffectPack.Attributes[j].EffectName, effectList.EffectPack.Attributes[j].EffectPath, 0, 30, -1);
                        App.ObjectPoolManager.Instantiate(effectList.EffectPack.Attributes[j].EffectName, (GameObject obj) =>
                        {
                            obj.SetActive(false);
                            effect.Init(obj);
                            effectList.Effects.Add(effect);
                        });
                        if(effectList.EffectPack.IsPlaySync == false)
                        {
                            if (stopWatch.ElapsedMilliseconds > m_maxCoroutineMilliseconds)
                            {
                                stopWatch.Stop();
                                yield return null;
                                stopWatch.Reset();
                                stopWatch.Start();
                            }
                        }
                        effectList.IsLoading = false;
                    }
                }
                stopWatch.Stop();
            }
        }

        /// <summary>
        /// 销毁Effect
        /// </summary>
        /// <returns></returns>
        private IEnumerator AsyncDestroyEffects()
        {
            while (true)
            {
                yield return null;
                if (m_destroyingEffectList.Count > 0)
                {
                    EffectList effectList = m_destroyingEffectList.Dequeue();

                    while (effectList.IsLoading)
                    {
                        yield return null;
                    }

                    while (effectList.EffectPack.IsFadeOut && (m_fadeTime > (Time.timeSinceLevelLoad - effectList.DestroyTime)))
                    {
                        yield return null;
                    }

                    for (int i = 0; i < effectList.Effects.Count; ++i)
                    {
                        DestroyEffect(effectList.Effects[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 销毁Effect
        /// </summary>
        /// <param name="effect"></param>
        private void DestroyEffect(Effect effect)
        {
            effect.Restore();
            DestroyEffectObject(effect.EffectObject);
            EffectFactory.DestroyEffect(effect);
        }

        /// <summary>
        /// 销毁EffectObject
        /// </summary>
        /// <param name="effectObject">粒子特效本体</param>
        private void DestroyEffectObject(GameObject effectObject)
        {
            if (effectObject)
            {
                App.ObjectPoolManager.Release(effectObject.name, effectObject);
            }
        }

        private IEnumerator AsnycDestroyGameObject(GameObject gameObject)
        {
            yield return new WaitForSeconds(m_fadeTime);
            Destroy(gameObject);
        }

        #endregion Private Method

    }
}
