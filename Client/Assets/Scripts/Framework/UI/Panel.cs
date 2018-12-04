using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework
{
    public class Panel : View
    {
        private Canvas m_Canvas;
        private UILayer[] m_ChildrenLayer;
        private Type m_objectType;
        private object m_object;

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            m_object = assembly.CreateInstance(viewName);
            m_objectType = Type.GetType(viewName);

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
            MethodInfo mi = m_objectType.GetMethod("CloseView");
            mi.Invoke(m_object, null);
        }

        public override void DestroyView()
        {
            MethodInfo mi = m_objectType.GetMethod("DestroyView");
            mi.Invoke(m_object, null);
            base.DestroyView();
        }

        protected virtual void OnInitViewEnd(params object[] args)
        {
            MethodInfo mi = m_objectType.GetMethod("InitView");
            mi.Invoke(m_object, null);
            m_isInited = true;
        }
        protected virtual void OnWaitInitOpenViewEnd(params object[] args)
        {
            MethodInfo mi = m_objectType.GetMethod("OpenView");
            mi.Invoke(m_object, null);
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
