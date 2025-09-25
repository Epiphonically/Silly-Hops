using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformLife : MonoBehaviour
{
    public bool isLight = false, up = true, bouncy = true, fall = false;
    
    /* This is a very subtle detail but "light" platforms weigh down a bit when the 
       player hops on them. */

    public float maxDown = 0.1f;
    public Vector3 origPos;
    void Start() {
        origPos = transform.localPosition;
    }

    void Update()
    {
        if (fall) {
            transform.position = transform.position - ((new Vector3(0, 150, 0)) * Time.deltaTime);
            if ((transform.position - origPos).magnitude >= 150) {
                Destroy(gameObject);
            }
        }

        if (!up) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, origPos - new Vector3(0, maxDown, 0), 8*Time.deltaTime);
        } else if (!fall) { 
            transform.localPosition = Vector3.Lerp(transform.localPosition, origPos, 8*Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision c) {
        if (isLight && c.gameObject.tag == "Player") {
            up = false;
        }
        
    }

    void OnCollisionExit(Collision c) {
        if (isLight && c.gameObject.tag == "Player") {
            up = true;
        }
    }
}
