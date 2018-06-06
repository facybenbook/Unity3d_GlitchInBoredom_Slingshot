using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confetti_Ribbon : MonoBehaviour {
    // cs - update point
    public ComputeShader mCs_updatePoints;
    public bool isDebug = false;

    private RenderTexture[] mRt_posLife, mRt_velScale;
    private RenderTexture mRt_rotDir;

    public RenderTexture curPosLife
    {
        get { return mRt_posLife[curFrame]; }
    }

    public RenderTexture curVelScale
    {
        get { return mRt_velScale[curFrame]; }
    }

    public bool mLaunchRibbon = false;
    private Vector3 mLaunchOrigin = Vector3.zero;
    private Vector3 mLaunchDir = Vector3.zero;
    private Vector3 mLaunchSeed = Vector3.zero;

    public float mTrailMaxLength;

    // render 
    public Cubemap mCubeMap;
    private RibbonMesh mRibbonMesh;
    public Shader mRibbonShader;
    private Material mRibbonMat;

    // global
    private uint curFrame = 0;

    private const int numRibbons = 8192;
    private const int numTrails = 64;
    private const int numParticles = numRibbons * numTrails;
    private const int numCSWorkerGroups = 8;


    void Start () {
        initResources();
        resetCsBuffers();
    }
	
	void Update () {
        if (mLaunchRibbon && isDebug)
            launchRibbon(Vector3.forward, Vector3.zero);

        updateCsBuffers();

        drawMesh();

        curFrame ^= 1;
    }

    private void OnDestroy()
    {
        destroyResources();
    }

    public void launchRibbon(Vector3 dir, Vector3 loc)
    {
        mLaunchRibbon = true;

        mLaunchDir = dir;
        mLaunchOrigin = loc;

        mLaunchSeed = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f));

        mCs_updatePoints.SetVector("uLaunchOrigin", mLaunchOrigin);
        mCs_updatePoints.SetVector("uLaunchDir", mLaunchDir);
        mCs_updatePoints.SetVector("uLaunchSeed", mLaunchSeed);
    }

    void initResources()
    {
        // ribbon mesh
        mRibbonMesh = new RibbonMesh(numTrails, numRibbons);

        // cs - update point
        mRt_posLife = new RenderTexture[2];
        mRt_velScale = new RenderTexture[2];
        for (int i = 0; i < 2; i++)
        {
            mRt_posLife[i] = initComputeRenderTexture(numTrails, numRibbons);
            mRt_velScale[i] = initComputeRenderTexture(numTrails, numRibbons);
        }
        mRt_rotDir = initComputeRenderTexture(numTrails, numRibbons);

        // render 
        mRibbonMat = new Material(mRibbonShader);
    }

    void destroyResources()
    {
        //ribbon mesh
        mRibbonMesh.destroy();

        // cs
        for (int i = 0; i < 2; i++)
        {
            if (mRt_posLife[i] != null)
            {
                mRt_posLife[i].Release();
                mRt_posLife[i] = null;
            }

            if (mRt_velScale[i] != null)
            {
                mRt_velScale[i].Release();
                mRt_velScale[i] = null;
            }
        }
        if(mRt_rotDir != null)
        {
            mRt_rotDir.Release();
            mRt_rotDir = null;
        }

        // gss
        if (mRibbonMat)
            Destroy(mRibbonMat);
    }

    RenderTexture initComputeRenderTexture(int w, int h)
    {
        RenderTexture rt = new RenderTexture(w, h, 0);
        rt.format = RenderTextureFormat.ARGBFloat; 
        rt.filterMode = FilterMode.Point;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    void resetCsBuffers()
    {
        // cs - point
        int kernel = mCs_updatePoints.FindKernel("SetupRibbon");

        mCs_updatePoints.SetTexture(kernel, "oPosLife", mRt_posLife[curFrame ^ 1]);
        mCs_updatePoints.SetTexture(kernel, "oVelScale", mRt_velScale[curFrame ^ 1]);
        mCs_updatePoints.SetTexture(kernel, "oRotDir", mRt_rotDir);

        mCs_updatePoints.Dispatch(
            kernel, numTrails / numCSWorkerGroups, numRibbons / numCSWorkerGroups, 1);

        mCs_updatePoints.SetTexture(kernel, "oPosLife", mRt_posLife[curFrame]);
        mCs_updatePoints.SetTexture(kernel, "oVelScale", mRt_velScale[curFrame]);

        mCs_updatePoints.Dispatch(
            kernel, numTrails / numCSWorkerGroups, numRibbons / numCSWorkerGroups, 1);
    }

    void updateCsBuffers()
    {
        int kernel = mCs_updatePoints.FindKernel("UpdateRibbon");

        mCs_updatePoints.SetTexture(kernel, "oPosLife", mRt_posLife[curFrame]);
        mCs_updatePoints.SetTexture(kernel, "oVelScale", mRt_velScale[curFrame]);
        mCs_updatePoints.SetTexture(kernel, "oRotDir", mRt_rotDir);

        mCs_updatePoints.SetTexture(kernel, "uPosLife", mRt_posLife[curFrame ^ 1]);
        mCs_updatePoints.SetTexture(kernel, "uVelScale", mRt_velScale[curFrame ^ 1]);

        mCs_updatePoints.SetBool("uLaunchRibbon", mLaunchRibbon);
        mCs_updatePoints.SetFloat("uTrailMaxLength", mTrailMaxLength);
        mCs_updatePoints.SetFloat("uFrame", Time.frameCount);

        mCs_updatePoints.Dispatch(
            kernel, numTrails / numCSWorkerGroups, numRibbons / numCSWorkerGroups, 1);

        mLaunchRibbon = false;
    }

    void drawMesh()
    {
        mRibbonMat.SetTexture("uPosLife", mRt_posLife[curFrame]);
        mRibbonMat.SetTexture("uVelScale", mRt_velScale[curFrame]);
        mRibbonMat.SetTexture("uRotDir", mRt_rotDir);
        mRibbonMat.SetTexture("uCubeMap", mCubeMap);

        mRibbonMat.SetInt("uNumTrails", numTrails);

        Graphics.DrawMesh(
            mRibbonMesh.getMesh(), 
            Vector3.zero, 
            Quaternion.identity, 
            mRibbonMat, 0);
    }
}
