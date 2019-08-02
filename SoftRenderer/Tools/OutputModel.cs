// jave.lin 2019.08.02
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class OutputModel : MonoBehaviour
{
    [MenuItem("Tools/ExportMesh")]
    public static void ExportMesh()
    {
        var go = Selection.activeGameObject;

        var mf = go.GetComponent<MeshFilter>();
        if (mf == null)
        {
            Debug.Log($"mesh filter is null.");
            return;
        }
        if (mf != null)
        {
            var m = mf.sharedMesh;
            if (!m.isReadable)
            {
                Debug.LogError($"only Exporting which the 'Mesh.isReable' is true .");
                return;
            }

            if (m.vertices.Length == 0)
            {
                Debug.Log($"mesh vertices is empyt.");
                return;
            }

            const string vertices = "vertices";
            const string indices = "indices";
            const string colors = "colors";
            //const string uv_prefix = "uv"; // uv_n
            const string normals = "normals";
            const string tangents = "tangents";

            const string label = "#";

            var sb = new StringBuilder();

            // vertices
            var len = m.vertices.Length;
            sb.AppendLine($"{label}{vertices}:{len}");
            for (int i = 0; i < len; i++)
            {
                var v = m.vertices[i];
                sb.Append($"{v.x},{v.y},{v.z}\n");
            }
            // indices
            len = m.triangles.Length;
            sb.AppendLine($"{label}{indices}:{len}");
            for (int i = 0; i < len; i+=3)
            {
                var v1 = m.triangles[i];
                var v2 = m.triangles[i + 1];
                var v3 = m.triangles[i +2];
                sb.Append($"{v1},{v2},{v3}\n");
            }
            // colors
            len = m.colors.Length;
            sb.AppendLine($"{label}{colors}:{len}");
            for (int i = 0; i < len; i ++)
            {
                var v = m.colors[i];
                sb.Append($"{v.r},{v.g},{v.b},{v.a}\n");
            }
            // uv
            //var uv_name = new string[] { "uv", "uv2", "uv3", "uv4", "uv5", "uv6", "uv7", "uv8" };
            var uv_name = new string[] { "uv" }; // 目前支持一种uv
            for (int uv_i = 0; uv_i < uv_name.Length; uv_i++)
            {
                var p = m.GetType().GetProperty(uv_name[uv_i]);
                if (p != null)
                {
                    var uvs = (Vector2[])p.GetValue(m);
                    len = uvs.Length;
                    sb.AppendLine($"{label}{uv_name[uv_i]}:{len}");
                    for (int i = 0; i < len; i++)
                    {
                        var v = uvs[i];
                        sb.Append($"{v.x},{v.y}\n");
                    }
                }
            }
            // normals
            len = m.normals.Length;
            sb.AppendLine($"{label}{normals}:{len}");
            for (int i = 0; i < len; i++)
            {
                var v = m.normals[i];
                sb.Append($"{v.x},{v.y},{v.z}\n");
            }
            // tangents
            len = m.tangents.Length;
            sb.AppendLine($"{label}{tangents}:{len}");
            for (int i = 0; i < len; i++)
            {
                var v = m.tangents[i];
                sb.Append($"{v.x},{v.y},{v.z},{v.w}\n");
            }

            try
            {
                if (!Directory.Exists("Models")) Directory.CreateDirectory("Models");
                var filename = $"Models/{go.name}_{DateTime.Now.Ticks}.m";
                File.WriteAllText(filename, sb.ToString(), Encoding.UTF8);
                Debug.Log($"output model file:{filename} complete!");
            }
            catch (Exception er)
            {
                Debug.LogError($"output model file failure, er:{er}");
            }
            sb.Clear();
        }
    }
}
