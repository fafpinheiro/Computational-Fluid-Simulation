using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsScript : MonoBehaviour
{
    public Animation[] anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveLeftWall()
    {
        Animator anim = GameObject.Find("Scenes").transform.Find("_ENVIRONMENT_3").Find("Wall (1)").GetComponent<Animator>();
        if (anim.enabled)
            anim.enabled = false;
        else anim.enabled = true;
    }

    public void moveRightWall()
    {
        Animator anim = GameObject.Find("Scenes").transform.Find("_ENVIRONMENT_3").Find("Wall (2)").GetComponent<Animator>();
        if (anim.enabled)
            anim.enabled = false;
        else anim.enabled = true;
    }
}
