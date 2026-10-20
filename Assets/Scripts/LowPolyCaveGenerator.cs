using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a small low-poly oval cave (no ceiling) for a top-down / 2.5D game.
/// - Rocky walls around an oval floor, opening on the right (+X)
/// - One blue point light lights most of the room, the rest falls into darkness
/// - Flat-shaded, no textures needed
///
/// Usage: add to an empty GameObject at (0,0,0), then right-click the component
/// header (or the "..." menu) -> "Generate Cave". Also auto-generates on Play if empty.
/// </summary>
public class LowPolyCaveGenerator : MonoBehaviour
{
    [Header("Shape")]
    public float radiusX = 9f;
    public float radiusZ = 6f;
    [Range(16, 64)] public int segments = 32;
    public float wallThickness = 1.6f;
    public float wallHeight = 2.5f;
    [Range(0f, 1f)] public float roughness = 0.5f;
    public int seed = 7;

    [Header("Opening (right side, +X)")]
    [Range(20f, 90f)] public float openingWidthDegrees = 45f;
    public float pathLength = 2.5f;

    [Header("Rocks")]
    public int wallBoulders = 10;
    public int coverRocks = 3;

    [Header("Lighting")]
    public Color lightColor = new Color(0.25f, 0.55f, 1f);
    public float lightIntensity = 3f;
    [Tooltip("Sets near-black ambient light and a black camera background")]
    public bool configureScene = true;
    [Tooltip("Moves Camera.main to a tilted top-down view of the cave")]
    public bool positionCamera = true;

