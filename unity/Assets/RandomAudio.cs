using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Valve.VR.Extras;
using System.IO;
using System;

public class RandomAudio : MonoBehaviour
{
    private int count = 0;
    private AudioSource[] audios;
    private Animator animator;
    private bool isStarted = false;

    public SteamVR_LaserPointer laserPointer;
 
    private string[] lines;
    private int currentRow = 1;
    private StreamWriter writer;
    public GameObject controller;
    public GameObject leftPicture;
    public GameObject rightPicture;

    private string type = "";
    private string pic_file = "";
    private string word = "";
    private string trigger1 = "";
    private string trigger2 = "";
    private string dis = "";
    private string correct = "";

    void Awake()
    {
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;
        Debug.Log("start");
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();
        animator.SetTrigger("Instruction");
        animator.SetBool("InstructionLoop", true);
        Debug.Log(audios);
        // TODO manually csv
        TextAsset csvText = (TextAsset)Resources.Load<TextAsset>("test");
        lines = csvText.text.Split("\n"[0]);

        writer = new StreamWriter("Assets/Resources/results" + Guid.NewGuid().ToString() + ".csv");
        writer.WriteLine("category_type,word,time,correctness");
        writer.Flush();
        // TODO update to actual length
        Invoke("EndInstruction", 49.0f);
    }

    void Update()
    {
        // Debug.Log(Vector3.Distance(leftPicture.transform.position, controller.transform.position));
        // Debug.Log(Vector3.Distance(rightPicture.transform.position, controller.transform.position));
    }



