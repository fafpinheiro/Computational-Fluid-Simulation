using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using SPH;
public class DefaultAlgo : MonoBehaviour, Interface
{
    // Consts
    private static Vector3 GRAVITY = new Vector3(0f, -9.81f, 0.0f);
    private const float GAS_CONST = 2000.0f;
    private const float DT = 0.0008f;
    private const float BOUND_DAMPING = -0.5f;

    // Properties
    [Header("Import")]
    [SerializeField] private GameObject character0Prefab = null;
    [SerializeField] private Material m_volumeMat = null;
    [SerializeField] private Material m_focos = null;
    [SerializeField] private Material m_iterated = null;
    [SerializeField] private Material m_adjacent = null;
    [Header("Parameters")]
    [SerializeField] private int parameterID = 0;


    // Data
    private SPH.SPHParticle[] particles;
    private int countReadyParticles = 0;

    //#pragma warning disable 0649 // This line removes the warning saying that the variable is never assigned to. You can't assign a variable in a struct...
    public float particleRadius; //Particle radius
    public float smoothingRadius; //Afecting radius of the smoothing (particles in radius that affect the particle considered)
    public float smoothingRadiusSq; //
    public float restDensity; //
    public float gravityMult; //Gravity multiplier
    public float particleMass;//Particle mass
    public float particleViscosity;
    public float particleDrag;//Drag is a force acting opposite to the relative motion of any object moving with respect to a surrounding fluid
    public int amount;
    public int rowSize;
    private float particlePercentage;
    //public Material mat;

    //#pragma warning restore 0649
    // private SPH.SPHParticle[] solids;
    public RenderTexture Volume;
    public GameObject spawn;
    public bool paintMesh;
    //public Data data;
    public static B s1;
    //public Material m_volumeMat;
    private ComputeShader m_shader;
    private GameObject m_mesh;
    private GameObject particlesFather;
    private int focosParticle;
    // private GameObject[] objectos;
    public void setParameters(float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass, float particleViscosity, float particleDrag,
        /*string type,*/ GameObject spawn, GameObject mesh, int amount, int rowSize, Material mat, float particlePercentage, Material focos, Material iterated, Material adjacent, bool paintMesh, int focosParticle, bool foamEffect)
    {
        this.particleRadius = particleRadius;
        this.smoothingRadius = smoothingRadius;
        this.smoothingRadiusSq = smoothingRadiusSq;
        this.restDensity = restDensity;
        this.gravityMult = gravityMult;
        this.particleMass = particleMass;
        this.particleViscosity = particleViscosity;
        this.particleDrag = particleDrag;
        this.spawn = spawn;
        this.character0Prefab = mesh;
        this.amount = amount;
        this.rowSize = rowSize;
        this.m_volumeMat = mat;
        this.particlePercentage = particlePercentage;
        this.m_focos = focos;
        this.m_iterated = iterated;
        this.m_adjacent = adjacent;
        this.paintMesh = paintMesh;
        this.focosParticle = focosParticle;
        //  this.objectos = objectos;
    }

    public void InitSPH(string type)
    {
        /*solids = new SPH.SPHParticle[6];
        particlesFather = new GameObject();
        for (int i = 0; i < 6; i++)
        {
            float x = objectos[i].transform.position.x;
            float y = objectos[i].transform.position.y;
            float z = objectos[i].transform.position.z;

            GameObject go = Instantiate(character0Prefab);
            go.transform.SetParent(particlesFather.transform);
            go.transform.localScale = Vector3.one * particleRadius;
            go.transform.position = new Vector3(x, y, z);
            go.name = "char" + i.ToString();
            solids[i].Init(new Vector3(x, y, z), parameterID, go);
        }*/
        if (type == "Group")
        {
            InitSPHGroup();
        }
        else
        {
            StartCoroutine(InitSPHOneByOne());
        }
    }


