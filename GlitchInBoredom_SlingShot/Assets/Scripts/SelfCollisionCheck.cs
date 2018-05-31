using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfCollisionCheck : MonoBehaviour {
    private bool isCol = false;

    public bool checkCollision
    {
        get { return isCol; }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.name == "GrabVolumeSmall" || col.name == "GrabVolumeBig")
            isCol = true;
    }

    private void OnTriggerExit(Collider col)
    {
        isCol = false;
    }

    private void OnTriggerStay(Collider col)
    {
        
    }
}
