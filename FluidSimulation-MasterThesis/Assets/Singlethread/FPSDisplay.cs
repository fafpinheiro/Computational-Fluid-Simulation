using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//namespace SPH
//{


    public class FPSDisplay : MonoBehaviour
    {
        public TextMeshProUGUI FpsText;
        public TextMeshProUGUI TimeText;
        public TextMeshProUGUI TimeToVelocity;

       // public A SPH;

        private float pollingTime = 1f;
        private float time;
        private int frameCount;
        private int currentFrame;
        private double timePassed;
        private float frameRate;
        public int frameObjective;
        public float averageVelocity;
        public Interface i;
        //private System.DateTime startDeltaTime;
        //private int currentDeltaTime;
        // Start is called before the first frame update
        
        void Start()
        {
            currentFrame = 0;
            frameCount = 0;
            //reached = false;
            // startDeltaTime = System.DateTime.Now;

        }
        
        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            frameCount++;
            currentFrame++;
            if (time >= pollingTime/* || currentFrame==500*/)
            {
                //frameRate = Mathf.RoundToInt(frameCount / time);
                frameRate = Mathf.Round(frameCount / time);
                
                FpsText.text = frameRate.ToString() + " FPS " + " Current Frame: " + currentFrame;
                


                time -= pollingTime;
                frameCount = 0;
            }
            if (currentFrame >= frameObjective)
            {
                //System.DateTime timeItTook = System.DateTime.Now - startDeltaTime;
                if (currentFrame == frameObjective)
                    timePassed = Time.realtimeSinceStartup;
                TimeText.text = " It tooked: " + timePassed.ToString() + " seconds to reach the frame " + frameObjective;
            }
            Vector3 aux = new Vector3(averageVelocity, averageVelocity, averageVelocity);
            //Debug.Log(aux);
            //Debug.Log(SPHOpt.s1.averageParticleVelocity());
            //Debug.Log(currentFrame);
           //// if (SPHOpt.s1.averageParticleVelocity(/*MasterController.i*/).x == aux.x && SPHOpt.s1.averageParticleVelocity(/*MasterController.i*/).z == aux.z && currentFrame >= 20)
            ////{
           //// Debug.Log("ggggggggggggggggggggggggggggggggggggggggggggggggg");
               //// TimeToVelocity.text = "The time taked to the particles reach " + averageVelocity + " was " + Time.realtimeSinceStartup.ToString();
           //// }

        }
    }
//}
