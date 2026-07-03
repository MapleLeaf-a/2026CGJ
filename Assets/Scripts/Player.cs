using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float moveSpeed = 5f;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.up);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.down);
        }   
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.left);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(moveSpeed * Time.deltaTime * Vector3.right);
        }
    }
}
