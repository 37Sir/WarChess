using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UILayer : MonoBehaviour
{
    public int relativeOrder = 1;
    public Canvas canvas;

    public void SetOrder(int baseOrder)
    {
        if (canvas != null) canvas.sortingOrder = baseOrder + relativeOrder;
    }
}
