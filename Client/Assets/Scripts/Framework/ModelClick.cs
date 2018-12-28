using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ModelClick : MonoBehaviour
{
    private Camera cam;//发射射线的摄像机
    private GameObject go;//射线碰撞的物体
    private PieceItem m_pieceItem;
    public static string btnName;//射线碰撞物体的名字
    private Vector3 screenSpace;
    private Vector3 offset;
    public bool isTurn = false;
    private bool isDrage = false;
    public PVP02Panel pvp02Panel;

    private GameObject m_qizi;

    //public delegate void DelegateOnDrag(Vector3 vector);
    //public delegate void DelegateOnDragEnd(int error, List<byte[]> bytes);

    //private Dictionary<GameObject, DelegateOnDrag> m_OnDragCalls = new Dictionary<GameObject, DelegateOnDrag>();

    void Start()
    {
        cam = Camera.main;
        m_qizi = GameObject.Find("qizi");
    }

    void Update()
    {
        if (isTurn == true)
        {
            //整体初始位置 
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            ////从摄像机发出到点击坐标的射线
            RaycastHit hitInfo;
            if (Input.GetMouseButtonDown(0))//点击
            {
                LayerMask mask1 = 1 << LayerMask.NameToLayer("Board");
                LayerMask mask2 = 1 << LayerMask.NameToLayer("Animal");
                if (Physics.Raycast(ray, out hitInfo, 100, mask1.value))
                {
                    //划出射线，只有在scene视图中才能看到
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    var worldPos = hitInfo.point;
                    var localPos = m_qizi.transform.InverseTransformPoint(worldPos);
                    OnModelClick(new Vector3(localPos.x + 1, localPos.y + 1, localPos.z + 1));
                }
                else if (Physics.Raycast(ray, out hitInfo, 100, mask2.value))
                {
                    //划出射线，只有在scene视图中才能看到
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    var worldPos = hitInfo.point;
                    var localPos = m_qizi.transform.InverseTransformPoint(worldPos);
                    OnAnimalClick(hitInfo);
                }
            }
        }
    }

    private void OnModelClick(Vector3 clickPoint)
    {
        if (pvp02Panel != null)
        {
            pvp02Panel.OnModelClick(clickPoint);
        }
    }

    private void OnAnimalClick(RaycastHit hitInfo)
    {
        var animalObj = hitInfo.collider.gameObject;
        var animator = animalObj.GetComponent<Animator>();
        animator.Play("Victory");
    }
}