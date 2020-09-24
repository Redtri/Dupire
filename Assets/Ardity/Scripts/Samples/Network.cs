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

	private Thread t;
	private UdpClient listener;
	private bool msgFromThread = false;
	private string msgName;
	private object msgPayload;
	string dat;
	byte[] receive_byte_array;
    int gameState = 1;
    int nbShoot = 0;

    public StrEvent onMessageReceive;
    public StrEvent onDeviceConnected;

	string datA, datB, datC;

    bool state1 = true, state2 = true, state3 = true, state4 = true;
    float timeWaiting = 0;

    private void Awake()
    {
        onMessageReceive = new StrEvent();
    }

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
            Debug.Log("Listener: Received a broadcast from " + groupEP.ToString());

            onMessageReceive?.Invoke(dat);

            if (dat == "target_A")
            {
                Debug.Log("Target A Shot");
                gameState++;
                TargetAShot();
            }

            if (dat == "target_B")
            {
                Debug.Log("Target B Shot");
                gameState++;
                TargetBShot();
            }

            if (dat == "target_C")
            {
                Debug.Log("Target C Shot");
                gameState++;
                TargetCShot();
            }
        }
    }

    void Update()
    {
        if (msgFromThread)
        {
            gameObject.SendMessage(msgName, msgPayload);
            msgFromThread = false;
        }

        if(gameState == 3 && state1)
        {
            timeWaiting += Time.deltaTime;

            if(timeWaiting > 3.0f)
            {
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Start");
                Debug.Log("Starting State 2");
                state1 = false;
                timeWaiting = 0;
            }
        }

        if (gameState == 6 && state2)
        {
            timeWaiting += Time.deltaTime;

            if (timeWaiting > 3.0f)
            {
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Start");
                Debug.Log("Starting State 2");
                state2 = false;
                timeWaiting = 0;
            }
        }

        if(gameState >= 10)
        {
            gameState = 1;
            state1 = true;
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
                nbShoot++;
                break;

            // State 2

            case 4:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                break;

            case 6:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                break;

            // State 3

            case 7:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                break;

            case 8:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                break;

            case 9:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
                break;

            // State 4

            case 10:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_A");
                nbShoot++;
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
                break;

            // State 2

            case 4:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                break;

            case 5:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                break;

            // State 3

            case 8:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
                break;

            case 9:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                nbShoot++;
                break;

            // State 4

            case 10:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_B");
                nbShoot++;
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
                break;

            // State 2

            case 5:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 2.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                break;

            case 6:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                break;

            // State 3

            case 7:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 3.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                break;

            case 8:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Miss_KillPeople");
                nbShoot++;
                break;

            case 9:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                break;

            // State 4

            case 10:
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", 4.0f);
                FMODUnity.RuntimeManager.PlayOneShot("event:/Voices/Voices_Hit_C");
                nbShoot++;
                break;

        }
    }
}