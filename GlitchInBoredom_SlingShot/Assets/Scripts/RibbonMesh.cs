using UnityEngine;

public class RibbonMesh
{
    Mesh mesh;

    // construct y-up mesh
    public RibbonMesh(int w, int h)
    {
        mesh = buildMesh(w, h);
    }

    public Mesh buildMesh(int w, int h)
    {
        Mesh m = new Mesh();

        int numPoint = w * h;
        int numVert = numPoint * 2;
        int numTri = (numPoint - 1) * 6;

        Vector3[] vertices = new Vector3[numVert];
        Vector3[] normals = new Vector3[numVert];
        Vector2[] uv = new Vector2[numVert];
        Vector2[] uv2 = new Vector2[numVert]; // to access point position in render texture
        int[] tri = new int[numTri];

        for (int i = 0; i < numPoint; i++)
        {
            int vertIndex = i * 2;
            float texCoord = i % w;
            Vector2 bufCoord = new Vector2(texCoord, Mathf.Floor((float)i / (float)w)); // mesh is based on trails
            texCoord /= (float)(w-1);

            //Debug.Log(texCoord + ", " + bufCoord);

            // left
            vertices[vertIndex] = new Vector3(-0.5f, 0f, 0f);
            normals[vertIndex] = Vector3.up;
            uv[vertIndex] = new Vector2(0f, texCoord);
            uv2[vertIndex] = bufCoord;

            vertIndex++;

            // right
            vertices[vertIndex] = new Vector3(0.5f, 0f, 0f);
            normals[vertIndex] = Vector3.up;
            uv[vertIndex] = new Vector2(1f, texCoord);
            uv2[vertIndex] = bufCoord;

            if (i > 0)
            {
                int triIndex = (i - 1) * 6;
                bool isTrailHead = texCoord == 0f;

                tri[triIndex++] = isTrailHead ? vertIndex - 3 : vertIndex - 3;
                tri[triIndex++] = isTrailHead ? vertIndex - 3 : vertIndex - 1;
                tri[triIndex++] = isTrailHead ? vertIndex - 3 : vertIndex - 2;

                tri[triIndex++] = isTrailHead ? vertIndex - 3 : vertIndex - 1;
                tri[triIndex++] = isTrailHead ? vertIndex - 3 : vertIndex - 0;
                tri[triIndex++] = isTrailHead ? vertIndex - 3 : vertIndex - 2;
            }
        }

        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = vertices;
        m.normals = normals;
        m.uv = uv;
        m.uv2 = uv2;
        m.triangles = tri;
        m.SetTriangles(tri, 0, true, 0); //?? is it necessary ??
        m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        return m;
    }

    public Mesh getMesh()
    {
        return mesh;
    }

    public void destroy()
    {
        MonoBehaviour.DestroyImmediate(mesh);
    }

    
}
