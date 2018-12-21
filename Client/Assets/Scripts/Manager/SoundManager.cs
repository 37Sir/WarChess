using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UObject = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// 声音管理器
    /// </summary>
    public class SoundManager : Manager
    {
        #region Private Field

        private AudioSource m_globalAudio;                                                                     //全局背景音乐      
        private Dictionary<string, AudioClip> m_sounds = new Dictionary<string, AudioClip>();                  //声音缓存
        private Dictionary<string, AudioSource> m_audioSources = new Dictionary<string, AudioSource>();
        private List<AudioSource> m_pauseList = new List<AudioSource>();                                       //暂停列表
        private Queue<List<EffectPack.AudioAttr>> m_loadingClipList = new Queue<List<EffectPack.AudioAttr>>(); //待播放的音效列表
        private float m_maxCoroutineMilliseconds = 20;                                                         //加载时间                                         
        private string m_backSoundKey = "";

        #endregion Private Field

        #region MonoBehaviour Method

        public void Awake()
        {
            StartCoroutine(AsyncPlaySoundClip());
        }

        public void Start()
        {
            m_globalAudio = GetComponent<AudioSource>();
        }

        #endregion MonoBehaviour Method

        #region Public Method

        /// <summary>
        /// 添加声音
        /// </summary>
        /// <param name="name">音频资源名</param>
        /// <param name="audio">对应的音频组件</param>
        public void RegisteAudioSource(string name, AudioSource audio)
        {
            if (m_audioSources.ContainsKey(name))
            {
                Debugger.LogDebug(name + " has registed!");
            }
            m_audioSources.Add(name, audio);
        }

        ///添加声音 需要传入gameObject 自动添加组件
        public void RegisteAudioSource(string name, string abName, string type, GameObject gameObject, bool isPlay = true)
        {
            if (!m_audioSources.ContainsKey(name))
            {
                AudioSource audio = gameObject.AddComponent<AudioSource>();
                if(isPlay == false)
                {
                    audio.playOnAwake = false;
                }
                audio.clip = GetAudioClip(abName, type);
                m_audioSources.Add(name, audio);
            }
            else
            {
                Debugger.LogDebug("name" + " is exists");
            }
        }

        ///根据名字查找声音
        public AudioSource FindAudioByName(string name)
        {
            if (m_audioSources.ContainsKey(name))
            {
                return m_audioSources[name];
            }
            else
            {
                Debugger.LogDebug(name + " not find");
                return null;
            }
        }

        ///根据名字关闭声音
        public void StopAudioByName(string name)
        {
            if (m_audioSources.ContainsKey(name))
            {
                m_audioSources[name].Stop();
            }
            else
            {
                Debugger.LogDebug(name + " not find");
            }
        }

        ///停止播放所有声音
        public void StopAllAudio()
        {
            foreach(AudioSource audio in m_audioSources.Values)
            {
                audio.Stop();
                
            }
            Debugger.LogDebug("Audios all closed");
        }

        ///暂停播放所有声音
        public void PauseAllAudio()
        {
            foreach (AudioSource audio in m_audioSources.Values)
            {
                if (audio.isPlaying)
                {
                    audio.Pause();
                    m_pauseList.Add(audio);
                }
            }
            Debugger.LogDebug("Audios all paused");
        }

        ///继续播放指定名字的声音
        public void ResumeAudioByName(string name)
        {
            if (m_audioSources.ContainsKey(name))
            {
                m_audioSources[name].Play();
                m_pauseList.Remove(m_audioSources[name]);
            }
            else
            {
                Debugger.LogDebug(name + " not find");
            }
        }

        ///继续播放所有被暂停的声音
        public void ResumeAllAudio()
        {
            foreach (AudioSource audio in m_pauseList)
            {
                audio.Play();
            }
            m_pauseList.Clear();
            Debugger.LogDebug("Audios all resume");
        }

        ///根据名字播放声音
        public void PlayAudioByName(string name)
        {
            if (m_audioSources.ContainsKey(name))
            {
                m_audioSources[name].Play();
            }
            else
            {
                Debugger.LogDebug(name + " not find");
            }
        }

        ///根据名字移除声音
        public void RemoveAudioByName(string name)
        {
            if (m_audioSources.ContainsKey(name))
            {
                Destroy(m_audioSources[name]);
                m_audioSources.Remove(name);
            }
            else
            {
                Debugger.LogDebug(name + " not find");
            }
        }

        ///移除所有声音
        public void RemoveAllAudio()
        {
            foreach (AudioSource audio in m_audioSources.Values)
            {
                Destroy(audio);    
            }
            m_audioSources.Clear();
            Debugger.LogDebug("Audios all destroy");
        }

        /// 获取声音资源
        public AudioClip GetAudioClip(string abName, string type)
        {
            string key = abName + "." + type;
            if (!m_sounds.ContainsKey(key))
            {
                string soundPath = Config.SoundsRoot + "/" + abName + "." + type;
                //todo
                //AudioClip audioClip = App.ResourceManager.LoadResourceSync<AudioClip>(soundPath);
                //if (audioClip == null)
                //{
                //    Debugger.LogDebug("Can't find file:" + abName + "." + type);
                //}
                //else
                //{
                //    m_sounds.Add(key, audioClip);
                //}              
                //return audioClip;
                return null;
            }
            else
            {
                return m_sounds[key] as AudioClip;
            }            
        }

        ///播放音效 默认位置
        public void PlaySoundClip(string abName, string type = "mp3")
        {
            Get(abName, (clip, key) =>
            {
                if (clip == null)
                    return;
                if (Camera.main == null)
                    return;
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            });
        }

        ///播放音效 默认位置
        public void PlaySoundClip(string abName, float delay, string type = "mp3")
        {
            Get(abName, (clip, key) =>
            {
                if (clip == null)
                    return;
                if (Camera.main == null)
                    return;
                StartCoroutine(StartPlayClip(delay, clip, Camera.main.transform.position));
            });
        }

        IEnumerator StartPlayClip(float delay, AudioClip clip, Vector3 pos)
        {
            yield return new WaitForSeconds(delay);
            AudioSource.PlayClipAtPoint(clip, pos);
        }

        ///播放音效 指定位置
        public void PlaySoundClip(string abName, Vector3 point, string type = "mp3")
        {
            Get(abName, (clip, key) =>
            {
                if (clip == null)
                    return;
                if (point == null)
                    return;
                AudioSource.PlayClipAtPoint(clip, point);
            });
        } 

        /// <summary>
        /// 注册要播放的音效信息
        /// </summary>
        /// <param name="audioAttrs">音效参数</param>
        public void RegisterAudioAttrList(List<EffectPack.AudioAttr> audioAttrs)
        {
            m_loadingClipList.Enqueue(audioAttrs);
        }

        ///播放背景音乐
        public void PlayBacksound(string abName, string type = "mp3", bool isLoop = true)
        {
            m_backSoundKey = abName + "." + type;
            Get(abName, (clip, key) =>
            {
                if (clip == null)
                    return;
                m_globalAudio.loop = isLoop;
                m_globalAudio.clip = clip;
                m_globalAudio.Play();
            });
        }

        ///停止播放背景音乐
        public void StopBackSound()
        {
            m_backSoundKey = "";
            m_globalAudio.Stop();
        }

        #endregion Public Method

        #region Private Method

        ///回调函数原型
        private delegate void GetBack(AudioClip clip, string key);

        ///获取声音资源
        private void Get(string abName, string type, GetBack cb)
        {
            string key = abName + "." + type;
            if (!m_sounds.ContainsKey(key))
            {
                LoadAudioSource(abName, type, (obj) =>
                {
                    if (obj == null)
                    {
                        Debugger.LogDebug("PlayBackSound fail");
                        cb(null, key);
                        return;
                    }
                    else
                    {
                        if (!m_sounds.ContainsKey(key))
                        {
                            m_sounds.Add(key, obj);
                            cb(obj as AudioClip, key);
                            return;
                        }
                        else
                        {
                            cb(m_sounds[key] as AudioClip, key);
                            return;
                        }
                    }
                });
            }
            else
            {
                cb(m_sounds[key] as AudioClip, key);
                return;
            }
        }

        ///获取声音资源
        private void Get(string clipName, GetBack cb)
        {
            string key = clipName;
            if (!m_sounds.ContainsKey(key))
            {
                LoadAudioSource(clipName, (obj) =>
                {
                    if (obj == null)
                    {
                        Debugger.LogDebug("PlayBackSound fail");
                        cb(null, key);
                        return;
                    }
                    else
                    {
                        if (!m_sounds.ContainsKey(key))
                        {
                            m_sounds.Add(key, obj);
                            cb(obj as AudioClip, key);
                            return;
                        }
                        else
                        {
                            cb(m_sounds[key] as AudioClip, key);
                            return;
                        }
                    }
                });
            }
            else
            {
                cb(m_sounds[key] as AudioClip, key);
                return;
            }
        }

        /// 载入音效资源
        private void LoadAudioSource(string abName, string type, Action<AudioClip> func)
        {
            string soundPath = Config.SoundsRoot + "/" + abName + "." + type;
            App.ResourceManager.LoadResource<AudioClip>(soundPath, func);
        }        
        
        /// 载入音效资源
        private void LoadAudioSource(string abName, Action<AudioClip> func)
        {
            string soundPath = Config.SoundsRoot + "/" + abName;
            App.ResourceManager.LoadResource<AudioClip>(soundPath, func);
        }

        ///播放音效 默认位置
        private IEnumerator PlaySoundClip(string clipName, float delayTime)
        {
            float playedTime = 0;
            while(playedTime < delayTime)
            {
                playedTime += Time.deltaTime;
                yield return null;
            }           
            Get(clipName, (clip, key) =>
            {
                if (clip == null)
                    return;
                if (Camera.main == null)
                    return;
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            });
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <returns></returns>
        private IEnumerator AsyncPlaySoundClip()
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            while (true)
            {
                yield return null;
                stopWatch.Reset();
                stopWatch.Start();
                if (m_loadingClipList.Count > 0)
                {
                    List<EffectPack.AudioAttr> audioAttrs = m_loadingClipList.Dequeue();
                    for(int i = 0; i < audioAttrs.Count; i++)
                    {
                        string fileName = audioAttrs[i].AudioName + "." + audioAttrs[i].AudioType;
                        StartCoroutine(PlaySoundClip(fileName, audioAttrs[i].DelayTime));
                        if (stopWatch.ElapsedMilliseconds > m_maxCoroutineMilliseconds)
                        {
                            stopWatch.Stop();
                            yield return null;
                            stopWatch.Reset();
                            stopWatch.Start();
                        }
                    }
                }
                stopWatch.Stop();
            }
        }

        #endregion Private Method
    }
}