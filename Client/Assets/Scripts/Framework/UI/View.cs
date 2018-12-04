using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class View : MonoBehaviour
    {
        protected bool m_isInited = false;
        public bool isInited
        {
            get
            {
                return m_isInited;
            }
        }
        public string moduleName;
        public string viewName;

        protected int handle = 0;

        public virtual void InitView(params object[] args)
        {      
        }

        public virtual void OpenView(params object[] args)
        {
        }

        public virtual void CloseView()
        {
        }
 
        public virtual void DestroyView()
        {
            m_isInited = false;
            handle = 0;
        }
    }
}
