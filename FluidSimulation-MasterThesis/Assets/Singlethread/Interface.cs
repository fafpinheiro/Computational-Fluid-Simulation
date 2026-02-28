
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interface
{
    void setParameters(float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass, float particleViscosity, float particleDrag,
        /*string type,*/ GameObject spawn,GameObject mesh, int amount, int rowSize, Material mat,float particlePercentage, Material focos,Material iterated, Material adjacent,bool paintMesh, int focosParticle, bool foamEffect);
    void InitSPH(string type);
    void ComputeDensityPressure();
    void ComputeForces();
    void Integrate();
    void ComputeColliders();

    void ApplyPosition();
    Vector3 averageParticleVelocity();

    void cleanParticles();

    void createObstacle(bool create);
}
