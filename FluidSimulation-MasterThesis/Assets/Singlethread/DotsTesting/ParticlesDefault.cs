using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using SPH;
using Unity.Jobs;
using Unity.Collections;


public struct computeDensityPressureJob : IJobParallelFor
{
    [ReadOnly] public int amount;
    [ReadOnly] public NativeArray<Vector3> particlesPositionArray;

    public NativeArray<float> particlesDensityArray;
    public NativeArray<float> particlesPressureArray;
    public NativeArray<ParticlesDefault.Data> particlesDefaultDataArray;

    public void Execute(int index)
    {
        var datas = particlesDefaultDataArray[index];
        datas.ComputeDensityPressure(index, amount, particlesDensityArray, particlesPressureArray, particlesPositionArray);
    }
}

public struct computeForcesJob : IJobParallelFor
{
    [ReadOnly] public int amount;
    [ReadOnly] public NativeArray<float> particlesDensityArray;
    [ReadOnly] public NativeArray<float> particlesPressureArray;
    [ReadOnly] public NativeArray<Vector3> particlesPositionArray;
    [ReadOnly] public NativeArray<Vector3> particlesVelocityArray;

    public NativeArray<Vector3> particlesForceArray;
    public NativeArray<ParticlesDefault.Data> particlesDefaultDataArray;
    public void Execute(int index)
    {
        var datas = particlesDefaultDataArray[index];
        datas.ComputeForces(index, amount, particlesDensityArray, particlesPressureArray, particlesPositionArray, particlesVelocityArray, particlesForceArray);
    }
}

public struct IntegrateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> particlesDensityArray;
    [ReadOnly] public NativeArray<Vector3> particlesForceArray;

    public NativeArray<Vector3> particlesPositionArray;
    public NativeArray<Vector3> particlesVelocityArray;
    public NativeArray<ParticlesDefault.Data> particlesDefaultDataArray;
    public void Execute(int index)
    {
        var datas = particlesDefaultDataArray[index];
        datas.Integrate(index, particlesPositionArray, particlesVelocityArray, particlesForceArray, particlesDensityArray);
    }
}

public struct ComputeCollidersJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<SPH.SPHCollider> collidersArray;

    public NativeArray<Vector3> particlesPositionArray;
    public NativeArray<Vector3> particlesVelocityArray;
    public NativeArray<ParticlesDefault.Data> particlesDefaultDataArray;
    public void Execute(int index)
    {
        var datas = particlesDefaultDataArray[index];
        datas.ComputeColliders(index, collidersArray, particlesPositionArray, particlesVelocityArray);
    }
}

public struct ApplyPositionsJob : IJobParallelFor
{
    public NativeArray<Vector3> particlesPositionArray;
    public NativeArray<ParticlesDefault.Data> particlesDefaultDataArray;
    public void Execute(int index)
    {
        var datas = particlesDefaultDataArray[index];
        datas.ApplyPosition(index, particlesPositionArray);
    }
}

public struct setDensity : IJobParallelFor//(NativeArray<float> density)
{
    public NativeArray<float> particlesDensityArray;
    public void Execute(int index)
    {
        particlesDensityArray[index] = 0.0f;
    }
    /*for (int i = 0; i < density.Length; i++)
        density[i] = 0.0f;*/
}

public struct setPressure : IJobParallelFor//(NativeArray<float> pressure)
{
    public NativeArray<float> particlesPressureArray;
    public void Execute(int index)
    {
        particlesPressureArray[index] = 0.0f;
    }
    /* for (int i = 0; i < pressure.Length; i++)
         pressure[i] = 0.0f;*/
}

public struct setForce: IJobParallelFor//(NativeArray<Vector3> force)
{
    public NativeArray<Vector3> particlesForceArray;
    public void Execute(int index)
    {
        particlesForceArray[index] = Vector3.zero;
    }
    /*for (int i = 0; i < force.Length; i++)
        force[i] = Vector3.zero;*/
}

