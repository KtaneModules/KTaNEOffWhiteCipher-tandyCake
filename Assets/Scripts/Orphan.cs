using System;
using System.Collections.Generic;
using UnityEngine;

public class Orphan
{
    public string name;
    public Swap[] swaps;
    public Orphan(string name, Swap[] swaps)
    {
        this.name = name;
        this.swaps = swaps;
    }
    public void ApplyRotation(ref char[] grid)
    {
        foreach (Swap swap in swaps)
        {
            char temp = grid[swap.a];
            grid[swap.a] = grid[swap.b];
            grid[swap.b] = temp;
        }
    }
}