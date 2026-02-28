using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedCalc : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CalcSpeed());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator CalcSpeed()
    {
        bool isPlaying = true;
        while (isPlaying)
        {
            Vector3 prevPos = transform.position;
            yield return new WaitForEndOfFrame();// new WaitForFixedUpdate();
            
            speed = Mathf.RoundToInt(Vector3.Distance(transform.position, prevPos) / Time.fixedDeltaTime/* Time.deltaTime*/);
        }
    }
}
