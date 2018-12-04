using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UObject = UnityEngine.Object;
using UnityEngine.U2D;

namespace Framework
{
    public class ResourceManager : Manager
    {

        Dictionary<string, UObject> m_LoadedUObjects = new Dictionary<string, UObject>();

        #region Public Method

        public void LoadPrefab(string assetPath, Action<GameObject> func)
        {
            LoadResource<GameObject>(assetPath, func);
        }

        public void LoadResource<T>(string assetPath, Action<T> func) where T : UObject
        {
            if (m_LoadedUObjects.ContainsKey(assetPath))
            {
                if (func != null)
                {
                    func(m_LoadedUObjects[assetPath] as T);
                    return;
                }
            }
            StartCoroutine(OnLoadAsset(assetPath, (UObject obj) =>
            {
                T go = null;
                if (obj != null)
                {
                    go = obj as T;
                }
                if (go != null && !m_LoadedUObjects.ContainsKey(assetPath))
                {
                    m_LoadedUObjects.Add(assetPath, go);
                }
                if (func != null) func(go);
            }));
        }

        #endregion Public Method


        #region Private Method

        private IEnumerator OnLoadAsset(string assetPath, Action<UObject> action = null)
        {
            ResourceRequest req = Resources.LoadAsync(assetPath, typeof(UObject));
            while (!req.isDone)
            {
                yield return null;
            }
            var obj = req.asset as UObject;
            action(obj);
        }

        #endregion Private Method
    }
}