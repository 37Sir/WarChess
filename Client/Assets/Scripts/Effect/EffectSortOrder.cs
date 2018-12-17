using UnityEngine;
using System.Collections;

public class EffectSortOrder : MonoBehaviour
{
    public int relativeOrder = 1;
    public int layer = 5;
    public bool autoSet = false;
    public Vector3 scale = Vector3.one;
    int m_Order = 0;

    public int currentOrder {
        get {
            return m_Order;
        }
    }
    Renderer[] renders;
    void Awake()
    {
        gameObject.layer = layer;
        if(m_Order==0)m_Order = relativeOrder;
        renders = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer render in renders)
        {
            render.gameObject.layer = layer;
            render.sortingOrder = m_Order;
            Vector3 defaultS = render.transform.localScale;
            render.transform.localScale = new Vector3(defaultS.x * scale.x, defaultS.y * scale.y, defaultS.z * scale.z);
        }
    }

    private void OnEnable()
    {
        if (autoSet)
        {
            SetOrder(relativeOrder);
        }
    }

    public void SetOrder(int order){
        m_Order = order;
        if(renders!=null){
            foreach (Renderer render in renders)
            {
                render.sortingOrder = m_Order;
            }
        }
    }
}
