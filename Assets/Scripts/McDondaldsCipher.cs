using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class McDonandaldsCipher : Cipher
{
    public McDonandaldsCipher(int moduleID) : base(moduleID) { }
    const int nuggetPrice = 31, macPrice = 41;
    const string alphaConv = "ZABCDEFGHIJKLMNOPQRSTUVWXY";

    public int[] orderNums;

    public string EncryptMcDondalds(string plaintext)
    {
        Log("Begin McDondalds™ Chicken Nugget Big Mac Cipher");
        string[] pairs = SplitToPairs(plaintext);
        orderNums = new int[pairs.Length];
        string output = "";
        for (int i = 0; i < pairs.Length; i++)
            output += _EncryptMcDonaldsPair(pairs[i], out orderNums[i]);
        Log("Cipher output: {0} with order number {1}.", output, orderNums.Join(""));
        return output;
    }
    private string _EncryptMcDonaldsPair(string pair, out int orderNum)
    {
        int nuggetCount = alphaConv.IndexOf(pair[0]);
        int macCount = alphaConv.IndexOf(pair[1]);
        int paidAmount = nuggetPrice * nuggetCount + macPrice * macCount;
        orderNum = paidAmount / 676;
        paidAmount %= 676;
        char firstLetter = alphaConv[paidAmount / 26];
        char secondLetter = alphaConv[paidAmount % 26];
        string output = "" + firstLetter + secondLetter;
        Log("{0}({2}) McNuggets + {1}({3}) Big Macs = {4}Ξ", nuggetCount, macCount, pair[0], pair[1], paidAmount);
        Log("676 * {0} + 26 * {1} + {2} = {3}; Pair is {1}{2} with an order number of {0}.", orderNum, firstLetter, secondLetter, paidAmount);
        return output;
    }
}