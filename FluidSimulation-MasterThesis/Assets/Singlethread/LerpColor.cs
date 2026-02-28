using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpColor : MonoBehaviour
{
    public Data data;
    bool teste = false;
    int teste2 = 0;
    int teste3 = 0;
    // Start is called before the first frame update
    void Start()
    {
        // meshRenderer = GetComponent<MeshRenderer>();
        data = GameObject.Find("_MANAGER (3)").GetComponent<Data>();
    }

    // Update is called once per frame
    void Update()
    {
        teste2++;
       // if (teste == false)
       // {
            
            if (gameObject.GetComponent<SpeedCalc>().speed >= 8)
            {
                gameObject.GetComponent<MeshRenderer>().material = GameObject.Find("_MANAGER (3)").GetComponent<Data>().vol3;
            }
            if (gameObject.GetComponent<SpeedCalc>().speed >= 4 && gameObject.GetComponent<SpeedCalc>().speed < 7)
            {

                gameObject.GetComponent<MeshRenderer>().material = GameObject.Find("_MANAGER (3)").GetComponent<Data>().vol2;
            }
      //  }
        if(teste2 >= 50 )
        if (gameObject.GetComponent<SpeedCalc>().speed >= 1 && gameObject.GetComponent<SpeedCalc>().speed < 3)
        {
                // if (teste2 == 3)
                teste3++;
                if (teste3 >= 10)
                {
                    teste = true;
                    gameObject.GetComponent<MeshRenderer>().material = GameObject.Find("_MANAGER (3)").GetComponent<Data>().vol1;
                }
        }

    } 
    
}
