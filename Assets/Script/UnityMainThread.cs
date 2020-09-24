using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

class UnityMainThread : MonoBehaviour
{
    internal static UnityMainThread wkr;
    Queue<Action> jobs = new Queue<Action>();

    void Awake()
    {
        wkr = this;
    }

    void Update()
    {
        while (jobs.Count > 0)
            jobs.Dequeue().Invoke();
    }

    internal void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }

    public void Enqueue(IEnumerator action)
    {
        lock (jobs) {
            jobs.Enqueue(() => {
                StartCoroutine(action);
            });
        }
    }
}
