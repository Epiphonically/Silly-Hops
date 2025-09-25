using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float minAltitude = 50f;
    public float maxAltitude = 100f;
    public float squareSideLen = 10f;
    public Material[] shades;
    public GameObject brick;

    /* This code generates bricks in a (rows X cols) area for decoration
       this is inspiried by the tree generation lecture */

    void Start() {
        float initx = transform.position.x - (cols/2)*(squareSideLen);
        float initz = transform.position.z - (rows/2)*(squareSideLen);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {

                GameObject it = Instantiate(brick, new Vector3(initx + i*squareSideLen, transform.position.y + Random.Range(minAltitude, maxAltitude), initz + j*squareSideLen), Quaternion.identity);
                Material[] temp = it.GetComponent<MeshRenderer>().materials;
                temp[0] = shades[Random.Range(0, shades.Length)];
                it.GetComponent<MeshRenderer>().materials = temp;
            }
        }
    }
}
