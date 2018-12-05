using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UObject = UnityEngine.Object;

namespace Framework
{

    public class UIManager : Manager
    {

        //每层Normal间允许的间隔层数
        public static int UIPanelSpacing = 10;
        //不同等级UI的起始层数
        public static int[] UILayerLevelBaseSpacing = { 0, 2000 };

        public static int DefaultScreenWidth = 640;//默认屏幕宽度
        public static int DefaultScreenHeight = 1136;//默认屏幕高度

        /// <summary>
        /// 屏幕适配比例
        /// </summary>
        public static float ScreenFixedK = 1.0f;

        GameObject m_UIRoot;
        Camera m_UICamera;

        Dictionary<UILayerLevel, int> m_LayersUidMap = new Dictionary<UILayerLevel, int>();
        //存储每类具体的UIlayer数据List
        Dictionary<UILayerType, List<UIPanelLayerData>> m_PanelLayerMap = new Dictionary<UILayerType, List<UIPanelLayerData>>();
        //存储每个UI具体的panel数据的字典
        Dictionary<string, UIPanelData> m_PanelMap = new Dictionary<string, UIPanelData>();

        //UI打开的请求
        List<UIPanelLayerData> m_OpenRequest = new List<UIPanelLayerData>();

        //UI需要隐藏的请求
        List<UIPanelLayerData> m_OpenRequestFinalHide = new List<UIPanelLayerData>();

        //处理具体UI打开的协程
        Coroutine m_OpenRequestDeal = null;

        public Camera UICamera
        {
            get
            {
                return m_UICamera;
            }
        }

        public GameObject UIRoot
        {
            get
            {
                return m_UIRoot;
            }
        }

        public override void OnManagerReady()
        {
            ScreenFixedK = (float)Screen.height / (float)Screen.width / ((float)DefaultScreenHeight / (float)DefaultScreenWidth);
            BindUIControl();
        }
        public override void OnManagerDestroy()
        {

        }

        void ResetLayer()
        {
            //UI分为2个大层级
            m_LayersUidMap.Clear();
            m_LayersUidMap.Add(UILayerLevel.Level0, 1);
            m_LayersUidMap.Add(UILayerLevel.Level1, 1);
            //UI分为3类
            m_PanelLayerMap.Clear();
            m_PanelLayerMap.Add(UILayerType.NormalLayer, new List<UIPanelLayerData>());
            m_PanelLayerMap.Add(UILayerType.OverLayer, new List<UIPanelLayerData>());
            m_PanelLayerMap.Add(UILayerType.TopLayer, new List<UIPanelLayerData>());
            m_PanelMap.Clear();

        }

        //得到当前层最上面的UILayer数据
        UIPanelLayerData GetTopPanel(UILayerType type)
        {
            List<UIPanelLayerData> list = m_PanelLayerMap[type];
            return (list.Count > 0) ? list[list.Count - 1] : null;
        }

        //得到当前层最上面的UI名字
        public string TopPanelName(UILayerType type)
        {
            List<UIPanelLayerData> list = m_PanelLayerMap[type];
            return (list.Count > 0) ? list[list.Count - 1].name : "";
        }

        //绑定并初始化UIControl
        public void BindUIControl()
        {
            this.m_UIRoot = GameObject.Find("UI");
            this.m_UICamera = GameObject.Find("UI/Camera").GetComponent<Camera>();
            ResetCameraSize();
            ReleaseAllPanel();
            App.ObjectPoolManager.InitObjectPool();
        }

        //释放所有的panel
        public void ReleaseAllPanel()
        {
            m_OpenRequest.Clear();
            foreach (var item in m_PanelLayerMap)
            {
                List<UIPanelLayerData> panelLayerList = item.Value;
                for (int j = 0; j < panelLayerList.Count; j++)
                {
                    UIPanelLayerData panelLayer = panelLayerList[j];
                    HidePanel(panelLayer, true);
                }
            }
            ResetLayer();
        }

