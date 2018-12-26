using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ModelDrag02 : MonoBehaviour
{
    private Camera cam;//发射射线的摄像机
    private GameObject go;//射线碰撞的物体
    private PieceItem02 m_pieceItem;
    public static string btnName;//射线碰撞物体的名字
    private Vector3 screenSpace;
    private Vector3 offset;
    public bool isTurn = false;
    private bool isDrage = false;

    //public delegate void DelegateOnDrag(Vector3 vector);
    //public delegate void DelegateOnDragEnd(int error, List<byte[]> bytes);

    //private Dictionary<GameObject, DelegateOnDrag> m_OnDragCalls = new Dictionary<GameObject, DelegateOnDrag>();

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (isTurn == true)
        {
            //整体初始位置 
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            ////从摄像机发出到点击坐标的射线
            RaycastHit hitInfo;
            if (isDrage == false)
            {
                if (Physics.Raycast(ray, out hitInfo))
                {
                    //划出射线，只有在scene视图中才能看到
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    go = hitInfo.collider.gameObject;
                    screenSpace = cam.WorldToScreenPoint(go.transform.position);
                    offset = go.transform.position - cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
                    //物体的名字  
                    btnName = go.name;
                }
                else
                {
                    btnName = null;
                }
            }
            if (Input.GetMouseButton(0) && isDrage == false)//开始拖动
            {
                OnDragBegin();
            }
            else if (Input.GetMouseButton(0) && isDrage == true)//正在拖动
            {
                OnMouseDrag();
            }
            else if (Input.GetMouseButtonUp(0) && isDrage == true)//结束拖动
            {
                OnDragEnd();
            }
            else
            {
                isDrage = false;
            }
        }
    }

    private void OnDragBegin()
    {
        if (go != null)
        {
            m_pieceItem = go.GetComponent<PieceItem02>();
            if (m_pieceItem != null)
            {
                m_pieceItem.OnDragBegin();
                isDrage = true;
            }
        }
    }

    private void OnMouseDrag()
    {
        Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
        Vector3 currentPosition = cam.ScreenToWorldPoint(currentScreenSpace) + offset;
        if (btnName != null && m_pieceItem != null)
        {
            m_pieceItem.OnDrag(currentPosition);
        }
    }

    private void OnDragEnd()
    {
        if (m_pieceItem != null)
        {
            m_pieceItem.OnDragEnd();
        }
        isDrage = false;
    }
}