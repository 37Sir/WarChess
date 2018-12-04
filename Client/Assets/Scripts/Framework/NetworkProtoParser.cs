using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class NetworkProtoParser{

    public static byte[] SizeToVariant32(byte[] data)
    {
        // convert size to variant32
        MemoryStream ms = new MemoryStream(5);
        uint size = (uint)data.Length + 1;
        while (size > 127)
        {
            ms.WriteByte((byte)((size & 0x7f) | 0x80));
            size >>= 7;
        }
        ms.WriteByte((byte)size);
        return ms.ToArray();
    }
}