        //打开panel
        public void OpenPanel(string name)
        {
            UIPanelData panelData = GetPanelData(name);
            if (panelData != null)
            {
                int curUid = m_LayersUidMap[panelData.layerLevel];
                List<UIPanelLayerData> panelLayerList = m_PanelLayerMap[panelData.layerType];
                UIPanelLayerData panelLayerLast = null;

                //在此UI的层中检测 此UI是否已经打开以及是否可以重复打开 
                for (int i = panelLayerList.Count - 1; i >= 0; i--)
                {
                    if (panelLayerList[i].name == name)
                    {
                        if (panelLayerList[i].active)
                        {
                            if (panelData.canRepeat) break;
                            Debugger.LogError("Panel:" + name + " Already Opened. If You Want to Open Many, Please Set 'canRepeat' True in Register.");
                            return;
                        }
                        else
                        {
                            panelLayerLast = panelLayerList[i];
                            break;
                        }
                    }
                }
                if (panelLayerLast == null)
                {
                    //从未打开过，需要将UI的数据加入到 此UI的层次list里面
                    UIPanelLayerData layerData = new UIPanelLayerData();
                    layerData.name = name;
                    layerData.resourceName = panelData.resourceName;
                    layerData.uid = curUid;
                    layerData.baseUid = curUid;
                    //layerData.intent = intent;
                    layerData.level = panelData.layerLevel;
                    panelLayerList.Add(layerData);
                    m_LayersUidMap[layerData.level]++;
                }
                SortPanelLayer();
            }
        }
        //语义化方法
        public void OverPanel(string name)
        {
            OpenPanel(name);
        }
        public void TopPanel(string name)
        {
            OpenPanel(name);
        }

        public void BackPanel()
        {
            string topName = TopPanelName(UILayerType.TopLayer);
            if (topName != string.Empty)
            {
                ClosePanel(topName);
                return;
            }
            topName = TopPanelName(UILayerType.NormalLayer);
            if (topName != string.Empty)
            {
                ClosePanel(topName);
                return;
            }
        }

        void ReopenLastPanel()
        {
            string topName = TopPanelName(UILayerType.TopLayer);
            if (topName != string.Empty)
            {
                OpenPanel(topName);
                return;
            }
            topName = TopPanelName(UILayerType.NormalLayer);
            if (topName != string.Empty)
            {
                OpenPanel(topName);
                return;
            }
        }

        public void BackPanelTo(string name)
        {
            string topName = TopPanelName(UILayerType.TopLayer);
            if (topName != string.Empty)
            {
                if (topName != name)
                {
                    _ClosePanel(topName, false);
                    BackPanelTo(name);
                }
                else
                {
                    ReopenLastPanel();
                }
                return;
            }
            topName = TopPanelName(UILayerType.NormalLayer);
            if (topName != string.Empty && topName != name)
            {
                if (topName != name)
                {
                    _ClosePanel(topName, false);
                    BackPanelTo(name);
                }
                else
                {
                    ReopenLastPanel();
                }
                return;
            }
        }

        //得到指定层的指定UI
        UIPanelLayerData GetPanelLayer(UILayerType layer, string name)
        {
            List<UIPanelLayerData> panelLayerList = m_PanelLayerMap[layer];
            for (int i = panelLayerList.Count - 1; i >= 0; i--)
            {
                if (panelLayerList[i].name == name) return panelLayerList[i];
            }
            return null;
        }

        public void ClosePanel(string panelName)
        {
            _ClosePanel(panelName, true);

        }

        void _ClosePanel(string panelName, bool reopen)
        {
            foreach (var item in m_PanelLayerMap)
            {
                List<UIPanelLayerData> panelLayerList = item.Value;
                for (int j = panelLayerList.Count - 1; j >= 0; j--)
                {
                    UIPanelLayerData panelLayer = panelLayerList[j];
                    if (panelLayer.name == panelName)
                    {
                        HidePanel(panelLayer, true);
                        panelLayerList.RemoveAt(j);
                        m_OpenRequest.Remove(panelLayer);
                        if (reopen) SortPanelLayer();
                        return;
                    }
                }
            }
        }

        public void TopOverLayer(string panelName)
        {
            UIPanelLayerData topPanel = GetTopPanel(UILayerType.NormalLayer);
            UIPanelLayerData overPanel = GetPanelLayer(UILayerType.OverLayer, panelName);
            if (topPanel != null && overPanel != null)
            {
                if (topPanel.topOverLayer.IndexOf(overPanel) == -1)
                {
                    //overPanel.intent = intent;
                    topPanel.topOverLayer.Add(overPanel);
                    SortPanelLayer();
                }
            }
        }

