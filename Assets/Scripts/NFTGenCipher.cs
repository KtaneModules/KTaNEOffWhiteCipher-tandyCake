using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class NFTGenCipher : Cipher
{
    public NFTGenCipher(int moduleID) : base(moduleID) { }

    public NFTColor[,] generatedNFT = new NFTColor[6, 6];

    int[] genSeq;

    public void PerformEncryption(string word)
    {
        Log("Begin Standard Procedure-NFT generating-Money-Laundering Cipher");
        GetSequences(word);
        GenerateNFT();
    }
    private void GetSequences(string word)
    {
        int[] seq1 = word.Select(x => (x - 'A' + 1) % 10).ToArray();
        int[] seq2 = Product(word.Select(x => x - 'A' + 1)).ToString().Select(x => x - '0').ToArray();
        genSeq = seq1.ToArray();
        do
            genSeq = genSeq.Concat(seq2).ToArray();
        while (genSeq.Length < 12);
        Log("First sequence is {0}; the second is {1}. The GENERATING sequence is thus {2}.", seq1.Join(""), seq2.Join(""), genSeq.Join(""));
    }
    private void GenerateNFT()
    {
        NFTColor[] usedColors = new NFTColor[3];
        for (int i = 0; i < 3; i++)
            usedColors[i] = Data.colorTable[i, genSeq[i]];
        int[] mod6Sequence = Enumerable.Range(3, 8).Select(x => (genSeq[x] + 5) % 6).ToArray();
        int[] usedCoords = Enumerable.Range(0, 4).Select(x => Flatten(mod6Sequence[2 * x], mod6Sequence[2 * x + 1])).ToArray();
        Log("The used colors are: {0}.", usedColors.Join(", "));
        Log("The used coordinates are: {0}.", 
            usedCoords.Select(coord => FormatCoord(coord)).Join(", "));
        for (int i = 0; i < 36; i++)
            generatedNFT[i / 6, i % 6] = usedColors[0];
        PaintRectangle(usedCoords[0], usedCoords[1], usedColors[1]);
        PaintRectangle(usedCoords[2], usedCoords[3], usedColors[2]);

        Log("Painted entire grid with {0}.", usedColors[0]);
        Log("Painted rectangle {0}-{1} with color {2}.", FormatCoord(usedCoords[0]), FormatCoord(usedCoords[1]), usedColors[1]);
        Log("Painted rectangle {0}-{1} with color {2}.", FormatCoord(usedCoords[2]), FormatCoord(usedCoords[3]), usedColors[2]);
        Log("The grid after painting rectangles is:");
        LogGrid(generatedNFT.Cast<NFTColor>().Select(x => Data.colorAbbrs[x]), 6, 6);

        bool[] usedPattern = Data.patterns[genSeq[11]];
        for (int i = 0; i < 36; i++)
            if (usedPattern[i])
                generatedNFT[i / 6, i % 6] ^= NFTColor.White;
        
        Log("The grid after inverting the cells given by number 12 is:");
        LogGrid(generatedNFT.Cast<NFTColor>().Select(x => Data.colorAbbrs[x]), 6, 6);
    }

    private void PaintRectangle(int a, int b, NFTColor paintColor)
    {
        int aX = a % 6, aY = a / 6;
        int bX = b % 6, bY = b / 6;
        int width = Math.Abs(aX - bX) + 1, height = Math.Abs(aY - bY) + 1;
        int tlX = Math.Min(aX, bX), tlY = Math.Min(aY, bY);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                generatedNFT[y + tlY, x + tlX] = paintColor;
    }
    private string FormatCoord(int coord)
    {
        return string.Format("({0}, {1})", coord % 6 + 1, coord / 6 + 1);
    }

    private int Product(IEnumerable<int> nums)
    {
        int output = 1;
        foreach (int num in nums)
            output *= num;
        return output;
    }

    private int Flatten(int x, int y)
    {
        return 6 * y + x;
    }

}