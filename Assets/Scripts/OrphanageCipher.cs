using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class OrphanageCipher : Cipher
{
    public OrphanageCipher(int moduleID) : base(moduleID) { }
    public string orphanString;
    private char[] _orphanage = "ABCDEFGHIJKLMNOPQRSTUVWYZ".ToCharArray();
    private Orphan[] _usedOrphans;
    private List<int> xIndices = new List<int>();
    public void GenerateOrphans()
    {
        Log("Begin Orphanage Construction");
        orphanString = Enumerable.Range(0, 26).ToArray().Shuffle().Take(4).Select(x => (char)('A' + x)).Join("");
        _usedOrphans = orphanString.Select(ch => Data.orphans[ch]).ToArray();
        foreach (Orphan orphan in _usedOrphans)
        {
            orphan.ApplyRotation(ref _orphanage);
            Log("Introduced {0} to the orphanage. Let us admire the enriched state of the facility.", orphan.name);
            LogGrid(_orphanage, 5, 5);
        }
    }
    public string EncryptLeapfrog(string plaintext)
    {
        Log("Begin Composite Spinning/Jumping Leapfrog Orphanage Cipher");
        string output = "";
        string[] pairs = SplitToPairs(plaintext);
        for (int i = 0; i < 3; i++)
            output += _EncryptLeapfrogPair(pairs[i].ToCharArray(), i);
        output = Enumerable.Range(0, 6).Select(ix => xIndices.Contains(ix) ? 'X' : output[ix]).Join("");

        Log("Cipher output: {0} (note: during encryption, each pair needs to be reversed)", output);
        return output;
    }
    private string _EncryptLeapfrogPair(char[] pair, int ix)
    {
        Log("--------");
        for (int i = 0; i < 2; i++)
            if (pair[i] == 'X')
            {
                Log("Substituting X at index {0} with a Y. It will be changed back to an X after the cipher.", 2 * ix + i + 1);
                pair[i] = 'Y';
                xIndices.Add(2 * ix + i);
            }
        string output = "";
        output += _JumpOver(pair[1], pair[0]);
        output += _JumpOver(pair[0], output[0]);
        return new string(output.Reverse().ToArray());
    }
    private char _JumpOver(char a, char b)
    {
        int aX = Array.IndexOf(_orphanage, a) % 5;
        int aY = Array.IndexOf(_orphanage, a) / 5;
        int bX = Array.IndexOf(_orphanage, b) % 5;
        int bY = Array.IndexOf(_orphanage, b) / 5;

        int xChange = Mod(bX - aX, 5);
        int yChange = Mod(bY - aY, 5);

        int cX = (bX + xChange) % 5;
        int cY = (bY + yChange) % 5;
        char c = _orphanage[5 * cY + cX];
        Log("{0} jumped over {1} = {2}", a, b, c);
        return c;
    }

}