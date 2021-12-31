using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class OffWhiteCipherScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public KMSelectable[] arrows, keys, tiles;
    public KMSelectable submit;
    public TextMesh[] screenTexts, scrollTexts;
    public Transform[] screenTFs, tileTFs;
    public MeshRenderer[] colorDiscs;
    public Transform scrollBox;
    public TextMesh sub;

    private bool[] arrsPressed = new bool[2], placedKeys = new bool[18], becameTiles = new bool[36];
    private bool transformed, animating;
    private bool nyoom;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private int[] colorKeys = Enumerable.Range(9, 8).ToArray();
    private int[] mitosisingKeys = Enumerable.Range(0, 26).Where(x => !Enumerable.Range(9, 8).Contains(x)).ToArray();
    private NFTColor[] buttonColorOrder;
    private NFTColor currentColor;

    private string solutionWord;
    private NFTColor[] generatedNFT, enteredNFT = new NFTColor[36];
    private string[] solveMessage = new string[3];
    private string[] displayedWords = new string[3];

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 2; i++)
        {
            int ix = i;
            arrows[ix].OnInteract += delegate () { StartCoroutine(ArrShrink(arrows[ix].transform, ix, true)); return false; };
        }
        foreach (KMSelectable key in keys)
            key.OnInteract += delegate () { Click(key); return false; };
        submit.OnInteract += delegate () { SubmitPress(); return false; };
    }

    void Start()
    {
        GenerateSolution();

        for (int i = 0; i < 36; i++)
            tiles[i].gameObject.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            screenTexts[i].text = displayedWords[i];
            scrollTexts[i].text = displayedWords[i];
        }
    }

    void GenerateSolution()
    {
        solutionWord = Data.allWords.PickRandom();
        Log("The generated word is {0}.", solutionWord);

        McDonandaldsCipher step2 = new McDonandaldsCipher(moduleId);
        OrphanageCipher step1 = new OrphanageCipher(moduleId);
        NFTGenCipher step3 = new NFTGenCipher(moduleId);


        string mcdonaldOutput = step2.EncryptMcDondalds(solutionWord);
        step1.GenerateOrphans();
        displayedWords[0] = step1.EncryptLeapfrog(mcdonaldOutput);
        displayedWords[1] = step1.orphanString;
        displayedWords[2] = step2.orderNums.Join("");

        step3.PerformEncryption(solutionWord);
        generatedNFT = step3.generatedNFT.Cast<NFTColor>().ToArray();
    }

    void SubmitPress()
    {
        submit.AddInteractionPunch();
        Audio.PlaySoundAtTransform("KeyboardPress", submit.transform);
        if (moduleSolved || animating)
            return;
        if (!transformed)
            InitiateAnimation();
        else if (generatedNFT.SequenceEqual(enteredNFT)) //Check inputted NFT
            Solve();
        else
            Strike();
    }
    void Click(KMSelectable key)
    {
        if (!transformed)
        {
            key.AddInteractionPunch(0.2f);
            Audio.PlaySoundAtTransform("KeyboardPress", key.transform);
        }
    }
    void TilePress(int ix)
    {
        tiles[ix].AddInteractionPunch(0.1f);
        Audio.PlaySoundAtTransform("beep", tiles[ix].transform);
        enteredNFT[ix] = currentColor;
        tiles[ix].GetComponent<MeshRenderer>().material.color = Data.colorLookup[currentColor];
    }
    void InitiateAnimation()
    {
        transformed = true;
        animating = true;
        StartCoroutine(ArrShrink(arrows[0].transform, 0, false));
        StartCoroutine(ArrShrink(arrows[1].transform, 1, false));
        StartCoroutine(ShrinkEachScreen());
        StartCoroutine(ClearKeyTexts());
        StartCoroutine(MoveSubmit());
        StartCoroutine(PlaceEachColor());
        StartCoroutine(PlaceEachTile());
        StartCoroutine(ActivateScrollingScreen(1));
    }

    void Solve()
    {
        moduleSolved = true;
        Audio.PlaySoundAtTransform("chaching", transform);
        Audio.PlaySoundAtTransform("orphanbeat", transform);
        solveMessage[0] = "SOLD!";
        solveMessage[1] = string.Format("{0:n0}", Rnd.Range(1, 10000)) + "Ξ";
        solveMessage[2] = "";
        Module.HandlePass();
    }
    void Strike()
    {
        Module.HandleStrike();
        Audio.PlaySoundAtTransform("StrikeSFX", transform);
    }
    void SetCurrent(NFTColor color, KMSelectable btn)
    {
        currentColor = color;
        btn.AddInteractionPunch(0.2f);
        if ((color & NFTColor.Red) != 0)
            Audio.PlaySoundAtTransform("RPress", btn.transform);
        if ((color & NFTColor.Green) != 0)
            Audio.PlaySoundAtTransform("GPress", btn.transform);
        if ((color & NFTColor.Blue) != 0)
            Audio.PlaySoundAtTransform("BPress", btn.transform);

    }
    IEnumerator ShrinkEachScreen()
    {
        int[] order = Enumerable.Range(0, 3).ToArray().Shuffle();
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(ShrinkScreenPt1(screenTFs[order[i]]));
            yield return new WaitForSeconds(0.125f);
        }
    }
    IEnumerator ShrinkScreenPt1(Transform tf)
    {
        float delta = 0;
        const float duration = 0.75f;
        Vector3 startScl = tf.localScale;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            yield return null;
            tf.localScale = Vector3.Lerp(startScl, new Vector3(startScl.x, 0.001f, 0.002f), Easing.InQuart(delta, 0, 1, 1));
        }
        Audio.PlaySoundAtTransform("snap", tf);
        yield return ShrinkScreenPt2(tf);
    }
    IEnumerator ShrinkScreenPt2(Transform tf)
    {
        yield return null;
        float delta = 0;
        const float duration = 0.5f;
        Vector3 startScl = tf.localScale;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            yield return null;
            Vector3 newV = Vector3.Lerp(startScl, new Vector3(0, 0.001f, startScl.z), Easing.InCirc(delta, 0, 1, 1));
            tf.localScale = newV.ToString().Contains('N') ? Vector3.zero : newV; // I am really desperate here. Other ways of checking for the vector being NaN just don't work???
        }
        Audio.PlaySoundAtTransform("pop", tf);
        tf.gameObject.SetActive(false);
    }
    IEnumerator ClearKeyTexts()
    {
        int[] order = Enumerable.Range(0, 26).ToArray().Shuffle();
        for (int i = 0; i < 26; i++)
        {
            keys[order[i]].GetComponentInChildren<TextMesh>().gameObject.SetActive(false);
            if (mitosisingKeys.Contains(order[i]))
                keys[order[i]].GetComponent<SphereCollider>().radius = 0;
            yield return new WaitForSeconds(0.025f);
        }
    }
    IEnumerator MoveSubmit()
    {
        sub.text = "SUB";
        yield return new WaitForSeconds(0.25f);
        float delta = 0;
        const float duration = 1;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            yield return null;
            submit.transform.localPosition = new Vector3(0.0574f, 0.0153f, Mathf.Lerp(-0.0021f, 0.0275f, delta));
        }
    }
    IEnumerator PlaceEachColor()
    {
        Vector3[] positions = Enumerable.Range(0, 8).Select(
            index => new Vector3(
                0.022f * (index % 2) + 0.049f,
                0.0158f,
                -0.022f * (index / 2) + -0.0045f)).ToArray();
        buttonColorOrder = Enumerable.Range(0, 8).Cast<NFTColor>().ToArray().Shuffle();
        for (int i = 0; i < 8; i++)
        {
            StartCoroutine(PlaceColor(i, positions[i], Data.colorLookup[buttonColorOrder[i]]));
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator PlaceEachTile()
    {
        Vector3[] positions = Enumerable.Range(0, 18).Select(
            index => new Vector3(
                0.04f * (index % 3) - 0.0625f,
                0.0158f,
                -0.02f * (index / 3) + 0.065f)).ToArray();
        for (int i = 0; i < 18; i++)
        {
            StartCoroutine(PlaceTile(i, positions[i]));
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator PlaceTile(int ix, Vector3 finalPosition)
    {
        yield return MoveKeyToPosition(keys[mitosisingKeys[ix]].transform, finalPosition, 1.5f);
        placedKeys[ix] = true;
        yield return new WaitUntil(() => placedKeys.All(x => x));
        yield return Mitosis(keys[mitosisingKeys[ix]].transform, 2 * ix);
    }
    IEnumerator Mitosis(Transform leftTF, int ix)
    {
        yield return new WaitForSeconds(0.25f);
        if (!nyoom)
            Audio.PlaySoundAtTransform("nyoom", transform);
        nyoom = true;
        Transform rightTF = leftTF.GetChild(3);
        rightTF.parent = leftTF.parent;
        
        StartCoroutine(MoveKeyToPosition(leftTF, leftTF.localPosition + 0.01f * Vector3.left, 1));
        StartCoroutine(MoveKeyToPosition(rightTF, rightTF.localPosition + 0.01f * Vector3.right, 1));
        yield return new WaitForSeconds(1);

        StartCoroutine(DecayToSquare(ix));
        StartCoroutine(DecayToSquare(ix + 1));
    }
    IEnumerator DecayToSquare(int ix)
    {
        Transform tf = tileTFs[ix];
        yield return new WaitForSeconds(Rnd.Range(0f, 0.6f));
        Vector3 startPos = tf.localPosition;
        for (int i = 0; i < 15; i++)
        {
            tf.localPosition = startPos + new Vector3(Rnd.Range(-0.0015f, +0.0015f), 0, Rnd.Range(-0.0015f, +0.0015f));
            yield return new WaitForSeconds(0.05f);
        }
        tf.localPosition = startPos;
        tf.GetComponent<MeshRenderer>().enabled = false;
        Audio.PlaySoundAtTransform("decay", tileTFs[ix]);
        tiles[ix].gameObject.SetActive(true);
        becameTiles[ix] = true;

        tiles[ix].OnInteract += delegate () { TilePress(ix); return false; };
    }
    IEnumerator PlaceColor(int ix, Vector3 finalPosition, Color finalColor)
    {
        yield return MoveKeyToPosition(keys[colorKeys[ix]].transform, finalPosition, 2);
        colorDiscs[ix].enabled = true;
        colorDiscs[ix].material.color = finalColor;
        keys[colorKeys[ix]].OnInteract += delegate () { SetCurrent(buttonColorOrder[ix], keys[colorKeys[ix]]); return false; };
    }
    IEnumerator MoveKeyToPosition(Transform tf, Vector3 finalPosition, float duration)
    {
        float delta = 0;
        Vector3 startPos = tf.localPosition;
        Vector3 startScl = tf.localScale;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            yield return null;
            tf.localPosition = Vector3.Lerp(startPos, finalPosition, delta);
            tf.localScale = Vector3.Lerp(startScl, new Vector3(0.02f, 0.001f, 0.02f), delta);
        }
    }

    IEnumerator ActivateScrollingScreen(float duration)
    {
        yield return new WaitUntil(() => becameTiles.All(x => x));

        scrollBox.gameObject.SetActive(true);
        float delta = 0;
        Vector3 startScl = scrollBox.localScale;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            scrollBox.localScale = new Vector3(startScl.x, startScl.y, Easing.InCubic(delta, 0, 0.03f, 1));
            yield return null;
        }
        Audio.PlaySoundAtTransform("bell", transform);
        animating = false;
        StartCoroutine(TextScroll(0));
    }

    IEnumerator TextScroll(int ix)
    {
        if (moduleSolved)
            scrollTexts[ix].text = solveMessage[ix];
        float delta = 0;
        bool sent = false;
        while (delta < 1)
        {
            delta += Time.deltaTime / 4;
            scrollTexts[ix].transform.localPosition = new Vector3(0, 0.8f, Mathf.Lerp(+1, -1, delta));
            if (!sent && scrollTexts[ix].transform.localPosition.z <= 0)
            {
                sent = true;
                StartCoroutine(TextScroll((ix + 1) % 3));
            }
            yield return null;
        }
    }

    IEnumerator ArrShrink(Transform tf, int ix, bool playSound)
    {
        if (arrsPressed[ix])
            yield break;
        arrsPressed[ix] = true;
        if (playSound)
            Audio.PlaySoundAtTransform("crunchy", tf);
        float delta = 0;
        const float duration = 0.5f;
        Vector3 startPos = tf.localScale;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            tf.localScale = Vector3.Lerp(startPos, Vector3.zero, delta);
            yield return null;
        }
        tf.gameObject.SetActive(false);
    }
    protected void Log(string message, params object[] args)
    {
        Debug.LogFormat("[Off-White Cipher #{0}] {1}", moduleId, string.Format(message, args));
    }
    protected void LogGrid(IEnumerable<char> grid, int width, int height)
    {
        for (int row = 0; row < height; row++)
            Log(grid.Skip(height * row).Take(width).Join());
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} submit> to press the submit button. Use <!{0} R a2 b3 G c4 f6> to press those buttons. Colors are abbreviated by their letters, coordinates are notated by column then row, with A1 in the top-left.";
#pragma warning restore 414
    IEnumerator Press(KMSelectable btn, float delay)
    {
        btn.OnInteract();
        yield return new WaitForSeconds(delay);
    }
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        string[] parameters = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (command == "SUBMIT")
        {
            yield return null;
            yield return Press(submit, 0);
        }
        else if (parameters.All(p => Regex.IsMatch(p, @"^((?:(?:[KBGCRMYW]|(?:[A-F][1-6]))(?:\s+|$))+)$")))
        {
            if (!transformed || animating)
            {
                yield return "sendtochaterror You cannot interact with the grid at this time.";
                yield break;
            }
            Debug.Log(parameters.Join());
            yield return null;
            foreach (string cmd in parameters)
            {
                if (cmd.Length == 1)
                    yield return Press(keys[colorKeys[Enumerable.Range(0, 8).First(ix => Data.colorAbbrs[buttonColorOrder[ix]] == cmd[0])]], 0.1f);
                else yield return Press(tiles[6 * (cmd[1] - '1') + (cmd[0] - 'A')], 0.1f);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!transformed)
            yield return Press(submit, 0.1f);
        while (animating)
            yield return true;
        for (int i = 0; i < 8; i++)
        {
            int[] squaresOfThisColor = Enumerable.Range(0, 36)
                .Where(x => generatedNFT[x] == buttonColorOrder[i] && generatedNFT[x] != enteredNFT[x]).ToArray().Shuffle();
            if (squaresOfThisColor.Length != 0)
            {
                yield return Press(keys[colorKeys[i]], 0.1f);
                foreach (int ix in squaresOfThisColor)
                    yield return Press(tiles[ix], 0.1f);
            }
        }
        yield return Press(submit, 0.1f);
    }
}