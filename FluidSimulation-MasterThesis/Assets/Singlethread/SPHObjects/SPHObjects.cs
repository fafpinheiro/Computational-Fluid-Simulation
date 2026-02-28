using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    //public class SPHObjects : MonoBehaviour
    //{
        public struct SPHParticle
        {
            public Vector3 position;

            public Vector3 velocity;
            public Vector3 forcePhysic;
           // public Vector3 forceHeading;

            public float density;
            public float pressure;

            public int parameterID;

            public GameObject go;



            public void Init(Vector3 _position, int _parameterID, GameObject _go)
            {
                position = _position;
                parameterID = _parameterID;
                go = _go;

                velocity = Vector3.zero;
                forcePhysic = Vector3.zero;
              //  forceHeading = Vector3.zero;
                density = 0.0f;
                pressure = 0.0f;
            }
        }

        public struct SPHCollider
        {
            public Vector3 position;
            public Vector3 right;
            public Vector3 up;
            public Vector3 forward;
            public Vector3 scale;

            public void Init(Transform _transform)
            {
                position = _transform.position;
                right = _transform.right;
                up = _transform.up;
                scale = new Vector3(_transform.lossyScale.x / 2f, _transform.lossyScale.y / 2f, _transform.lossyScale.z / 2f);

            }
        }

        public struct SPHObject
        {
        public Vector3 position;

        public Vector3 velocity;
        public Vector3 forcePhysic;
        public Vector3 forceHeading;

        public float density;
        public float pressure;
       // public float mass;
        public int parameterID;

        public GameObject go;



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
           // mass = 200;
        }
    }

    // }
}
