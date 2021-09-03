using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

public class RandomAudio : MonoBehaviour
{
	private int count = 0;
	private AudioSource[] audios;
    private Animator animator;

    public SteamVR_LaserPointer laserPointer;
 
    private string[] lines;
    private int currentRow = 1;
    public GameObject controller;
    public GameObject leftPicture;
    public GameObject rightPicture;

    private string pic_file = "";
    private string word = "";
    private string trigger1 = "";
    private string trigger2 = "";
    private string dis = "";

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
        TextAsset csvText = (TextAsset)Resources.Load<TextAsset>("Парадигма_test");
        lines = csvText.text.Split("\n"[0]);
        // TODO update to actual length
        Invoke("EndInstruction", 19.0f);
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
            Debug.Log("Da was clicked");
            Invoke("EndInstruction", 0.0f);
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

    // Start is called before the first frame update
    void Start()
    {
        
    }


    void nextRound() 
    {
        string[] row = lines[currentRow].Split(',');
        string type = row[5];
        pic_file = row[12].Replace(@"\\", @"/");;
        word = row[2].Replace(@"\\", @"/");
        if (type.Contains("VR"))
        {
            Debug.Log("VR is happenning");
            if (type.Contains("импл")) {
                trigger1 = row[17].Replace(@"\\", @"/");
                dis = row[29].Replace(@"\\", @"/");
                Invoke ("AskQuestionFM", 1.0f);
            }
            else {
                trigger1 = row[21].Replace(@"\\", @"/");
                trigger2 = row[25].Replace(@"\\", @"/");
                dis = row[28].Replace(@"\\", @"/");
                Invoke ("AskQuestionEE", 1.0f);
            }

        }
        currentRow++;
    }

    void EndInstruction()
    {
        animator.ResetTrigger("Instruction");
        animator.SetTrigger("FinishInstruction");
        animator.SetBool("InstructionLoop", false);
        audios[0].Stop();
        audios[1].Stop();
        audios[2].Stop();
        // animator.ResetTrigger("FinishInstruction");
        nextRound();
    }


    void PutCardsDown()
    {
        animator.ResetTrigger("ShowCards");
        animator.SetTrigger("StopShowCards");
        // TODO show cards with yes/no
        Invoke("PutYesNo", 0.5f);
    }

    void PutYesNo()
    {
        animator.ResetTrigger("StopShowCards");
        animator.SetTrigger("PutCardsDown");
        Invoke("ShowCards", 0.5f);
    }


    void ShowCards()
    {
        animator.ResetTrigger("PutCardsDown");
        animator.SetTrigger("StopPutCardsDown");
        // TODO update material
        // TODO wait 3 seconds and go next round
        Invoke("StopShowCards", 3.0f);
    }

    void StopShowCards()
    {
        animator.ResetTrigger("StopPutCardsDown");

        Invoke("nextRound", 0.5f);
    }

    // Update is called once per frame
    // void Update()
    // {

    // 	count++;
    // 	if (count % 300 == 0) {
            
    //     	// Invoke ("RandomSoundness", 10);
    //         // obj.GetComponent<MeshRenderer> ().material = materials[Random.Range(0, materials.Length)];
    //         // TODO update image?
    // 	}
    // }
 
     void AskQuestionFM()
     {
        // TODO activate animation
        animator.SetTrigger("ShowCards");
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

        // animator.SetTrigger("ShowCards");
        //this plays the first clip and schedules the second to play immediately after it
        audios[0].clip = trigger1Clip;
        audios[1].clip = wordClip;
        audios[0].PlayScheduled(AudioSettings.dspTime + 0.1);
        audios[1].PlayScheduled(AudioSettings.dspTime + 0.0 + trigger1ClipDuration);
        // TODO 
        Invoke ("PutCardsDown", 7.0f);
     }

     void AskQuestionEE()
     {
        animator.SetTrigger("ShowCards");
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
        Invoke ("PutCardsDown", 7.0f);
     }
}
