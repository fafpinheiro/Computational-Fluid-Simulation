using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Data : MonoBehaviour
{

    /// Properties
       [Header("Import")]
    [Tooltip("Default particle material.")] public GameObject character0Prefab = null;
    [Tooltip("Water particle material.")] public Material m_volumeMat = null;
    [Tooltip("Focus particle material.")] public Material m_focosParticle = null;
    [Tooltip("Iterated particles material.")] public Material m_iteratedParticle = null;
    [Tooltip("Particles in interaction material.")] public Material m_adjacentParticle = null;
    [Tooltip("Particle that we want to follow the interactions.")] public int analisedParticle;


#pragma warning disable 0649 // This line removes the warning saying that the variable is never assigned to. You can't assign a variable in a struct...
    [Header("Particle data")]
    [Tooltip("Particle Radius.")] public float particleRadius; //Particle radius
    [Tooltip("Smoothing Radius.")] public float smoothingRadius; //Afecting radius of the smoothing (particles in radius that affect the particle considered)
    [Tooltip("Smoothing Radius Squared.")] public float smoothingRadiusSq; //
    [Tooltip("Density of a small portion of fluid that is considered that the fluid is in rest.")] public float restDensity; //
    [Tooltip("Gravity Multiplier.")] public float gravityMult; //Gravity multiplier
    [Tooltip("Particle Mass.")] public float particleMass;//Particle mass
    [Tooltip("Fluid viscosity.")] public float particleViscosity;
    [Tooltip("Particle Drag is a force acting in opposite to the relative motion of any object moving with respect to a surrounding fluid.")] public float particleDrag;//Drag is a force acting opposite to the relative motion of any object moving with respect to a surrounding fluid
        public enum renderType { Group, OneByOne } ///Group spawns all particles at the same time, OneByOne spwans the particles one by one like a cascade in rows
    [Tooltip("Position where the fluid will be instanciated.")] public GameObject spawn;
    #pragma warning restore 0649
       [Header("Properties")]
    [Tooltip("Amount of Particles.")] public int amount = 250;
    [Tooltip("Size of each row of particles.")] public int rowSize = 16;


    [Header("Foam Effect materials.")]
    [Tooltip("Material for particles that are at lower velocities.")] public Material vol1 = null;
    [Tooltip("Material for particles that are at medium velocities.")] public Material vol2 = null;
    [Tooltip("Material for particles that are at higher velocities.")] public Material vol3 = null;

    // public GameObject[] objectos;

    /*  Range(1, amount)]
       public int focusParticle =*/
}

