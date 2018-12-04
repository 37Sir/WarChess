using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework
{
    //对象池
    public class ObjectPoolManager : Manager
    {
        bool m_AutoClean = true;
        private Dictionary<string, PoolObjectState> m_ObjectMap = new Dictionary<string, PoolObjectState>();
        private Dictionary<string, List<PoolInstanceState>> m_ObjectInstanceMap = new Dictionary<string, List<PoolInstanceState>>();
        GameObject m_ObjectPool;
        public bool HasRegisted(string name)
        {
            return m_ObjectMap.ContainsKey(name);
        }
        public void RegisteObject(string name, string resourcePath, int min, int max, float cleanDuration)
        {
            if (m_ObjectMap.ContainsKey(name))
            {
                //Debugger.LogWarning(name+" Has Registed! Ignore...");
                return;
            }
            if (min > max || max <= 0)
            {
                Debugger.LogError("ObjectPool Max must > 0!");
                return;
            }
            PoolObjectState pos = new PoolObjectState();
            pos.min = min;
            pos.max = max;
            pos.cleanDuration = cleanDuration;
            pos.resource = resourcePath;
            pos.lastCleanTime = Time.time;
            pos.name = name;
            m_ObjectMap.Add(name, pos);
            m_ObjectInstanceMap.Add(name, new List<PoolInstanceState>());
            App.ResourceManager.LoadPrefab(resourcePath, (GameObject obj) =>
            {
                if (obj != null)
                {
                    pos.gameObject = (GameObject)obj;
                    for (int i = 0; i < pos.min; i++)
                    {
                        GameObject goI = Instantiate<GameObject>(pos.gameObject);
                        goI.SetActive(false);
                        goI.transform.SetParent(m_ObjectPool.transform);
                        goI.transform.localScale = Vector3.one;
                        goI.transform.localPosition = Vector3.one;
                        PoolInstanceState pis = new PoolInstanceState();
                        goI.name = pos.name;
                        pis.gameObject = goI;
                        pis.name = pos.name;
                        pis.stateId = -1;
                        pis.state = PoolInstanceStateType.Sleep;
                        m_ObjectInstanceMap[name].Add(pis);
                    }
                }
            });
        }

        public void Instantiate(string name, Action<GameObject> callback)
        {
            StartCoroutine(OnInstantiate(name, callback));
        }
 
        public IEnumerator OnInstantiate(string name, Action<GameObject> callback)
        {
            if (m_ObjectMap.ContainsKey(name))
            {
                PoolObjectState pos = m_ObjectMap[name];
                List<PoolInstanceState> poiss = m_ObjectInstanceMap[name];
                int activeCount = 0;
                int maxId = 0;
                PoolInstanceState oldest = null;
                PoolInstanceState sleepOne = null;
                for (int i = 0; i < poiss.Count; i++)
                {
                    if (poiss[i].state == PoolInstanceStateType.Sleep)
                    {
                        sleepOne = poiss[i];
                    }
                    else
                    {
                        activeCount++;
                        if (maxId < poiss[i].stateId)
                        {
                            maxId = poiss[i].stateId;
                        }
                    }
                }
                if (sleepOne != null)
                {
                    sleepOne.stateId = maxId + 1;
                    sleepOne.state = PoolInstanceStateType.Active;
                    sleepOne.gameObject.name = name;
                    if (callback != null) callback(sleepOne.gameObject);
                }
                else
                {
                    if (activeCount >= pos.max)
                    {
                        sleepOne = oldest;
                        sleepOne.stateId = maxId + 1;
                        sleepOne.state = PoolInstanceStateType.Active;
                        sleepOne.gameObject.name = name;
                        if (callback != null) callback(sleepOne.gameObject);
                    }
                    else
                    {
                        while (pos.gameObject == null)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        GameObject goI = Instantiate<GameObject>(pos.gameObject);
                        goI.SetActive(false);
                        goI.transform.SetParent(m_ObjectPool.transform);
                        yield return goI;
                        goI.transform.localScale = Vector3.one;
                        goI.transform.localPosition = Vector3.one;
                        PoolInstanceState pis = new PoolInstanceState();
                        pis.gameObject = goI;
                        pis.name = pos.name;
                        pis.stateId = maxId + 1;
                        pis.state = PoolInstanceStateType.Active;
                        if (m_ObjectInstanceMap.ContainsKey(name))
                        {
                            m_ObjectInstanceMap[name].Add(pis);
                            goI.name = name;
                            if (callback != null) callback(goI);
                        }
                        else
                        {//场景被切换
                            Destroy(goI);
                        }
                    }
                }
            }
            else
            {
                Debugger.LogError("ObjectPool No Registe:" + name);
            }
        }

        public void Release(string name, GameObject instanceObject)
        {
            if (m_ObjectMap.ContainsKey(name))
            {
                PoolObjectState pos = m_ObjectMap[name];
                List<PoolInstanceState> poiss = m_ObjectInstanceMap[name];
                for (int i = 0; i < poiss.Count; i++)
                {
                    if (poiss[i].gameObject == instanceObject)
                    {
                        poiss[i].state = PoolInstanceStateType.Sleep;
                        instanceObject.SetActive(false);
                        instanceObject.transform.SetParent(m_ObjectPool.transform);
                    }
                }

            }
        }

        IEnumerator AutoClean()
        {
            while (m_AutoClean)
            {
                float t = Time.time;
                List<string> needDestroyKey = new List<string>();
                foreach (var item in m_ObjectMap)
                {
                    PoolObjectState pos = item.Value;
                    if (pos.cleanDuration > 0 && t - pos.lastCleanTime >= pos.cleanDuration)
                    {
                        needDestroyKey.Add(item.Key);
                        pos.lastCleanTime = t;
                    }
                }
                for (int j = 0; j < needDestroyKey.Count; j++)
                {
                    string name = needDestroyKey[j];
                    List<PoolInstanceState> needDestroy = new List<PoolInstanceState>();
                    PoolObjectState pos = m_ObjectMap[name];
                    List<PoolInstanceState> poiss = m_ObjectInstanceMap[name];
                    if (poiss.Count > pos.min)
                    {
                        needDestroy.Clear();
                        for (int i = 0; i < poiss.Count; i++)
                        {
                            if (poiss[i].state == PoolInstanceStateType.Sleep)
                            {
                                needDestroy.Add(poiss[i]);
                            }
                            if (poiss.Count <= pos.min + needDestroy.Count) break;
                        }
                        for (int i = needDestroy.Count - 1; i >= 0; i--)
                        {
                            Destroy(needDestroy[i].gameObject);
                            poiss.Remove(needDestroy[i]);

                        }
                        needDestroy.Clear();
                    }
                }
                yield return new WaitForSeconds(1);
            }
        }

        public void StopAutoClean()
        {
            m_AutoClean = false;
        }
        public void StartAutoClean()
        {
            m_AutoClean = true;
            StartCoroutine(AutoClean());
        }

        public void InitObjectPool()
        {
            //创建Pool
            GameObject o = GameObject.Find("ObjectPool");
            if (m_ObjectPool != o)
            {
                Destroy(m_ObjectPool);
                m_ObjectPool = null;
            }
            if (o == null)
            {
                o = new GameObject();
                o.name = "ObjectPool";

            }
            m_ObjectPool = o;
            //m_ObjectPool.transform.parent = gameObject.transform;
            m_ObjectPool.transform.localScale = Vector3.one;
            m_ObjectPool.transform.localPosition = Vector3.zero;
            StartAutoClean();
        }

        public void ReleaseObjectPool()
        {
            m_ObjectInstanceMap.Clear();
            m_ObjectMap.Clear();
            StopAutoClean();
        }

        public override void OnManagerReady()
        {

        }
        public override void OnManagerDestroy()
        {

        }
    }

    public class PoolObjectState
    {
        public string name;
        public int min;
        public int max;
        public float cleanDuration;
        public GameObject gameObject;
        public string resource;
        public float lastCleanTime;
    }

    public class PoolInstanceState
    {
        public string name;
        public int stateId;
        public GameObject gameObject;
        public PoolInstanceStateType state = PoolInstanceStateType.Sleep;
    }

    public enum PoolInstanceStateType
    {
        Active,
        Sleep,
    }

}
