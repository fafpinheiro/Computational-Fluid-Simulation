using System.IO;
using UnityEngine;
public class Automatization : MonoBehaviour
{
    public Interface i;
    private string metodo;

    public Data.renderType type;
    public Data data;
    [SerializeField] private int currentFrame = 0;

    public int ObjectiveFrame;
    public int MinParticles;
    public int MaxParticles;
    public int particlesInterval;

    [SerializeField] private int iterationStep;

    private double[] durations;
    public enum Optimization
    {
        Default,
        OptimizationNbyTwo,
        MonteCarlo,
        HashGrid
    }
    public Optimization optimization;

    public enum Tests
    {
        ParticlesTimeTest,
        GridTimeAndHash,
        MonteCarloPercentageTest,
        SimetricErroTimes
    }
    public Tests tests;


    [Range(0.01f, 1.0f)]
    public float MontecarloParticlePercentage = 0.2f;

    public bool VolumeShader;
    public bool FoamEffect;
    string filename = "";
    TextWriter tw;
    TextReader tr;
    private System.DateTime startTime;
    private double tmp = 0;
    private bool print= true;
    // Start is called before the first frame update
    /*void Start()
    {
        ///File.Delete(Application.dataPath + "/test.csv");
        filename = Application.dataPath + "/test.csv";
        /// gameObject.GetComponent<Data>().amount = 100;
        /// 
        durations = new double[MaxParticles-MinParticles];
        gameObject.GetComponent<Data>().amount = MinParticles;
        iterationStep = 0;
        startTime = System.DateTime.UtcNow;
        initIteration();

    }
    // Update is called once per frame
    void Update()
    {
        main.ComputeDensityPressure(i);
        main.ComputeForces(i);
        main.Integrate(i);
        main.ComputeColliders(i);
        main.ApplyPosition(i);
        currentFrame++;
        if (iterationStep == 10)
        {
            Application.Quit();
        }
        if(gameObject.GetComponent<Data>().amount == MaxParticles)
        {
            gameObject.GetComponent<Data>().amount = MinParticles;
            iterationStep += 1;
        }
        if (currentFrame >= ObjectiveFrame)
        {
            
            System.TimeSpan ts = System.DateTime.UtcNow- startTime;
            //write csv file
            double tempo = ts.TotalMilliseconds;
            WriteCSV(tempo);
            
            gameObject.GetComponent<Data>().amount += StepParticles;

            main.cleanParticles(i);
            initIteration();
        }
        

    }

    private void setParameters(Interface i, float particlePercentage)
    {
        if (VolumeShader)
            main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                                    data.particleMass, data.particleViscosity, data.particleDrag, data.spawn, data.character0Prefab, data.amount, data.rowSize,
                                                    data.m_volumeMat, particlePercentage,null,null,null,true);
        else
            main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                                    data.particleMass, data.particleViscosity, data.particleDrag, data.spawn, data.character0Prefab, data.amount, data.rowSize,
                                                    null, particlePercentage,null,null,null,false);
    }
    private void initIteration()
    {
        
        currentFrame = 0;
       

        switch (optimization)
        {
            case Optimization.Default:
                i = (Interface)gameObject.AddComponent<B>();
                setParameters(i, 0.0f);
                break;
            case Optimization.OptimizationNbyTwo:
                i = (Interface)gameObject.AddComponent<A>();
                setParameters(i, 0.0f);
                break;
            case Optimization.MonteCarlo:
                i = (Interface)gameObject.AddComponent<C>();
                setParameters(i, MontecarloParticlePercentage);
                break;
            case Optimization.HashGrid:
                i = (Interface)gameObject.AddComponent<HashSolver>();
                setParameters(i, 0.0f);
                break;
        }

        if (type == Data.renderType.Group || optimization == Optimization.HashGrid)
            metodo = "Group";
        else
            metodo = "OneByOne";

        startTime = System.DateTime.UtcNow;
        main.InitSPH(i, metodo);
    }
   
   private void WriteCSV(double ts)
    {
        tw = new StreamWriter(filename, true);

        tw.WriteLine(gameObject.GetComponent<Data>().amount + "/" + ts);

        tw.Close();
    }*/

