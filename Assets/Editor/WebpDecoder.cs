using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WebPDecoder
{
    [DllImport("webpDECODER",CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr DecodeWebP(
        byte[] data,
        int dataSize,
        out int width,
        out int height
    );

    [DllImport("webpDECODER", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeWebP(IntPtr ptr);

    public static Texture2D Decode(byte[] data)
    {
        IntPtr ptr = DecodeWebP(data, data.Length, out int w, out int h);
        if (ptr == IntPtr.Zero)
            return null;

        int size = w * h * 4;
        byte[] raw = new byte[size];
        Marshal.Copy(ptr, raw, 0, size);
        FreeWebP(ptr);

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.LoadRawTextureData(raw);
        tex.Apply();
        return tex;
    }
}
