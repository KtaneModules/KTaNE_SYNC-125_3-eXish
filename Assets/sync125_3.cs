using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using KModkit;

public class sync125_3 : MonoBehaviour
{
    public KMSelectable[] buttons;
    public KMSelectable submitButton;
    public TextMesh screenText;
    public TextMesh digitText;
    public MeshRenderer[] stageLights;
    public KMBombInfo info;

    public Material off;
    public Material on;

    int digit = 0;

    string[] words = new[] {
        "İ'ms'", "ăĠ'n'", "kğ'i", "kĞ'p'", "ăut'",
        "ăġ'r", "ăġ'm", "ărs", "kğp'", "kğk",
        "ċ'iĊ", "ċ'it", "ĕĂn'", "ĕĪ'n'", "ėrs",
        "Ěit'", "Ěin", "Ěis'", "ĚmĮ'", "ĜĻ'r'",
        "Ĝrs", "Đim", "ďmĭ'", "Įin", "Įđk",
        "ĭrČ", "pğ'i", "pġ't", "İnt'", "İrt",
        "Ğim", "ĆĠ'n'", "ĆČn'", "ĳim", "ĳip",
        "sğ'i", "ģif", "ģin", "ģir", "ğĂn'"
    };

    string[] trans = new[] {
        "bombs", "calling", "clay", "club", "code",
        "color", "column", "course", "crab", "crack",
        "data", "date", "hacking", "having", "horse",
        "maid", "main", "maze", "member", "module",
        "morse", "name", "number", "pain", "panic",
        "party", "play", "plot", "pond", "port",
        "rhyme", "selling", "setting", "shame", "shape",
        "slay", "wife", "wine", "wire", "wrecking"
    };

    int[] values = new[] {
        0, 8, 15, 7, -1,
        1, 9, 14, 6, -4,
        2, 10, 13, 5, -3,
        3, 11, 12, 4, -2,
        4, 12, 11, 3, -1,
        5, 13, 10, 2, -4,
        6, 14, 9, 1, -3,
        7, 15, 8, 0, -2
    };

    int stage = 0;
    int textId = 0;
    bool isActive = false;

    static int _moduleIdCounter = 1;
    int _moduleId = 0;

