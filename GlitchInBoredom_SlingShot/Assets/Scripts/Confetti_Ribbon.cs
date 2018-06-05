using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confetti_Ribbon : MonoBehaviour {
    // cs - update point
    public ComputeShader mCs_updatePoints;
    
    private RenderTexture[] mRt_posLife, mRt_velScale;
    private RenderTexture mRt_rotDir;

    public bool mLaunchRibbon = false;
    private Vector3 mLaunchOrigin = Vector3.zero;
    private Vector3 mLaunchDir = Vector3.zero;
    private Vector3 mLaunchSeed = Vector3.zero;

    public float mTrailMaxLength;

    // cs - generate mesh
    public ComputeShader mCs_generateMesh;

    private RenderTexture mRt_vert;
    private RenderTexture mRt_norm;
    private RenderTexture mRt_texCoord;
    private RenderTexture mRt_tri;

    // render 
    public Shader mRibbonShader;
    private Material mRibbonMat;
    private ComputeBuffer mDrawProcedural_args;

    // global
    private uint curFrame = 0;

    private const int numRibbons = 16;
    private const int numTrails = 16;
    private const int numParticles = numRibbons * numTrails;
    private const int numCSWorkerGroups = 8;


    void Start () {
        initResources();
        resetCsBuffers();
    }
	
	void Update () {
        if (mLaunchRibbon)
            launchRibbon();

        updateCsBuffers();

        generateMesh();
    }

    private void OnRenderObject()
    {
        drawMesh();

        curFrame ^= 1;
    }

    private void OnDestroy()
    {
        destroyResources();
    }

    void launchRibbon()
    {
        // todo swap this to slingshot's direction
        mLaunchDir = Vector3.forward;
        mLaunchOrigin = Vector3.zero;

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
        // cs - update point
        mRt_posLife = new RenderTexture[2];
        mRt_velScale = new RenderTexture[2];
        for (int i = 0; i < 2; i++)
        {
            mRt_posLife[i] = initComputeRenderTexture(numRibbons, numTrails);
            mRt_velScale[i] = initComputeRenderTexture(numRibbons, numTrails);
        }
        mRt_rotDir = initComputeRenderTexture(numRibbons, numTrails);

        // cs - generate mesh
        int numVert = numParticles * 2;
        int numTri = (numParticles - 1) * 6;
        mRt_vert = initComputeRenderTexture(numVert, 1);
        mRt_norm = initComputeRenderTexture(numVert, 1);
        mRt_texCoord = initComputeRenderTexture(numVert, 1);
        mRt_tri = initComputeRenderTexture(numTri, 1);

        // render 
        mRibbonMat = new Material(mRibbonShader);

        uint[] mArgs = new uint[5] { 0, 0, 0, 0, 0 };
        mDrawProcedural_args = new ComputeBuffer(
            1, mArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        mArgs[0] = (uint)numTri;
        mArgs[1] = (uint)numVert;

        mDrawProcedural_args.SetData(mArgs);
    }

    void destroyResources()
    {
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

        // generate mesh
        if(mRt_vert)
        {
            mRt_vert.Release();
            mRt_vert = null;
        }
        if (mRt_norm)
        {
            mRt_norm.Release();
            mRt_norm = null;
        }
        if (mRt_texCoord)
        {
            mRt_texCoord.Release();
            mRt_texCoord = null;
        }
        if (mRt_tri)
        {
            mRt_tri.Release();
            mRt_tri = null;
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

        mCs_updatePoints.SetTexture(kernel, "oPosLife", mRt_posLife[curFrame^1]);
        mCs_updatePoints.SetTexture(kernel, "oVelScale", mRt_velScale[curFrame^1]);
        mCs_updatePoints.SetTexture(kernel, "oRotDir", mRt_rotDir);

        mCs_updatePoints.Dispatch(kernel, numRibbons / numCSWorkerGroups, numTrails / numCSWorkerGroups, 1);

        // cs - meshgen
        kernel = mCs_generateMesh.FindKernel("InitMesh");

        mCs_generateMesh.SetTexture(kernel, "oVert", mRt_vert);
        mCs_generateMesh.SetTexture(kernel, "oNorm", mRt_norm);
        mCs_generateMesh.SetTexture(kernel, "oTexCoord", mRt_texCoord);
        mCs_generateMesh.SetTexture(kernel, "oTri", mRt_tri);

        mCs_generateMesh.SetInt("uNumRibbons", numRibbons);
        mCs_generateMesh.SetInt("uNumTrails", numTrails);

        mCs_generateMesh.Dispatch(kernel, numRibbons / numCSWorkerGroups, numTrails / numCSWorkerGroups, 1);
    }

    void updateCsBuffers()
    {
        int kernel = mCs_updatePoints.FindKernel("UpdateRibbon");

        mCs_updatePoints.SetTexture(kernel, "oPosLife", mRt_posLife[curFrame]);
        mCs_updatePoints.SetTexture(kernel, "oVelScale", mRt_velScale[curFrame]);
        mCs_updatePoints.SetTexture(kernel, "oRotDir", mRt_rotDir);

        mCs_updatePoints.SetTexture(kernel, "uPosLife", mRt_posLife[curFrame^1]);
        mCs_updatePoints.SetTexture(kernel, "uVelScale", mRt_velScale[curFrame ^ 1]);

        mCs_updatePoints.SetBool("uLaunchRibbon", mLaunchRibbon);

        mCs_updatePoints.SetFloat("uTrailMaxLength", mTrailMaxLength);

        mCs_updatePoints.SetInt("uNumTrails", numTrails);

        mCs_updatePoints.Dispatch(kernel, numRibbons / numCSWorkerGroups, numTrails / numCSWorkerGroups, 1);

        mLaunchRibbon = false;
    }

    void generateMesh()
    {
        int kernel = mCs_generateMesh.FindKernel("GenerateMesh");

        mCs_generateMesh.SetTexture(kernel, "oVert", mRt_vert);
        mCs_generateMesh.SetTexture(kernel, "oNorm", mRt_norm);
        mCs_generateMesh.SetTexture(kernel, "oTexCoord", mRt_texCoord);
        mCs_generateMesh.SetTexture(kernel, "oTri", mRt_tri);

        mCs_generateMesh.SetTexture(kernel, "uPosLife", mRt_posLife[curFrame]);
        mCs_generateMesh.SetTexture(kernel, "uVelScale", mRt_velScale[curFrame]);
        mCs_generateMesh.SetTexture(kernel, "uRotDir", mRt_rotDir);

        mCs_generateMesh.SetInt("uNumRibbons", numRibbons);
        mCs_generateMesh.SetInt("uNumTrails", numTrails);

        mCs_generateMesh.Dispatch(kernel, numRibbons / numCSWorkerGroups, numTrails / numCSWorkerGroups, 1);
    }

    void drawMesh()
    {
        mRibbonMat.SetPass(0);

        mRibbonMat.SetTexture("uVert", mRt_vert);
        mRibbonMat.SetTexture("uNorm", mRt_norm);
        mRibbonMat.SetTexture("uTexCoord", mRt_texCoord);
        mRibbonMat.SetTexture("uTri", mRt_texCoord);

        mRibbonMat.SetTexture("uPosLife", mRt_posLife[curFrame]);
        mRibbonMat.SetTexture("uVelScale", mRt_velScale[curFrame]);
        mRibbonMat.SetTexture("uRotDir", mRt_rotDir);

        Graphics.DrawProceduralIndirect(
            MeshTopology.Triangles, mDrawProcedural_args, 0);
    }
}