public class ParticleManager : MonoBehaviour
{
    //private SPH.SPHParticle[] particles;
    [Header("Import")]
    [SerializeField] private GameObject character0Prefab;
    [SerializeField] private Material m_volumeMat;
    [SerializeField] private Material m_focos;
    [SerializeField] private Material m_iterated;
    [SerializeField] private Material m_adjacent;
    [Header("Parameters")]
    [SerializeField] private int parameterID;


    // Data
    //  [ReadOnly] public NativeArray<SPH.SPHParticle> particles1;
    public NativeArray<SPH.SPHParticle> /*SPH.SPHParticle[]*/ particles;
    private int countReadyParticles;

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
    public bool foamEffect;
    //public Material mat;

    //#pragma warning restore 0649
    public GameObject spawn;
    public bool paintMesh;
    public static B s1;

    private GameObject particlesFather;
    private int focosParticle;

    public Data data;
    public NativeArray<ParticlesDefault.Data> particlesDefaultDataArray;
    public NativeArray<Vector3> particlesPositionArray;
    public NativeArray<Vector3> particlesVelocityArray;
    public NativeArray<Vector3> particlesForceArray;
    public NativeArray<float> particlesPressureArray;
    public NativeArray<float> particlesDensityArray;
    public NativeArray<SPH.SPHCollider> collidersArray;

    public void initParticles()
    {
        particlesDefaultDataArray = new NativeArray<ParticlesDefault.Data>(amount, Allocator.TempJob);
        particlesPositionArray = new NativeArray<Vector3>(amount, Allocator.TempJob);
        particlesVelocityArray = new NativeArray<Vector3>(amount, Allocator.TempJob);

        particlesForceArray = new NativeArray<Vector3>(amount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        particlesPressureArray = new NativeArray<float>(amount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        particlesDensityArray = new NativeArray<float>(amount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        // NativeArray<Vector3> particlesGameObjectArray = new NativeArray<Vector3>(particles.Length, Allocator.TempJob);
        Random.seed = 52794;
        for (var i = 0; i < particles.Length; i++)
        {
            particlesDefaultDataArray[i] = new ParticlesDefault.Data();
            particlesDefaultDataArray[i].setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                data.particleMass, data.particleViscosity, data.particleDrag, data.spawn, data.character0Prefab, data.amount, data.rowSize, data.m_volumeMat, data.m_focosParticle, data.m_iteratedParticle, data.m_adjacentParticle, true, data.analisedParticle, false);
            float jitter = (Random.value * 2f - 1f) * particleRadius * 0.1f;
            float x = spawn.transform.position.x + (i % rowSize) + Random.Range(-0.1f, 0.1f);
            float y = spawn.transform.position.y + 2 + (float)((i / rowSize) / rowSize) * 1.1f;
            float z = spawn.transform.position.z + ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f);

            GameObject go = Instantiate(character0Prefab);
            go.transform.SetParent(particlesFather.transform);
            //  go.
            go.transform.localScale = Vector3.one * particleRadius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter);
            go.name = "char" + i.ToString();
            // go.AddComponent<SphereCollider>();
            // go.AddComponent<Rigidbody>();
            // go.GetComponent<Rigidbody>().mass = 5;
            if (m_volumeMat != null)
                go.GetComponent<MeshRenderer>().material = m_volumeMat;
            particles[i].Init(new Vector3(x, y, z), parameterID, go);
            //particlesDataArray[i].Init(new Vector3(x, y, z), parameterID, go);

            particlesPositionArray[i] = new Vector3(x, y, z);
            particlesVelocityArray[i] = Vector3.zero;
            particlesForceArray[i] = Vector3.zero;
            particlesPressureArray[i] = 0.0f;
            particlesDensityArray[i] = 0.0f;
            // SPH.SPHParticle aux= new SPH.SPHParticle();
            // aux.Init(new Vector3(x, y, z), parameterID, go);
            // particlesGameObjectArray[i] = aux.go.transform.position;
            // countReadyParticles++;
        }
    }

