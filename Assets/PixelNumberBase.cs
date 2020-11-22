using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;
public class PixelNumberBase : MonoBehaviour {
    public GameObject[][] rows = new GameObject[][]{
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4],
        new GameObject[4]
    };
    public GameObject[] rowsFlattened;
    public GameObject[] borders;
    public KMSelectable[] numpad;
    public KMSelectable reset;
    public TextMesh inputText;
    public bool[][] binary = new bool[][]{
         new bool[] {false, false, false, false },
         new bool[] {false, false, false, true },
         new bool[] {false, false, true, false },
         new bool[] {false, false, true, true },
         new bool[] {false, true, false, false },
         new bool[] {false, true, false, true },
         new bool[] {false, true, true, false },
         new bool[] {false, true, true, true },
         new bool[] {true, false, false, false },
         new bool[] {true, false, false, true },
         new bool[] {true, false, true, false },
         new bool[] {true, false, true, true },
         new bool[] {true, true, false, false },
         new bool[] {true, true, false, true },
         new bool[] {true, true, true, false },
         new bool[] {true, true, true, true }
    };
    string[] hexidecimal = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
    int[] chosenBinaryIndicies = new int[16];
    string expectedString;
    string inputtedString;
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable i in numpad)
        {
            KMSelectable j = i;
            j.OnInteract += delegate { numpadHandler(j); return false; };
        }
        reset.OnInteract += delegate { resetHandler(); return false; };
        for (int i = 0; i < 16; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                rows[i][j] = rowsFlattened[4 * i + j];
            }
        }
    }
    // Use this for initialization
    void Start () {
        for (int i = 0; i < 16; i++)
        {
            chosenBinaryIndicies[i] = rnd.Range(0, 16);
            expectedString += hexidecimal[chosenBinaryIndicies[i]];
        }
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                rows[i][j].SetActive(binary[chosenBinaryIndicies[i]][j]);
                borders[j].SetActive(true);
            }
        }
        Debug.LogFormat("[Pixel Number Base #{0}] The number on the module, in hexidecimal, is {1}.", moduleId, expectedString);
    }
	
    void numpadHandler (KMSelectable but)
    {


        if (!solved)
        {
            but.AddInteractionPunch(.5f);
            inputtedString += but.GetComponentInChildren<TextMesh>().text;
            inputText.text = inputtedString;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    rows[i][j].SetActive(false);
                    borders[j].SetActive(false);
                }
            }
            if (inputtedString.Length == 16)
            {
             Debug.LogFormat("[Pixel Number Base #{0}] You entered {1}.", moduleId, inputtedString);
                if (Equals(inputtedString, expectedString)){
                    Debug.LogFormat("[Pixel Number Base #{0}] That was correct. Module solved.", moduleId);
                    sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    module.HandlePass();
                    solved = true;
                }
                else
                {
                    Debug.LogFormat("[Pixel Number Base #{0}] That was incorrect. Strike!", moduleId);
                    module.HandleStrike();
                    inputText.text = "";
                    inputtedString = "";
                    expectedString = "";
                    Start();
                }
                
            }
        }
    }

    void resetHandler()
    {
        if (!solved)
        {
            reset.AddInteractionPunch(.5f);
            inputText.text = "";
            inputtedString = "";
            for (int i = 0; i < 16; i++)
            {
                chosenBinaryIndicies[i] = rnd.Range(0, 16);
                expectedString += hexidecimal[chosenBinaryIndicies[i]];
                for (int j = 0; j < 4; j++)
                {
                    rows[i][j].SetActive(binary[chosenBinaryIndicies[i]][j]);
                    borders[j].SetActive(true);
                }
            }
        }
    }
    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.ToLowerInvariant();
        string validcmds = "0123456789abcdef";
        if (command == "reset")
        {
            yield return null;
            reset.OnInteract();
        }
        else
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (!validcmds.Contains(command[i].ToString()))
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
            }
            for (int i = 0; i < command.Length; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (command[i] == validcmds[j])
                    {
                        yield return null;
                        numpad[j].OnInteract();
                    }
                }
            }
        }
    }

}
