﻿using UnityEngine;
using System.Collections;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using UnityEngine.Events;

using System.Threading;
using System.Collections.Generic;

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

    public UnityEvent<string> onMessageReceive;

	string datA, datB, datC;

    bool state1 = true, state2 = true, state3 = true, state4 = true;
    float timeWaiting = 0;

	void Start () {
		t = new Thread( new ThreadStart(ListenThread) );
		t.IsBackground = true;
		t.Start();
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
        switch (gameState)
        {
            // State 1

            case 1:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 1.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                instanceTargetA.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                nbShoot++;
                stateChange();
                break;

            // State 2

            case 4:
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                stateChange();
                break;

            case 6:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                stateChange();
                break;

            // State 3

            case 7:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                break;

            case 8:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                break;

            case 9:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                
                nbShoot++;
                stateChange();
                break;

            // State 4

            case 10:
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                stateChange();
                break;

        }
    }

    void TargetBShot()
    {
        switch (gameState)
        {
            // State 1

            case 2:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                stateChange();
                break;

            // State 2

            case 4:
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                stateChange();
                break;

            case 5:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                stateChange();
                break;

            // State 3

            case 8:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                stateChange();
                break;

            case 9:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                break;

            // State 4

            case 10:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                stateChange();
                break;

        }
    }

    void TargetCShot()
    {
        switch (gameState)
        {
            // State 1

            case 3:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                stateChange();
                break;

            // State 2

            case 5:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                stateChange();
                break;

            case 6:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                stateChange();
                break;

            // State 3

            case 7:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                stateChange();
                break;

            case 8:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                break;

            case 9:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                stateChange();
                break;

            // State 4

            case 10:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                stateChange();
                break;

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
                gameState++;
                break;

            case 16:
                nbShoot = 0;
                state1 = true;
                state2 = true;
                state3 = true;
                gameState = 1;
                break;
        }
        Debug.Log("State is Changing :" + gameState);
    }
}