    public void PointerClick(object sender, PointerEventArgs e)
    {
        if (e.target.name == "Da")
        {
            if (!isStarted) 
            {
                isStarted = true;
                nextRound();
            }
            if (isStarted) 
            {
                writer.WriteLine("{0},{1},{2},{3}", type, pic_file, 0, correct);
                writer.Flush();
            }
            Debug.Log("Da was clicked");
            
        } else if (e.target.name == "Net")
        {
            Debug.Log("Net was clicked");
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.name == "Da")
        {
            Debug.Log("Da was entered");
        }
        else if (e.target.name == "Net")
        {
            Debug.Log("Net was entered");
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (e.target.name == "Da")
        {
            Debug.Log("Da was exited");
        }
        else if (e.target.name == "Net")
        {
            Debug.Log("Net was exited");
        }
    }

    void nextRound() 
    {
        animator.ResetTrigger("StartExperiment");
        animator.SetTrigger("EndIntro");
        string[] row = lines[currentRow].Split(',');
        type = row[5];
        pic_file = row[12].Replace(@"\\", @"/");;
        word = row[2].Replace(@"\\", @"/");
        if (type.Contains("VR"))
        {
            Debug.Log("VR is happenning");
            if (type.Contains("импл")) {
                trigger1 = row[17].Replace(@"\\", @"/");
                dis = row[29].Replace(@"\\", @"/");
                correct = row[18];
                Invoke ("AskQuestionFM", 1.0f);
            }
            else {
                trigger1 = row[21].Replace(@"\\", @"/");
                trigger2 = row[25].Replace(@"\\", @"/");
                dis = row[28].Replace(@"\\", @"/");
                correct = row[26];
                Debug.Log(correct);
                Invoke ("AskQuestionEE", 1.0f);
            }

        }
        currentRow++;
        writer.WriteLine("{0},{1},{2},{3}", type, pic_file, 0, correct);
        writer.Flush();
    }

    void EndInstruction()
    {
        Debug.Log("Ending instuction");
        animator.SetBool("InstructionLoop", false);
        animator.ResetTrigger("Instruction");
        animator.SetTrigger("FinishInstruction");
        audios[0].Stop();
        audios[1].Stop();
        audios[2].Stop();
        Invoke ("StartExperiment", 0.5f);
    }

    void StartExperiment() 
    {
        animator.ResetTrigger("FinishInstruction");
        audios[0].clip = (AudioClip) Resources.Load<AudioClip>("Инструкция_2");
        audios[0].Play();
        animator.SetTrigger("StartExperiment");
        // TODO remove!!!!!
        Invoke ("nextRound", 7.5f);
        // TODO remove!!!!!
    }

    void PutCardsDown()
    {
        animator.ResetTrigger("ShowCards8sec");
        animator.SetTrigger("StopShowCards");
        Invoke("PutYesNo", 0.5f);
    }

    void PutYesNo()
    {
        animator.ResetTrigger("StopShowCards");
        animator.SetTrigger("ShowCards5sec");
        Material yes = Resources.Load<Material>("Da");
        Material no = Resources.Load<Material>("Net");
        leftPicture.GetComponent<MeshRenderer>().material = yes;
        rightPicture.GetComponent<MeshRenderer>().material = no;
        // TODO wait for actions
        Invoke("ShowCards", 4.5f);
    }


    void ShowCards()
    {
        animator.ResetTrigger("ShowCards5sec");
        animator.SetTrigger("StopShowCards");
        Invoke("nextRound", 0.5f);
    }
 
     void AskQuestionFM()
     {
        // TODO activate animation
        animator.SetTrigger("ShowCards8sec");
        Debug.Log(trigger1.Substring(0, trigger1.Length - 4));
        Debug.Log(word.Substring(0, word.Length - 4));
        AudioClip trigger1Clip = (AudioClip) Resources.Load<AudioClip>(trigger1.Substring(0, trigger1.Length - 4));
        AudioClip wordClip = (AudioClip) Resources.Load<AudioClip>(word.Substring(0, word.Length - 4));
        // This gets the exact duration of the first clip, note that you have to cast the samples value as a double
        Material word_picture = Resources.Load<Material>(pic_file.Substring(0, pic_file.Length - 4));
        Material dis_picture = Resources.Load<Material>(dis.Substring(0, dis.Length - 4));

        double trigger1ClipDuration = (double)trigger1Clip.samples / trigger1Clip.frequency;

        leftPicture.GetComponent<MeshRenderer>().material = word_picture;
        rightPicture.GetComponent<MeshRenderer>().material = dis_picture;

        audios[0].clip = trigger1Clip;
        audios[1].clip = wordClip;
        audios[0].PlayScheduled(AudioSettings.dspTime + 0.1);
        audios[1].PlayScheduled(AudioSettings.dspTime + 0.0 + trigger1ClipDuration);
        // TODO 
        Invoke ("PutCardsDown", 7.5f);
     }

     void AskQuestionEE()
     {
        animator.SetTrigger("ShowCards8sec");
        Debug.Log(trigger1.Substring(0, trigger1.Length - 4));
        Debug.Log(trigger2.Substring(0, trigger2.Length - 4));
        Debug.Log(word.Substring(0, word.Length - 4));
        AudioClip trigger1Clip = (AudioClip) Resources.Load<AudioClip>(trigger1.Substring(0, trigger1.Length - 4));
        AudioClip trigger2Clip = (AudioClip) Resources.Load<AudioClip>(trigger2.Substring(0, trigger2.Length - 4));
        AudioClip wordClip = (AudioClip) Resources.Load<AudioClip>(word.Substring(0, word.Length - 4));
        Material word_picture = Resources.Load<Material>(pic_file.Substring(0, pic_file.Length - 4));
        Material dis_picture = Resources.Load<Material>(dis.Substring(0, dis.Length - 4));

        leftPicture.GetComponent<MeshRenderer>().material = word_picture;
        rightPicture.GetComponent<MeshRenderer>().material = dis_picture;

        double trigger1ClipDuration = (double)trigger1Clip.samples / trigger1Clip.frequency;
        double wordDuration = (double)wordClip.samples / wordClip.frequency;

        audios[0].clip = trigger1Clip;
        audios[1].clip = wordClip;
        audios[2].clip = trigger2Clip;
        audios[0].PlayScheduled(AudioSettings.dspTime + 0.1);
        audios[1].PlayScheduled(AudioSettings.dspTime + 0.1 + trigger1ClipDuration);
        audios[2].PlayScheduled(AudioSettings.dspTime + 0.3 + trigger1ClipDuration + wordDuration);
        // TODO
        Invoke ("PutCardsDown", 7.5f);
     }
}
