using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EffectFactory
{
    public static Effect CreateEffect()
    {
        return new Effect();
    }

    public static void DestroyEffect(Effect effect)
    {
    }
}
