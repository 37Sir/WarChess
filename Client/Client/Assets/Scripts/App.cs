using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Framework
{
    public class App
    {
        static App m_instance;
        static GameObject m_GameGlobal;
        static Dictionary<string, object> m_Managers = new Dictionary<string, object>();
        static string m_MainName = "Global";

        GameObject GameGlobal
        {
            get
            {
                if (m_GameGlobal == null)
                {
                    m_GameGlobal = GameObject.Find(m_MainName);

                }
                return m_GameGlobal;
            }
        }

        public App()
        {
            m_GameGlobal = GameObject.Find(m_MainName);
        }

        public static App Instance
        {
            get
            {
                if (m_instance == null) m_instance = new App();
                return m_instance;
            }
        }
        /// <summary>
        /// 添加管理器
        /// </summary>
        public void AddManager(string typeName, object obj)
        {
            if (!m_Managers.ContainsKey(typeName))
            {
                m_Managers.Add(typeName, obj);
            }
        }

        /// <summary>
        /// 添加Unity对象
        /// </summary>
        public T AddManager<T>(string typeName) where T : Component
        {
            object result = null;
            m_Managers.TryGetValue(typeName, out result);
            if (result != null)
            {
                return (T)result;
            }
            Component c = GameGlobal.AddComponent<T>();
            m_Managers.Add(typeName, c);
            return default(T);
        }

        /// <summary>
        /// 获取系统管理器
        /// </summary>
        public T GetManager<T>(string typeName) where T : class
        {
            if (!m_Managers.ContainsKey(typeName))
            {
                return default(T);
            }
            object manager = null;
            m_Managers.TryGetValue(typeName, out manager);
            return (T)manager;
        }

        /// <summary>
        /// 删除管理器
        /// </summary>
        public void RemoveManager(string typeName)
        {
            if (!m_Managers.ContainsKey(typeName))
            {
                return;
            }
            object manager = null;
            m_Managers.TryGetValue(typeName, out manager);
            Type type = manager.GetType();
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                GameObject.Destroy((Component)manager);
            }
            m_Managers.Remove(typeName);
        }

        #region Manager
        public static NetworkManager NetworkManager
        {
            get
            {
                return App.Instance.GetManager<NetworkManager>(ManagerName.Network);
            }
        }

        public static NSceneManager NSceneManager
        {
            get
            {
                return App.Instance.GetManager<NSceneManager>(ManagerName.Scene);
            }
        }
        #endregion
    }

}
