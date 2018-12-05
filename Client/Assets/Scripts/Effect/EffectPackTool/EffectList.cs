using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class EffectList
{
    public List<Effect> Effects
    {
        get;
        set;
    }

    public bool IsLoading
    {
        get;
        set;
    }

    public EffectPack EffectPack
    {
        get;
        set;
    }

    public float DestroyTime
    {
        get;
        set;
    }

    public EffectList(EffectPack effectPack)
    {
        EffectPack = effectPack;
        Effects = new List<Effect>();
        IsLoading = true;
        DestroyTime = 0.0f;
    }
}