        public void BackOverLayer(string panelName)
        {
            UIPanelLayerData topPanel = GetTopPanel(UILayerType.NormalLayer);
            UIPanelLayerData overPanel = GetPanelLayer(UILayerType.OverLayer, panelName);
            if (topPanel != null && overPanel != null)
            {
                if (topPanel.topOverLayer.IndexOf(overPanel) != -1)
                {
                    topPanel.topOverLayer.Remove(overPanel);
                    SortPanelLayer();
                }
            }
        }

        public void SortPanelLayer()
        {
            //baseUid是UI打开的基础顺序，uid是计算打开层级的基础值，zindex是具体的打开canvas层级
            //对每层UI的uid进行重置
            foreach (var item in m_PanelLayerMap)
            {
                List<UIPanelLayerData> panelLayerList = item.Value;
                for (int j = 0; j < panelLayerList.Count; j++)
                {
                    UIPanelLayerData panelLayer = panelLayerList[j];
                    panelLayer.uid = panelLayer.baseUid;
                }
            }
            List<UIPanelLayerData> normalLayerList = m_PanelLayerMap[UILayerType.NormalLayer];
            List<UIPanelLayerData> overLayerList = m_PanelLayerMap[UILayerType.OverLayer];
            List<UIPanelLayerData> topLayerList = m_PanelLayerMap[UILayerType.TopLayer];
            List<UIPanelLayerData> needShow = new List<UIPanelLayerData>();
            List<UIPanelLayerData> needHide = new List<UIPanelLayerData>();

            int topUid = -1;
            if (normalLayerList.Count > 0)
            {
                UIPanelLayerData topPanelLayer = GetTopPanel(UILayerType.NormalLayer);
                topUid = topPanelLayer.uid;

                //topOverLayer是存储固定浮在一个UI界面上面的界面，例如在选人界面上面的玩家的金币UI
                List<UIPanelLayerData> topPanel = topPanelLayer.topOverLayer;

                //对于悬浮的界面的排序基值进行计算
                for (int i = 0; i < topPanel.Count; i++)
                {
                    topPanel[i].uid = topPanelLayer.uid + 1 + i;
                }

                //对于普通UI层的UI进行显示与隐藏
                for (int i = normalLayerList.Count - 1; i >= 0; i--)
                {
                    if (normalLayerList[i] != topPanelLayer)
                    {
                        needHide.Add(normalLayerList[i]);
                    }
                    else
                    {
                        needShow.Add(normalLayerList[i]);
                    }
                }
            }

            //所有UI的zIndex计算
            foreach (var item in m_PanelLayerMap)
            {
                List<UIPanelLayerData> panelLayerList = item.Value;
                for (int j = 0; j < panelLayerList.Count; j++)
                {
                    UIPanelLayerData panelLayer = panelLayerList[j];
                    panelLayer.zIndex = UIManager.UILayerLevelBaseSpacing[(int)panelLayer.level] + UIManager.UIPanelSpacing * panelLayer.uid;
                }
            }

            //OverLayer进行显示与隐藏计算
            for (int i = overLayerList.Count - 1; i >= 0; i--)
            {
                if (topUid < 0 || topUid < overLayerList[i].uid)
                {
                    needShow.Add(overLayerList[i]);
                }
                else
                {
                    needHide.Add(overLayerList[i]);
                }
            }

            //top层全显示
            for (int i = topLayerList.Count - 1; i >= 0; i--)
            {
                needShow.Add(topLayerList[i]);
            }
            m_OpenRequestFinalHide = needHide;
            for (int i = 0; i < needShow.Count; i++)
            {
                ShowPanel(needShow[i]);
            }
            //////////////////////////////////////////////////////
            if (m_OpenRequestDeal != null)
            {
                StopCoroutine(m_OpenRequestDeal);
                m_OpenRequestDeal = null;
            }
            if (m_OpenRequest.Count > 0 && m_OpenRequestDeal == null)
            {
                m_OpenRequestDeal = StartCoroutine(_DealOpenRequest());
            }
        }

