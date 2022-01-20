using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    public ParticleSystem ps;
    public ParticleSystemRenderer psr;
    public ParticleSystemForceField psf;

    public Transform target;

    bool fading;

    public Color[] colors;

    public IEnumerator ColourFade;

    public ParticleSystem.Particle[] particles;

    float sqrDist;

    public float affectDistance;

    int pNumber;



    //// Start is called before the first frame update
    //void Start()
    //{

    //    //ps = this.GetComponent<ParticleSystem>();
    //    InitializeIfNeeded();
    //    int pNumber = ps.GetParticles(particles);
    //    foreach (ParticleSystem.Particle particle in particles)
    //    {
    //        //Debug.Log(particle);
    //    }
    //    sqrDist = affectDistance * affectDistance;

    //}


    //void InitializeIfNeeded()
    //{
    //    if (ps == null)
    //        ps = GetComponent<ParticleSystem>();

    //    if (particles == null || particles.Length < ps.main.maxParticles)
    //        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    //}

    //private void Update()
    //{
    //    ChangeInfluenceOnPosition();
    //}


    public void ToggleExternalForces(bool enabled)
    {
        var externalForces = ps.externalForces;
        externalForces.enabled = enabled;
    }

    public void SetGravity(float strength)
    {

        psf.gravity = strength;

    }
    public void Expell()
    {
        //Debug.Log("Expell Expell Expell ");
        psf.gravity = new ParticleSystem.MinMaxCurve(-2f, -4f); ;
        psf.directionX = 0;
        psf.rotationRandomness = Vector2.one;
        CancelInvoke("NeutraliseGravity");
        Invoke("NeutraliseGravity", 2f);

    }
    public void ExpellAfterTime(float time)
    {
        //Debug.Log("ExpellAfterTime at time: "+ time);
        CancelInvoke("NeutraliseGravity");

        Invoke("NeutraliseGravity", time);
    }

    public void NeutraliseGravity()
    {
        psf.gravity = 0f;
        psf.directionX = 0;
        psf.rotationRandomness = Vector2.zero;

    }

    public void SetRotation(float rotation)
    {
        psf.rotationSpeed = rotation;
    }

    public void DrawIn()
    {
        CancelInvoke("NeutraliseGravity");

        ToggleExternalForces(true);
        psf.gravity = new ParticleSystem.MinMaxCurve(-2f, -4f); ;
        psf.directionX = 6;

    }

    public void Reset()
    {
        ps.Stop();
    }

    public void FadeOut(float fadeTime)
    {
        ColourFade = Lerp(fadeTime);
        StartCoroutine(ColourFade);
    }

    public void StopFade()
    {
        StopCoroutine(ColourFade);
    }

    public IEnumerator Lerp(float lerpTime)
    {
        float rate = 1.0f / lerpTime;
        float normal = 0;
        fading = true;
        while (normal < 1 )
        {
            normal += Time.deltaTime * rate;
            psr.material.color = Color.Lerp(colors[0], colors[1], normal);
            yield return null;

        }
        fading = false;
        ps.gameObject.SetActive(false);
    }

    public void SetColor(int colorID)
    {
        if(fading) StopCoroutine(ColourFade);
        psr.material.color = colors[colorID];

    }

    //public void ChangeInfluenceOnPosition()
    //{
    //    float dist;
    //    int i = 0;
    //    foreach (ParticleSystem.Particle particle in particles)
    //    {
    //        dist = Vector3.SqrMagnitude(target.position - particles[i].position);
    //        if(i==0) Debug.Log(dist);
    //        if (dist < sqrDist)
    //        {
    //            if (i == 0) Debug.Log("in the circle" + dist / sqrDist);
    //            //particles[i].position = Vector3.Lerp(particles[i].position, transform.position, Time.deltaTime / 2.0f);
    //            particles[i].velocity = Vector3.Lerp(Vector3.zero, Vector3.one, dist/sqrDist);    

    //            //particles[i].
    //            //Mathf.Lerp()
    //        }
    //        i++;
    //    }

    //}
}
