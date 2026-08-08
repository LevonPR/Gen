using UnityEngine;

namespace MicroEvolution.Visuals
{
    public static class MeshFactory
    {
        public static Mesh UnitSphere(int lon = 24, int lat = 16)
        {
            var mesh = new Mesh { name = "CellSphere" };
            var verts = new Vector3[(lon + 1) * (lat + 1)];
            var norms = new Vector3[verts.Length];
            var uvs = new Vector2[verts.Length];
            var tris = new int[lon * lat * 6];

            var vi = 0;
            for (var y = 0; y <= lat; y++)
            {
                var v = y / (float)lat;
                var pitch = (v - 0.5f) * Mathf.PI;
                var cy = Mathf.Sin(pitch);
                var cr = Mathf.Cos(pitch);
                for (var x = 0; x <= lon; x++)
                {
                    var u = x / (float)lon;
                    var yaw = u * Mathf.PI * 2f;
                    var p = new Vector3(Mathf.Cos(yaw) * cr, cy, Mathf.Sin(yaw) * cr);
                    // Flatten toward camera plane for 2.5D readability
                    p.z *= 0.35f;
                    verts[vi] = p * 0.5f;
                    norms[vi] = p.normalized;
                    uvs[vi] = new Vector2(u, v);
                    vi++;
                }
            }

            var ti = 0;
            for (var y = 0; y < lat; y++)
            for (var x = 0; x < lon; x++)
            {
                var i0 = y * (lon + 1) + x;
                var i1 = i0 + lon + 1;
                tris[ti++] = i0;
                tris[ti++] = i1;
                tris[ti++] = i0 + 1;
                tris[ti++] = i0 + 1;
                tris[ti++] = i1;
                tris[ti++] = i1 + 1;
            }

            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Capsule(int lon = 18, int lat = 12)
        {
            var mesh = UnitSphere(lon, lat);
            var v = mesh.vertices;
            for (var i = 0; i < v.Length; i++)
            {
                v[i].x *= 0.62f;
                v[i].y *= 1.25f;
            }

            mesh.vertices = v;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.name = "CellCapsule";
            return mesh;
        }
    }
}
