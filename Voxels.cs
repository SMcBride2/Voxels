using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxels : MonoBehaviour {
    public int hugeVert;
    public Heightmap m_hm;
    private int m_maxVerts = 64998;

    public GameObject m_terrList, m_undList;
    private List<Mesh> m;
    private MeshFilter[] f0, f1;
    private MeshCollider[] c0, c1;

    private List<Heightmap.ModelType> m_voxModel;//, m_subModel;

    private float stepX = 1.0f, stepZ = 1.0f, space = 0.22f, core, cutU, cutV, cellHeight = 1.0f, cubeMin, cubeMax;
    private bool m_finLoading = false;

    private readonly Vector3[] cornerNorm = { new Vector3(1.0f, 1.0f, .0f), new Vector3(1.0f, -1.0f, .0f), new Vector3(.0f, 1.0f, 1.0f), new Vector3(.0f, -1.0f, 1.0f) };
    private Vector3[] pnt = new Vector3[4], ind = new Vector3[4], bod = new Vector3[4], normList;
    private Vector2[] t = new Vector2[4], b = new Vector2[4];
    private float sqFactor;

    void Start() {
        core = cellHeight - space - space;
        sqFactor = Mathf.Sqrt(space * space * 2);
        cubeMin = sqFactor - space;
        cubeMax = 1.0f - cubeMin;

        m_voxModel = new List<Heightmap.ModelType>();
        //m_subModel = new List<Heightmap.ModelType>();
        m = new List<Mesh>();
        f0 = m_terrList.GetComponentsInChildren<MeshFilter>();
        f1 = m_undList.GetComponentsInChildren<MeshFilter>();
        c0 = m_terrList.GetComponentsInChildren<MeshCollider>();
        c1 = m_undList.GetComponentsInChildren<MeshCollider>();


        for (int i = 0; i < 8; i++) {
            BuildVoxelModels(i);
        }
        DrawTerrain();
    }

    void Update() {
        
    }

    bool DrawTerrain() {
        int lSize = f0.Length, len = m_voxModel.Count;
/*        List<int> iL = new List<int>();
        normList = new Vector3[len];
        int[] map = new int[len], posCount = new int[len];

        for (int v = 0; v < len; v++) {
            normList[v] = m_voxModel[v].norm();
            if (!iL.Contains(v)) {
                iL.Add(v);
                map[v] = v;
                posCount[v] = 1;
                for (int w = v + 1; w < len; w++) {
                    if (!iL.Contains(w)) {
                        if (m_voxModel[v].x == m_voxModel[w].x && m_voxModel[v].z == m_voxModel[w].z) {
                            iL.Add(w);
                            map[w] = v;
                            posCount[v]++;
                            normList[v] += m_voxModel[w].norm();
                        }
                    }
                }
            } else {
                posCount[v] = 0;
            }
        }*/
        /*
        for (int v = 0; v < len; v++) {
            if (posCount[v] > 0) {
                normList[v] /= posCount[v];
            }
        }*/

        List<Vector3>[] l = new List<Vector3>[lSize],
            n = new List<Vector3>[lSize];
        List<Vector2>[] uv = new List<Vector2>[lSize];
        int[][] indices = new int[lSize][];
        for (int j = 0; j < lSize; j++) {
            l[j] = new List<Vector3>();
            n[j] = new List<Vector3>();
            uv[j] = new List<Vector2>();
            indices[j] = new int[m_maxVerts];
        }

        int totalVerts = 0;
        int i = 0, p = 0, count = 0;
            int numVerts = m_voxModel.Count;// m_hm.GetNumVerts(p);
        hugeVert = numVerts;
        while (i < numVerts) {//for (p = 0; p < lSize; p++) {
            count = 0;
            for (i = totalVerts; i < numVerts && i < totalVerts + m_maxVerts; i++) {
                l[p].Add(m_voxModel[i].pos());
                n[p].Add(m_voxModel[i].norm()); //normList[map[i]]);
                uv[p].Add(m_voxModel[i].tex());
                indices[p][count] = count;
                count++;
            }
                totalVerts = i;
            if (totalVerts >= numVerts) { break; }
            
            p++;
            if (p > 4) { break; }
            
        }

        //m_subModel
        int r = 0;
        /*numVerts = m_subModel.Count;
        i = 0;
        count = 0;
        totalVerts = 0;
        while (i < numVerts) {
            count = 0;
            for (i = totalVerts; i < numVerts && i < totalVerts + m_maxVerts; i++) {
                l[p + r].Add(m_subModel[i].pos());
                n[p + r].Add(m_subModel[i].norm());
                uv[p + r].Add(m_subModel[i].tex());
                indices[p + r][count] = count;
                count++;
            }
            totalVerts = i;
            r++;
            break;
        }*/


        int g;
        for (g = 0; g < p + r && g < f0.Length && g < lSize; g++) {
            m.Add(new Mesh());

            m[g].SetVertices(l[g]);
            m[g].SetNormals(n[g]);
            m[g].SetUVs(0, uv[g]);
            if (g == p + r) {
                int[] indTmp = new int[count];
                for (int u = 0; u < count; u++) {
                    indTmp[u] = indices[g][u];
                }
                m[g].triangles = indTmp;
            } else { 
                m[g].triangles = indices[g];
            }
            m[g].RecalculateBounds();
            if (f0[g] != null) {
                f0[g].sharedMesh = m[g];
            }
            /*if (c0[g] != null) {
                c0[g].sharedMesh = m[g];
            }*/
        }

        m_finLoading = true;
        return true;
    }





















    private int WallT1(int index, int a, int b, Vector3 n) {//, float inSet, float outSet) {//TODO: x1,0
        m_voxModel.Add(m_hm.Model(index, pnt[a], n, new Vector2(1.0f, .0f))); 
        index++;
        m_voxModel.Add(m_hm.Model(index, pnt[b], n, new Vector2(.0f, .0f))); 
        index++;
        m_voxModel.Add(m_hm.Model(index, ind[a], n, new Vector2(1.0f - t[0].x, 1.0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, pnt[b], n, new Vector2(.0f, .0f))); 
        index++;
        m_voxModel.Add(m_hm.Model(index, ind[b], n, new Vector2(1.0f - t[1].x, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, ind[a], n, new Vector2(1.0f - t[0].x, 1.0f)));
        index++;

        return index;
    }

    private int WallT2(int index, int a, int b, int c, Vector3 n, float inSet, float outSet) {//TODO: y3,1
        m_voxModel.Add(m_hm.Model(index, pnt[a], n, new Vector2(1.0f, .0f)));
        index++;
		m_voxModel.Add(m_hm.Model(index, pnt[b], n, new Vector2(.0f, .0f)));
        index++;
		m_voxModel.Add(m_hm.Model(index, ind[a], n, new Vector2(1.0f - t[1].y, 1.0f)));
		index++;

		m_voxModel.Add(m_hm.Model(index, pnt[b], n, new Vector2(.0f, .0f)));
        index++;
		m_voxModel.Add(m_hm.Model(index, ind[c], n, new Vector2(1.0f - t[3].y, 1.0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[a], n, new Vector2(1.0f - t[1].y, 1.0f)));
		index++;

        return index;
    }

    private int WallT3(int index, int a, int b, Vector3 n, float inSet, float outSet) {//TODO: x2,3
        m_voxModel.Add(m_hm.Model(index, pnt[a], n, new Vector2(1.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, pnt[b], n, new Vector2(.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[b], n, new Vector2(t[3].x, 1.0f)));
		index++;

		m_voxModel.Add(m_hm.Model(index, pnt[b], n, new Vector2(.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[a], n, new Vector2(t[2].x, 1.0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[b], n, new Vector2(t[3].x, 1.0f)));
		index++;
        return index;
    }

    private int WallT4(int index, int a, int b, int c, Vector3 n, float inSet, float outSet) {//TODO: y0,2
        m_voxModel.Add(m_hm.Model(index, pnt[a], -cornerNorm[1], new Vector2(1.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, pnt[b], -cornerNorm[1], new Vector2(.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[c], -cornerNorm[1], new Vector2(t[2].y, 1.0f)));
		index++;

		m_voxModel.Add(m_hm.Model(index, pnt[b], -cornerNorm[1], new Vector2(.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[b], -cornerNorm[1], new Vector2(t[0].y, 1.0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[c], -cornerNorm[1], new Vector2(t[2].y, 1.0f)));
		index++;
        /*m_voxModel.Add(m_hm.Model(index, pnt[a], -cornerNorm[1], new Vector2(.0f, 1.0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, pnt[b], -cornerNorm[1], new Vector2(.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[c], -cornerNorm[1], new Vector2(1.0f, inSet)));
		index++;

		m_voxModel.Add(m_hm.Model(index, pnt[b], -cornerNorm[1], new Vector2(.0f, .0f)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[b], -cornerNorm[1], new Vector2(1.0f, outSet)));
		index++;
		m_voxModel.Add(m_hm.Model(index, ind[c], -cornerNorm[1], new Vector2(1.0f, inSet)));
		index++;*/

        return index;
    }

    private int WallB1(int index, int a, int i, Vector3 n) {//, float inSet, float outSet) {
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[i].x, pnt[i].y - core, pnt[i].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[i].x, ind[i].y - cellHeight, ind[i].z), n, new Vector2(1.0f - b[0].x, .0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[i].x, ind[i].y - cellHeight, ind[i].z), n, new Vector2(1.0f - b[0].x, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[a].x, ind[a].y - cellHeight, ind[a].z), n, new Vector2(1.0f - b[1].x, .0f)));
        index++;

        return index;
    }
    
    private int WallB2(int index, int a, int i, int c, Vector3 n, float inSet, float outSet) {
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[i].x, pnt[i].y - core, pnt[i].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[i].x, ind[i].y - cellHeight, ind[i].z), n, new Vector2(1.0f - b[1].y, .0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[i].x, ind[i].y - cellHeight, ind[i].z), n, new Vector2(1.0f - b[1].y, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(1.0f - b[3].y, .0f)));
        index++;
        /*
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[b].x, pnt[b].y - core, pnt[b].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[b].x, ind[b].y - cellHeight, ind[b].z), n, new Vector2(1.0f, .0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[b].x, ind[b].y - cellHeight, ind[b].z), n, new Vector2(1.0f, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(.0f, .0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[b].x, pnt[b].y - core, pnt[b].z), n, new Vector2(1.0f, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[b].x, ind[b].y - cellHeight, ind[b].z), n, new Vector2(.0f, inSet)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[b].x, ind[b].y - cellHeight, ind[b].z), n, new Vector2(.0f, inSet)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(.0f, outSet)));
        index++;*/

        return index;
    }

    private int WallB3(int index, int a, int i, Vector3 n, float inSet, float outSet) {
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[i].x, pnt[i].y - core, pnt[i].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[a].x, ind[a].y - cellHeight, ind[a].z), n, new Vector2(b[3].x, .0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[a].x, ind[a].y - cellHeight, ind[a].z), n, new Vector2(b[3].x, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[i].x, ind[i].y - cellHeight, ind[i].z), n, new Vector2(b[2].x, .0f)));
        index++;

        return index;
    }

    private int WallB4(int index, int a, int i, int c, Vector3 n, float inSet, float outSet) {
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[i].x, pnt[i].y - core, pnt[i].z), n, new Vector2(1.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(b[2].y, .0f)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(b[2].y, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[a].x, ind[a].y - cellHeight, ind[a].z), n, new Vector2(b[0].y, .0f)));
        index++;

        /*m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[i].x, pnt[i].y - core, pnt[i].z), n, new Vector2(.0f, 1.0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(1.0f, inSet)));
        index++;

        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[a].x, pnt[a].y - core, pnt[a].z), n, new Vector2(.0f, .0f)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[c].x, ind[c].y - cellHeight, ind[c].z), n, new Vector2(1.0f, inSet)));
        index++;
        m_voxModel.Add(m_hm.Model(index, new Vector3(ind[a].x, ind[a].y - cellHeight, ind[a].z), n, new Vector2(1.0f, outSet)));
        index++;*/

        return index;
    }


    private bool BuildVoxelModels(int num) {
        int i, j, index = 0, indS = 0, tH = m_hm.GetHeight(), tW = m_hm.GetWidth();
        int m_vertexCount = (tH - 1) * (tW - 1) * 6, maxSize = m_vertexCount * 7, stackSize;
        float space = .22f, height = .0f, stackFactor = m_hm.GetDiv() * cellHeight;
        int[] p = new int[9];
        bool[] nextLayer = m_hm.GetVertStatus(num);
        Heightmap.HeightMapType[] nextHM = m_hm.GetHM(num);

        //double uPos = 0, vPos = 0, uSize = 1.0f / m_segTerWidth, vSize = 1.0f / m_segTernHeight;

        bool built;
        for (j = 0; j < (tH - 2); j++) {
            for (i = 0; i < (tW - 2); i++) {
                built = false;
                p[4] = (tW * (j)) + i;

                if (nextLayer[p[4]]) {
                    bool[] c = new bool[8];
                    float bottom = height - core;
                    stackSize = Mathf.RoundToInt(nextHM[p[4]].d * m_hm.GetDiv());
                    height = nextHM[p[4]].y;
                    ind[0] = new Vector3(i, height + space, 256 - j);
                    ind[1] = new Vector3(i + 1, ind[0].y, ind[0].z);
                    ind[2] = new Vector3(i, ind[0].y, 255 - j);
                    ind[3] = new Vector3(ind[1].x, ind[0].y, ind[2].z);

                    if (j < 1) {
                        p[0] = p[1] = p[2] = -257;
                        p[7] = p[4] + tW;// (tW * (j + 1)) + i;
                        p[6] = p[7] - 1;// (tW * (j + 1)) + i - 1;
                        p[8] = p[7] + 1;// (tW * (j + 1)) + i + 1;
                    } else {
                        p[1] = p[4] - tW;//(tW * (j - 1)) + i;
                        p[0] = p[1] - 1;//(tW * (j - 1)) + i - 1;
                        p[2] = p[1] + 1;//(tW * (j - 1)) + i + 1;

                        if (j > tH - 2) {
                            p[6] = p[7] = p[8] = -257;
                        } else {
                            p[7] = p[4] + tW;// (tW * (j + 1)) + i;
                            p[6] = p[7] - 1;// (tW * (j + 1)) + i - 1;
                            p[8] = p[7] + 1;// (tW * (j + 1)) + i + 1;
                        }
                    }
                    if (i < 1) {
                        p[0] = p[3] = p[6] = -257;
                        p[5] = p[4] + 1;// (tW * (j)) + i + 1;
                    } else if (i > tW - 2) {
                        p[2] = p[5] = p[8] = -257;
                        p[3] = p[4] - 1;// (tW * (j)) + i - 1;
                    } else {
                        p[3] = p[4] - 1;// (tW * (j)) + i - 1;
                        p[5] = p[4] + 1;// (tW * (j)) + i + 1;
                    }


                    pnt[0] = new Vector3(stepX * i, height, stepZ * (256 - j));
                    pnt[1] = new Vector3(pnt[0].x + stepX, height, pnt[0].z);
                    pnt[2] = new Vector3(pnt[1].x, height, pnt[0].z - stepZ);
                    pnt[3] = new Vector3(pnt[0].x, height, pnt[2].z);

                    float[] stackSz = new float[4];
                    if (p[1] < -256) {
                        stackSz[0] = 0;
                    } else {
                        stackSz[0] = nextHM[p[1]].d * stackFactor;
                    }
                    if (p[3] < -256) {
                        stackSz[1] = 0;
                    } else {
                        stackSz[1] = nextHM[p[3]].d * stackFactor;
                    }
                    
                    if (p[5] < -256) {
                        stackSz[2] = 0;
                    } else {
                        stackSz[2] = nextHM[p[5]].d * stackFactor;
                    }
                    if (p[7] < -256) {
                        stackSz[3] = 0;
                    } else {
                        stackSz[3] = nextHM[p[7]].d * stackFactor;
                    }
                    c[0] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y - space || pnt[0].y < nextHM[p[3]].y - stackSz[1]);
                    c[1] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y - space || pnt[0].y < nextHM[p[1]].y - stackSz[0]);
                    c[2] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y - space || pnt[0].y < nextHM[p[5]].y - stackSz[2]);
                    c[3] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y - space || pnt[0].y < nextHM[p[7]].y - stackSz[3]);
                    
                    /*

                    c[4] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y || pnt[0].y - space - core < nextHM[p[3]].y - nextHM[p[3]].d * stackFactor);
                    c[5] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y || pnt[0].y - space - core < nextHM[p[1]].y - nextHM[p[1]].d * stackFactor);
                    c[6] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y || pnt[0].y - space - core < nextHM[p[5]].y - nextHM[p[5]].d * stackFactor);
                    c[7] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y || pnt[0].y - space - core < nextHM[p[7]].y - nextHM[p[7]].d * stackFactor);


                    */
                    Vector2[] top = new Vector2[4];
                    top[0].x = top[2].x = top[0].y = top[1].y = t[0].x = t[2].x = b[0].x = b[2].x = t[0].y = t[1].y = b[0].y = b[1].y = .0f;
                    top[1].x = top[3].x = top[2].y = top[3].y = t[1].x = t[3].x = b[1].x = b[3].x = t[2].y = t[3].y = b[2].y = b[3].y = 1.0f;

                    if (c[0]) {
					    built = true;
                        t[0].x += space;
                        t[2].x += space;
                        top[1].x -= space;
                        top[3].x -= space;
					    ind[0].x += space;
					    ind[2].x += space;
				
					    if (c[1]) {
                            t[0].y += space;
                            t[1].y += space;
                            top[0].y += space;
                            top[1].y += space;
                            ind[0].z -= space;
						    ind[1].z -= space;

						    if (c[2]) {
                                t[1].x -= space;
                                t[3].x -= space;
                                top[0].x += space;
                                top[2].x += space;
                                ind[1].x -= space;
							    ind[3].x -= space;
                                if (c[3]) {
                                    t[2].y -= space;
                                    t[3].y -= space;
                                    top[2].y -= space;
                                    top[3].y -= space;
                                    ind[2].z += space;
                                    ind[3].z += space;
                                }
						    } else {
							    if (c[3]) {//1,3,4
                                    t[2].y -= space;
                                    t[3].y -= space;
                                    top[2].y -= space;
                                    top[3].y -= space;
                                    ind[2].z += space;
								    ind[3].z += space;
							    }
						    }
					    } else if (c[3]) {
                            t[2].y -= space;
                            t[3].y -= space;
                            top[2].y -= space;
                            top[3].y -= space;
                            ind[2].z += space;
						    ind[3].z += space;

						    if (c[2]) {//2,3,4
                                t[1].x -= space;
                                t[3].x -= space;
                                top[0].x += space;
                                top[2].x += space;
                                ind[1].x -= space;
							    ind[3].x -= space;
						    }
					    } else {
						    if (c[2]) {//2,4
                                t[1].x -= space;
                                t[3].x -= space;
                                top[0].x += space;
                                top[2].x += space;
                                ind[1].x -= space;
							    ind[3].x -= space;
						    }
					    }
				    } else if (c[2]) {
					    built = true;
                        t[1].x -= space;
                        t[3].x -= space;
                        top[0].x += space;
                        top[2].x += space;
                        ind[1].x -= space;
					    ind[3].x -= space;
					    if (c[1]) {
                            t[0].y += space;
                            t[1].y += space;
                            top[0].y += space;
                            top[1].y += space;
                            ind[0].z -= space;
						    ind[1].z -= space;

						    if (c[3]) {//1,2,3
                                t[2].y -= space;
                                t[3].y -= space;
                                top[2].y -= space;
                                top[3].y -= space;
                                ind[2].z += space;
							    ind[3].z += space;
						    }
					    } else if (c[3]) {//2,3
                            t[2].y -= space;
                            t[3].y -= space;
                            top[2].y -= space;
                            top[3].y -= space;
                            ind[2].z += space;
						    ind[3].z += space;
					    }
				    } else if (c[1]) {
					    built = true;
                        t[0].y += space;
                        t[1].y += space;
                        top[0].y += space;
                        top[1].y += space;
                        ind[0].z -= space;
					    ind[1].z -= space;

					    if (c[3]) {//1,3
                            t[2].y -= space;
                            t[3].y -= space;
                            top[2].y -= space;
                            top[3].y -= space;
                            ind[2].z += space;
						    ind[3].z += space;
					    }
				    } else if (c[3]) {//3
					    built = true;
                        t[2].y -= space;
                        t[3].y -= space;
                        top[2].y -= space;
                        top[3].y -= space;
                        ind[2].z += space;
					    ind[3].z += space;
				    }


                    
                    if (stackSize > 0) {
                        float rimHeight = pnt[0].y - space;
                        c[0] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y || rimHeight < nextHM[p[3]].y - stackSz[1]);
                        c[1] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y || rimHeight < nextHM[p[1]].y - stackSz[0]);
                        c[2] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y || rimHeight < nextHM[p[5]].y - stackSz[2]);
                        c[3] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y || rimHeight < nextHM[p[7]].y - stackSz[3]);

                        c[4] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y - space || pnt[0].y - space < nextHM[p[3]].y - stackSz[1] || p[0] < -256 || p[6] < -256 || nextHM[p[3]].y - space > nextHM[p[0]].y || nextHM[p[3]].y - space > nextHM[p[6]].y);
                        c[5] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y - space || pnt[0].y - space < nextHM[p[1]].y - stackSz[0] || p[0] < -256 || p[2] < -256 || nextHM[p[1]].y - space > nextHM[p[0]].y || nextHM[p[1]].y - space > nextHM[p[2]].y);
                        c[6] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y - space || pnt[0].y - space < nextHM[p[5]].y - stackSz[2] || p[2] < -256 || p[8] < -256 || nextHM[p[5]].y - space > nextHM[p[2]].y || nextHM[p[5]].y - space > nextHM[p[8]].y);
                        c[7] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y - space || pnt[0].y - space < nextHM[p[7]].y - stackSz[3] || p[6] < -256 || p[8] < -256 || nextHM[p[7]].y - space > nextHM[p[6]].y || nextHM[p[7]].y - space > nextHM[p[8]].y);

                        // Platform 
                        if (!c[1] && !c[2] && (p[2] < -256 || !nextLayer[p[2]] || (nextHM[p[2]].y < pnt[0].y - space || pnt[0].y < nextHM[p[2]].y - nextHM[p[2]].d * stackFactor))) { 
                            m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z), Vector3.up, new Vector2(space, .0f)));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;

                            /*m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, t[1]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z), Vector3.up, t[0]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z - space), Vector3.up, t[3]));
                            index++;*/
                            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z), Vector3.up, new Vector2(space, .0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, ind[1].y, ind[1].z - space), Vector3.up, new Vector2(.0f, space)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, ind[1].y, ind[1].z - space), Vector3.up, new Vector2(.0f, space)));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
				            index++;

                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, ind[1].y, ind[1].z - space), Vector3.up, new Vector2(.0f, space)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z), Vector3.up, new Vector2(space, .0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y, pnt[1].z), Vector3.forward, new Vector2(.0f, .0f)));
                            index++;
                            /*m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x - space, ind[0].y, ind[0].z), Vector3.up, t[1]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z - space), Vector3.up, t[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, t[3]));
				            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, t[3]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x - space, ind[1].y, ind[1].z - space), Vector3.up, t[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x - space, ind[3].y, ind[3].z - space), Vector3.up, t[2]));
				            index++;*/
                        //} else if (!c[2] && !c[3] &&  (p[0] < -256 || !nextLayer[p[0]] || (nextHM[p[0]].y < pnt[0].y - space || pnt[0].y < nextHM[p[0]].y - nextHM[p[0]].d * stackFactor))) {
                        //} else if ((p[8] < -256 || !nextLayer[p[8]] || (nextHM[p[8]].y < pnt[0].y - space || pnt[0].y < nextHM[p[8]].y - nextHM[p[8]].d * stackFactor))) {
                        } else if (!c[0] && !c[1] && (p[0] < -256 || !nextLayer[p[0]] || (nextHM[p[0]].y < pnt[0].y - space || pnt[0].y < nextHM[p[0]].y - nextHM[p[0]].d * stackFactor))) {
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x + space, ind[0].y, ind[0].z), Vector3.up, new Vector2(1.0f - space, .0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x, ind[0].y, ind[0].z - space), Vector3.up, new Vector2(1.0f, space)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x + space, ind[0].y, ind[0].z), Vector3.up, new Vector2(1.0f - space, .0f)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x + space, ind[0].y, ind[0].z), Vector3.up, new Vector2(1.0f - space, .0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x, ind[0].y, ind[0].z - space), Vector3.up, new Vector2(1.0f, space)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x, ind[0].y - space, ind[0].z), Vector3.up, new Vector2(1.0f, .0f)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x, ind[0].y, ind[0].z - space), Vector3.up, new Vector2(1.0f, space)));
				            index++;
                            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;
                            /*m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
				            index++;*/
                        } else if (!c[3] && !c[0] && (p[6] < -256 || !nextLayer[p[6]] || (nextHM[p[6]].y < pnt[0].y - space || pnt[0].y < nextHM[p[6]].y - nextHM[p[6]].d * stackFactor))) {
                            m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, ind[2].y, ind[2].z + space), Vector3.up, new Vector2(1.0f, 1.0f - space)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x + space, ind[2].y, ind[2].z), Vector3.up, new Vector2(1.0f - space, 1.0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, ind[2].y, ind[2].z + space), Vector3.up, new Vector2(1.0f, 1.0f - space)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, ind[2].y, ind[2].z + space), Vector3.up, new Vector2(1.0f, 1.0f - space)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x + space, ind[2].y, ind[2].z), Vector3.up, new Vector2(1.0f - space, 1.0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, ind[2].y - space, ind[2].z), Vector3.up, new Vector2(1.0f, 1.0f)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
				            index++;
                            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x + space, ind[2].y, ind[2].z), Vector3.up, new Vector2(1.0f - space, 1.0f)));
				            index++;
                        } else if (!c[2] && !c[3] && (p[8] < -256 || !nextLayer[p[8]] || (nextHM[p[8]].y < pnt[0].y - space || pnt[0].y < nextHM[p[8]].y - nextHM[p[8]].d * stackFactor))) {
                            m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x, ind[3].y, ind[3].z + space), Vector3.up, new Vector2(.0f, 1.0f - space)));
				            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x, ind[3].y, ind[3].z + space), Vector3.up, new Vector2(.0f, 1.0f - space)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x - space, ind[3].y, ind[3].z), Vector3.up, new Vector2(space, 1.0f)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x - space, ind[3].y, ind[3].z), Vector3.up, new Vector2(space, 1.0f)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x, ind[3].y, ind[3].z + space), Vector3.up, new Vector2(.0f, 1.0f - space)));
                            index++;
                            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x, ind[3].y - space, ind[3].z), Vector3.up, new Vector2(.0f, 1.0f)));
                            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x - space, ind[3].y, ind[3].z), Vector3.up, new Vector2(space, 1.0f)));
				            index++;
                        } else { 
                            m_voxModel.Add(m_hm.Model(index, ind[0], Vector3.up, top[1]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;

                            m_voxModel.Add(m_hm.Model(index, ind[2], Vector3.up, top[3]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[1], Vector3.up, top[0]));
				            index++;
				            m_voxModel.Add(m_hm.Model(index, ind[3], Vector3.up, top[2]));
				            index++;
                        }

                        // First Under Wall
                        if (c[1]) {
                            index = WallT1(index, 0, 1, cornerNorm[2]);

                            if (c[5]) {
                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core - space, pnt[0].z), Vector3.forward, new Vector2(1.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - core - space, pnt[1].z), Vector3.forward, new Vector2(.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f - t[0].y)));
                                index++;

                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - core - space, pnt[1].z), Vector3.forward, new Vector2(.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.forward, new Vector2(.0f, 1.0f - t[1].y)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f - t[0].y)));
                                index++;
                            }
                        }

                        if (c[2]) {
                            index = WallT2(index, 1, 2, 3, cornerNorm[0], space, cutV);

                            if (c[6]) {
                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - core - space, pnt[1].z), Vector3.right, new Vector2(1.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - core - space, pnt[2].z), Vector3.right, new Vector2(.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, t[1].x)));
                                index++;

                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - core - space, pnt[2].z), Vector3.right, new Vector2(.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.right, new Vector2(.0f, t[3].x)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, t[1].x)));
                                index++;
                            }
                        }

                        if (c[3]) { 
                            index = WallT3(index, 2, 3, -cornerNorm[3], cutU, space);

                            if (c[7]) {
                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - core - space, pnt[2].z), Vector3.back, new Vector2(1.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core - space, pnt[3].z), Vector3.back, new Vector2(.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, t[3].x)));
                                index++;

                                m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core - space, pnt[3].z), Vector3.back, new Vector2(.0f, .0f)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.back, new Vector2(.0f, t[1].x)));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, t[3].x)));
                                index++;
                            }
                        }

                        if (c[0]) { 
                            index = WallT4(index, 3, 0, 2, -cornerNorm[1], cutV, space);

                            if (c[4]) {
                                /*if (p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y - space - core || pnt[0].y - space - core < nextHM[p[3]].y - stackSz[1] || p[0] < -256 || p[6] < -256 || nextHM[p[3]].y - space > nextHM[p[0]].y || nextHM[p[3]].y - space > nextHM[p[6]].y)) {
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core - space, pnt[3].z), Vector3.left, new Vector2(1.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core - space, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left, new Vector2(1.0f, 1.0f - t[2].x)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core - space, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.left, new Vector2(.0f, 1.0f - t[0].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left,new Vector2(1.0f, 1.0f - t[2].x)));
                                    index++;
                                } else { */
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core - space, pnt[3].z), Vector3.left, new Vector2(1.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core - space, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left, new Vector2(1.0f, 1.0f - t[2].x)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core - space, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.left, new Vector2(.0f, 1.0f - t[0].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left,new Vector2(1.0f, 1.0f - t[2].x)));
                                    index++;
                                //}
                            }
                        }

                        if (stackSize > 1) {
                            height -= space + core;
                            for (int column = 0; column < stackSize - 1 && column < 5; column++) {
                                for (int ps = 0; ps < 4; ps++) {
                                    pnt[ps].y -= (space + core);
                                    ind[ps].y -= (space + core);
                                }
                                t[0].x = t[2].x = b[0].x = b[2].x = t[0].y = t[1].y = b[0].y = b[1].y = .0f;
                                t[1].x = t[3].x = b[1].x = b[3].x = t[2].y = t[3].y = b[2].y = b[3].y = 1.0f;

                                c[4] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y || pnt[0].y - cellHeight < nextHM[p[3]].y - stackSz[1]);
                                c[5] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y || pnt[0].y - cellHeight < nextHM[p[1]].y - stackSz[0]);
                                c[6] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y || pnt[0].y - cellHeight < nextHM[p[5]].y - stackSz[2]);
                                c[7] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y || pnt[0].y - cellHeight < nextHM[p[7]].y - stackSz[3]);

                                // First Under Wall
                                if (c[5]) {
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - cellHeight, pnt[0].z), Vector3.forward, new Vector2(1.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - cellHeight, pnt[1].z), Vector3.forward, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f)));
                                    index++;
                                    
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - cellHeight, pnt[1].z), Vector3.forward, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.forward, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                // Second Under Wall
                                if (c[6]) {
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - cellHeight, pnt[1].z), Vector3.right, new Vector2(1.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - cellHeight, pnt[2].z), Vector3.right, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, 1.0f)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - cellHeight, pnt[2].z), Vector3.right, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.right, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                // Third Under Wall
                                if (c[7]) {
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - cellHeight, pnt[2].z), Vector3.back, new Vector2(1.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - cellHeight, pnt[3].z), Vector3.back, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, 1.0f)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - cellHeight, pnt[3].z), Vector3.back, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.back, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                // Fourth Under Wall
                                if (c[4]) {
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - cellHeight, pnt[3].z), Vector3.left, new Vector2(1.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - cellHeight, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left, new Vector2(1.0f, 1.0f)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - cellHeight, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.left, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                for (int ps = 0; ps < 4; ps++) {
                                    pnt[ps].y -= space;
                                    ind[ps].y -= space;
                                }
                                height -= cellHeight;
                            }
                            for (int ps = 0; ps < 4; ps++) {
                                pnt[ps].y -= space + core;
                                ind[ps].y -= space + core;
                            }

                            if (c[0]) {
					            built = true;
					            b[0].x += space;
					            b[2].x += space;
					            bod[0].x += space;
					            bod[2].x += space;
				
					            if (c[1]) {
						            b[0].y += space;
						            b[1].y += space;
						            bod[0].z -= space;
						            bod[1].z -= space;

						            if (c[2]) {
							            b[1].x -= space;
							            b[3].x -= space;
							            bod[1].x -= space;
							            bod[3].x -= space;
                                        if (c[3]) {
                                            b[2].y -= space;
                                            b[3].y -= space;
                                            bod[2].z += space;
                                            bod[3].z += space;
                                        }
						            } else {
							            if (c[3]) {//1,3,4
								            b[2].y -= space;
								            b[3].y -= space;
								            bod[2].z += space;
								            bod[3].z += space;
							            }
						            }
					            } else if (c[3]) {
						            b[2].y -= space;
						            b[3].y -= space;
						            bod[2].z += space;
						            bod[3].z += space;

						            if (c[2]) {//2,3,4
							            b[1].x -= space;
							            b[3].x -= space;
							            bod[1].x -= space;
							            bod[3].x -= space;
						            }
					            } else {
						            if (c[2]) {//2,4
							            b[1].x -= space;
							            b[3].x -= space;
							            bod[1].x -= space;
							            bod[3].x -= space;
						            }
					            }
				            } else if (c[2]) {
					            built = true;
					            b[1].x -= space;
					            b[3].x -= space;
					            bod[1].x -= space;
					            bod[3].x -= space;
					            if (c[1]) {
						            b[0].y += space;
						            b[1].y += space;
						            bod[0].z -= space;
						            bod[1].z -= space;

						            if (c[3]) {//1,2,3
							            b[2].y -= space;
							            b[3].y -= space;
							            bod[2].z += space;
							            bod[3].z += space;
						            }
					            } else if (c[3]) {//2,3
						            b[2].y -= space;
						            b[3].y -= space;
						            bod[2].z += space;
						            bod[3].z += space;
					            }
				            } else if (c[1]) {// not inset if any cases
					            built = true;
					            b[0].y += space;
					            b[1].y += space;
					            bod[0].z -= space;
					            bod[1].z -= space;

					            if (c[3]) {//1,3
						            b[2].y -= space;
						            b[3].y -= space;
						            bod[2].z += space;
						            bod[3].z += space;
					            }
				            } else if (c[3]) {//3
					            built = true;
					            b[2].y -= space;
					            b[3].y -= space;
					            bod[2].z += space;
					            bod[3].z += space;
				            }
                            if (built) {
                                c[0] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[3]].y - stackSz[1]);//c[0] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y - core || pnt[0].y - core < nextHM[p[3]].y - nextHM[p[3]].d * stackFactor);
                                c[1] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[1]].y - stackSz[0]);
                                c[2] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[5]].y - stackSz[2]);
                                c[3] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[7]].y - stackSz[3]);

                                
                                if (c[1]) {
                                    // First Under Wall
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - space - core, pnt[0].z), Vector3.forward, new Vector2(1.0f, b[1].y)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - space - core, pnt[1].z), Vector3.forward, new Vector2(.0f, b[0].y)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f)));
                                    index++;
                                    
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - space - core, pnt[1].z), Vector3.forward, new Vector2(.0f, b[0].y)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.forward, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                if (c[2]) {
                                    // Second Under Wall
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - space - core, pnt[1].z), Vector3.right, new Vector2(1.0f, 1.0f - b[3].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - space - core, pnt[2].z), Vector3.right, new Vector2(.0f, 1.0f - b[1].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, 1.0f)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - space - core, pnt[2].z), Vector3.right, new Vector2(.0f, 1.0f - b[1].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.right, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                if (c[3]) {
                                    // Third Under Wall
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - space - core, pnt[2].z), Vector3.back, new Vector2(1.0f, 1.0f - b[2].y)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - space - core, pnt[3].z), Vector3.back, new Vector2(.0f, 1.0f - b[3].y)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, 1.0f)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - space - core, pnt[3].z), Vector3.back, new Vector2(.0f, 1.0f - b[3].y)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.back, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                if (c[0]) {
                                    // Fourth Under Wall
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - space - core, pnt[3].z), Vector3.left, new Vector2(1.0f, b[2].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - space - core, pnt[0].z), Vector3.left, new Vector2(.0f, b[0].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left, new Vector2(1.0f, 1.0f)));
                                    index++;

                                    m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - space - core, pnt[0].z), Vector3.left, new Vector2(.0f, b[0].x)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.left, new Vector2(.0f, 1.0f)));
                                    index++;
                                    m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left,new Vector2(1.0f, 1.0f)));
                                    index++;
                                }

                                for (int ps = 0; ps < 4; ps++) {
                                    pnt[ps].y -= space;
                                    ind[ps].y -= space;
                                }

                                /*c[0] = p[3] < -256 || !nextLayer[p[3]] || (nextHM[p[3]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[3]].y - nextHM[p[3]].d * stackFactor);
                                c[1] = p[1] < -256 || !nextLayer[p[1]] || (nextHM[p[1]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[1]].y - nextHM[p[1]].d * stackFactor);
                                c[2] = p[5] < -256 || !nextLayer[p[5]] || (nextHM[p[5]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[5]].y - nextHM[p[5]].d * stackFactor);
                                c[3] = p[7] < -256 || !nextLayer[p[7]] || (nextHM[p[7]].y < pnt[0].y - cellHeight || pnt[0].y < nextHM[p[7]].y - nextHM[p[7]].d * stackFactor);
                                */
                                /*index = WallT1(index, 0, 1, cornerNorm[2], .0f, 1.0f);
                                index = WallT2(index, 1, 2, 3, cornerNorm[0], .0f, 1.0f);
                                index = WallT3(index, 2, 3, -cornerNorm[3], 1.0f, .0f);
                                index = WallT4(index, 3, 0, 2, -cornerNorm[1], 1.0f, .0f);*/

                                if (c[1]) {
                                    index = WallB1(index, 1, 0, cornerNorm[3]);
                                }

                                if (c[2]) {
                                    index = WallB2(index, 2, 1, 3, cornerNorm[1], space, cutV);
                                }

                                if (c[3]) {
                                    index = WallB3(index, 3, 2, -cornerNorm[2], cutU, space);
                                }

                                if (c[0]) {
                                    index = WallB4(index, 0, 3, 2, -cornerNorm[0], cutV, space);
                                }

                                // Under Platform
                                m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, height - cellHeight, ind[1].z), new Vector3(.0f, 1.0f, .0f), b[1]));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x, height - cellHeight, ind[0].z), new Vector3(.0f, 1.0f, .0f), b[0]));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, height - cellHeight, ind[2].z), new Vector3(.0f, 1.0f, .0f), b[2]));
                                index++;

                                m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, height - cellHeight, ind[1].z), new Vector3(.0f, 1.0f, .0f), b[1]));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, height - cellHeight, ind[2].z), new Vector3(.0f, 1.0f, .0f), b[2]));
                                index++;
                                m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x, height - cellHeight, ind[3].z), new Vector3(.0f, 1.0f, .0f), b[3]));
                                index++;
                            }
                        }
                    } else { 
                        if (c[0]) {
					        built = true;
					        b[0].x += space;
					        b[2].x += space;
					        bod[0].x += space;
					        bod[2].x += space;
				
					        if (c[1]) {
						        b[0].y += space;
						        b[1].y += space;
						        bod[0].z -= space;
						        bod[1].z -= space;

						        if (c[2]) {
							        b[1].x -= space;
							        b[3].x -= space;
							        bod[1].x -= space;
							        bod[3].x -= space;
                                    if (c[3]) {
                                        b[2].y -= space;
                                        b[3].y -= space;
                                        bod[2].z += space;
                                        bod[3].z += space;
                                    }
						        } else {
							        if (c[3]) {//1,3,4
								        b[2].y -= space;
								        b[3].y -= space;
								        bod[2].z += space;
								        bod[3].z += space;
							        }
						        }
					        } else if (c[3]) {
						        b[2].y -= space;
						        b[3].y -= space;
						        bod[2].z += space;
						        bod[3].z += space;

						        if (c[2]) {//2,3,4
							        b[1].x -= space;
							        b[3].x -= space;
							        bod[1].x -= space;
							        bod[3].x -= space;
						        }
					        } else {
						        if (c[2]) {//2,4
							        b[1].x -= space;
							        b[3].x -= space;
							        bod[1].x -= space;
							        bod[3].x -= space;
						        }
					        }
				        } else if (c[2]) {
					        built = true;
					        b[1].x -= space;
					        b[3].x -= space;
					        bod[1].x -= space;
					        bod[3].x -= space;
					        if (c[1]) {
						        b[0].y += space;
						        b[1].y += space;
						        bod[0].z -= space;
						        bod[1].z -= space;

						        if (c[3]) {//1,2,3
							        b[2].y -= space;
							        b[3].y -= space;
							        bod[2].z += space;
							        bod[3].z += space;
						        }
					        } else if (c[3]) {//2,3
						        b[2].y -= space;
						        b[3].y -= space;
						        bod[2].z += space;
						        bod[3].z += space;
					        }
				        } else if (c[1]) {// not inset if any cases
					        built = true;
					        b[0].y += space;
					        b[1].y += space;
					        bod[0].z -= space;
					        bod[1].z -= space;

					        if (c[3]) {//1,3
						        b[2].y -= space;
						        b[3].y -= space;
						        bod[2].z += space;
						        bod[3].z += space;
					        }
				        } else if (c[3]) {//3
					        built = true;
					        b[2].y -= space;
					        b[3].y -= space;
					        bod[2].z += space;
					        bod[3].z += space;
				        }

                        // First Wall
                        index = WallT1(index, 0, 1, cornerNorm[2]);
                        index = WallB1(index, 1, 0, cornerNorm[3]);

                        // Second Wall
                        index = WallT2(index, 1, 2, 3, cornerNorm[0], space, cutV);
                        index = WallB2(index, 2, 1, 3, cornerNorm[1], space, cutV);

                        // Third Wall
                        index = WallT3(index, 2, 3, -cornerNorm[3], cutU, space);
                        index = WallB3(index, 3, 2, -cornerNorm[2], cutU, space);

                        // Fourth Wall
                        index = WallT4(index, 3, 0, 2, -cornerNorm[1], cutV, space);
                        index = WallB4(index, 0, 3, 2, -cornerNorm[0], cutV, space);
                    }
                    
                    


				    if (built) {
					    // Under Platform
					    /*m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, height - core - space, ind[1].z), new Vector3(.0f, 1.0f, .0f), new Vector2(t[1].x, -t[1].y)));
					    index++;
					    m_voxModel.Add(m_hm.Model(index, new Vector3(ind[0].x, height - core - space, ind[0].z), new Vector3(.0f, 1.0f, .0f), new Vector2(t[0].x, -t[0].y)));
					    index++;
					    m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, height - core - space, ind[2].z), new Vector3(.0f, 1.0f, .0f), new Vector2(t[2].x, -t[2].y)));
					    index++;

					    m_voxModel.Add(m_hm.Model(index, new Vector3(ind[1].x, height - core - space, ind[1].z), new Vector3(.0f, 1.0f, .0f), new Vector2(t[1].x, -t[1].y)));
					    index++;
					    m_voxModel.Add(m_hm.Model(index, new Vector3(ind[2].x, height - core - space, ind[2].z), new Vector3(.0f, 1.0f, .0f), new Vector2(t[2].x, -t[2].y)));
					    index++;
					    m_voxModel.Add(m_hm.Model(index, new Vector3(ind[3].x, height - core - space, ind[3].z), new Vector3(.0f, 1.0f, .0f), new Vector2(t[3].x, -t[3].y)));
					    index++;*/

                        /*Vector2[] corner = new Vector2[4];
                        corner[0] = new Vector2(.0f, subWallB);
                        corner[1] = new Vector2(1.0f, subWallB);
                        corner[2] = new Vector2(.0f, subWallT);
                        corner[3] = new Vector2(1.0f, subWallT);*/
                        //float topEdge = 1.0f - sqFactor, botEdge = sqFactor;




                        /*for (int q = 0; q < 4; q++) {
                            if (t[q].x > 0) {
                                t[q].x = topEdge;
                            }
                        }*/
                        
                        // First Under Wall
                        /*m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core, pnt[0].z), Vector3.forward, new Vector2(1.0f, space)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - core, pnt[1].z), Vector3.forward, new Vector2(.0f, space)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f - t[0].x)));
                        index++;

                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - core, pnt[1].z), Vector3.forward, new Vector2(.0f, space)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.forward, new Vector2(.0f, 1.0f - t[2].x)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.forward, new Vector2(1.0f, 1.0f - t[0].x)));
                        index++;

                        // Second Under Wall
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[1].x, pnt[1].y - core, pnt[1].z), Vector3.right, new Vector2(1.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - core, pnt[2].z), Vector3.right, new Vector2(.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, 1.0f - t[2].x)));
                        index++;

                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - core, pnt[2].z), Vector3.right, new Vector2(.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.right, new Vector2(.0f, t[3].x)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[1], Vector3.right, new Vector2(1.0f, 1.0f - t[2].x)));
                        index++;

                        // Third Under Wall
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[2].x, pnt[2].y - core, pnt[2].z), Vector3.back, new Vector2(1.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core, pnt[3].z), Vector3.back, new Vector2(.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, t[3].x)));
                        index++;

                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core, pnt[3].z), Vector3.back, new Vector2(.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.back, new Vector2(.0f, t[1].x)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[2], Vector3.back, new Vector2(1.0f, t[3].x)));
                        index++;

                        // Fourth Under Wall
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[3].x, pnt[3].y - core, pnt[3].z), Vector3.left, new Vector2(1.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left, new Vector2(1.0f, 1.0f - t[2].x)));
                        index++;

                        m_voxModel.Add(m_hm.Model(index, new Vector3(pnt[0].x, pnt[0].y - core, pnt[0].z), Vector3.left, new Vector2(.0f, .0f)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[0], Vector3.left, new Vector2(.0f, 1.0f - t[0].x)));
                        index++;
                        m_voxModel.Add(m_hm.Model(index, pnt[3], Vector3.left,new Vector2(1.0f, 1.0f - t[2].x)));
                        index++;*/
                    }
                }
		    }
	    }
        return true;
    }
}