    [Header("Options")]
    public bool addColliders = true;
    public bool generateOnStart = true;
    [Tooltip("Optional. Leave empty to auto-create simple materials.")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material rockMaterial;

    const string RootName = "CaveGenerated";

    void Start()
    {
        if (generateOnStart && transform.Find(RootName) == null) Generate();
    }

    [ContextMenu("Generate Cave")]
    public void Generate()
    {
        var old = transform.Find(RootName);
        if (old != null)
        {
            old.name = "_old";
            if (Application.isPlaying) Destroy(old.gameObject); else DestroyImmediate(old.gameObject);
        }

        var root = new GameObject(RootName).transform;
        root.SetParent(transform, false);

        var floorMat = floorMaterial != null ? floorMaterial : MakeMat("CaveFloor", new Color(0.50f, 0.52f, 0.56f));
        var wallMat = wallMaterial != null ? wallMaterial : MakeMat("CaveWall", new Color(0.55f, 0.57f, 0.62f));
        var rockMat = rockMaterial != null ? rockMaterial : MakeMat("CaveRock", new Color(0.45f, 0.47f, 0.52f));

        int N = segments;
        float step = Mathf.PI * 2f / N;
        float half = openingWidthDegrees * 0.5f * Mathf.Deg2Rad;
        var rnd = new System.Random(seed);
        float R() => (float)(rnd.NextDouble() * 2.0 - 1.0);   // -1..1
        float R01() => (float)rnd.NextDouble();                // 0..1
        float Wrap(float a) { a %= Mathf.PI * 2f; if (a > Mathf.PI) a -= Mathf.PI * 2f; if (a < -Mathf.PI) a += Mathf.PI * 2f; return a; }
        bool SkipSeg(int i) => Mathf.Abs(Wrap((i + 0.5f) * step)) < half;

        // ---------- ring data (shared by neighbouring segments so there are no cracks) ----------
        var inner = new Vector3[N];
        var outer = new Vector3[N];
        var normal = new Vector3[N];
        var h = new float[N];
        var bump = new float[N];
        float so = Mathf.Abs(seed) * 0.37f;

        for (int i = 0; i < N; i++)
        {
            float a = i * step;
            var p = new Vector3(Mathf.Cos(a) * radiusX, 0f, Mathf.Sin(a) * radiusZ);
            var n = new Vector3(Mathf.Cos(a) / radiusX, 0f, Mathf.Sin(a) / radiusZ).normalized;
            normal[i] = n;
            inner[i] = p + n * (R() * roughness * 0.9f);
            outer[i] = inner[i] + n * (wallThickness * (1f + R() * 0.3f * roughness));
            float noise = Mathf.PerlinNoise(Mathf.Cos(a) * 1.6f + so + 10f, Mathf.Sin(a) * 1.6f + 10f);
            h[i] = wallHeight * (0.7f + 0.6f * noise) + R() * 0.3f * roughness;
            bump[i] = 0.2f + R01() * 0.5f;
        }

        // ---------- walls ----------
        var wall = new MeshBuilder();

        void Cap(int k, float dir)
        {
            float a = k * step;
            var tangent = new Vector3(-Mathf.Sin(a) * radiusX, 0f, Mathf.Cos(a) * radiusZ).normalized * dir;
            var ib = inner[k];
            var it = inner[k] + Vector3.up * h[k];
            var pk = (inner[k] + outer[k]) * 0.5f + Vector3.up * (h[k] + bump[k]);
            var ot = outer[k] + Vector3.up * h[k] * 0.55f;
            var ob = outer[k];
            wall.Tri(ib, it, pk, tangent);
            wall.Tri(ib, pk, ot, tangent);
            wall.Tri(ib, ot, ob, tangent);
        }

        for (int i = 0; i < N; i++)
        {
            if (SkipSeg(i)) continue;
            int j = (i + 1) % N;

            Vector3 ibI = inner[i], ibJ = inner[j];
            Vector3 itI = inner[i] + Vector3.up * h[i], itJ = inner[j] + Vector3.up * h[j];
            Vector3 pkI = (inner[i] + outer[i]) * 0.5f + Vector3.up * (h[i] + bump[i]);
            Vector3 pkJ = (inner[j] + outer[j]) * 0.5f + Vector3.up * (h[j] + bump[j]);
            Vector3 otI = outer[i] + Vector3.up * h[i] * 0.55f, otJ = outer[j] + Vector3.up * h[j] * 0.55f;
            Vector3 obI = outer[i], obJ = outer[j];
            Vector3 nMid = normal[i] + normal[j];

            wall.Quad(ibI, ibJ, itJ, itI, -nMid);        // inner face
            wall.Quad(itI, itJ, pkJ, pkI, Vector3.up);   // top, inner slope
            wall.Quad(pkI, pkJ, otJ, otI, Vector3.up);   // top, outer slope
            wall.Quad(otI, otJ, obJ, obI, nMid);         // outer face

            if (SkipSeg((i - 1 + N) % N)) Cap(i, -1f);   // close the end next to the opening
            if (SkipSeg(j)) Cap(j, 1f);
        }

        MakePart("Walls", root, wall.ToMesh(), wallMat, addColliders, false);

        // ---------- floor ----------
        var r1 = new Vector3[N];
        var r2 = new Vector3[N];
        var r3 = new Vector3[N];
        for (int i = 0; i < N; i++)
        {
            r1[i] = inner[i] * 0.35f + new Vector3(R() * 0.3f, R() * 0.06f, R() * 0.3f);
            r2[i] = inner[i] * 0.70f + new Vector3(R() * 0.3f, R() * 0.06f, R() * 0.3f);
            float w = Mathf.Clamp01(1f - Mathf.Abs(Wrap(i * step)) / half);   // 1 in the middle of the opening
            r3[i] = inner[i] + normal[i] * (wallThickness * 0.5f * (1f - w) + pathLength * w);
        }

        var floor = new MeshBuilder();
        for (int i = 0; i < N; i++)
        {
            int j = (i + 1) % N;
            floor.Tri(Vector3.zero, r1[i], r1[j], Vector3.up);
            floor.Quad(r1[i], r1[j], r2[j], r2[i], Vector3.up);
            floor.Quad(r2[i], r2[j], r3[j], r3[i], Vector3.up);
        }
        MakePart("Floor", root, floor.ToMesh(), floorMat, addColliders, false);

        // ---------- rocks ----------
        var rocks = new GameObject("Rocks").transform;
        rocks.SetParent(root, false);

        for (int placed = 0, tries = 0; placed < wallBoulders && tries < 200; tries++)
        {
            int i = rnd.Next(N);
            if (Mathf.Abs(Wrap(i * step)) < half + 0.35f) continue;
            MakeRock(rocks, inner[i] - normal[i] * (0.3f + R01() * 0.9f), 0.6f + R01() * 0.7f, rnd, rockMat);
            placed++;
        }

        for (int placed = 0, tries = 0; placed < coverRocks && tries < 100; tries++)
        {
            float a = R01() * Mathf.PI * 2f;
            if (Mathf.Abs(Wrap(a)) < half + 0.6f) continue;
            float f = 0.35f + R01() * 0.35f;
            MakeRock(rocks, new Vector3(Mathf.Cos(a) * radiusX, 0f, Mathf.Sin(a) * radiusZ) * f, 0.5f + R01() * 0.4f, rnd, rockMat);
            placed++;
        }

        int g = Mathf.Max(1, Mathf.RoundToInt(half / step));
        MakeRock(rocks, outer[g] + normal[g] * 0.8f, 1.1f, rnd, rockMat);
        MakeRock(rocks, outer[N - g] + normal[N - g] * 0.8f, 0.9f, rnd, rockMat);

        // ---------- light ----------
        var lg = new GameObject("Cave Light");
        lg.transform.SetParent(root, false);
        lg.transform.localPosition = new Vector3(-radiusX * 0.15f, wallHeight * 0.9f, 0f); // slightly left, so the opening fades to dark
        var l = lg.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = lightColor;
        l.intensity = lightIntensity;
        l.range = radiusX * 1.5f;
        l.shadows = LightShadows.Soft;

        // ---------- scene + camera ----------
        if (configureScene)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.015f, 0.02f, 0.035f);
        }

