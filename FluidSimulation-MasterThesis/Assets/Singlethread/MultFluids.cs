using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Grid;
using Unity.Jobs;
using Unity.Collections;
public class MultFluids : MonoBehaviour
{
    public Interface i;
    private string metodo;
    private Transform cameraPosition;
    private Transform environment;
    //private Data.presets preset;
    [Header("Choose type of particle instanciation.")]
    public Data.renderType type;
    [Header("Select data component that will specify the fluid properties.")]
    public Data data;
    //public main mainM;
    // public int currentFrame=0;

    public Transform spawn;
    private bool createObjs = false;

    private int teste = 0;
    public enum Optimization
    {
        Default,
        OptimizationNbyTwo,
        MonteCarlo,
        HashGrid
    }
    [Header("Choose interarction optimization.")]
    public Optimization optimization;
    [Header("Percentage of interaction particles.")]
    [Range(0.01f, 1.0f)]
    public float MontecarloParticlePercentage = 0.2f;
    [Header("Select if want to see particle computations.")]
    public bool InteractionEffect;
    [Header("Select if particle shader is needed.")]
    [Tooltip("Doesnt work when VolumeShader is on.")] public bool FoamEffect;

    public bool Parallel;
    private ParticleManager parallelM;
    private JobHandle job;
    // Start is called before the first frame update
    void Start()
    {
        //setMapSample();
        if (Parallel)
        {
            parallelM = gameObject.AddComponent<ParticleManager>();
            parallelM.initParticles();
        }
        else
            initIteration();

    }
    // Update is called once per frame
    void Update()
    {
        //  teste++;
        // print("testeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee: " + teste);

        if (Parallel)
        {
            job = parallelM.ComputeJobs(job);
        }
        else
        {
            main.ComputeDensityPressure(i);
            main.ComputeForces(i);
            main.Integrate(i);
            main.ComputeColliders(i);
            main.ApplyPosition(i);
            //currentFrame++;
            /*for (int j = 0; j < 100; j++)
            {
                Debug.DrawLine(new Vector3(j, j, 0), new Vector3(j, j, 100), Color.red, 5, false);
                Debug.DrawLine(new Vector3(0, j, j), new Vector3(100, j, j), Color.red, 5, false);
                Debug.DrawLine(new Vector3(j, 0, j), new Vector3(j, 100, j), Color.red, 5, false);
            }*/
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //  cameraPosition = scene.GetChild(0);
                GameObject.Find("Main Camera").transform.position = cameraPosition.GetChild(0).transform.position;
                GameObject.Find("Main Camera").transform.rotation = cameraPosition.GetChild(0).transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //  cameraPosition = scene.GetChild(0);
                GameObject.Find("Main Camera").transform.position = cameraPosition.GetChild(1).transform.position;
                GameObject.Find("Main Camera").transform.rotation = cameraPosition.GetChild(1).transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //  cameraPosition = scene.GetChild(0);
                GameObject.Find("Main Camera").transform.position = cameraPosition.GetChild(2).transform.position;
                GameObject.Find("Main Camera").transform.rotation = cameraPosition.GetChild(2).transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                //  cameraPosition = scene.GetChild(0);
                GameObject.Find("Main Camera").transform.position = cameraPosition.GetChild(3).transform.position;
                GameObject.Find("Main Camera").transform.rotation = cameraPosition.GetChild(3).transform.rotation;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                //  cameraPosition = scene.GetChild(0);
                GameObject.Find("Main Camera").transform.position = environment.Find("Camera position").transform.position;
                GameObject.Find("Main Camera").transform.rotation = environment.Find("Camera position").transform.rotation;
            }
        }
    }
    
    private void setParameters(Interface i, float particlePercentage)
    {

        if (InteractionEffect)
            main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                                    data.particleMass, data.particleViscosity, data.particleDrag, spawn.gameObject, data.character0Prefab, data.amount, data.rowSize,
                                                    data.m_volumeMat, particlePercentage, data.m_focosParticle, data.m_iteratedParticle, data.m_adjacentParticle, true, data.analisedParticle, false/*, data.objectos*/);
        else
            if (FoamEffect)
            main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                                    data.particleMass, data.particleViscosity, data.particleDrag, spawn.gameObject, data.character0Prefab, data.amount, data.rowSize,
                                                    null, particlePercentage, null, null, null, false, data.analisedParticle, true/*, data.objectos*/);
        else
            main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                            data.particleMass, data.particleViscosity, data.particleDrag, spawn.gameObject, data.character0Prefab, data.amount, data.rowSize,
                                            null, particlePercentage, null, null, null, false, data.analisedParticle, false/*, data.objectos*/);
    }
    public void initIteration()
    {
        //currentFrame = 0;


        switch (optimization/*preset*/)
        {
            case Optimization.Default/*Data.presets.DefaultMethod*/:
                i = /*(Interface)new B();*/(Interface)gameObject.AddComponent</*B*/DefaultAlgo>();// (Interface)(new GameObject("SPHMethodDefault " + this.gameObject.name)).AddComponent<B>();/*this.gameObject.AddComponent<B>()*/;
                // GameObject.Find("SPHMethodDefault " + this.gameObject.name).AddComponent<main>();
                setParameters(i, 0.0f);
                break;
            case Optimization.OptimizationNbyTwo/*Data.presets.OptimizationNByTwo*/:
                i = /*(Interface)new A();*/(Interface)gameObject.AddComponent</*A*/SymmetricAlgo>(); //(Interface)(new GameObject("SPHMethodOptimizationNByTwo " + this.gameObject.name)).AddComponent<A>();/*this.gameObject.AddComponent<A>()*/;
                // GameObject.Find("SPHMethodOptimizationNByTwo " + this.gameObject.name).AddComponent<main>();
                setParameters(i, 0.0f);
                break;
            case Optimization.MonteCarlo:
                i = (Interface)gameObject.AddComponent<C>();// (Interface)(new GameObject("SPHMethodMonteCarlo " + this.gameObject.name)).AddComponent<C>();
                //i.GetType().GetMember(). = AudioReverbZone;
                setParameters(i, MontecarloParticlePercentage);
                break;
            case Optimization.HashGrid:
                i = (Interface)gameObject.AddComponent<HashSolver>();
                //  gameObject.AddComponent<HashGrid>();
                setParameters(i, 0.0f);
                break;
        }

        if (type == Data.renderType.Group || optimization == Optimization.HashGrid)
            metodo = "Group";
        else
            metodo = "OneByOne";

        main.InitSPH(i, metodo);
    }
    public void createObjects()
    {
        if (!createObjs)
        {
            main.createObstacle(i, true);
            createObjs = true;
        }
        else
        {
            main.createObstacle(i, false);
            createObjs = false;
        }
    }

    /*public Transform defineScene(Transform scene)
    {
       // Transform cameraPosition;
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            cameraPosition = scene.GetChild(0);
        }


        return cameraPosition;
    }*/
    public void cleanParticles()
    {
        main.cleanParticles(i);
        this.enabled = false;
    }

    public void startParticles()
    {
        if (!this.enabled)
            this.enabled = true;
        main.cleanParticles(i);
        initIteration();
    }

    public void createObstacle()
    {
        GameObject obs = GameObject.Find("Scenes").transform.Find("_ENVIRONMENT_4").Find("obstacle").gameObject;
        if (!obs.active)
            obs.SetActive(true);
        else obs.SetActive(false);
    }
}
