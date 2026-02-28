using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C : MonoBehaviour, Interface
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
    private int countReadyParticles = 0;

    private float[] currenteDesity;
    //#pragma warning disable 0649 // This line removes the warning saying that the variable is never assigned to. You can't assign a variable in a struct...
    private float particleRadius; //Particle radius
    private float smoothingRadius; //Afecting radius of the smoothing (particles in radius that affect the particle considered)
    private float smoothingRadiusSq; //
    private float restDensity; //
    private float gravityMult; //Gravity multiplier
    private float particleMass;//Particle mass
    private float particleViscosity;
    private float particleDrag;//Drag is a force acting opposite to the relative motion of any object moving with respect to a surrounding fluid
    private int amount;
    private int rowSize;
    private float particlePercentage;
    private bool test = true;
    private List<int> oldIterated = new List<int>();
    public GameObject spawn;
    public bool paintMesh;
    //public Data data;
    public static A s1;
    private Dictionary<int, List<int>> currentParticles;
    private GameObject particlesFather;
    private bool flag;
    private int focosParticle;

    public void setParameters(float particleRadius, float smoothingRadius, float smoothingRadiusSq, float restDensity, float gravityMult, float particleMass, float particleViscosity,
        float particleDrag, /*string type,*/ GameObject spawn, GameObject mesh, int amount, int rowSize, Material mat, float particlePercentage, Material focos, Material iterated, Material adjacent, bool paintMesh, int focosParticle,bool foamEffect)
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
    }

    public void InitSPH(string type)
    {

        if (type == "Group")
        {
            InitSPHGroup();
        }
        else
        {
            StartCoroutine(InitSPHOneByOne());
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
    private void InitSPHGroup()
    {
        particles = new SPH.SPHParticle[amount];
        Random.seed = 52794;
        particlesFather = new GameObject();
        for (int i = 0; i < amount; i++)
        {
            //Random.seed = i + 1;
            float jitter = (Random.value * 2f - 1f) * particleRadius * 0.1f;
            float x = spawn.transform.position.x + (i % rowSize) + Random.Range(-0.1f, 0.1f);
            float y = spawn.transform.position.y + 2 + (float)((i / rowSize) / rowSize) * 1.1f;
            float z = spawn.transform.position.z + ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f);

            GameObject go = Instantiate(character0Prefab);
            go.transform.SetParent(particlesFather.transform);
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
   /* private void otimizado()
    {
        var dup = new Vector3[countReadyParticles];///teste count
        currenteDesity = new float[countReadyParticles];
        for (int i = 0; i < countReadyParticles; i++) ///teste do count
        {
            particles[i].density = 0.0f;
            dup[i] = Vector3.zero;
            currenteDesity[i] = 0.0f;
        }
        int rnd;
        for (int i = 0; i < countReadyParticles; i++)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;
            particles[i].density = 0.0f;
            particles[i].pressure = 0.0f;
            for (int j = i; j < countReadyParticles; j = j + rnd)
            {
                rnd = Random.Range(1, (amount / (int)(amount * particlePercentage)) * 2);
                particles[j].density = 0.0f;
                particles[j].pressure = 0.0f;
                Vector3 rij;
                float r2;
                distBetweenParticles(i, j, out rij, out r2);
                if (r2 < smoothingRadiusSq)
                {
                    float tempDensity = particleDensity(i, r2);
                    particles[i].density = tempDensity;
                    particles[i].density += particleDensity(i, 0);
                    currenteDesity[i] += tempDensity;
                    if (i != j)
                    {
                        particles[j].density = tempDensity;
                        particles[j].density += particleDensity(j, 0);
                        currenteDesity[j] += tempDensity;
                    }

                }
                particles[i].pressure = particlePressure(i);
                particles[j].pressure = particlePressure(j); 
                if (i == j) continue;
                float r = Mathf.Sqrt(r2);//meter dentro do if
                if (r < smoothingRadius)// r -> r2
                {
                    forcePressure = pressureForce(i, j, rij, r);
                    forceViscosity = viscosityForce(i, j, r);
                    dup[i] += (forcePressure + forceViscosity);
                    dup[j] += (-forcePressure + (-forceViscosity));
                }

            }
            particles[i].forcePhysic = dup[i] + GRAVITY * currenteDesity[i] * gravityMult;
        }
    }*/
    public void ComputeDensityPressure()
    {
        //print("frame1----------------------");
        if (paintMesh)
            interactionColors(1, 0, 0.0, 0);
        Dictionary<string, int> temp = new Dictionary<string, int>();
        currentParticles = new Dictionary<int, List<int>>();
        if (paintMesh)
            interactionColors(2, 0, focosParticle, 0);
        //Reset density
        for (int i = 0; i < countReadyParticles; i++) ///teste do count
        {
            particles[i].density = 0.0f;
        }
        //
        int rnd;
        for (int i = 0; i < countReadyParticles; i++)
        {
            for (int j = i; j < countReadyParticles; j = j + rnd)
            {
                rnd = Random.Range(1, (amount / (int)(amount * particlePercentage)) * 2);
               // print(i + "random : " + rnd);
                Vector3 rij;
                float r2;
                distBetweenParticles(i, j, out rij, out r2);
                if (paintMesh /*&& i == 100*/)
                {
                    interactionColors(3, i, focosParticle, j);
                   // test = false;
                }
                if (r2 < smoothingRadiusSq)
                {
                    float tempDensity = particleDensity(i, r2);
                    particles[i].density += tempDensity;
                    if (paintMesh)
                        interactionColors(4, i, focosParticle, j);
                    if (i != j)
                    {
                        if (!currentParticles.ContainsKey(j))
                            currentParticles.Add(j, new List<int>());
                        particles[j].density += tempDensity;
                    }
                }
            }
            if (!currentParticles.ContainsKey(i) && particles[i].density != 0)
                currentParticles.Add(i, new List<int>());
            particles[i].pressure = particlePressure(i);
           // test = false;
        }
        /*Dictionary<string, int> temp = new Dictionary<string, int>();
        currentParticles = new Dictionary<int, List<int>>();

        //Reset density
        for (int i = 0; i < countReadyParticles; i++) ///teste do count
        {
            particles[i].density = 0.0f;
        }
        //

        for (int i = 0; i < countReadyParticles; i++)
        {
            int rnd = Random.Range(1, (amount / (int)(amount * particlePercentage)) * 2);
            for (int j = i; j < countReadyParticles; j = j+rnd)
            {
                Vector3 rij;
                float r2;
                distBetweenParticles(i, j, out rij, out r2);
                if (r2 < smoothingRadiusSq)
                {
                    
                   if(particles[i].density != 0)
                    if (!currentParticles.ContainsKey(i) )
                        {
                            currentParticles.Add(i, new List<int>());
                        }
                    else
                        {
                            currentParticles[i].Add(j);
                        }
                    float tempDensity = particleDensity(i, r2);
                    particles[i].density += tempDensity;

                    if (i != j)
                    {
                        //if (!currentParticles.ContainsKey(j))
                          //  currentParticles.Add(j, new List<int>());
                        if (particles[i].density != 0)
                            if (!currentParticles.ContainsKey(j))
                            {
                                currentParticles.Add(j, new List<int>());
                            }
                            else
                            {
                                currentParticles[j].Add(i);
                            }
                        particles[j].density += tempDensity;
                    }
                }
            }
            //if (!currentParticles.ContainsKey(i) && particles[i].density != 0)
              //  currentParticles.Add(i, new List<int>());
            particles[i].pressure = particlePressure(i);
        }*/

    }

    public void ComputeForces()
    {
        var focosParticle = countReadyParticles * 0.5;
        var dupP = new Vector3[countReadyParticles];///teste count
        var dupV = new Vector3[countReadyParticles];
        /* if (paintMesh)
             interactionColors(2, 0, focosParticle, 0);*/
        for (int j = 0; j < countReadyParticles; j++)///teste count
        {
            dupP[j] = Vector3.zero;
            dupV[j] = Vector3.zero;
        }
        for (int i = 0; i < countReadyParticles; i++)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;

           /* if (m_volumeMat != null && i == focosParticle && m_focos != null && m_adjacent != null)
                particles[i].go.GetComponent<MeshRenderer>().material = m_focos;*/

            // Physics
            for (int j = i; j < countReadyParticles; j++)
            {
                if (i == j) continue;

                Vector3 rij;
                float r2;
                distBetweenParticles(i, j, out rij, out r2);
                float r = Mathf.Sqrt(r2);//meter dentro do if
                /*if (paintMesh)
                    interactionColors(3, i, focosParticle, j);*/
                /*if (i == focosParticle && j != focosParticle && m_volumeMat != null && m_focos != null && m_adjacent != null)
                    particles[j].go.GetComponent<MeshRenderer>().material = m_iterated;*/
                if (r < smoothingRadius)// r -> r2
                {
                    /*if (paintMesh)
                        interactionColors(4, i, focosParticle, j);*/
                    /*if (i == focosParticle && j != focosParticle && m_volumeMat != null && m_focos != null && m_adjacent != null)
                        particles[j].go.GetComponent<MeshRenderer>().material = m_adjacent;*/
                    forcePressure = pressureForce(i, j, rij, r);
                    forceViscosity = viscosityForce(i, j, r);

                    /* dup[i] += (forcePressure + forceViscosity);
                     dup[j] += (-forcePressure + (-forceViscosity));*/

                    dupP[i] += (pressureForce(i, j, rij, r) /*/ (2.0f * particles[j].density)*/);
                    dupV[i] += (viscosityForce(i, j, r)/* / particles[j].density*/);
                    dupP[j] -= (pressureForce(j, i, rij, r) /*/ (2.0f * particles[i].density)*/);
                    dupV[j] += (viscosityForce(j, i, r) /*/ particles[i].density*/);
                }

            }

            // Apply
           // Debug.Log("DUP: " + i +" x " + dup[i].x + " y " + dup[i].y + " z " + dup[i].z);
            particles[i].forcePhysic = /*dup[i]*/ dupP[i] + dupV[i] + gravityForce(i);
        }
        /*var dup = new Vector3[countReadyParticles];///teste count
        for (int j = 0; j < countReadyParticles; j++)///teste count
        {
            dup[j] = Vector3.zero;
        }
        //for (int i = 0; i < currentParticles.Count; i++)
        foreach(var item in currentParticles.Keys)
        {
            Vector3 forcePressure = Vector3.zero;
            Vector3 forceViscosity = Vector3.zero;

            // Physics
            //for (int j = i; j < countReadyParticles; j++)
            //{
               // if (i == j) continue;

                Vector3 rij;
                float r2;
            foreach (var item2 in currentParticles[item])
            {
                distBetweenParticles(item, item2, out rij, out r2);
                float r = Mathf.Sqrt(r2);//meter dentro do if

                if (r < smoothingRadius)// r -> r2
                {
                    forcePressure = pressureForce(item, item2, rij, r);
                    forceViscosity = viscosityForce(item, item2, r);

                    dup[item] += (forcePressure + forceViscosity);
                    //dup[j] += (-forcePressure + (-forceViscosity));
                }

                // }
            }
            // Apply
            particles[item].forcePhysic = dup[item] + gravityForce(item);
        }*/
    }



    public void ApplyPosition()
    {
        for (int i = 0; i < countReadyParticles; i++)
        {
            if (particles[i].go != null && currentParticles.ContainsKey(i))
                particles[i].go.transform.position = particles[i].position;
        }
    }

    public void cleanParticles()
    {
        print("destroy C");
        Destroy(particlesFather);
    }

    //Method used to paint the diferent particles that are computed in interaction with the focus particle.
    private void interactionColors(int type, int i, double focosParticle, int otherParticle)
    {
        if (type == 1)
        {//trocar isto para a cor do fuido e inves de ser o add no adjacente é no iterated
            if (flag)
                for (int k = 0; k < oldIterated.Count; k++)
                    particles[oldIterated[k]].go.GetComponent<MeshRenderer>().material = m_volumeMat;
        }
        if (type == 2)
        {
            if (m_volumeMat != null /*&& i == focosParticle*/ && m_focos != null && m_adjacent != null)
                particles[(int)focosParticle].go.GetComponent<MeshRenderer>().material = m_focos;
        }
        if (type == 3)
        {
            if (m_volumeMat != null && m_focos != null && m_adjacent != null && i == focosParticle && otherParticle != focosParticle)
            {
                particles[otherParticle].go.GetComponent<MeshRenderer>().material = m_iterated;
                oldIterated.Add(otherParticle);
                flag = true;
            }
        }
        if (type == 4)
        {
            if (m_volumeMat != null && m_focos != null && m_adjacent != null && i == focosParticle && otherParticle != focosParticle)
            {
                particles[otherParticle].go.GetComponent<MeshRenderer>().material = m_adjacent;
                
            }
        }
    }

    //add the value to the temp dic
    private void addOnDictionary(Dictionary<int,Vector3> dic, int key, Vector3 value)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = dic[key] + value;
        }
        else
        {
            dic.Add(key, value);
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


    /////////////////////////   Extern Calculations  ////////////////////////////////
    ///

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
//}