        var cam = Camera.main;
        if (cam != null)
        {
            if (configureScene)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
            }
            if (positionCamera)
            {
                Vector3 c = transform.position;
                cam.transform.position = c + new Vector3(0f, radiusX * 1.6f, -radiusX * 0.55f);
                cam.transform.LookAt(c);
                cam.fieldOfView = 50f;
            }
        }
    }

    // ---------------------------------------------------------------------------------------

    static readonly Vector3 T = new Vector3(0, 0, 0);
    static readonly float Phi = (1f + Mathf.Sqrt(5f)) / 2f;
    static readonly Vector3[] IcoVerts =
    {
        new Vector3(-1, Phi, 0), new Vector3(1, Phi, 0), new Vector3(-1, -Phi, 0), new Vector3(1, -Phi, 0),
        new Vector3(0, -1, Phi), new Vector3(0, 1, Phi), new Vector3(0, -1, -Phi), new Vector3(0, 1, -Phi),
        new Vector3(Phi, 0, -1), new Vector3(Phi, 0, 1), new Vector3(-Phi, 0, -1), new Vector3(-Phi, 0, 1)
    };
    static readonly int[] IcoFaces =
    {
        0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
        1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
        3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
        4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
    };

    void MakeRock(Transform parent, Vector3 localPos, float size, System.Random rnd, Material mat)
    {
        var v = new Vector3[IcoVerts.Length];
        for (int k = 0; k < v.Length; k++)
            v[k] = IcoVerts[k].normalized * (0.75f + (float)rnd.NextDouble() * 0.45f);

        var mb = new MeshBuilder();
        for (int f = 0; f < IcoFaces.Length; f += 3)
        {
            Vector3 a = v[IcoFaces[f]], b = v[IcoFaces[f + 1]], c = v[IcoFaces[f + 2]];
            mb.Tri(a, b, c, a + b + c);
        }

        var go = MakePart("Rock", parent, mb.ToMesh(), mat, addColliders, true);
        go.transform.localPosition = new Vector3(localPos.x, size * 0.25f, localPos.z);
        go.transform.localRotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
        go.transform.localScale = new Vector3(size * (0.9f + (float)rnd.NextDouble() * 0.4f), size * 0.65f, size * (0.9f + (float)rnd.NextDouble() * 0.4f));
    }

    GameObject MakePart(string name, Transform parent, Mesh mesh, Material mat, bool collider, bool convex)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        if (collider)
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
            mc.convex = convex;
        }
        return go;
    }

    static Material MakeMat(string name, Color c)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s) { name = name };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
        return m;
    }

    /// <summary>Builds flat-shaded meshes (every triangle gets its own vertices).</summary>
    class MeshBuilder
    {
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<int> t = new List<int>();

        // 'facing' = rough direction the triangle should face; winding is fixed automatically.
        public void Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 facing)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), facing) < 0f) { var tmp = b; b = c; c = tmp; }
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 facing)
        {
            Tri(a, b, c, facing);
            Tri(a, c, d, facing);
        }

        public Mesh ToMesh()
        {
            var m = new Mesh();
            if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(v);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }
}