    void Start()
    {
        screenText.text = "";
        digitText.text = "";
        _moduleId = _moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    void Init()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j); return false; };
        }
        submitButton.OnInteract += delegate () { OnSubmit(); return false; };
    }

    IEnumerator Randomize(bool wait = false)
    {
        if (wait)
        {
            screenText.text = "";
            yield return new WaitForSeconds(0.8f);
        }
        yield return new WaitForSeconds(0.4f);
        int newTextId = UnityEngine.Random.Range(0, words.Length - 1);
        textId = newTextId + ((newTextId < textId) ? 0 : 1);
        screenText.text = words[textId];
        Debug.LogFormat("[SYNC-125-3 #{0}] Stage {1}. The word is \"{2}\".", _moduleId, stage + 1, trans[textId]);
        switch (values[textId])
        {
            case -1:
                Debug.LogFormat("[SYNC-125-3 #{0}] There are {1} batteries. Expecting number {2}.", _moduleId, info.GetBatteryCount(), info.GetBatteryCount() % 16);
                break;
            case -2:
                int chr = 0;
                string sn = info.GetSerialNumber();
                for (int i = 0; i < sn.Length; i++)
                {
                    if (char.IsDigit(sn[i]))
                    {
                        chr = (int)(sn[i] - '0');
                        break;
                    }
                    if (sn[i] >= 'A' && sn[i] <= 'F')
                    {
                        chr = (int)(sn[i] - 'A' + 10);
                        break;
                    }
                }
                Debug.LogFormat("[SYNC-125-3 #{0}] First hex digit in SN is {1}. Expecting number {2}.", _moduleId, Convert.ToString(chr, 16).ToUpper(), chr);
                break;
            case -3:
                Debug.LogFormat("[SYNC-125-3 #{0}] There are {1} modules. Expecting number {2}.", _moduleId, info.GetModuleNames().Count, info.GetModuleNames().Count % 16);
                break;
            case -4:
                Debug.LogFormat("[SYNC-125-3 #{0}] There are {1} solved modules. Expecting number {2}.", _moduleId, info.GetSolvedModuleNames().Count, info.GetSolvedModuleNames().Count % 16);
                break;
            default:
                Debug.LogFormat("[SYNC-125-3 #{0}] Expecting number {1}.", _moduleId, values[textId]);
                break;
        }
        isActive = true;
    }

    void ActivateModule()
    {
        digitText.text = "0";
        digit = 0;
        StartCoroutine(Randomize());
        Init();
    }

    IEnumerator StageLight(int st)
    {
        yield return new WaitForSeconds(0.15f);
        stageLights[stage++].material = on;
    }

    void OnPress(int pressedButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[pressedButton].transform);
        buttons[pressedButton].AddInteractionPunch();
        if (!isActive)
        {
            return;
        }
        digit = digit % 4 * 4 + pressedButton;
        digitText.text = Convert.ToString(digit, 16).ToUpper();
    }

    void OnSubmit()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitButton.transform);
        submitButton.AddInteractionPunch();
        if (!isActive)
        {
            return;
        }
        bool correct = (digit == values[textId]);
        switch (values[textId])
        {
            case -1:
                correct = (digit == info.GetBatteryCount() % 16);
                break;
            case -2:
                int chr = 0;
                string sn = info.GetSerialNumber();
                for (int i = 0; i < sn.Length; i++)
                {
                    if (char.IsDigit(sn[i]))
                    {
                        chr = (int)(sn[i] - '0');
                        break;
                    }
                    if (sn[i] >= 'A' && sn[i] <= 'F')
                    {
                        chr = (int)(sn[i] - 'A' + 10);
                        break;
                    }
                }
                correct = (digit == chr);
                break;
            case -3:
                correct = (digit == info.GetModuleNames().Count % 16);
                break;
            case -4:
                correct = (digit == info.GetSolvedModuleNames().Count % 16);
                break;
            default:
                break;
        }
        if (correct)
        {
            isActive = false;
            StartCoroutine(StageLight(stage));
            if (stage < 3)
            {
                Debug.LogFormat("[SYNC-125-3 #{0}] Number {1} was correct. Advancing to stage {2}.", _moduleId, digit, stage + 2);
                StartCoroutine(Randomize(true));
            }
            else
            {
                screenText.text = "";
                digitText.text = "0";
                Debug.LogFormat("[SYNC-125-3 #{0}] Number {1} was correct.", _moduleId, digit);
                digit = 0;
                Debug.LogFormat("[SYNC-125-3 #{0}] Module solved!", _moduleId);
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                GetComponent<KMBombModule>().HandlePass();
            }
        }
        else
        {
            Debug.LogFormat("[SYNC-125-3 #{0}] Number {1} was incorrect. Resetting the stage.", _moduleId, digit);
            GetComponent<KMBombModule>().HandleStrike();
            isActive = false;
            StartCoroutine(Randomize(true));
        }
    }

    //twitch plays
    private bool cmdIsValid(string s)
    {
        char[] valids = { '0', '1', '2', '3' };
        if(s.Length > 2)
        {
            return false;
        }
        foreach (char c in s)
        {
            if (!valids.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit <num> [Submits the specified number ONLY IF it is in base 4] | Valid numbers cannot contain more than 2 digits";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                if (cmdIsValid(parameters[1]))
                {
                    if (digit != 0)
                    {
                        buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                        buttons[0].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    for (int i = 0; i < parameters[1].Length; i++)
                    {
                        if (parameters[1].ElementAt(i).Equals('0'))
                        {
                            buttons[0].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('1'))
                        {
                            buttons[1].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('2'))
                        {
                            buttons[2].OnInteract();
                        }
                        else if (parameters[1].ElementAt(i).Equals('3'))
                        {
                            buttons[3].OnInteract();
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                    submitButton.OnInteract();
                }
                else
                {
                    yield return "sendtochaterror The specified number to submit '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the number you wish to submit!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        int start = stage;
        for (int i = start; i < 4; i++)
        {
            while (!isActive) { yield return true; yield return new WaitForSeconds(0.1f); }
            string base4 = "";
            int div = values[textId];
            switch (values[textId])
            {
                case -1:
                    div = info.GetBatteryCount() % 16;
                    break;
                case -2:
                    int chr = 0;
                    string sn = info.GetSerialNumber();
                    for (int k = 0; k < sn.Length; k++)
                    {
                        if (char.IsDigit(sn[k]))
                        {
                            chr = (int)(sn[k] - '0');
                            break;
                        }
                        if (sn[k] >= 'A' && sn[k] <= 'F')
                        {
                            chr = (int)(sn[k] - 'A' + 10);
                            break;
                        }
                    }
                    div = chr;
                    break;
                case -3:
                    div = info.GetModuleNames().Count % 16;
                    break;
                case -4:
                    div = info.GetSolvedModuleNames().Count % 16;
                    break;
                default:
                    break;
            }
            while (div != 0)
            {
                base4 = base4.Insert(0, (div % 4).ToString());
                div /= 4;
            }
            if (digit != 0)
            {
                buttons[0].OnInteract();
                yield return new WaitForSeconds(0.1f);
                buttons[0].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            for (int j = 0; j < base4.Length; j++)
            {
                if (base4[j].Equals('0'))
                {
                    buttons[0].OnInteract();
                }
                else if (base4[j].Equals('1'))
                {
                    buttons[1].OnInteract();
                }
                else if (base4[j].Equals('2'))
                {
                    buttons[2].OnInteract();
                }
                else if (base4[j].Equals('3'))
                {
                    buttons[3].OnInteract();
                }
                yield return new WaitForSeconds(0.1f);
            }
            submitButton.OnInteract();
        }
    }
}