    protected void /*OnStartRunning*/Start()
    {
        // Get the colliders
        GameObject[] collidersGO = GameObject.FindGameObjectsWithTag("SPHCollider");
        collidersArray = new NativeArray<SPH.SPHCollider>(collidersGO.Length, Allocator.TempJob);
        for (int i = 0; i < collidersArray.Length; i++)
        {
            collidersArray[i].Init(collidersGO[i].transform);
        }
    }

    
    public JobHandle ComputeJobs(JobHandle inputDeps)
    {
        //falta um forzin
        setDensity densityJob = new setDensity
        {
            particlesDensityArray = particlesDensityArray
        };
        setPressure pressureJob = new setPressure
        {
            particlesPressureArray = particlesPressureArray
        };
        setForce forceJob = new setForce
        {
            particlesForceArray = particlesForceArray
        };

        JobHandle particlesPressureJobHandle = densityJob.Schedule(amount, 64, inputDeps);
        JobHandle particlesDensityJobHandle = pressureJob.Schedule(amount, 64, inputDeps);
        JobHandle particlesForceJobHandle = forceJob.Schedule(amount, 64, inputDeps);

        JobHandle mergedDensityPressureJobHandle = JobHandle.CombineDependencies(particlesPressureJobHandle, particlesDensityJobHandle);

        computeDensityPressureJob jobDensityPressure = new computeDensityPressureJob
        {
            amount = amount,
            particlesPositionArray = particlesPositionArray,
            particlesDensityArray = particlesDensityArray,
            particlesPressureArray = particlesPressureArray,
            particlesDefaultDataArray = particlesDefaultDataArray
        };
        JobHandle computeDensityPressureJobHandle = jobDensityPressure.Schedule(particles.Length, 64, mergedDensityPressureJobHandle);

        JobHandle mergedDensityPressureForceJobHandle = JobHandle.CombineDependencies(computeDensityPressureJobHandle, particlesForceJobHandle);
        //computeDensityPressureJobHandle.Complete();
        computeForcesJob jobComputeForces = new computeForcesJob
        {
            amount = amount,
            particlesDensityArray = particlesDensityArray,
            particlesPressureArray = particlesPressureArray,
            particlesPositionArray = particlesPositionArray,
            particlesVelocityArray = particlesVelocityArray,
            particlesForceArray = particlesForceArray,
            particlesDefaultDataArray = particlesDefaultDataArray
        };
        JobHandle computeForcesJobHandle = jobComputeForces.Schedule(amount, 64, mergedDensityPressureForceJobHandle);

        IntegrateJob JobIntegrate = new IntegrateJob
        {
            particlesDensityArray = particlesDensityArray,
            particlesForceArray = particlesForceArray,
            particlesPositionArray = particlesPositionArray,
            particlesVelocityArray = particlesVelocityArray,
            particlesDefaultDataArray = particlesDefaultDataArray
        };
        JobHandle integrateJobHandle = JobIntegrate.Schedule(amount, 64, computeForcesJobHandle);
        //fica a faltar o merge com o colliderJobHandler
        //JobHandle mergedIntegrateCollider = JobHandle.CombineDependencies(integrateJobHandle, collidersToNativeArrayJobHandle);
      ///  JobHandle mergedIntegrateCollider = JobHandle.CombineDependencies(integrateJobHandle, collidersToNativeArrayJobHandle);

        // Compute Colliders
        ComputeCollidersJob JobComputeColliders = new ComputeCollidersJob
        {
            collidersArray = collidersArray,
            particlesPositionArray = particlesPositionArray,
            particlesVelocityArray = particlesVelocityArray,
            particlesDefaultDataArray = particlesDefaultDataArray
        };
        JobHandle computeCollidersJobHandle = JobComputeColliders.Schedule(amount, 64, /*mergedIntegrateCollider*/integrateJobHandle);

        ApplyPositionsJob applyPositionsJob = new ApplyPositionsJob
        {
            particlesPositionArray = particlesPositionArray,
            particlesDefaultDataArray = particlesDefaultDataArray
        };
        JobHandle applyTranslationsJobHandle = applyPositionsJob.Schedule(amount,64, computeCollidersJobHandle);

        inputDeps = applyTranslationsJobHandle;

        //uniqueTypes.Clear();
        return inputDeps;

    }

