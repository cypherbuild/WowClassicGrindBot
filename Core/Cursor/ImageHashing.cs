﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Core;

/// <summary>
/// Contains a variety of methods useful in generating image hashes for image comparison
/// and recognition.
/// https://github.com/jforshee/ImageHashing
/// Credit for the AverageHash implementation to David Oftedal of the University of Oslo.
/// </summary>
public static class ImageHashing
{
    /// <summary>
    /// Bitcounts array used for BitCount method (used in Similarity comparisons).
    /// Don't try to read this or understand it, I certainly don't. Credit goes to
    /// David Oftedal of the University of Oslo, Norway for this.
    /// http://folk.uio.no/davidjo/computing.php
    /// </summary>
    private static readonly byte[] bitCounts = {
        0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,1,2,2,3,2,3,3,4,
        2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,
        2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,
        4,5,5,6,5,6,6,7,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
        2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,2,3,3,4,3,4,4,5,
        3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
        4,5,5,6,5,6,6,7,5,6,6,7,6,7,7,8
    };

    /// <summary>
    /// Computes the average hash of an image according to the algorithm given by Dr. Neal Krawetz
    /// on his blog: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html.
    /// </summary>
    /// <param name="image">The image to hash.</param>
    /// <returns>The hash of the image.</returns> 
    [SkipLocalsInit]
    public static unsafe ulong AverageHash(Bitmap image, Bitmap squeezed, Graphics canvas)
    {
        Rectangle rect = new(0, 0, 8, 8);
        canvas.Clear(Color.Transparent);
        canvas.DrawImage(image, rect);

        // Reduce colors to 6-bit grayscale and calculate average color value
        Span<byte> grayscale = stackalloc byte[64];

        const int bytesPerPixel = 4; //Image.GetPixelFormatSize(squeezed.PixelFormat) / 8;
        BitmapData data = squeezed.LockBits(rect, ImageLockMode.ReadOnly, squeezed.PixelFormat);

        uint averageValue = 0;
        for (int i = 0; i < 64; i++)
        {
            byte* pixel = (byte*)data.Scan0 + (data.Stride * i / 8) + (bytesPerPixel * i % 8);
            uint argb = (uint)(pixel[0] | (pixel[1] << 8) | (pixel[2] << 16) | (pixel[3] << 24));
            uint gray = (argb & 0x00ff0000) >> 16;
            gray += (argb & 0x0000ff00) >> 8;
            gray += (argb & 0x000000ff);
            gray >>= 2; // divide by 12

            grayscale[i] = (byte)gray;
            averageValue += gray;
        }
        squeezed.UnlockBits(data);

        averageValue /= 64;

        // Compute the hash: each bit is a pixel
        // 1 = higher than average, 0 = lower than average
        ulong hash = 0;
        for (int i = 0; i < 64; i++)
        {
            if (grayscale[i] >= averageValue)
            {
                hash |= 1UL << (63 - i);
            }
        }

        return hash;
    }

    /// <summary>
    /// Returns a percentage-based similarity value between the two given hashes. The higher
    /// the percentage, the closer the hashes are to being identical.
    /// </summary>
    /// <param name="hash1">The first hash.</param>
    /// <param name="hash2">The second hash.</param>
    /// <returns>The similarity percentage.</returns>
    public static double Similarity(ulong hash1, ulong hash2)
    {
        return (64.0 - (uint)BitOperations.PopCount(hash1 ^ hash2)) * 100.0 / 64.0;
    }
}