using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectCollision : MonoBehaviour
{
    // Start is called before the first frame update
    private Collider thisCollider;
    void Start()
    {
        thisCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        //bool overLapped= 
    }
    public bool collided(Collider collider, Vector3 direction, float dist)
    {

        return Physics.ComputePenetration(thisCollider,transform.position,transform.rotation,collider,collider.transform.position,collider.transform.rotation, out direction, out dist);
    }
}