        void HidePanel(UIPanelLayerData panelLayerData, bool destroy = false)
        {
            panelLayerData.active = false;
            if (panelLayerData.gameObject != null)
            {
                if (panelLayerData.panel != null)
                {
                    if (panelLayerData.gameObject.activeSelf) panelLayerData.panel.CloseView();
                    if (destroy) panelLayerData.panel.DestroyView();
                }
                if (destroy)
                {
                    //归还gameobject给对象池
                    App.ObjectPoolManager.Release(panelLayerData.resourceName, panelLayerData.gameObject);
                    panelLayerData.canvas = null;
                    panelLayerData.gameObject = null;
                    panelLayerData.panel = null;
                    //panelLayerData.intent = null;
                    panelLayerData.doInstance = false;
                    CleanLayerUid();
                }
                else
                {
                    panelLayerData.gameObject.SetActive(false);
                }
            }
        }

        void CleanLayerUid()
        {
            Dictionary<UILayerLevel, int> topLayerUid = new Dictionary<UILayerLevel, int>();
            topLayerUid.Add(UILayerLevel.Level0, 1);
            topLayerUid.Add(UILayerLevel.Level1, 1);

            foreach (var item in m_PanelLayerMap)
            {
                UIPanelLayerData panelTopLayer = GetTopPanel(item.Key);
                if (panelTopLayer != null)
                {
                    if (panelTopLayer.baseUid > topLayerUid[panelTopLayer.level])
                    {
                        topLayerUid[panelTopLayer.level] = panelTopLayer.baseUid;
                    }
                }

            }
            m_LayersUidMap = topLayerUid;
        }

        void ShowPanel(UIPanelLayerData panelLayerData)
        {
            panelLayerData.active = true;
            UIPanelData panelData = GetPanelData(panelLayerData.name);
            m_OpenRequest.Add(panelLayerData);
            if (panelLayerData.gameObject == null)
            {
                if (!panelLayerData.doInstance)
                {
                    panelLayerData.doInstance = true;
                    App.ObjectPoolManager.Instantiate(panelLayerData.resourceName, (GameObject obj) =>
                    {
                        panelLayerData.gameObject = obj;
                        panelLayerData.canvas = obj.GetComponent<Canvas>();
                        panelLayerData.panel = obj.GetComponent<Panel>();
                        panelLayerData.canvas.worldCamera = this.m_UICamera;
                        panelLayerData.canvas.sortingOrder = panelLayerData.zIndex;
                        panelLayerData.canvas.planeDistance = 1;
                        panelLayerData.panel.UpdateChildrenOrder(panelLayerData.zIndex);
                        panelLayerData.gameObject.name = panelLayerData.name;
                        panelLayerData.gameObject.transform.SetParent(this.m_UIRoot.transform);
                        panelLayerData.gameObject.transform.localScale = Vector3.one;
                        panelLayerData.gameObject.transform.localPosition = Vector3.zero;
                    });
                }

            }
            else
            {
                panelLayerData.canvas.sortingOrder = panelLayerData.zIndex;
                panelLayerData.panel.UpdateChildrenOrder(panelLayerData.zIndex);
                panelLayerData.gameObject.name = panelLayerData.name;
                panelLayerData.gameObject.transform.SetParent(this.m_UIRoot.transform);
                panelLayerData.gameObject.transform.localScale = Vector3.one;
                panelLayerData.gameObject.transform.localPosition = Vector3.zero;
                bool needOpenView = !panelLayerData.gameObject.activeSelf;
                if (!panelLayerData.inited)
                {
                    panelLayerData.inited = true;
                    if (panelLayerData.panel != null && !panelLayerData.panel.isInited)
                    {
                        panelLayerData.gameObject.SetActive(true);
                        panelLayerData.panel.InitView(panelData.canRepeat);
                        needOpenView = true;
                    }
                }
                if (needOpenView)
                {
                    panelLayerData.gameObject.SetActive(true);
                    if (panelLayerData.panel != null)
                    {
                        panelLayerData.panel.OpenView();
                        //panelLayerData.intent = null;
                    }
                }
            }
        }