    public void OnStopRunning()
    {
        particlesDefaultDataArray.Dispose();
        particlesPositionArray.Dispose();
        particlesVelocityArray.Dispose();
        particlesForceArray.Dispose();
        particlesPressureArray.Dispose();
        particlesDensityArray.Dispose();
        collidersArray.Dispose();
        for(int i =0; i < particlesDefaultDataArray.Length; i++)
            particlesDefaultDataArray[i].particles.Dispose();
            
    }
}

public class ParticlesDefault : MonoBehaviour//, Interface
{
    public struct Data {
        // Consts
        private static Vector3 GRAVITY = new Vector3(0f, -9.81f, 0.0f);
        private const float GAS_CONST = 2000.0f;
        private const float DT = 0.0008f;
        private const float BOUND_DAMPING = -0.5f;

        // Properties
        [Header("Import")]
        [SerializeField] private GameObject character0Prefab;
        [SerializeField] private Material m_volumeMat;
        [SerializeField] private Material m_focos;
        [SerializeField] private Material m_iterated;
        [SerializeField] private Material m_adjacent;
        [Header("Parameters")]
        [SerializeField] private int parameterID;


        // Data
        public NativeArray<SPH.SPHParticle>/* SPH.SPHParticle[]*/ particles;
        private int countReadyParticles;
        private bool foamEffect;

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
        //public Material mat;

        //#pragma warning restore 0649
        // private SPH.SPHParticle[] solids;
        public GameObject spawn;
        public bool paintMesh;
        //public Data data;
        //public static B s1;
        //public Material m_volumeMat;

        private GameObject particlesFather;
        private int focosParticle;
        // private GameObject[] objectos;
        private int index;

      //  private NativeArray<SPHCollider> collidersArray;

        public void setParameters(int index,float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass, float particleViscosity, float particleDrag,
        /*string type,*/ GameObject spawn, GameObject mesh, int amount, int rowSize, Material mat, Material focos, Material iterated, Material adjacent, bool paintMesh, int focosParticle, bool foamEffect)
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
            // this.particlePercentage = particlePercentage;
            this.m_focos = focos;
            this.m_iterated = iterated;
            this.m_adjacent = adjacent;
            this.paintMesh = paintMesh;
            this.focosParticle = focosParticle;
            this.index = index;
            this.foamEffect = foamEffect;
            //  this.objectos = objectos;
        }

        /*public void InitSPH(string type, int index)
        {
            countReadyParticles = 0;
            // parameterID = 0;
            //m_volumeMat = null;
            // m_focos = null;
            // m_iterated = null;
            // m_adjacent = null;
            if (type == "Group")
            {
                InitSPHGroup(index);
            }
            else
            {
                print("else");// StartCoroutine(InitSPHOneByOne());
            }
        }*/

        /*public SPH.SPHParticle InitSPHGroup(int index)
        {

            particles = new SPH.SPHParticle[amount];

            Random.seed = 52794;

            for (int i = 0; i < amount; i++)
            {
                float jitter = (Random.value * 2f - 1f) * particleRadius * 0.1f;
                float x = spawn.transform.position.x + (i % rowSize) + Random.Range(-0.1f, 0.1f);
                float y = spawn.transform.position.y + 2 + (float)((i / rowSize) / rowSize) * 1.1f;
                float z = spawn.transform.position.z + ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f);

                GameObject go = Instantiate(character0Prefab);
                go.transform.SetParent(particlesFather.transform);
                //  go.
                go.transform.localScale = Vector3.one * particleRadius;
                go.transform.position = new Vector3(x + jitter, y, z + jitter);
                go.name = "char" + i.ToString();
                // go.AddComponent<SphereCollider>();
                // go.AddComponent<Rigidbody>();
                // go.GetComponent<Rigidbody>().mass = 5;
                if (m_volumeMat != null)
                    go.GetComponent<MeshRenderer>().material = m_volumeMat;
                particles[i].Init(new Vector3(x, y, z), parameterID, go);
                countReadyParticles++;
            }
            return particles[index];
        }*/


        /*IEnumerator InitSPHOneByOne()
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
        }*/

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



