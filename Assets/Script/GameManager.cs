using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;

public enum eTARGET_TYPE { Disabled, Hostile, Hostage};
public enum ePHASE_STATE { Awaiting, Playing, Over };

public class GameManager : MonoBehaviour
{
    public MeshRenderer[] debugTargets;
    public int score;
    public int currentPhase;
    public List<Phase> phases;
    public Network network;
    public int stat { get; private set; }
    private Dictionary<int, string> targetIPs;

    public Wave CurrentWave() {return phases[currentPhase].waves[phases[currentPhase].currentWave]; }

    [System.Serializable]
    public class Target
    {
        public eTARGET_TYPE targetType;
        public bool isAlive;
    }

    [System.Serializable]
    public class Wave
    {
        [HideInInspector] public double startTime;
        public float duration;
        [HideInInspector] public int hostilesDown;
        [HideInInspector] public int hostagesDown;
        [HideInInspector] public ePHASE_STATE phaseState;

        public Target[] targets;
        public int NbOfType(eTARGET_TYPE targetType)
        {
            int count = 0;
            foreach(Target target in targets) {
                if (target.targetType == targetType)
                    count++;
            }
            return count;
        }
    }

    [System.Serializable]
    public class Phase
    {
        [HideInInspector] public int currentWave = -1;
        public List<Wave> waves;
    }

    private void Start()
    {
        network.onMessageReceive = new StrEvent();
        network.onMessageReceive.AddListener(ReceiveMessage);
        StartGame();
    }

    private void OnDisable()
    {
        network.onMessageReceive.RemoveListener(ReceiveMessage);
    }

    public void StartGame()
    {
        Debug.Log("Starting a new game");
        currentPhase = 0;
        score = 0;
        //Select targets
        Wave wave = CurrentWave();

        for (int i = 0; i < wave.targets.Length; ++i) {
            switch (wave.targets[i].targetType) {
                case eTARGET_TYPE.Disabled:
                    debugTargets[i].material.color = Color.yellow;
                    break;
                case eTARGET_TYPE.Hostage:
                    debugTargets[i].material.color = Color.green;
                    wave.targets[i].isAlive = true;
                    break;
                case eTARGET_TYPE.Hostile:
                    debugTargets[i].material.color = Color.red;
                    wave.targets[i].isAlive = true;
                    break;
            }
        }
        StartCoroutine(StartingWaveRoutine());
        //StartNewPhase();
    }
     
    public void EndGame()
    {
        Debug.Log("End of the game");
    }

    public bool StartNewPhase()
    {
        Debug.Log("Starting new phase");
        //Last phase reached
        if (currentPhase == phases.Count - 1)
        {
            EndGame();
            return true;
        }

        ++currentPhase;

        //If the final wave is not reached, the game keeps going
        if (!StartNewWave()) {
            return false;
        }
        return false;
    }

    public bool StartNewWave()
    {
        Debug.Log("Starting new wave");
        Phase tmpPhase = phases[currentPhase];

        //Final wave reached
        if (tmpPhase.currentWave == tmpPhase.waves.Count - 1) {
            Debug.Log("Final wave reached");
            StartNewPhase();
            return true;
        }

        //Select targets
        Wave wave = CurrentWave();

        for(int i = 0; i < wave.targets.Length; ++i)
        {
            Color color = new Color();
            switch(wave.targets[i].targetType)
            {
                case eTARGET_TYPE.Disabled:
                    color = Color.yellow;
                    break;
                case eTARGET_TYPE.Hostage:
                    color = Color.green;
                    wave.targets[i].isAlive = true;
                    break;
                case eTARGET_TYPE.Hostile:
                    color = Color.red;
                    wave.targets[i].isAlive = true;
                    break;
            }
            UnityMainThread.wkr.Enqueue(Colorize(debugTargets[i], color));
        }
        UnityMainThread.wkr.Enqueue(StartingWaveRoutine());
        return false;
    }



    private IEnumerator StartingWaveRoutine()
    {
        //TODO: Send start wave audio event
        yield return new WaitForSeconds(CurrentWave().duration);
        Debug.Log("Wave started");
        CurrentWave().phaseState = ePHASE_STATE.Playing;
        //Send activation network messages for targets
        yield return null;
    }

    private void HitTarget(int target)
    {
        Target shotTarget = CurrentWave().targets[target];
        if (shotTarget.targetType != eTARGET_TYPE.Disabled && shotTarget.isAlive)
        {
            CurrentWave().targets[target].isAlive = false;

            Color color = Color.black;
            UnityMainThread.wkr.Enqueue(Colorize(debugTargets[target], color));

            //Shot a hostage
            if(shotTarget.targetType == eTARGET_TYPE.Hostage) {
                Debug.Log("Shot a hostage" + target);
                score = Mathf.Clamp(score - 1, 0, 100);
                CurrentWave().hostagesDown++;
            }
            //Shot a hostile
            else {
                Debug.Log("Shot a hostile" + target);
                ++score;
                CurrentWave().hostilesDown++;
            }

            if (CurrentWave().hostilesDown == CurrentWave().NbOfType(eTARGET_TYPE.Hostile)) {
                CurrentWave().phaseState = ePHASE_STATE.Over;
                ++phases[currentPhase].currentWave;
                StartNewWave();
            }
        }
        else {
            //Debug.Log("Shot disabled or dead target " + target);
        }
    }

    private IEnumerator Colorize(MeshRenderer renderer, Color color)
    {
        renderer.material.color = color;
        yield return null;
    }

    private void ReceiveMessage(string message)
    {
        Debug.Log(message);
        if (message.Contains("target") && CurrentWave().phaseState == ePHASE_STATE.Playing)
        {
            switch(message)
            {
                case "target_A":
                    HitTarget(0);
                    break;
                case "target_B":
                    HitTarget(1);
                    break;
                case "target_C":
                    HitTarget(2);
                    break;
            }
        }
    }
}