        IEnumerator _DealOpenRequest()
        {
            while (m_OpenRequest.Count > 0)
            {
                UIPanelLayerData panelLayerData = m_OpenRequest[0];
                if (panelLayerData.gameObject != null)
                {
                    UIPanelData panelData = GetPanelData(panelLayerData.name);
                    bool needOpenView = !panelLayerData.gameObject.activeSelf;
                    if (!panelLayerData.inited)
                    {
                        panelLayerData.inited = true;
                        if (panelLayerData.panel != null && !panelLayerData.panel.isInited)
                        {
                            panelLayerData.gameObject.SetActive(true);
                            panelLayerData.panel.InitView(panelLayerData.gameObject);
                            needOpenView = true;
                        }
                    }
                    while (!panelLayerData.panel.isInited)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    if (needOpenView)
                    {
                        panelLayerData.gameObject.SetActive(true);
                        if (panelLayerData.panel != null)
                        {
                            panelLayerData.panel.OpenView();
                            //panelLayerData.intent = null;
                        }
                    }
                    if (!panelLayerData.active)
                    {
                        HidePanel(panelLayerData);
                    }
                    m_OpenRequest.Remove(panelLayerData);
                }
                yield return new WaitForEndOfFrame();
            }
            if (m_OpenRequestFinalHide != null)
            {
                for (int i = 0; i < m_OpenRequestFinalHide.Count; i++)
                {
                    HidePanel(m_OpenRequestFinalHide[i]);
                }
            }
            m_OpenRequestFinalHide = null;
            m_OpenRequestDeal = null;
        }

        public void RegisterPanel(string module, string name, UILayerType layerType, bool canRepeat)
        {
            UIPanelData panelData = new UIPanelData(module, name, layerType, canRepeat);
            if (!m_PanelMap.ContainsKey(name))
            {
                m_PanelMap.Add(name, panelData);
            }
            else
            {
                m_PanelMap[name] = panelData;
            }
            App.ObjectPoolManager.RegisteObject(panelData.resourceName, panelData.resourceName, 0, 30, -1);
        }

        UIPanelData GetPanelData(string name)
        {
            if (m_PanelMap.ContainsKey(name)) return m_PanelMap[name];
            Debugger.LogError("Panel:" + name + " Do not Exist!");
            return null;
        }

        public string GetPanelResourceName(string panel)
        {
            UIPanelData data = GetPanelData(panel);
            if (data != null)
            {
                return data.resourceName;
            }
            return "";
        }

        void ResetCameraSize()
        {
            int ScreenWidth = Screen.width;
            int ScreenHeight = Screen.height;
            float k1 = (float)ScreenHeight / (float)ScreenWidth;
            float k = (float)DefaultScreenHeight / (float)DefaultScreenWidth;
            float realUIHeight = DefaultScreenHeight;
            if (k1 > k)
            {
                realUIHeight = k1 * DefaultScreenWidth;
            }
            m_UICamera.orthographicSize = realUIHeight / 2 / 100;
        }
    }

    //UI三层
    public enum UILayerType
    {
        NormalLayer = 0,
        OverLayer = 1,
        TopLayer = 2,
    }
    //UI优先等级
    public enum UILayerLevel
    {
        Level0 = 0,
        Level1 = 1,
    }


    //panel的数据
    class UIPanelData
    {
        //ui的名字
        public string name
        {
            get
            {
                return m_Name;
            }
        }
        //prefab的地址
        public string resourceName
        {
            get
            {
                return m_ResourceName;
            }
        }
        string m_Name;
        string m_ResourceName;
        string m_ModuleName;
        public UILayerType layerType;
        public bool canRepeat;
        public UILayerLevel layerLevel;

        public UIPanelData(string module, string name, UILayerType layerType, bool canRepeat = false)
        {
            this.m_Name = name;
            this.m_ResourceName = Config.UIBaseDir + "/" + module + "/" + name;
            this.layerType = layerType;
            this.canRepeat = canRepeat;
            UILayerLevel level = UILayerLevel.Level0;
            switch (layerType)
            {
                case UILayerType.NormalLayer:
                    level = UILayerLevel.Level0;
                    break;
                case UILayerType.OverLayer:
                    level = UILayerLevel.Level0;
                    break;
                case UILayerType.TopLayer:
                    level = UILayerLevel.Level1;
                    break;
                default:
                    level = UILayerLevel.Level0;
                    break;
            }
            this.layerLevel = level;
        }
    }

    class UIPanelResource
    {
        public GameObject resource;
        public string path;
    }

    //层级数据
    class UIPanelLayerData
    {
        public string name;
        public string resourceName;
        public int uid;
        public GameObject gameObject;
        public int zIndex;
        public int baseUid;
        public bool active;
        public Canvas canvas;
        public Panel panel;
        public bool inited;
        public bool doInstance;
        public List<UIPanelLayerData> topOverLayer = new List<UIPanelLayerData>();
        public UILayerLevel level;
    }
}
