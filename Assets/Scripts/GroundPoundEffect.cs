using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TS = System.TimeSpan;
using UnityEngine;

public class GroundPoundEffect : MonoBehaviour {

    public ParticleSystem particles;
    public GameObject distorter;

    public async void PlayEffect() {
        particles.Play(false);
        Instantiate(distorter,transform);
        await Task.Delay(TS.FromSeconds(0.1));
        particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        GameObject.Destroy(gameObject, 5);
    }
}
