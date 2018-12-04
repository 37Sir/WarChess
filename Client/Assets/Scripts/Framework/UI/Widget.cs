using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class Widget : View
    {
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
            if (handle > 0) App.LuaManager.CallObjectFunction(handle, "InitView", gameObject);
            m_isInited = true;
        }
        protected virtual void OnWaitInitOpenViewEnd(params object[] args)
        {
            if (handle > 0) App.LuaManager.CallObjectFunction(handle, "OpenView");
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