    public void InitSPHGroup(/*string type*/)
    {

        particles = new SPH.SPHParticle[amount];

        Random.seed = 52794;
        particlesFather = new GameObject();
        for (int i = 0; i < amount; i++)
        {
            float jitter = (Random.value * 2f - 1f) * particleRadius * 0.1f;
            //print("bacon");
            // print("i: " + i + "smoothing soma: " + (i * smoothingRadius));
            //var margin = 1 / smoothingRadius;
            float x = spawn.transform.position.x + (i % rowSize) /**smoothingRadius*/ + (Random.Range(-0.1f, 0.1f)/**smoothingRadius*/);
            float y = spawn.transform.position.y/**smoothingRadius*/ + 2/**smoothingRadius*/ + (float)((i / rowSize) / rowSize) * 1.1f /** smoothingRadius*/;
            float z = spawn.transform.position.z + ((i  / rowSize) % rowSize)/**smoothingRadius*/ + Random.Range(-0.1f, 0.1f)/**smoothingRadius*/;
           // print(x + " y " + y + "z" + z);
            GameObject go = Instantiate(character0Prefab);
            go.transform.SetParent(particlesFather.transform);
            //  go.
            go.transform.localScale = Vector3.one * particleRadius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter);
            go.name = "char" + i.ToString();
            // go.AddComponent<SphereCollider>();
            //  go.AddComponent<Rigidbody>();
            // go.GetComponent<Rigidbody>().mass = 5;
            if (m_volumeMat != null)
                go.GetComponent<MeshRenderer>().material = m_volumeMat;
            particles[i].Init(new Vector3(x, y, z), parameterID, go);
            countReadyParticles++;
        }
    }


    IEnumerator InitSPHOneByOne()
    {
        particles = new SPH.SPHParticle[amount];
        Random.seed = 52794;
        for (int i = 0; i < amount; i++)
        {

            yield return new WaitForSeconds(0.001f);
            float jitter = (Random.value * 2f - 1f) * particleRadius * 0.1f;
            float x = spawn.transform.position.x + (i % rowSize) + Random.Range(-0.1f, 0.1f);
            float y = spawn.transform.position.y + 15;
            float z = spawn.transform.position.z + 0 + Random.Range(-0.1f, 0.1f); ;
            GameObject go = Instantiate(character0Prefab);
            go.transform.localScale = Vector3.one * particleRadius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter);
            go.name = "char" + i.ToString();

            if (m_volumeMat != null)
                go.GetComponent<MeshRenderer>().material = m_volumeMat;
            particles[i].Init(new Vector3(x, y, z), parameterID, go);
            countReadyParticles++;
        }
    }

    private static bool Intersect(SPH.SPHCollider collider, Vector3 position, float radius, out Vector3 penetrationNormal, out Vector3 penetrationPosition, out float penetrationLength)
    {
        Vector3 colliderProjection = collider.position - position;

        penetrationNormal = Vector3.Cross(collider.right, collider.up); //blue axis (cross between red and green)
        penetrationLength = Mathf.Abs(Vector3.Dot(colliderProjection, penetrationNormal)) - (radius / 2.0f);
        penetrationPosition = collider.position - colliderProjection;

        return penetrationLength < 0.0f
            && Mathf.Abs(Vector3.Dot(colliderProjection, collider.right)) < collider.scale.x
            && Mathf.Abs(Vector3.Dot(colliderProjection, collider.up)) < collider.scale.y;
    }



    private static Vector3 DampVelocity(SPH.SPHCollider collider, Vector3 velocity, Vector3 penetrationNormal, float drag)
    {
        Vector3 newVelocity = Vector3.Dot(velocity, penetrationNormal) * penetrationNormal * BOUND_DAMPING
                            + Vector3.Dot(velocity, collider.right) * collider.right * drag
                            + Vector3.Dot(velocity, collider.up) * collider.up * drag;
        newVelocity = Vector3.Dot(newVelocity, Vector3.forward) * Vector3.forward
                    + Vector3.Dot(newVelocity, Vector3.right) * Vector3.right
                    + Vector3.Dot(newVelocity, Vector3.up) * Vector3.up;
        return newVelocity;
    }



    public void ComputeColliders()
    {
        // Get colliders
        GameObject[] collidersGO = GameObject.FindGameObjectsWithTag("SPHCollider");
        SPH.SPHCollider[] colliders = new SPH.SPHCollider[collidersGO.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].Init(collidersGO[i].transform);
        }

        for (int i = 0; i < countReadyParticles; i++)
        {
            for (int j = 0; j < colliders.Length; j++)
            {
                // Check collision
                Vector3 penetrationNormal;
                Vector3 penetrationPosition;
                float penetrationLength;
                if (Intersect(colliders[j], particles[i].position, particleRadius, out penetrationNormal, out penetrationPosition, out penetrationLength))
                {
                    particles[i].velocity = DampVelocity(colliders[j], particles[i].velocity, penetrationNormal, 1.0f - particleDrag);
                    particles[i].position = penetrationPosition - penetrationNormal * Mathf.Abs(penetrationLength);
                }
            }
        }
    }



    public void Integrate()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].velocity += DT * (particles[i].forcePhysic) / particles[i].density;
            particles[i].position += DT * (particles[i].velocity);
        }
    }
    /*public void IntegrateOne(int i)
    {
      //  for (int i = 0; i < particles.Length; i++)
       // {
            particles[i].velocity += DT * (particles[i].forcePhysic) / particleMass;
            particles[i].position += DT * (particles[i].velocity);
      //  }
    }*/

    public void ComputeDensityPressure()
    {
        for (int i = 0; i < countReadyParticles; i++)
        {
            // particles[i].velocity += DT * gravityForce(i);
            // particles[i].position = particles[i].go.transform.position;
            // IntegrateOne(i);
            particles[i].density = 0.0f;
            for (int j = 0; j < countReadyParticles; j++)
            {
                Vector3 rij;
                float r2;
                // particles[j].position = particles[j].go.transform.position;

                distBetweenParticles(i, j, out rij, out r2);


                if (r2 < smoothingRadiusSq)
                {
                    /*if (i == 945)
                        print("densityPressure i: " + i + " j " + j);*/
                    particles[i].density += particleDensity(i, r2);//particleMass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f))) * Mathf.Pow(smoothingRadiusSq - r2, 3.0f);
                }
            }
            /*
            for(int j = 0; j < solids.Length; j++)
            {
                Vector3 rij;
                float r2;

                distBetweenParticlesObstacles(i, j, out rij, out r2);


                if (r2 < smoothingRadiusSq)
                {
                    particles[i].density += particleDensity(i, r2);//particleMass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f))) * Mathf.Pow(smoothingRadiusSq - r2, 3.0f);
                }
            }
            */

            particles[i].pressure = particlePressure(i);
            // print("densidade " + " i : "+ i + particles[i].density + "pressão " + " i : " + i + particles[i].pressure);
        }
    }



    public void ComputeForces()
    {

        if (paintMesh)
            interactionColors(2, 0, focosParticle, 0);
        for (int i = 0; i < countReadyParticles /*particles.Length*/; i++)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;
            /* if (m_volumeMat != null && i == focosParticle && m_focos!=null && m_adjacent!=null)
                 particles[i].go.GetComponent<MeshRenderer>().material = m_focos;*/

            // Physics
            for (int j = 0; j < countReadyParticles/* particles.Length*/; j++)
            {
                if (i == j) continue;

                Vector3 rij;
                float r2;
                distBetweenParticles(i, j, out rij, out r2);
                float r = Mathf.Sqrt(r2);//meter dentro do if

                if (paintMesh)
                    interactionColors(3, i, focosParticle, j);
                /*if (i == focosParticle && j != focosParticle && m_volumeMat != null && m_focos != null && m_adjacent != null)
                    particles[j].go.GetComponent<MeshRenderer>().material = m_iterated;*/
                if (r < smoothingRadius)//r-> r2
                {
                    /*if (i == 945)
                        print("computeForces i: " + i + " j " + j);*/
                    if (paintMesh)
                        interactionColors(4, i, focosParticle, j);
                    /*if (i == focosParticle && j != focosParticle && m_volumeMat != null && m_focos != null && m_adjacent != null)
                        particles[j].go.GetComponent<MeshRenderer>().material = m_adjacent;*/
                    forcePressure += pressureForce(i, j, rij, r);//-rij.normalized * particleMass * (particles[i].pressure + particles[j].pressure) / (2.0f * particles[j].density) * (-45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * Mathf.Pow(smoothingRadius - r, 2.0f);
                    forceViscosity += viscosityForce(i, j, r);//particleViscosity * particleMass * (particles[j].velocity - particles[i].velocity) / particles[j].density * (45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * (smoothingRadius - r);
                }
            }
            /*
            for( int j = 0; j< solids.Length; j++)
            {
                Vector3 rij;
                float r2;
                distBetweenParticlesObstacles(i, j, out rij, out r2);
                float r = Mathf.Sqrt(r2);
                if (r < smoothingRadius)
                {
                    forcePressure += pressureForceObstacles(i, j,rij,r2);                  
                }
            }
            */
            // Apply

            particles[i].forcePhysic = forcePressure + forceViscosity + gravityForce(i);// forceGravity;
                                                                                        //  print("fisicas " + particles[i].forcePhysic);
        }
    }



    public void ApplyPosition()
    {
        for (int i = 0; i < countReadyParticles; i++)
        {
            if (particles[i].go != null)
                particles[i].go.transform.position = particles[i].position;
            // print(i + "  " + particles[i].go.transform.position);
        }
    }
    public void cleanParticles()
    {
        print("destroy B");
        Destroy(particlesFather);
    }
    //Method used to paint the diferent particles that are computed in interaction with the focus particle.
    private void interactionColors(int type, int i, double focosParticle, int otherParticle)
    {

        if (type == 2)
        {
            if (m_volumeMat != null /*&& i == focosParticle*/ && m_focos != null && m_adjacent != null)
                particles[(int)focosParticle].go.GetComponent<MeshRenderer>().material = m_focos;
        }
        if (type == 3)
        {
            if (m_volumeMat != null && m_focos != null && m_adjacent != null && i == focosParticle && otherParticle != focosParticle)
                particles[otherParticle].go.GetComponent<MeshRenderer>().material = m_iterated;
        }
        if (type == 4)
        {
            if (m_volumeMat != null && m_focos != null && m_adjacent != null && i == focosParticle && otherParticle != focosParticle)
            {
                particles[otherParticle].go.GetComponent<MeshRenderer>().material = m_adjacent;
                // oldIterated[j] = otherParticle;
                //oldIterated.Add(otherParticle);
                //flag = true;
            }
        }
    }

    ///////////                                            Calculos auxiliares                                          ///////////////


    /* private void distBetweenParticlesObstacles(int i, int j, out Vector3 rij, out float r2)
     {
         rij = solids[j].position - particles[i].position;
         r2 = rij.sqrMagnitude;
     }


     //Calculate the pressure force
     private Vector3 pressureForceObstacles(int i, int j, Vector3 rij, float r)
     {
         return -particleMass * (particles[i].pressure + solids[j].pressure) / (2.0f * solids[j].density) *
             SmoothingKernels.GradientSpiky(rij, smoothingRadius, r);
     }*/


    //Calculate the distance between two particles
    private void distBetweenParticles(int i, int j, out Vector3 rij, out float r2)
    {
        rij = particles[j].position - particles[i].position;
        r2 = rij.sqrMagnitude;
    }

    //Create a gravity field
    private Vector3 gravityForce(int i)
    {
        return GRAVITY * particles[i].density * gravityMult;
    }

    //Calculate the particle pressure
    private float particlePressure(int i)
    {
        return GAS_CONST * (particles[i].density - restDensity);
    }

    //Calclate the particle density
    private float particleDensity(int i, float r2)
    {

        return particleMass * SmoothingKernels.Poly6(smoothingRadius, smoothingRadiusSq, r2);
    }

    //Calculate the viscosity force
    private Vector3 viscosityForce(int i, int j, float r)
    {
        return particleViscosity * particleMass * (particles[j].velocity - particles[i].velocity) / particles[j].density * SmoothingKernels.ViscosityLaplacian(smoothingRadius, r);
    }

    //Calculate the pressure force
    private Vector3 pressureForce(int i, int j, Vector3 rij, float r)
    {
        return -particleMass * (particles[i].pressure + particles[j].pressure) / (2.0f * particles[j].density) *
            SmoothingKernels.GradientSpiky(rij, smoothingRadius, r);
    }

    public Vector3 averageParticleVelocity()
    {
        Vector3 average = Vector3.zero;
        for (int i = 0; i < amount; i++)
        {
            average += particles[i].velocity;
        }

        return average / amount;
    }

    public void createObstacle(bool createObj)
    {
        /* if (createObj)
         {
             objs = new SPH.SPHObject();
             // Random.seed = 52794;
             //var i = 15;
             // string sphere = string.Format("Sphere ({0})" , i.ToString());
             objFather = new GameObject("objFather");
             GameObject gameObj = GameObject.Find("ObjsSpawn").gameObject;
             float x = gameObj.transform.position.x;
             float y = gameObj.transform.position.y;
             float z = gameObj.transform.position.z;

             GameObject go = Instantiate(character0Prefab);
             //  go.
             go.transform.SetParent(objFather.transform);
             go.transform.localScale = Vector3.one * 4;
             go.transform.position = new Vector3(x, y, z);
             go.name = "collider";// + i.ToString();

             objs.Init(new Vector3(x, y, z), parameterID, go);
             objectCollider = true;
         }
         else
         {
             Destroy(objFather);
             objectCollider = false;
         }*/
    }
}