        public void ComputeColliders(int index, NativeArray<SPH.SPHCollider> collidersArray, NativeArray<Vector3> particlesPositionArray, NativeArray<Vector3> particlesVelocityArray)
        {
            // Get colliders
            GameObject[] collidersGO = GameObject.FindGameObjectsWithTag("SPHCollider");
            SPH.SPHCollider[] colliders = new SPH.SPHCollider[collidersGO.Length];
         //   colliders = SPHColliderGroup.ToComponentDataArray<SPHCollider>(Allocator.Persistent, out collidersToNativeArrayJobHandle);
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].Init(collidersGO[i].transform);
            }

            //for (int i = 0; i < countReadyParticles; i++)
           // {
                for (int j = 0; j < collidersArray.Length/*colliders.Length*/; j++)
                {
                    // Check collision
                    Vector3 penetrationNormal;
                    Vector3 penetrationPosition;
                    float penetrationLength;
                    if (Intersect(collidersArray[j], /*particles[i].position*/particlesPositionArray[index], particleRadius, out penetrationNormal, out penetrationPosition, out penetrationLength))
                    {
                        /*particles[i].velocity*/particlesVelocityArray[index] = DampVelocity(collidersArray[j], /*particles[i].velocity*/particlesVelocityArray[index], penetrationNormal, 1.0f - particleDrag);
                        /*particles[i].position*/particlesPositionArray[index] = penetrationPosition - penetrationNormal * Mathf.Abs(penetrationLength);
                    }
                }
            //}
        }



        public void Integrate(int index, NativeArray<Vector3> particlesPositionArray, NativeArray<Vector3> particlesVelocityArray, NativeArray<Vector3> particlesForceArray, NativeArray<float> particlesDensityArray)
        {
            // for (int i = 0; i < particles.Length; i++)
            // {
            /*particles[i].velocity*/particlesVelocityArray[index] += DT * (/*particles[i].forcePhysic*/particlesForceArray[index]) / /*particles[i].density*/particlesDensityArray[index];
            /*particles[i].position*/particlesPositionArray[index] += DT * (/*particles[i].velocity*/particlesVelocityArray[index]);
           // }
        }

        public void ComputeDensityPressure(int index, int amount, NativeArray<float> particlesDensityArray, NativeArray<float> particlesPressureArray, NativeArray<Vector3> particlesPositionArray)
        {
           // for (int i = 0; i < amount; i++)
           // {
                //particles[i].density = 0.0f;
                particlesDensityArray[index] = 0.0f;

                for (int j = 0; j < amount; j++)
                {
                    Vector3 rij;
                    float r2;

                    distBetweenParticles(index, j, out rij, out r2, particlesPositionArray);


                    if (r2 < smoothingRadiusSq)
                    {
                        // particles[i].density += particleDensity(i, r2);//particleMass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(smoothingRadius, 9.0f))) * Mathf.Pow(smoothingRadiusSq - r2, 3.0f);
                        particlesDensityArray[index] += particleDensity(index, r2);
                    }
                }
                //particles[i].pressure = particlePressure(i);
                particlesPressureArray[index] = particlePressure(index,particlesDensityArray);
          //  }
        }

        public void ComputeForces(int index, int amount, NativeArray<float> particlesDensityArray, NativeArray<float> particlesPressureArray, NativeArray<Vector3> particlesPositionArray, NativeArray<Vector3> particlesVelocityArray, NativeArray<Vector3> particlesForceArray)
        {

           /* if (paintMesh)
                interactionColors(2, 0, focosParticle, 0);*/
           // for (int i = 0; i < countReadyParticles /*particles.Length*/; i++)
           // {
                Vector3 forcePressure = Vector3.zero;
                Vector3 forceViscosity = Vector3.zero;

                // Physics
                for (int j = 0; j < amount/* particles.Length*/; j++)
                {
                    if (index == j) continue;

                    Vector3 rij;
                    float r2;
                    distBetweenParticles(index, j, out rij, out r2, particlesPositionArray);
                    float r = Mathf.Sqrt(r2);//meter dentro do if

                   /* if (paintMesh)
                        interactionColors(3, index, focosParticle, j);*/
                    if (r < smoothingRadius)//r-> r2
                    {
                       /* if (paintMesh)
                            interactionColors(4, index, focosParticle, j);*/
                        forcePressure += pressureForce(index, j, rij, r,particlesDensityArray, particlesPressureArray);//-rij.normalized * particleMass * (particles[i].pressure + particles[j].pressure) / (2.0f * particles[j].density) * (-45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * Mathf.Pow(smoothingRadius - r, 2.0f);
                        forceViscosity += viscosityForce(index, j, r, particlesVelocityArray,particlesDensityArray);//particleViscosity * particleMass * (particles[j].velocity - particles[i].velocity) / particles[j].density * (45.0f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6.0f))) * (smoothingRadius - r);
                    }
                }
                // Apply

                /*particles[index].forcePhysic*/particlesForceArray[index] = forcePressure + forceViscosity + gravityForce(index, particlesDensityArray);// forceGravity;
                                                                                            //  print("fisicas " + particles[i].forcePhysic);
            //}
        }



        public void ApplyPosition(int index/*, int amount*/, NativeArray<Vector3> particlesPositionArray)
        {
           // for (int i = 0; i < countReadyParticles; i++)
          //  {
            if (particles[index].go != null)
                particles[index].go.transform.position = /*particles[i].position*/particlesPositionArray[index];
                // print(i + "  " + particles[i].go.transform.position);
          //  }
        }
       /* public void cleanParticles()
        {
            print("destroy B");
            Destroy(particlesFather);
        }*/
        //Method used to paint the diferent particles that are computed in interaction with the focus particle.
       /* private void interactionColors(int type, int i, double focosParticle, int otherParticle)
        {

            if (type == 2)
            {
                if (m_volumeMat != null  && m_focos != null && m_adjacent != null)
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
                }
            }
        }*/

        ///////////                                            Calculos auxiliares                                          ///////////////


        //Calculate the distance between two particles
        private void distBetweenParticles(int i, int j, out Vector3 rij, out float r2, NativeArray<Vector3> particlesPositionArray)
        {
            //rij = particles[j].position - particles[i].position;
            rij = particlesPositionArray[j] - particlesPositionArray[i];
            r2 = rij.sqrMagnitude;
        }

        //Create a gravity field
        private Vector3 gravityForce(int i, NativeArray<float> particlesDensityArray)
        {
            return GRAVITY * /*particles[i].density*/particlesDensityArray[i] * gravityMult;
        }

        //Calculate the particle pressure
        private float particlePressure(int i, NativeArray<float> particlesDensityArray)
        {
            return GAS_CONST * (/*particles[i].density*/particlesDensityArray[i] - restDensity);
        }

        //Calclate the particle density
        private float particleDensity(int i, float r2)
        {

            return particleMass * SmoothingKernels.Poly6(smoothingRadius, smoothingRadiusSq, r2);
        }

        //Calculate the viscosity force
        private Vector3 viscosityForce(int i, int j, float r,NativeArray<Vector3> particlesVelocityArray, NativeArray<float> particlesDensityArray)
        {
            return particleViscosity * particleMass * (/*particles[j].velocity - particles[i].velocity*/particlesVelocityArray[j] - particlesVelocityArray[i]) / /*particles[j].density*/particlesDensityArray[j] * SmoothingKernels.ViscosityLaplacian(smoothingRadius, r);
        }

        //Calculate the pressure force
        private Vector3 pressureForce(int i, int j, Vector3 rij, float r, NativeArray<float> particlesDensityArray, NativeArray<float> particlesPressureArray)
        {
            return -particleMass * (/*particles[i].pressure + particles[j].pressure*/particlesPressureArray[i] + particlesPressureArray[j]) / (2.0f * /*particles[j].density*/particlesDensityArray[j]) *
                SmoothingKernels.GradientSpiky(rij, smoothingRadius, r);
        }

       /* public Vector3 averageParticleVelocity()
        {
            Vector3 average = Vector3.zero;
            for (int i = 0; i < amount; i++)
            {
                average += particles[i].velocity;
            }

            return average / amount;
        }*/

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
   
}
