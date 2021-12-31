using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Cipher
{
    private int _moduleID;
    public Cipher(int moduleID)
    {
        _moduleID = moduleID;
    }

    protected void Log(string message, params object[] args)
    {
        Debug.LogFormat("[Off-White Cipher #{0}] {1}", _moduleID, string.Format(message, args));
    }
    protected void LogGrid(IEnumerable<char> grid, int width, int height)
    {
        for (int row = 0; row < height; row++)
            Log(grid.Skip(height * row).Take(width).Join());
    }
    protected int Mod(int a, int modulus)
    {
        return (a % modulus + modulus) % modulus;
    }
    protected string[] SplitToPairs(string message)
    {
        return Enumerable.Range(0, message.Length / 2).Select(x => message.Substring(2 * x, 2)).ToArray();

    }
}