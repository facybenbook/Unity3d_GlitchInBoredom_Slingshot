using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTexture : MonoBehaviour {
    public Confetti_Ribbon mComp;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
         RenderTexture rt = mComp.curPosLife;
        //RenderTexture rt = mComp.curVelScale;

        Graphics.Blit(rt, destination);
    }
}
