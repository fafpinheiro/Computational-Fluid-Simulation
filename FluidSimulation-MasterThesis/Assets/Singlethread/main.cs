using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    static public void setParameters(Interface x, float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass,
        float particleViscosity, float particleDrag, /*string type,*/ GameObject spawn, GameObject mesh, int amount, int rowSize, Material mat, float particlePercentage,
        Material focos, Material iterated, Material adjacent,bool paintMesh, int focosParticle, bool foamEffect)
    {
        x.setParameters(particleRadius,smoothingRadius,smoothingRadiusSq,restDensity,gravityMult,particleMass,particleViscosity,particleDrag,spawn,mesh,
            amount,rowSize,mat, particlePercentage,focos,iterated,adjacent,paintMesh, focosParticle,foamEffect/*,objectos*/);
    }
    static public void InitSPH(Interface x, string type)
    {   
        x.InitSPH(type);
    }
   static public void ComputeDensityPressure(Interface x)
    {
        x.ComputeDensityPressure();
    }

    static public void ComputeForces(Interface x)
    {
        x.ComputeForces();
    }

    static public void Integrate(Interface x)
    {
        x.Integrate();
    }

    static public void ComputeColliders(Interface x)
    {
        x.ComputeColliders();
    }

    static public void ApplyPosition(Interface x)
    {
        x.ApplyPosition();
    }

    static public void averageParticleVelocity(Interface x)
    {
        x.averageParticleVelocity();
    }
    static public void cleanParticles(Interface x)
    {
        x.cleanParticles();
    }
    static public void createObstacle(Interface x, bool create)
    {
        x.createObstacle(create);
    }
}
