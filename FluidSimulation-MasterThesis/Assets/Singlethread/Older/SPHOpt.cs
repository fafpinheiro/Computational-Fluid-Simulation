using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Tuples;

//namespace SPH
//{

    public class SPHOpt : MonoBehaviour
    {

        private struct SPHParticle
        {
            public Vector3 position;

            public Vector3 velocity;
            public Vector3 forcePhysic;
            public Vector3 forceHeading;

            public float density;
            public float pressure;

            public int parameterID;

            public GameObject go;
        //


            public void Init(Vector3 _position, int _parameterID, GameObject _go)
            {
                position = _position;
                parameterID = _parameterID;
                go = _go;

                velocity = Vector3.zero;
                forcePhysic = Vector3.zero;
                forceHeading = Vector3.zero;
                density = 0.0f;
                pressure = 0.0f;
            }
        }



        [System.Serializable]
        private struct SPHParameters
        {
#pragma warning disable 0649 // This line removes the warning saying that the variable is never assigned to. You can't assign a variable in a struct...
            public float particleRadius; //Particle radius
            public float smoothingRadius; //Afecting radius of the smoothing (particles in radius that affect the particle considered)
            public float smoothingRadiusSq; //
            public float restDensity; //
            public float gravityMult; //Gravity multiplier
            public float particleMass;//Particle mass
            public float particleViscosity;
            public float particleDrag;//Drag is a force acting opposite to the relative motion of any object moving with respect to a surrounding fluid
            public enum presets { poly6 }
            public enum renderType { Group, OneByOne } ///Group spawns all particles at the same time, OneByOne spwans the particles one by one like a cascade in rows 
            public presets _presets;
            //public renderType _renderType;
#pragma warning restore 0649
        }



        private struct SPHCollider
        {
            public Vector3 position;
            public Vector3 right;
            //public Vector3 left;
            public Vector3 up;
            public Vector3 forward;
            //public Vector2 scale;
            public Vector3 scale;

            public void Init(Transform _transform)
            {
                position = _transform.position;
                right = _transform.right;
                //left = _transform.left;//
                up = _transform.up;
                //forward = _transform.forward;
                // scale = new Vector2(_transform.lossyScale.x / 2f, _transform.lossyScale.y / 2f);
                scale = new Vector3(_transform.lossyScale.x / 2f, _transform.lossyScale.y / 2f, _transform.lossyScale.z / 2f);

            }
        }



        // Consts
        private static Vector3 GRAVITY = new Vector3(0.0f, -9.81f, 0.0f);
        private const float GAS_CONST = 2000.0f;
        private const float DT = 0.0008f;
        private const float BOUND_DAMPING = -0.5f;

        // Properties
        [Header("Import")]
        [SerializeField] private GameObject character0Prefab = null;

        [Header("Parameters")]
        [SerializeField] private int parameterID = 0;
        [SerializeField] private SPHParameters[] parameters = null;
        [SerializeField] private SPHParameters.renderType type;

        [Header("Properties")]
        [SerializeField] private int amount = 250;
        [SerializeField] private int rowSize = 16;

        // Data
        private SPHParticle[] particles;
        private int countReadyParticles;
    // public Tuples<T, U> tuplo;

    public static SPHOpt s1;

        private void Start()
        {
            s1 = this;
            countReadyParticles = 0;
            if (type == SPHParameters.renderType.Group)
            {
                InitSPHGroup(/*type*/);
            }
            else
            {
                StartCoroutine(InitSPHOneByOne());
                ///countReadyParticles = 0;
            }
        }



        private void Update()
        {
            ComputeDensityPressure();
            ComputeForces();
            Integrate();
            ComputeColliders();

            ApplyPosition();
        }

        IEnumerator InitSPHOneByOne()
        {
            particles = new SPHParticle[amount];

            for (int i = 0; i < amount; i++)
            {

                yield return new WaitForSeconds(0.001f);
                float jitter = (Random.value * 2f - 1f) * parameters[parameterID].particleRadius * 0.1f;
                float x = (i % rowSize) + Random.Range(-0.1f, 0.1f);
                // float y = 2 + (float)((i / rowSize) / rowSize) * 1.1f;
                // float z = ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f);
                //float x = 0;
                float y = 15;
                float z = 0;
                GameObject go = Instantiate(character0Prefab);
                go.transform.localScale = Vector3.one * parameters[parameterID].particleRadius;
                go.transform.position = new Vector3(x + jitter, y, z + jitter);
                go.name = "char" + i.ToString();

                particles[i].Init(new Vector3(x, y, z), parameterID, go);
                countReadyParticles++;
            }
        }
        private void InitSPHGroup(/*SPHParameters.renderType type*/)
        {
            //Random rand = new Random(52794);
            particles = new SPHParticle[amount];

            for (int i = 0; i < amount; i++)
            {
                Random.seed = i + 1;
                float jitter = (Random.value * 2f - 1f) * parameters[parameterID].particleRadius * 0.1f;
                float x = this.transform.position.x + (i % rowSize) + Random.Range(-0.1f, 0.1f);
                float y = this.transform.position.y + 2 + (float)((i / rowSize) / rowSize) * 1.1f;
                float z = this.transform.position.z + ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f);

                GameObject go = Instantiate(character0Prefab);
                go.transform.localScale = Vector3.one * parameters[parameterID].particleRadius;
                go.transform.position = new Vector3(x + jitter, y, z + jitter);
                go.name = "char" + i.ToString();

                particles[i].Init(new Vector3(x, y, z), parameterID, go);
                countReadyParticles++;
            }
        }

        private static bool Intersect(SPHCollider collider, Vector3 position, float radius, out Vector3 penetrationNormal, out Vector3 penetrationPosition, out float penetrationLength)
        {
            Vector3 colliderProjection = collider.position - position;

            penetrationNormal = Vector3.Cross(collider.right, collider.up); //blue axis (cross between red and green)
            penetrationLength = Mathf.Abs(Vector3.Dot(colliderProjection, penetrationNormal)) - (radius / 2.0f);
            penetrationPosition = collider.position - colliderProjection;

            return penetrationLength < 0.0f
                && Mathf.Abs(Vector3.Dot(colliderProjection, collider.right)) < collider.scale.x
                && Mathf.Abs(Vector3.Dot(colliderProjection, collider.up)) < collider.scale.y;
        }



        private static Vector3 DampVelocity(SPHCollider collider, Vector3 velocity, Vector3 penetrationNormal, float drag)
        {
            Vector3 newVelocity = Vector3.Dot(velocity, penetrationNormal) * penetrationNormal * BOUND_DAMPING
                                + Vector3.Dot(velocity, collider.right) * collider.right * drag
                                + Vector3.Dot(velocity, collider.up) * collider.up * drag;
            newVelocity = Vector3.Dot(newVelocity, Vector3.forward) * Vector3.forward
                        + Vector3.Dot(newVelocity, Vector3.right) * Vector3.right
                        + Vector3.Dot(newVelocity, Vector3.up) * Vector3.up;
            return newVelocity;
        }



        private void ComputeColliders()
        {
            // Get colliders
            GameObject[] collidersGO = GameObject.FindGameObjectsWithTag("SPHCollider");
            SPHCollider[] colliders = new SPHCollider[collidersGO.Length];
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
                    if (Intersect(colliders[j], particles[i].position, parameters[particles[i].parameterID].particleRadius, out penetrationNormal, out penetrationPosition, out penetrationLength))
                    {
                        particles[i].velocity = DampVelocity(colliders[j], particles[i].velocity, penetrationNormal, 1.0f - parameters[particles[i].parameterID].particleDrag);
                        particles[i].position = penetrationPosition - penetrationNormal * Mathf.Abs(penetrationLength);
                    }
                }
            }
        }



        private void Integrate()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].velocity += DT * (particles[i].forcePhysic) / particles[i].density;
                particles[i].position += DT * (particles[i].velocity);
            }
        }

        private void ComputeDensityPressure()
        {

            for (int i = 0; i < countReadyParticles/* amount*/; i++) ///teste do count
            {
                particles[i].density = 0.0f;
                /// dup[i] = 0.0f;
            }
            for (int i = 0; i < countReadyParticles/* amount*/; i++)
            {

                for (int j = i; j < countReadyParticles/* amount*/; j++)
                {

                    /*Vector3 rij = particles[j].position - particles[i].position;
                    float r2 = rij.sqrMagnitude;*/
                    // float r2 = distBetweenParticles(i, j);
                    //var dists = distBetweenParticles(i, j);
                    Vector3 rij;
                    float r2;
                    distBetweenParticles(i, j, out rij, out r2);
                    if (r2 < parameters[particles[i].parameterID].smoothingRadiusSq)
                    {
                        //float tempDen = parameters[particles[i].parameterID].particleMass * (315.0f / (64.0f * Mathf.PI * Mathf.Pow(parameters[particles[i].parameterID].smoothingRadius, 9.0f))) * Mathf.Pow(parameters[particles[i].parameterID].smoothingRadiusSq - r2, 3.0f);
                        float tempDensity = particleDensity(i, r2);
                        particles[i].density += tempDensity;

                        if (i != j)
                            particles[j].density += tempDensity;///              
                    }
                }
                particles[i].pressure = particlePressure(i);
            }
        }

        private void ComputeForces()
        {
            var dup = new Vector3[countReadyParticles/*amount*/];///teste count
            for (int j = 0; j < countReadyParticles/*amount*/; j++)///teste count
            {
                dup[j] = Vector3.zero;
            }
            for (int i = 0; i < countReadyParticles/* particles.Length*/; i++)
            {
                Vector3 forcePressure = Vector3.zero;
                Vector3 forceViscosity = Vector3.zero;

                // Physics
                for (int j = i; j < countReadyParticles/* particles.Length*/; j++)
                {
                    if (i == j) continue;

                    ///Vector3 rij = particles[j].position - particles[i].position;
                    ///float r2 = rij.sqrMagnitude;
                    //float r2 = distBetweenParticles(i, j);
                    Vector3 rij;
                    float r2;
                    distBetweenParticles(i, j, out rij, out r2);
                    float r = Mathf.Sqrt(r2);//meter dentro do if

                    if (r < parameters[particles[i].parameterID].smoothingRadius)// r -> r2
                    {

                        /*Vector3 tempForcePressure = pressureForce(i, j, rij, r);
                        forcePressure = tempForcePressure;
                        Vector3 tempForceViscosity = viscosityForce(i, j, r);
                        forceViscosity = tempForceViscosity;*/
                        forcePressure = pressureForce(i, j, rij, r);
                        forceViscosity = viscosityForce(i, j, r);

                        dup[i] += (forcePressure + forceViscosity);
                        dup[j] += (-forcePressure + (-forceViscosity));
                    }

                }

                //Vector3 forceGravity = GRAVITY * particles[i].density * parameters[particles[i].parameterID].gravityMult;

                // Apply
                particles[i].forcePhysic = /*forcePressure + forceViscosity  + forceGravity*/  dup[i] + gravityForce(i);
            }
        }



        private void ApplyPosition()
        {
            for (int i = 0; i < countReadyParticles/* particles.Length*/; i++)
            {
                if (particles[i].go != null)
                    particles[i].go.transform.position = particles[i].position;
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
            return GRAVITY * particles[i].density * parameters[particles[i].parameterID].gravityMult;
        }

        //Calculate the particle pressure
        private float particlePressure(int i)
        {
            return GAS_CONST * (particles[i].density - parameters[particles[i].parameterID].restDensity);
        }

        //Calclate the particle density
        private float particleDensity(int i, float r2)
        {
            return parameters[particles[i].parameterID].particleMass *
                                    SmoothingKernels.Poly6(parameters[particles[i].parameterID].smoothingRadius, parameters[particles[i].parameterID].smoothingRadiusSq, r2);
        }

        //Calculate the viscosity force
        private Vector3 viscosityForce(int i, int j, float r)
        {
            return parameters[particles[i].parameterID].particleViscosity * parameters[particles[i].parameterID].particleMass * (particles[j].velocity - particles[i].velocity) / particles[j].density *
                SmoothingKernels.ViscosityLaplacian(parameters[particles[i].parameterID].smoothingRadius, r);
        }

        //Calculate the pressure force
        private Vector3 pressureForce(int i, int j, Vector3 rij, float r)
        {
            return -parameters[particles[i].parameterID].particleMass * (particles[i].pressure + particles[j].pressure) / (2.0f * particles[j].density) *
                SmoothingKernels.GradientSpiky(rij, parameters[particles[i].parameterID].smoothingRadius, r);
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
    }
//}
