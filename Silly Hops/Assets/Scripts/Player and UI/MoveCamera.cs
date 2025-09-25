using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform moveToMe;

    void Update()
    {
        transform.position = moveToMe.position;
    }
}
