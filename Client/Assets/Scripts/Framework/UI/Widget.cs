using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework
{
    public class Widget : View
    {
        private Type m_objectType;
        private object m_object;

        public override void InitView(params object[] args)
        {
            base.InitView(args);
            Assembly assembly = Assembly.GetExecutingAssembly();
            m_object = assembly.CreateInstance(viewName);
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
            while (true)
            {
                int inited = 0;
                for (int i = 0; i < widgets.Length; i++)
                {
                    if (widgets[i].isInited) inited++;
                }
                if (inited >= widgets.Length-1)
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
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
