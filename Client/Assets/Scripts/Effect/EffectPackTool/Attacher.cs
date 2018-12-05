using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Link a GameObject to another GameObject but the transform can be fixed.
/// </summary>
[AddComponentMenu("Gamemag/Misc/Attacher")]
public sealed class Attacher : MonoBehaviour
{
    #region Public Field

    /// <summary>
    /// Is the position fixed?
    /// </summary>
    public bool IsFixedTranslate = false;

    /// <summary>
    /// Is the rotation fixed?
    /// </summary>
    public bool IsFixedRotation = false;

    /// <summary>
    /// Is the scale fixed?
    /// </summary>
    public bool IsFixedScale = false;

    #endregion Public Field

    #region Private Field

    private Vector3 _position;
    private Quaternion _rotation;

    #endregion Private Field

    #region MonoBehaviour Method

    private void Start()
    {
        _position = transform.position;
        _rotation = transform.rotation;
    }

    private void Update()
    {
        if (transform.parent)
        {
            if (IsFixedTranslate)
            {
                transform.position = _position;
            }
            else if (IsFixedRotation)
            {
                transform.rotation = _rotation;
            }

            if (IsFixedScale)
            {
                transform.localScale = new Vector3(1.0f / transform.parent.lossyScale.x,
                    1.0f / transform.parent.lossyScale.y, 1.0f / transform.parent.lossyScale.z);
            }
        }
    }

    #endregion MonoBehaviour Method
}
