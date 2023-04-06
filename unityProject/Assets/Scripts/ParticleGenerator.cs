using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantiate a rigidbody then set the velocity

public class ParticleGenerator : MonoBehaviour{
    public GameObject particle;
    
    public ParticleSPH sph;

    private bool isShooting = false;

    public int maxParticles = 5000;

    public void Start(){
        // Debug.Log("Start");
        Grid.InitGrid();
        sph = new ParticleSPH(particle, maxParticles);
    }

    public void Update(){
        // UpdatePosition();
        // ManageInput();
        if(CanShoot()) GenerateParticle();
        sph.Update();
    }

    private void UpdatePosition(){
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(mousePosition.x, mousePosition.y);
    }

    public void OnApplicationQuit(){
        sph.OnApplicationQuit();
    }

    private void GenerateParticle(){
        GameObject circle = sph.GenerateParticle(transform.position);
        // circle.GetComponent<Rigidbody2D>().AddRelativeForce(circle.GetComponent<Particle>().mVelocity);
    }

    private void ManageInput(){
        if(Input.GetButtonDown("Fire1")) isShooting = true;
        if(Input.GetButtonUp("Fire1")) isShooting = false;
    }

    private bool CanShoot(){
        // return isShooting && sph.CanGenerateParticle();
        return sph.CanGenerateParticle();
    }
}