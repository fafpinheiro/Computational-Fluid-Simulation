using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using SPH;
using Grid;
public class teste : MonoBehaviour, Interface
{
    // Consts
    private static Vector3 GRAVITY = new Vector3(0.0f, -9.81f, 0.0f);
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
    public SPH.SPHObject objs;
    private int countReadyParticles = 0;
    private List<int> oldIterated = new List<int>();
    private bool flag = false;
    private bool objectCollider = false;
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
    private GameObject objFather;
    private int focosParticle;

    private Vector3 forcas = new Vector3(0,0,0);

    private GameObject drawParticle;
    public List<GameObject> drawParticleList = new List<GameObject>();
    public void setParameters(float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass, float particleViscosity,
        float particleDrag, /*string type,*/ GameObject spawn, GameObject mesh, int amount, int rowSize, Material mat, float particlePercentage, Material focos, Material iterated, Material adjacent, bool paintMesh, int focosParticle, bool foamEffect)
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
        this.foamEffect = foamEffect;
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

           /* drawParticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            drawParticle.AddComponent<Rigidbody>();
            drawParticle.transform.position = new Vector3(x + jitter, y, z + jitter);
            drawParticle.transform.localScale = Vector3.one * particleRadius;
            drawParticle.name = "FluidParticle";

            drawParticleList.Add(drawParticle);*/

             GameObject go = Instantiate(character0Prefab);
             //  go.
             go.transform.SetParent(particlesFather.transform);
             go.transform.localScale = Vector3.one * particleRadius;
             go.transform.position = new Vector3(x + jitter, y, z + jitter);
             go.name = "char" + i.ToString();
          //   go.AddComponent<SphereCollider>();
           //  go.AddComponent<Rigidbody>();
           // go.GetComponent<Rigidbody>().useGravity = false;
         //   go.GetComponent<Rigidbody>().mass = 5;
             if (m_volumeMat != null)
                 go.GetComponent<MeshRenderer>().material = m_volumeMat;
             if (foamEffect)
             {
                 go.AddComponent<SpeedCalc>();
                 go.AddComponent<LerpColor>();
             }

