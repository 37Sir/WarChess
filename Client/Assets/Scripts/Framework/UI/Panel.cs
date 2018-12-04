using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class Panel : View
    {
        Canvas m_Canvas;
        UILayer[] m_ChildrenLayer;

        public void UpdateChildrenOrder(int baseOrder)
        {
            m_ChildrenLayer = GetComponentsInChildren<UILayer>(true);

            if (m_ChildrenLayer != null)
            {
                for (int i = 0; i < m_ChildrenLayer.Length; i++)
                {
                    if (m_ChildrenLayer[i] != null)m_ChildrenLayer[i].SetOrder(baseOrder);
                }
            }
        }

        public override void InitView(params object[] args)
        {
            base.InitView(args);
            handle = App.LuaManager.InitializeLuaObject(viewName);
            if (gameObject.activeSelf)
            {
                StartCoroutine(OnInitView(args));
            }
            else
            {
                OnInitViewEnd(args);
            }
        }

        public override void OpenView(params object[] args)
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(OnWaitInitedOpenView(args));
            }
            else
            {
                OnWaitInitOpenViewEnd(args);
            }
        }
        

        public override void CloseView()
        {
            if (handle > 0) App.LuaManager.CallObjectFunction(handle, "CloseView");
        }

        public override void DestroyView()
        {
            if (handle > 0) App.LuaManager.CallObjectFunction(handle, "DestroyView");
            base.DestroyView();
        }

        protected virtual void OnInitViewEnd(params object[] args)
        {
            if (handle > 0) App.LuaManager.CallObjectFunction(handle, "InitView", gameObject, ((bool)args[0] == true) ? handle : -1);
            m_isInited = true;
        }
        protected virtual void OnWaitInitOpenViewEnd(params object[] args)
        {
            if (handle > 0) App.LuaManager.CallObjectFunction(handle, "OpenView", args[0]);
        }

        protected virtual IEnumerator OnInitView(params object[] args)
        {
            Widget[] widgets = GetComponentsInChildren<Widget>();
            //等待widgets初始化完成，panel不主动调用widegs的初始化方法，仅等待
            while (true)
            {
                int inited = 0;
                for (int i = 0; i < widgets.Length; i++)
                {
                    if (widgets[i].isInited) inited++;
                }
                if (inited >= widgets.Length)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
            //运行lua的initview
            OnInitViewEnd(args);
        }

        protected virtual IEnumerator OnWaitInitedOpenView(params object[] args)
        {
            while (true)
            {
                if (isInited)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
            OnWaitInitOpenViewEnd(args);
        }
    }
}
