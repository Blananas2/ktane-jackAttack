using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class jackAttackScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable Button;
    public TextMesh[] texts; //0=clue, 1=bigWord, 2=smallWord, 3=JACK, 4=ATTACK, 5=one, 6=deaf, 7=money
    public Material[] mats;  //0=normal, 1=black, 2=white
    public GameObject back;
    public Color[] colors; //0=white, 1=for money, 2=for answer, 3=for "1", 4=black

    int clue = 0;
    int anchor = 0;
    int stage = 0;
    int correctStages = 0;
    int missedStages = 0;
    int time = 0;
    int startTime = 90;
    int sectionTime = 120; //138 before
    bool strikeGet = false;
    bool animating = false;

    bool cycleInstead = false;
    Coroutine cycle;
    bool dontDisp = true;

    int otherTime = 0;
    bool canClick = true;
    public List<int> bigWordOrder = new List<int> { 0, 1, 2, 3, 4 };
    public List<int> smallWordOrder = new List<int> { 2, 3, 4, 5, 6, 7 };
    KMAudio.KMAudioRef soundEffect;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () {
        moduleId = moduleIdCounter++;
        Button.OnInteract += delegate () { PressButton(); return false; };
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(delay());
        clue = UnityEngine.Random.Range(0, 14);
        anchor = 41 * clue;
        texts[0].text = PhraseList.phrases[anchor];
        Debug.LogFormat("[Jack Attack #{0}] The clue is: \"{1}\"", moduleId, PhraseList.phrases[anchor].Replace("\n", " "));
    }
	
	// Update is called once per frame
	void Update () {
        if (stage != 0 && moduleSolved == false)
        {
            time += 1;
            if (time == 1) {
                soundEffect.StopSound();
                soundEffect = Audio.PlaySoundAtTransformWithRef(string.Format("Chunk {0}", stage), transform);
                back.GetComponent<MeshRenderer>().material = mats[0];
                Button.GetComponent<MeshRenderer>().material = mats[2];
                texts[1].text = "";
                texts[2].text = "";
                texts[1].color = colors[4];
                texts[2].color = colors[0];
                texts[5].text = "";
                texts[6].text = "";
                texts[7].text = "";
            }
            if(cycleInstead == false)
            {
                if (time < startTime)
                {
                    //texts[1].text = ""; I HAVE NO CLUE WHY HAVING THE IF STATEMENT HERE HELPS, IT JUST DOES
                    //texts[2].text = "";
                    canClick = false;
                }
                else if (time < sectionTime + startTime)
                {
                    canClick = true;
                    texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                    texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[0])];
                }
                else if (time < (sectionTime * 2) + startTime)
                {
                    texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                    texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[1])];
                }
                else if (time < (sectionTime * 3) + startTime)
                {
                    texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                    texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[2])];
                }
                else if (time < (sectionTime * 4) + startTime)
                {
                    texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                    texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[3])];
                }
                else if (time < (sectionTime * 5) + startTime)
                {
                    texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                    texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[4])];
                }
                else if (time < (sectionTime * 6) + startTime)
                {
                    texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                    texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[5])];
                }
                else
                {
                    //MISS
                    time = 0;
                    smallWordOrder.Shuffle();
                    stage += 1;
                    missedStages += 1;
                    bigWordOrder.Add(bigWordOrder[(stage - 1) % 5]);
                    Debug.LogFormat("[Jack Attack #{0}] Stage {1} missed. Current misses: {2}", moduleId, stage - 1, missedStages);
                    Debug.LogFormat("[Jack Attack #{0}] The big word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)].Replace("\n", " "));
                    Debug.LogFormat("[Jack Attack #{0}] The correct small word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6)].Replace("\n", " "));
                    Check(0);
                }
            }
            else
            {
                if (time < startTime && time > 0)
                {
                    //texts[1].text = ""; I HAVE NO CLUE WHY HAVING THE IF STATEMENT HERE HELPS, IT JUST DOES
                    //texts[2].text = "";
                    canClick = false;
                }
                else if(time > 0)
                {
                    canClick = true;
                    dontDisp = false;
                }
                if(time > sectionTime + startTime)
                {
                    //MISS
                    time = 0;
                    smallWordOrder.Shuffle();
                    stage += 1;
                    missedStages += 1;
                    bigWordOrder.Add(bigWordOrder[(stage - 1) % 5]);
                    Debug.LogFormat("[Jack Attack #{0}] Stage {1} missed. Current misses: {2}", moduleId, stage - 1, missedStages);
                    Debug.LogFormat("[Jack Attack #{0}] The big word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)].Replace("\n", " "));
                    Debug.LogFormat("[Jack Attack #{0}] The correct small word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6)].Replace("\n", " "));
                    Check(0);
                }
            }
        }

        if (moduleSolved == true)
        {
            time += 1;
            if (time == 60)
            {
                soundEffect.StopSound();
            }
        }

        if (strikeGet == true)
        {
            time += 1;
            if (time == 95)
            {
                soundEffect.StopSound();
                strikeGet = false;
                canClick = true;
            }
        }

        if (animating == true)
        {
            otherTime += 1;
            if (otherTime == 50)
            {
                texts[7].text = "$" + correctStages + ",000";
            } else if (otherTime == 90)
            {
                otherTime = 0;
                animating = false;
                canClick = true;
            }
        }
	}

    void PressButton () {
        Button.AddInteractionPunch();
        if (moduleSolved == false && canClick == true)
        {
            if (stage == 0)
            {
                stage += 1;
                texts[3].text = "";
                texts[4].text = "";
                texts[0].text = "";
                bigWordOrder.Shuffle();
                smallWordOrder.Shuffle();
                soundEffect = Audio.PlaySoundAtTransformWithRef("blank", transform);
                soundEffect.StopSound();
                soundEffect = Audio.PlaySoundAtTransformWithRef("Chunk 1", transform);
                Debug.LogFormat("[Jack Attack #{0}] The big word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)].Replace("\n", " "));
                Debug.LogFormat("[Jack Attack #{0}] The correct small word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6)].Replace("\n", " "));
            }
            else
            {
                if (texts[2].text == PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6)])
                {
                    smallWordOrder.Shuffle();
                    correctStages += 1;
                    time = -100;
                    stage += 1;
                    soundEffect.StopSound();
                    soundEffect = Audio.PlaySoundAtTransformWithRef("correct", transform);
                    back.GetComponent<MeshRenderer>().material = mats[1];
                    Button.GetComponent<MeshRenderer>().material = mats[1];
                    Debug.LogFormat("[Jack Attack #{0}] Stage {1} is correct. Current correct stages: {2}", moduleId, stage - 1, correctStages);
                    Check(1);
                    if(moduleSolved == false)
                    {
                        Debug.LogFormat("[Jack Attack #{0}] The big word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)].Replace("\n", " "));
                        Debug.LogFormat("[Jack Attack #{0}] The correct small word is: \"{1}\"", moduleId, PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6)].Replace("\n", " "));
                    }
                }
                else
                {
                    Debug.LogFormat("[Jack Attack #{0}] Stage {1} striked. Module reset.", moduleId, stage);
                    time = 0;
                    stage = 0;
                    missedStages = 0;
                    bigWordOrder.Shuffle();
                    smallWordOrder.Shuffle();
                    if (cycleInstead == true)
                    {
                        dontDisp = true;
                    }
                    texts[0].text = PhraseList.phrases[anchor];
                    texts[1].text = "";
                    texts[2].text = "";
                    texts[3].text = "JACK";
                    texts[4].text = "ATTACK";
                    strikeGet = true;
                    canClick = false;
                    soundEffect.StopSound();
                    soundEffect = Audio.PlaySoundAtTransformWithRef("scream", transform);
                    GetComponent<KMBombModule>().HandleStrike();
                }
            }
        } else
        {
            Debug.Log("YOU CANNOT CLICK RIGHT NOW");
        }
    }

    void Check (int i)
    {
        if (correctStages == 5)
        {
            time = 0;
            stage = 42;
            Debug.LogFormat("[Jack Attack #{0}] 5 stages solved correctly, module solved.", moduleId);
            texts[0].text = "You have\ndefeated the";
            texts[1].text = "";
            texts[2].text = "";
            texts[3].text = "JACK";
            texts[4].text = "ATTACK";
            soundEffect.StopSound();
            soundEffect = Audio.PlaySoundAtTransformWithRef("correct", transform);
            back.GetComponent<MeshRenderer>().material = mats[0];
            Button.GetComponent<MeshRenderer>().material = mats[2];
            if(cycleInstead == true)
            {
                StopCoroutine(cycle);
            }
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
        } else if (missedStages == 3)
        {
            Debug.LogFormat("[Jack Attack #{0}] 3 stages missed, module striked.", moduleId);
            time = 0;
            stage = 0;
            missedStages = 0;
            bigWordOrder.Shuffle();
            smallWordOrder.Shuffle();
            if (cycleInstead == true)
            {
                dontDisp = true;
            }
            texts[0].text = PhraseList.phrases[anchor];
            texts[1].text = "";
            texts[2].text = "";
            texts[3].text = "JACK";
            texts[4].text = "ATTACK";
            strikeGet = true;
            canClick = false;
            soundEffect.StopSound();
            soundEffect = Audio.PlaySoundAtTransformWithRef("scream", transform);
            GetComponent<KMBombModule>().HandleStrike();
        } else if (i == 1 && correctStages != 5)
        {
            GoodAnimation();
            if (cycleInstead == true)
            {
                dontDisp = true;
            }
        }
    }

    void GoodAnimation ()
    {
        if (moduleSolved == false && correctStages != 5)
        {
            canClick = false;
            animating = true;
            texts[5].text = "1";
            texts[6].text = "deaf";
            texts[1].color = colors[0];
            texts[2].color = colors[2];
        }
    }

    private IEnumerator wordCycle()
    {
        int counter = 0;
        while (moduleSolved != true)
        {
            if(dontDisp == false)
            {
                texts[1].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 1)];
                texts[2].text = PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[counter])];
                yield return new WaitForSeconds(1f);
                counter++;
                if (counter == 6)
                {
                    counter = 0;
                }
            }
            else
            {
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    private IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
        if (TwitchPlaysActive == true)
        {
            cycleInstead = true;
            sectionTime = 2800;
            cycle = StartCoroutine(wordCycle());
        }
        Debug.LogFormat("[Jack Attack #{0}] Twitch Plays mode: {1}", moduleId, TwitchPlaysActive);
        StopCoroutine("delay");
    }

    //twitch plays
    bool TwitchPlaysActive;
    private bool isInputValid(string sn)
    {
        if(sn.EqualsIgnoreCase(PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[0])]) || sn.EqualsIgnoreCase(PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[1])]) || sn.EqualsIgnoreCase(PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[2])]) || sn.EqualsIgnoreCase(PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[3])]) || sn.EqualsIgnoreCase(PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[4])]) || sn.EqualsIgnoreCase(PhraseList.phrases[anchor + (bigWordOrder[stage - 1] + 6) + (5 * smallWordOrder[5])]))
        {
            return true;
        }
        return false;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} start [Starts the module] | !{0} submit <small phrase> [Submits the given 'small phrase' when it appears] | The module is slower to make it possible for TP";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*start\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(stage == 0)
            {
                yield return null;
                Button.OnInteract();
                yield break;
            }
            else
            {
                yield return "sendtochaterror I'm already cycling through small phrases! (I've been started already)";
                yield break;
            }
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length >= 2)
            {
                string phrase = "";
                for(int i = 1; i < parameters.Length; i++)
                {
                    phrase += parameters[i]+" ";
                }
                phrase = phrase.Remove(phrase.Length-1);
                if (stage == 0)
                {
                    yield return "sendtochaterror I haven't got any small phrases to submit! (Start me plz)";
                    yield break;
                }
                else if (isInputValid(phrase))
                {
                    yield return null;
                    while (!texts[2].text.EqualsIgnoreCase(phrase)) yield return "trycancel The small phrase was not pressed due to a request to cancel.";
                    Button.OnInteract();
                }
                else
                {
                    yield return "sendtochaterror That small phrase does not appear in the cycle of small phrases!";
                }
            }
            yield break;
        }
    }
}
