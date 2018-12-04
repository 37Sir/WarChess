using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class NSceneManager : Manager
    {
        private string loadingScene;//Only Used In Sync
        public string ActiveScene()
        {
            return SceneManager.GetActiveScene().name;
        }

        public void LoadScene(string sceneName)
        {
            loadingScene = sceneName;
            SceneManager.LoadScene(sceneName);
        }

        //public void OnSceneChanged(Scene s1, Scene s2)
        //{
        //    if (loadingScene != null && s2.name == loadingScene)
        //    {
        //        App.LuaManager.CallFunction("SceneLoader." + loadingScene);
        //        loadingScene = null;
        //    }
        //}

        public void LoadSceneAsync(string sceneName, Action<string, float> callback)
        {
            loadingScene = null;
            StartCoroutine(OnLoadScene(sceneName, callback));
        }

        IEnumerator OnLoadScene(string sceneName, Action<string, float> callback)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            float last = 0;
            while (!async.isDone)
            {
                if (callback != null && async.progress != last)
                {
                    callback(sceneName, async.progress);
                    if (async.progress > 0.8) async.allowSceneActivation = true;
                }
                last = async.progress;
                yield return null;
            }
            if (callback != null && async.isDone)
            {
                callback(sceneName, 1);
            }
        }

        //public override void OnManagerReady()
        //{
        //    SceneManager.activeSceneChanged += OnSceneChanged;
        //}
        //public override void OnManagerDestroy()
        //{
        //    SceneManager.activeSceneChanged -= OnSceneChanged;
        //}
    }
}