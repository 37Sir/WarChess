using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class GameObjectExtension
    {
        #region Public Method
        /// <summary>
        /// 通过名字搜索子物体
        /// </summary>
        /// <param name="gameObject">被搜索的物体</param>
        /// <param name="name">搜索的名字</param>
        /// <returns></returns>
        public static GameObject GetGameObjectByName(this GameObject gameObject, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return gameObject.transform.FindRecursively<GameObject>(transform =>
                    (transform.gameObject.name.Equals(name) ? transform.gameObject : null));
            }
            return null;
        }

        /// <summary>
        /// 在子物体中递归的搜索
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="transform"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        public static TResult FindRecursively<TResult>(this Transform transform, System.Func<Transform, TResult> find)
        {
            TResult result = find(transform);
            if (result != null)
            {
                return result;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                result = FindRecursively<TResult>(transform.GetChild(i), find);
                if (result != null)
                {
                    return result;
                }
            }
            return default(TResult);
        }

        /// <summary>
        /// 用递归的方式设置子物体的层级
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="layer">设置的层数</param>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.transform.DoRecursively(SetLayer, layer);
        }

        /// <summary>
        /// 用递归的方式做某事
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="action">某事</param>
        public static void DoRecursively<T>(this Transform transform, System.Action<Transform, T> action, T arg)
        {
            action(transform, arg);
            for (int i = 0; i < transform.childCount; ++i)
            {
                DoRecursively<T>(transform.GetChild(i), action, arg);
            }
        }

        #endregion Public Method

        #region Private Method
        private static void SetLayer(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
        }
        #endregion Private Method
    }
}
