using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour
{
    public virtual void OnManagerDestroy()
    {

    }
    public virtual void OnManagerReady()
    {

    }
    private void OnDestroy()
    {
        OnManagerDestroy();
    }
    private void Awake()
    {
        OnManagerReady();
    }
}