             particles[i].Init(new Vector3(x, y, z), parameterID, go);
          //  particles[i].Init(drawParticle.transform.position, parameterID, drawParticle);
            countReadyParticles++;
        }
       // createObstacle(true);


    }
   /* public void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            print(objs.forcePhysic);
            //   objs.forcePhysic += new Vector3(1000, 0, 0);
            forcas = new Vector3(10000, 0, 0);
            print(objs.forcePhysic);
          //  ComputeDensityPressureCol();
          //  ComputeForcesCol();
          //  IntegrateCol();
          //  ApplyPositionCol();
           // ApplyPosition();
        }
        if (Input.GetKey(KeyCode.D))
            objs.forcePhysic += new Vector3(-3, 0, 0);
        if (Input.GetKey(KeyCode.W))
            objs.forcePhysic += new Vector3(0, 0, 3);
        if (Input.GetKey(KeyCode.S))
            objs.forcePhysic += new Vector3(0, 0, -3);
       
        
    }*/
    void InitSPHOneByOne()
    {
        //Not used in this solver
    }

    private static bool Intersect(SPH.SPHCollider collider, Vector3 position, float radius, out Vector3 penetrationNormal, out Vector3 penetrationPosition, out float penetrationLength)
    {
        Vector3 colliderProjection = collider.position - position;
       // var blue = Vector3.Cross(collider.right, collider.up);
        penetrationNormal = Vector3.Cross(collider.right,collider.up); //blue axis (cross between red and green)
        penetrationLength = Mathf.Abs(Vector3.Dot(colliderProjection, penetrationNormal)) - (radius / 2.0f);
        penetrationPosition = collider.position - colliderProjection;

        return penetrationLength < 0.0f
            && Mathf.Abs(Vector3.Dot(colliderProjection, collider.right)) < collider.scale.x
            && Mathf.Abs(Vector3.Dot(colliderProjection,collider.up)) < collider.scale.y;
    }



    private static Vector3 DampVelocity(SPH.SPHCollider collider, Vector3 velocity, Vector3 penetrationNormal, float drag)
    {
        Vector3 newVelocity = Vector3.Dot(velocity, penetrationNormal/*Vector3.Cross(collider.right,Vector3.Cross(collider.right,collider.up))*/) /** Vector3.Cross(collider.right, Vector3.Cross(collider.right, collider.up)) *drag*/* penetrationNormal * BOUND_DAMPING
                            + Vector3.Dot(velocity, collider.right) * collider.right * drag
                            + Vector3.Dot(velocity, collider.up/*penetrationNormal*/) * /*penetrationNormal *BOUND_DAMPING*/collider.up * drag;
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
        if(objectCollider)
            for (int j = 0; j < colliders.Length; j++)
            {
                Vector3 penetrationNormal2;
                Vector3 penetrationPosition2;
                float penetrationLength2;
                if (Intersect(colliders[j], objs.position, 4, out penetrationNormal2, out penetrationPosition2, out penetrationLength2))
                {
                    objs.velocity = DampVelocity(colliders[j], objs.velocity, penetrationNormal2, 1.0f - particleDrag);
                    objs.position = penetrationPosition2 - penetrationNormal2 * Mathf.Abs(penetrationLength2);
                }
            }
    }



    public void Integrate()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].velocity += DT * (particles[i].forcePhysic) / particles[i].density;// particleMass; //particles[i].density;
            particles[i].position += DT * (particles[i].velocity);
        }
        if (objectCollider)
        {
            objs.velocity += DT * (objs.forcePhysic) / particleMass;
            objs.position += DT * (objs.velocity);
        }
        
    }
    /* public void IntegrateCol()
     {

         if (objectCollider)
         {
             objs.velocity += DT * (objs.forcePhysic) / particleMass;
             objs.position += DT * (objs.velocity);
         }

     }*/

   /* public void IntegrateOne()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].velocity += DT * (particles[i].forcePhysic) / particleMass;// particleMass; //particles[i].density;
            particles[i].position += DT * (particles[i].velocity);
        }
        

    }
    private void reCalc()
    {
        for(int i = 0; i < countReadyParticles; i++)
        {
            particles[i].position = particles[i].go.transform.position;
            
        }
        IntegrateOne();
    }*/

    public void ComputeDensityPressure()
    {
        
        _hashGrid.ClearHashGrid(countReadyParticles);
      //  reCalc();
        _hashGrid.CalcParticleHashes(particles);
        _hashGrid.CheckParticleInterations(particles, smoothingRadiusSq);
        objs.density = particleDensityObj(0, 0, 5);
        for (int i = 0; i < countReadyParticles; i++)
        {
            

            particles[i].density = 0.0f;
            //objs.density = 0.0f;
            for (int j = 0; j < _hashGrid._neighbourTracker[i]; j++)
            {
                Vector3 rij;
                float r2;
                int otherParticle = _hashGrid._neighbourList[i * _hashGrid.maximumParticlesPerCell * 8 + j];

                

                distBetweenParticles(i, otherParticle, out rij, out r2);
                //Smoothing Range verification already done in hash grid
                particles[i].density += particleDensity(i, r2);
            }
            particles[i].density += 0.000001f;
           // particles[i].pressure = particlePressure(i);//GAS_CONST * (particles[i].density - restDensity);

            if (objectCollider)
            {
                Vector3 rij2 = objs.position - particles[i].position;
                float r22 = rij2.sqrMagnitude;

               // objs.density = particleDensityObj(i, 0,60);
                if (r22 < smoothingRadiusSq)
                {
                    objs.density += particleDensityObj(i, r22,5);
                    particles[i].density += particleDensity(i, r22);
                }
                //objs.pressure = particlePressureObj(i);
            }
            particles[i].pressure = particlePressure(i);
            //print("densidade " + " i : " + i + particles[i].density + "pressão " + " i : " + i + particles[i].pressure);
        }
        objs.pressure = particlePressureObj();
    }

   /* public void ComputeDensityPressureCol()
    {
        for (int i = 0; i < countReadyParticles; i++)
        {
            
            objs.density = 0.0f;
            
            if (objectCollider)
            {
                Vector3 rij2 = objs.position - particles[i].position;
                float r22 = rij2.sqrMagnitude;

                // print("deu");
                objs.density = particleDensityObj(i, 0, 40);
                if (r22 < smoothingRadiusSq)
                {
                    objs.density += particleDensityObj(i, r22, 40);
                    particles[i].density += particleDensity(i, r22);//particleMass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f))) * Mathf.Pow(smoothingRadiusSq - r2, 3.0f);
                }
                objs.pressure = particlePressureObj(i);
            }
        }
    }*/


    public void ComputeForces()
    {

        //1- Clean the old interaction particles
        if (paintMesh)
            interactionColors(1, 0, 0.0, 0);

        //2- Paint mesh of focus Particle
        if (paintMesh)
            interactionColors(2, 0, focosParticle, 0);
        Vector3 forcePressureCollider = Vector3.zero;
        Vector3 forceViscosityCollider = Vector3.zero;
        for (int i = 0; i < countReadyParticles ; i++)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;
            
            // Physics
            for (int j = 0; j < _hashGrid._neighbourTracker[i]; j++)
            {
                int otherParticle = _hashGrid._neighbourList[i * _hashGrid.maximumParticlesPerCell * 8 + j];

                //  if (i == otherParticle) continue;

                //3-Paint the iterated Particles
                if (paintMesh)
                    interactionColors(3, i, focosParticle, otherParticle);

                Vector3 rij;
                float r2;
                distBetweenParticles(i, otherParticle, out rij, out r2);

                if (r2 > 0.0f)
                {
                    //4-Paint the interaction particles
                    if (paintMesh)
                        interactionColors(4, i, focosParticle, otherParticle);

                    float r = Mathf.Sqrt(r2);//meter dentro do if

                    forcePressure += pressureForce(i, otherParticle, rij, r);
                    forceViscosity += viscosityForce(i, otherParticle, r);
                }
            }
            // Apply
            if (objectCollider)
            {
                Vector3 rij2 = objs.position - particles[i].position;
                float r22 = rij2.sqrMagnitude;
                float r23 = Mathf.Sqrt(r22);
                if (r23 < smoothingRadius)
                {
                    forcePressure += pressureForceObj(i, rij2, r23,particleMass);
                    forceViscosity += viscosityForceObj(i, r23,particleMass);
                    forcePressureCollider += pressureForceObj(i, rij2, r23,5);
                    forceViscosityCollider += viscosityForceObj(i, r23,5);
                   // print("1 " + forcePressureCollider);
                   // print("2 " +forceViscosityCollider);
                }
               
               // print("3 " +objs.forcePhysic);
            }
            particles[i].forcePhysic = forcePressure + forceViscosity + gravityForce(i);
            
        }
        objs.forcePhysic = (-forcePressureCollider) + (-forceViscosityCollider) + gravityForceObj(5) + forcas;
    }

    /*public void ComputeForcesCol()
    {

        
        Vector3 forcePressureCollider = Vector3.zero;
        Vector3 forceViscosityCollider = Vector3.zero;
        for (int i = 0; i < countReadyParticles; i++)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;

            
            // Apply
            if (objectCollider)
            {
                Vector3 rij2 = objs.position - particles[i].position;
                float r22 = rij2.sqrMagnitude;
                float r23 = Mathf.Sqrt(r22);
                if (r23 < smoothingRadius)
                {
                    forcePressure += pressureForceObj(i, rij2, r23, particleMass);
                    forceViscosity += viscosityForceObj(i, r23, particleMass);
                  //  forcePressureCollider += -pressureForceObj(i, rij2, r23, 60);
                   // forceViscosityCollider += -viscosityForceObj(i, r23, 60);
                    // print("1 " + forcePressureCollider);
                    // print("2 " +forceViscosityCollider);
                }
               // objs.forcePhysic += forcePressureCollider + forceViscosityCollider + gravityForceObj(10);
                // print("3 " +objs.forcePhysic);
            }
            particles[i].forcePhysic = forcePressure + forceViscosity + gravityForce(i);

        }
    }*/

    public void ApplyPosition()
    {
        for (int i = 0; i < countReadyParticles; i++)
        {
            if (particles[i].go != null /*&& _hashGrid._hashGrid.ContainsKey(SpatialHashing.Hash(SpatialHashing.GetCell(particles[i].position)))*/)
            {
                particles[i].go.transform.position = particles[i].position;
            }
        }
        if(objs.go != null && objectCollider)
            objs.go.transform.position = objs.position;
    }
   /* public void ApplyPositionCol()
    {
        
        if (objs.go != null && objectCollider)
            objs.go.transform.position = objs.position;
       
    }*/

    public void cleanParticles()
    {
        print("destroy hash");
        Destroy(particlesFather);
    }

    //Method used to paint the diferent particles that are computed in interaction with the focus particle.
    private void interactionColors(int type, int i, double focosParticle, int otherParticle)
    {
        if (type == 1)
        {
            if (flag)
                for (int k = 0; k < oldIterated.Count; k++)
                    particles[oldIterated[k]].go.GetComponent<MeshRenderer>().material = m_volumeMat;//m_iterated;
        }
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
                oldIterated.Add(otherParticle);
                flag = true;
            }
        }
    }
    ///////////                                            Calculos auxiliares                                          ///////////////

    private Vector3 viscosityForceObj(int i,  float r, float mass)
    {
        return particleViscosity * mass * (objs.velocity - particles[i].velocity) / objs.density * SmoothingKernels.ViscosityLaplacian(smoothingRadius, r);
    }

    //Calculate the pressure force
    private Vector3 pressureForceObj(int i,  Vector3 rij, float r, float mass)
    {
        return -mass * (particles[i].pressure + objs.pressure) / (2.0f * objs.density) *
            SmoothingKernels.GradientSpiky(rij, smoothingRadius, r);
    }
    private Vector3 gravityForceObj(float mass)
    {
        //print(objs.density);
        return GRAVITY * objs.density * gravityMult;
    }

    private float particleDensityObj(int i, float r2, float mass)
    {
        // print(particleMass * SmoothingKernels.Poly6(smoothingRadius, smoothingRadiusSq, r2));
        return mass * SmoothingKernels.Poly6(smoothingRadius, smoothingRadiusSq, r2);
    }

    private float particlePressureObj()
    {
        return GAS_CONST * (objs.density - restDensity);
    }

    //Calculate the distance between two particles
    private void distBetweenParticles(int i, int j, out Vector3 rij, out float r2)
    {
        rij = particles[j].position - particles[i].position;
        r2 = rij.sqrMagnitude;
    }

    //Create a gravity field
    private Vector3 gravityForce(int i)
    {
       // print(particles[i].density);
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
       // print(particleMass * SmoothingKernels.Poly6(smoothingRadius, smoothingRadiusSq, r2));
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
        if (createObj)
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
            go.transform.localScale = Vector3.one * 2;
            go.transform.position = new Vector3(x, y, z);
            go.name = "collider";// + i.ToString();

            objs.Init(new Vector3(x, y, z), parameterID, go);
            objectCollider = true;
          //  go.AddComponent<playerMovement>();
           // go.GetComponent<playerMovement>().teste = gameObject;
        }
        else
        {
            Destroy(objFather);
            objectCollider = false;
        }
    }
}
