using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using SPH;
using Grid;
public class HashSolver : MonoBehaviour, Interface
{
    // Consts
    private static Vector3 GRAVITY = new Vector3(0.0f, -9.81f, 0.0f);
    private const float GAS_CONST = 2000.0f;
    private const float DT =  0.0008f;
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
    private List<int> oldIterated = new List<int>();
    private bool flag = false;

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
    public HashGrid _hashGrid;
    public RenderTexture Volume;
    public GameObject spawn;
    public bool paintMesh;
    public bool foamEffect;

    public GameObject gm;
    //public Data data;
   // public static B s1;
    //public Material m_volumeMat;
    private ComputeShader m_shader;
    private GameObject m_mesh;
    private GameObject particlesFather;
    private int focosParticle;
    public void setParameters(float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass, float particleViscosity,
        float particleDrag, /*string type,*/ GameObject spawn, GameObject mesh, int amount, int rowSize, Material mat, float particlePercentage, Material focos, Material iterated, Material adjacent,bool paintMesh, int focosParticle, bool foamEffect)
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
        this.foamEffect= foamEffect;
    }

    public void InitSPH(string type)
    {
          _hashGrid = gameObject.GetComponent<HashGrid>();//new HashGrid();
       // _hashGrid = new HashGrid();
        if (type == "Group")
        {
            InitSPHGroup();
        }
       
        _hashGrid.InitNeighbourHashing(amount, smoothingRadius);
    }

   
    public void InitSPHGroup()
    {

        particles = new SPH.SPHParticle[amount];
        Random.seed = 52794;
        particlesFather = new GameObject();
        for (int i = 0; i < amount; i++)
        {
            float jitter = (Random.value * 2f - 1f) * particleRadius * 0.1f;
            float x = spawn.transform.position.x + (i % rowSize) + Random.Range(-0.1f, 0.1f);
            float y = spawn.transform.position.y + 2 + (float)((i / rowSize) / rowSize) * 1.1f;
            float z = spawn.transform.position.z + ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f);

            GameObject go = Instantiate(character0Prefab);
            //  go.
            go.transform.SetParent(particlesFather.transform);
            go.transform.localScale = Vector3.one * particleRadius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter);
            go.name = "char" + i.ToString();
            if (m_volumeMat != null)
                go.GetComponent<MeshRenderer>().material = m_volumeMat;
            if (foamEffect)
            {
                go.AddComponent<SpeedCalc>();
                go.AddComponent<LerpColor>();
            }

            particles[i].Init(new Vector3(x, y, z), parameterID, go);
            
            countReadyParticles++;
            // createMesh();
        }
       // gm = GameObject.Find("Sphere").gameObject;
       // particles[amount].Init(gm.gameObject.transform.position, parameterID, gm);


    }
    void InitSPHOneByOne()
    {
        //Not used in this solver
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

        for (int i = 0; i < countReadyParticles/*particles.Length*/; i++)
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
            particles[i].velocity += DT * (particles[i].forcePhysic) / particles[i].density;// particleMass; //particles[i].density;
            particles[i].position += DT * (particles[i].velocity);
            
            // print("particule " + i + "density " + particles[i].velocity);
        }
    }

    public void ComputeDensityPressure()
    {
        //Debug.Log("Init b");
        _hashGrid.ClearHashGrid(countReadyParticles);
        _hashGrid.CalcParticleHashes(particles);
        _hashGrid.CheckParticleInterations(particles, smoothingRadiusSq);

        for (int i = 0; i < countReadyParticles; i++)
        {
            particles[i].density = 0.0f;
            for (int j = 0; j < _hashGrid._neighbourTracker[i]; j++)
            {
                Vector3 rij;
                float r2;
                int otherParticle = _hashGrid._neighbourList[i * _hashGrid.maximumParticlesPerCell * 8 + j];
                distBetweenParticles(i, otherParticle, out rij, out r2);
                //Smoothing Range verification already done in hash grid
                if (i == 945)
                    print("densityPressure i: " + i + " j " + otherParticle);
                particles[i].density += particleDensity(i, r2);//particleMass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f))) * Mathf.Pow(smoothingRadiusSq - r2, 3.0f);
                //  }
            }

            
           // particles[i].density += 0.000001f;
            particles[i].pressure = particlePressure(i);//GAS_CONST * (particles[i].density - restDensity);
            
            //  print("particule " + i + "density " + particles[i].pressure);
        }
    }



    public void ComputeForces()
    {
        
        //1- Clean the old interaction particles
        if(paintMesh)
            interactionColors(1,0,0.0,0);

        //2- Paint mesh of focus Particle
        if(paintMesh)
            interactionColors(2, 0, focosParticle, 0);
        for (int i = 0; i < countReadyParticles /*particles.Length*/; i++)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;

            // Physics
            for (int j = 0; j < _hashGrid._neighbourTracker[i]/* particles.Length*/; j++)
            {
                int otherParticle = _hashGrid._neighbourList[i * _hashGrid.maximumParticlesPerCell * 8 + j];

                //  if (i == otherParticle) continue;

                //3-Paint the iterated Particles
                if(paintMesh)
                    interactionColors(3, i,focosParticle, otherParticle);
                                
                Vector3 rij;
                float r2;
                distBetweenParticles(i, otherParticle, out rij, out r2);
                
                if (r2 > 0.0f)
                {
                    if (i == 945)
                        print("computeForces i: " + i + " j " + otherParticle);
                    //4-Paint the interaction particles
                    if (paintMesh)
                        interactionColors(4, i,focosParticle, otherParticle);
                    
                    float r = Mathf.Sqrt(r2);//meter dentro do if
                    ///   var direction = (particles[i].position - particles[otherParticle].position) / r;
                    //if (r < smoothingRadius)//r-> r2
                    // {
                    
                    forcePressure += pressureForce(i, otherParticle, rij, r);//-rij.normalized * particleMass * (particles[i].pressure + particles[j].pressure) / (2.0f * particles[j].density) * (-45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * Mathf.Pow(smoothingRadius - r, 2.0f);
                    forceViscosity += viscosityForce(i, otherParticle, r);//particleViscosity * particleMass * (particles[j].velocity - particles[i].velocity) / particles[j].density * (45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * (smoothingRadius - r);
                    ///forcePressure -= (particleMass * particleMass) * (particles[i].pressure / (particles[i].density * particles[i].density) + particles[otherParticle].pressure / (particles[otherParticle].density * particles[otherParticle].density)) * SmoothingKernels.SpikyGradient(r,direction,rij,smoothingRadius);
                    /// forceViscosity += particleViscosity * (particleMass * particleMass) * (particles[otherParticle].velocity - particles[i].velocity) / particles[otherParticle].density * SmoothingKernels.SpikySecondDerivative(smoothingRadius, r);
                    // }
                    /// }
                }              
            }
            // Apply
            
            particles[i].forcePhysic = forcePressure + forceViscosity + gravityForce(i);
            //print(i + " grav " + particles[i].forcePhysic);
        }
    }



    public void ApplyPosition()
    {
        for (int i = 0; i < countReadyParticles; i++)
        {
            if (particles[i].go != null /*&& _hashGrid._hashGrid.ContainsKey(SpatialHashing.Hash(SpatialHashing.GetCell(particles[i].position)))*/)
            {
                particles[i].go.transform.position = particles[i].position;
            }
        }
    }

    public void cleanParticles()
    {
        print("destroy hash");
        Destroy(particlesFather);
    }

    //Method used to paint the diferent particles that are computed in interaction with the focus particle.
    private void interactionColors(int type, int i, double focosParticle, int otherParticle)
    {
        if(type == 1)
        {
            if (flag)
                for (int k = 0; k < oldIterated.Count; k++)
                    particles[oldIterated[k]].go.GetComponent<MeshRenderer>().material = m_volumeMat;//m_iterated;
        }
        if(type == 2)
        {
            if (m_volumeMat != null /*&& i == focosParticle*/ && m_focos != null && m_adjacent != null)
                particles[(int)focosParticle].go.GetComponent<MeshRenderer>().material = m_focos;
        }
        if(type == 3)
        {
            if (m_volumeMat != null && m_focos != null && m_adjacent != null && i == focosParticle && otherParticle != focosParticle)
                particles[otherParticle].go.GetComponent<MeshRenderer>().material = m_iterated;
        }
        if(type == 4)
        {
            if (m_volumeMat != null && m_focos != null && m_adjacent != null && i == focosParticle && otherParticle != focosParticle)
            {
                particles[otherParticle].go.GetComponent<MeshRenderer>().material = m_adjacent;
                // oldIterated[j] = otherParticle;
                oldIterated.Add(otherParticle);
                flag = true;
            }
        }
    }
    ///////////                                            Calculos auxiliares                                          ///////////////



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
