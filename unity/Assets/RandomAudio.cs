using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Valve.VR.Extras;


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

    private Stopwatch stopWatch = new Stopwatch();

    private string type = "";
    private string pic_file = "";
    private string word = "";
    private string trigger1 = "";
    private string trigger2 = "";
    private string dis = "";
    private string correct = "";
    private bool isAnsweredInTime = false;
    private bool isWaitingAnswer = false;
    private bool isLaser = true;

    void Awake()
    {
        laserPointer.PointerClick += PointerClick;
        Debug.Log("start");
        animator = GetComponent<Animator>();
        audios = GetComponents<AudioSource>();
        animator.SetTrigger("Instruction");
        animator.SetBool("InstructionLoop", true);
        Debug.Log(audios);

        TextAsset csvText = (TextAsset)Resources.Load<TextAsset>("test");
        lines = csvText.text.Split("\n"[0]);

        writer = new StreamWriter("Assets/Resources/results" + Guid.NewGuid().ToString() + ".csv");
        writer.WriteLine("category_type,word,time,correctness");
        writer.Flush();
        // TODO update to actual length
        Invoke("EndInstruction", 50.0f);
    }


    void Update()
    {
        if (!isLaser && isWaitingAnswer) {
            Debug.Log(Vector3.Distance(leftPicture.transform.position, controller.transform.position));
            if (Vector3.Distance(leftPicture.transform.position, controller.transform.position) < 0.5f) 
            {
                Debug.Log("touched left");
                stopWatch.Stop();
                isAnsweredInTime = true;
                isWaitingAnswer = false;
                if (correct == "да") 
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "correct");
                else
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "not_correct");
                CancelInvoke("ShowCards");
                Invoke("ShowCards", 0.1f);
            } else if (Vector3.Distance(rightPicture.transform.position, controller.transform.position) < 0.5f) 
            {
                Debug.Log("touched right");
                stopWatch.Stop();
                isAnsweredInTime = true;
                isWaitingAnswer = false;
                if (correct == "нет") 
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "correct");
                else
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "not_correct");
                CancelInvoke("ShowCards");
                Invoke("ShowCards", 0.1f);
            }
        }
    }

    public void PointerClick(object sender, PointerEventArgs e)
    {

        if (e.target.name == "Da")
        {
            Debug.Log("Da was clicked");
            if (!isStarted) 
            {
                isStarted = true;
                nextRound();
            }
            if (isStarted && isLaser && isWaitingAnswer) 
            {
                stopWatch.Stop();
                isAnsweredInTime = true;
                isWaitingAnswer = false;
                if (correct == "да")
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "correct");
                else
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "not_correct");
                writer.Flush();
                CancelInvoke("ShowCards");
                Invoke("ShowCards", 0.1f);
            }
            
        } else if (e.target.name == "Net")
        {
            Debug.Log("Net was clicked");
            if (isStarted && isLaser && isWaitingAnswer) 
            {
                stopWatch.Stop();
                isAnsweredInTime = true;
                isWaitingAnswer = false;
                if (correct == "нет")
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "correct");
                else
                    writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "not_correct");
                writer.Flush();
                CancelInvoke("ShowCards");
                Invoke("ShowCards", 0.1f);
            }
        }
    }

    void nextRound() 
    {
        if (!isAnsweredInTime) 
        {
            isWaitingAnswer = false;
            stopWatch.Stop();
            writer.WriteLine("{0},{1},{2},{3}", type, pic_file, stopWatch.Elapsed, "not_correct");
            writer.Flush();
        }
        isAnsweredInTime = false;
        animator.ResetTrigger("StartExperiment");
        animator.SetTrigger("EndIntro");
        string[] row = lines[currentRow].Split(',');
        type = row[5];
        pic_file = row[12].Replace(@"\\", @"/");;
        word = row[2].Replace(@"\\", @"/");
        if (type.Contains("VR"))
        {
            Debug.Log("VR is happenning");
            if (type.Contains("покой")) 
            {
                Debug.Log("Laser is active");
                laserPointer.active = true;
                isLaser = true;
            }
            else 
            {
                Debug.Log("Laser is inactive");
                laserPointer.active = false;
                isLaser = false;
            }
            if (type.Contains("импл")) 
            {
                trigger1 = row[17].Replace(@"\\", @"/");
                dis = row[29].Replace(@"\\", @"/");
                correct = row[18];
                Invoke ("AskQuestionFM", 1.0f);
            }
            else 
            {
                trigger1 = row[21].Replace(@"\\", @"/");
                trigger2 = row[25].Replace(@"\\", @"/");
                dis = row[28].Replace(@"\\", @"/");
                correct = row[26];
                Invoke ("AskQuestionEE", 1.0f);
            }

        }
        currentRow++;
        
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
        // Invoke ("nextRound", 7.5f);
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
        stopWatch.Restart();
        isWaitingAnswer = true;
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
