using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicStates : MonoBehaviour
{
    public float gamePhase = 1;
    public float param;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameLevels", gamePhase);
        FMODUnity.RuntimeManager.StudioSystem.getParameterByName("GameLevels", out param);
    }
}