    void Start()
    {
        ///File.Delete(Application.dataPath + "/test.csv");
        filename = Application.dataPath + "/test.csv";
        /// gameObject.GetComponent<Data>().amount = 100;
        /// 
       // durations = new double[MaxParticles/StepParticles];
        gameObject.GetComponent<Data>().amount = MinParticles;
        iterationStep = 0/*1*/;
       // startTime = System.DateTime.UtcNow;
        initIteration();

    }
    // Update is called once per frame
    void Update()
    {
        
        

        if (currentFrame == 0)
            startTime = System.DateTime.UtcNow;
        main.ComputeDensityPressure(i);
        main.ComputeForces(i);
        main.Integrate(i);
        main.ComputeColliders(i);
        main.ApplyPosition(i);
        currentFrame++;
        switch (tests)
        {
            case Tests.ParticlesTimeTest:
                /* if (iterationStep == 3 && print == true)
                {
                    print = false;
                    print(MaxParticles/StepParticles);
                    for (int i = MinParticles/StepParticles; i <= MaxParticles / StepParticles; i++)
                    {
                        WriteCSV(durations[i],i* StepParticles);
                    }
                    new WaitForSeconds(100);
                }*/
                if (currentFrame == ObjectiveFrame && iterationStep </*=*/ 10)
                {

                    System.TimeSpan ts = System.DateTime.UtcNow - startTime;
                    //write csv file
                    double tempo = ts.TotalMilliseconds;
                    //durations[gameObject.GetComponent<Data>().amount /StepParticles] += tempo;
                    WriteCSV1(tempo);
                    if (gameObject.GetComponent<Data>().amount == MaxParticles && currentFrame == ObjectiveFrame)
                    {

                        gameObject.GetComponent<Data>().amount = MinParticles;
                        iterationStep += 1;
                    }
                    else { gameObject.GetComponent<Data>().amount += particlesInterval; }


                    main.cleanParticles(i);
                    initIteration();
                }
                break;
            case Tests.SimetricErroTimes:

                break;
            case Tests.MonteCarloPercentageTest:

                if (currentFrame == ObjectiveFrame && iterationStep </*=*/ 5 && gameObject.GetComponent<Automatization>().MontecarloParticlePercentage <= 1.0f)
                {

                    System.TimeSpan ts = System.DateTime.UtcNow - startTime;
                    //write csv file
                    double tempo = ts.TotalMilliseconds;
                    //durations[gameObject.GetComponent<Data>().amount /StepParticles] += tempo;
                    /// WriteCSV(tempo);
                    /*if (gameObject.GetComponent<Data>().amount == MaxParticles && currentFrame == ObjectiveFrame)
                    {

                        gameObject.GetComponent<Data>().amount = MinParticles;
                        //// tmp += tempo;
                        iterationStep += 1;
                    }
                    else { gameObject.GetComponent<Data>().amount += particlesInterval; }*/
                    if( iterationStep < 5)
                    {
                        tmp += tempo;
                        iterationStep+=1;
                    }
                    if (iterationStep == 5)
                    {

                        WriteCSV2(tmp / iterationStep, gameObject.GetComponent<Automatization>().MontecarloParticlePercentage);
                        tmp = 0;
                        if (gameObject.GetComponent<Data>().amount == MaxParticles)
                        {
                            gameObject.GetComponent<Automatization>().MontecarloParticlePercentage += 0.2f;
                            gameObject.GetComponent<Data>().amount = MinParticles;

                        }
                        else
                        {
                            gameObject.GetComponent<Data>().amount += particlesInterval;
                        }
                        iterationStep = 0/*1*/;
                    }

                    main.cleanParticles(i);
                    initIteration();
                }

                break;
            case Tests.GridTimeAndHash:

                break;
        }

        


    }

    private void setParameters(Interface i, float particlePercentage)
    {
        if (VolumeShader)
            main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                                    data.particleMass, data.particleViscosity, data.particleDrag, data.spawn, data.character0Prefab, data.amount, data.rowSize,
                                                    data.m_volumeMat, particlePercentage, null, null, null, true,data.analisedParticle, false/*,data.objectos*/);
        else
            if(FoamEffect)
                main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                                    data.particleMass, data.particleViscosity, data.particleDrag, data.spawn, data.character0Prefab, data.amount, data.rowSize,
                                                    null, particlePercentage, null, null, null, false, data.analisedParticle,false/*,data.objectos*/);
            else
                main.setParameters(i, data.particleRadius, data.smoothingRadius, data.smoothingRadiusSq, data.restDensity, data.gravityMult,
                                            data.particleMass, data.particleViscosity, data.particleDrag, data.gameObject, data.character0Prefab, data.amount, data.rowSize,
                                            null, particlePercentage, null, null, null, false, data.analisedParticle, false/*, data.objectos*/);
    }
    private void initIteration()
    {

        currentFrame = 0;


        switch (optimization)
        {
            case Optimization.Default:
                i = (Interface)gameObject.AddComponent<DefaultAlgo>();//b
                setParameters(i, 0.0f);
                break;
            case Optimization.OptimizationNbyTwo:
                i = (Interface)gameObject.AddComponent<SymmetricAlgo>();//a
                setParameters(i, 0.0f);
                break;
            case Optimization.MonteCarlo:
                i = (Interface)gameObject.AddComponent<MonteCarloAlgo>();//c
                setParameters(i, MontecarloParticlePercentage);
                break;
            case Optimization.HashGrid:
                i = (Interface)gameObject.AddComponent<HashSolver>();
                setParameters(i, 0.0f);
                break;
        }

        if (type == Data.renderType.Group || optimization == Optimization.HashGrid)
            metodo = "Group";
        else
            metodo = "OneByOne";

        startTime = System.DateTime.UtcNow;
        main.InitSPH(i, metodo);

        //Meter aqui a situação para ter o tempo de iniciação das particulas, assim é possivel mostrar que o hash demora muito por causa da grelha
    }

    private void WriteCSV1(double ts/*, int nParticles*/)
    {
        tw = new StreamWriter(filename, true);

        tw.WriteLine(gameObject.GetComponent<Data>().amount + "/" + ts);

        tw.Close();
    }

    private void WriteCSV2(double ts/*, int nParticles*/, float percentage)
    {
        tw = new StreamWriter(filename, true);

        tw.WriteLine("amount: " + gameObject.GetComponent<Data>().amount + "particlePercentage: " + percentage + "/" + ts);

        tw.Close();
    }
}
