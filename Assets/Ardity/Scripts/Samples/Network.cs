using UnityEngine;
using System.Collections;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using UnityEngine.Events;

using System.Threading;
using System.Collections.Generic;

public class StrEvent : UnityEvent<string> { };

public class Network : MonoBehaviour {

	public int listenPort;
    public float timeToWait = 3.0f;

	private Thread t;
	private UdpClient listener;
	private bool msgFromThread = false;
	private string msgName;
	private object msgPayload;
	string dat;
	byte[] receive_byte_array;
    int gameState = 1;
    int nbShoot = 0;
    private FMOD.Studio.EventInstance instanceTargetA;
    private FMOD.Studio.EventInstance instanceTargetB;
    private FMOD.Studio.EventInstance instanceTargetC;

    public int score;

    public StrEvent onMessageReceive;

	string datA, datB, datC;

    bool state1 = true, state2 = true, state3 = true, state4 = true;
    float timeWaiting = 0;

	void Start () {
		t = new Thread( new ThreadStart(ListenThread) );
		t.IsBackground = true;
		t.Start();
        StartGame();
    }

    void StartGame()
    {
        StartCoroutine(NewStateCoroutine());
        gameState = 1;
        nbShoot = 0;
        StartingAudio();
    }

    void StartingAudio()
    {
        instanceTargetA = FMODUnity.RuntimeManager.CreateInstance("event:/Voices/Voices_Ostages");
        instanceTargetA.start();
        FMODUnity.RuntimeManager.PlayOneShot("event:/Music_Play");
        FMODUnity.RuntimeManager.PlayOneShot("event:/AMB/AMB_Details");
        FMODUnity.RuntimeManager.PlayOneShot("event:/AMB/Parc_Sound");
        FMODUnity.RuntimeManager.PlayOneShot("event:/AMB/AMB_Scary");
        FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Start");
    }

	void ListenThread() {
		listener = new UdpClient(listenPort);
		IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
		
		Debug.Log("Listener: Waiting for broadcasts...\n");

        // route message
        /*if (dat.StartsWith("REGISTER")) {
            // register a kiosk
            string name = dat.Substring(9);
            Kiosk k = new Kiosk(name, groupEP.Address, groupEP.Port);
            msgName = "AddNewKiosk";
            msgPayload = k;
            msgFromThread = true;
        }*/

        while (true)
        {
            receive_byte_array = listener.Receive(ref groupEP);
            dat = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
            //Debug.Log("Listener: Received a broadcast from " + groupEP.ToString());

            onMessageReceive?.Invoke(dat);

            Debug.Log(dat + " shot signal");

            if (dat == "target_A")
            {
                //Debug.Log("Target A Shot");
                TargetAShot();
            }

            if (dat == "target_B")
            {
                //Debug.Log("Target B Shot");
                TargetBShot();
            }

            if (dat == "target_C")
            {
                //Debug.Log("Target C Shot");
                TargetCShot();
            }
        }
    }

    void Update()
    {
        speakerOn();
        if (msgFromThread)
        {
            gameObject.SendMessage(msgName, msgPayload);
            msgFromThread = false;
        }


        if(gameState > 10)
        {
            gameState = 1;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 1.0f);
        }

    }

    void OnDestroy()
    {
        if (t.IsAlive) t.Abort();
        if (listener != null) listener.Close();
    }

    void TargetAShot()
    {
        if(!waiting) {
            Debug.Log("Target C validated shot");
            switch (gameState) {
                // State 1

                case 1:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 1.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    instanceTargetA.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 2

                case 4:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                case 6:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 3

                case 7:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                    if (score > 0)
                        score--;
                    break;

                case 8:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                    if (score > 0)
                        score--;
                    break;

                case 9:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 4

                case 10:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

            }
        }
    }

    void TargetBShot()
    {
        if (!waiting) {
            Debug.Log("Target C validated shot");
            switch (gameState) {
                // State 1

                case 2:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 2

                case 4:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                case 5:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 3

                case 8:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                case 9:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                    if (score > 0)
                        score--;
                    break;

                // State 4

                case 10:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

            }
        }
    }

    void TargetCShot()
    {
        if(!waiting) {
            Debug.Log("Target C validated shot");
            switch (gameState) {
                // State 1

                case 3:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 2

                case 5:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                case 6:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 3

                case 7:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                case 8:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                    if(score > 0)
                        score--;
                    break;

                case 9:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

                // State 4

                case 10:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                    FMODUnity.RuntimeManager.PlayOneShot("event:/TargetTouched");
                    nbShoot++;
                    stateChange();
                    score++;
                    break;

            }
        }
    }

    void speakerOn()
    {
        switch (gameState)
        {
            case 1:
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Target Pan", 0.0f);
                break;
            case 2:
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Target Pan", 1.0f);
                break;
            case 3:
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Target Pan", 2.0f);
                break;
        }
    }

    private bool waiting = false;

    void stateChange()
    {
        switch (nbShoot)
        {
            case 1:
                gameState++;
                break;

            case 2:
                gameState++;
                break;

            case 3:
                if (state1)
                {
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Start");
                    Debug.Log("State 2 begins");
                    state1 = false;
                }
                UnityMainThread.wkr.Enqueue(NewStateCoroutine());
                gameState++;
                break;

            case 5:
                gameState++;
                break;

            case 7:
                gameState++;
                break;

            case 9:
                if (state2)
                {
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Start");
                    Debug.Log("State 3 begins");
                    state2 = false;
                }
                UnityMainThread.wkr.Enqueue(NewStateCoroutine());
                gameState++;
                break;

            case 10:
                gameState++;
                break;

            case 11:
            gameState++;
            break;

            case 13:
                if (state3)
                {
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Start");
                    Debug.Log("State 4 begins");
                    state3 = false;
                }
                UnityMainThread.wkr.Enqueue(NewStateCoroutine());
                gameState++;
                break;

            case 16:
                nbShoot = 0;
                state1 = true;
                state2 = true;
                state3 = true;
                gameState = 1;
                score = 0;
                StartGame();
                break;
        }
        Debug.Log("State has Changed to :" + gameState);
    }

    private IEnumerator NewStateCoroutine()
    {
        Debug.Log("New state awaiting delay");
        waiting = true;
        yield return new WaitForSeconds(10f);
        Debug.Log("New state starting");
        waiting = false;
        yield return null;
    }
}