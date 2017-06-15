using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class CircleSectorControl : MonoBehaviour
{
    private class CircleSector
    {
        public float alphaMin;
        public float alphaMax;
        public float planePercents;
        public float radius;
        public Color color1;
        public Color color2;

        public override string ToString()
        {
            return string.Format("CircleSector (alphaMin: {0}, alphaMax: {1}, planePercents: {2}, radius: {3}, color1: {4}, color2: {5})",
                alphaMin, alphaMax, planePercents, radius, color1, color2);
        }
    }

    [System.Serializable]
    public class CircleSectorDescription
    {
        public float value = 1f;
        public float radius = 1f;
        public Color color1;
        public Color color2;
    }

    private Mesh mDynamicMesh = null;
    private Material mMaterialInstance = null;

    private int mOldCount = 0;
    private Vector3[] mVertices = null;
    private Color[] mColors1 = null;
    private Color[] mColors2 = null;
    private int[] mIndices = null;

    public Mesh DiagramMesh { get { return mDynamicMesh; } }

    private const float ANGLE_DELTA_MAX = Mathf.PI / 120f;

    public void BuildCircleDiagram(CircleSectorDescription[] data)
    {
        var sectors = DivideCircle(data);

        //считаем сколько вершин в данный момент должно быть в диаграмме
        var vertexCount = 0;
        for (var i = 0; i < sectors.Count; ++i)
        {
            var sector = sectors[i];
            for (var angle = sector.alphaMin; angle < sector.alphaMax; angle += ANGLE_DELTA_MAX)
                vertexCount += 3;
        }

        if (mOldCount != vertexCount)
        {
            mVertices = new Vector3[vertexCount];
            mColors1 = new Color[vertexCount];
            mColors2 = new Color[vertexCount];
            mIndices = new int[vertexCount];
            mOldCount = vertexCount;
        }

        int vertexIndex = 0;
        for (int i = 0; i < sectors.Count; ++i)
        {
            var sector = sectors[i];
            for (var angle = sector.alphaMin; angle < sector.alphaMax; angle += ANGLE_DELTA_MAX)
            {
                var a = vertexIndex;
                var b = vertexIndex + 1;
                var c = vertexIndex + 2;

                mIndices[a] = a;
                mIndices[b] = b;
                mIndices[c] = c;

                mVertices[a] = Vector3.zero;
                var angle1 = Mathf.PI - angle;
                mVertices[b] = new Vector3(Mathf.Cos(angle1), 0f, Mathf.Sin(angle1)) * sector.radius;
                var angle2 = Mathf.PI - Mathf.Min(angle + ANGLE_DELTA_MAX, sector.alphaMax);
                mVertices[c] = new Vector3(Mathf.Cos(angle2), 0f, Mathf.Sin(angle2)) * sector.radius;

                mColors1[a] = sector.color1;
                mColors1[b] = sector.color1;
                mColors1[c] = sector.color1;

                mColors2[a] = sector.color2;
                mColors2[b] = sector.color2;
                mColors2[c] = sector.color2;

                vertexIndex += 3;
            }
        };

        mDynamicMesh.Clear();

        mDynamicMesh.vertices = mVertices;
        mDynamicMesh.triangles = mIndices;
        mDynamicMesh.colors = mColors1;

        var uv1 = new Vector2[vertexCount];
        var uv2 = new Vector2[vertexCount];

        for (int i = 0; i < mColors2.Length; ++i)
        {
            var color = mColors2[i];

            uv1[i] = new Vector2(color.r, color.g);
            uv2[i] = new Vector2(color.b, color.a);
        }

        mDynamicMesh.uv = uv1;
        mDynamicMesh.uv2 = uv2;

        mDynamicMesh.RecalculateBounds();
        mDynamicMesh.RecalculateNormals();
    }

    private List<CircleSector> DivideCircle(CircleSectorDescription[] data)
    {
        List<CircleSector> output = new List<CircleSector>();

        float value = 0f;
        for (int i = 0; i < data.Length; ++i)
        {
            value += data[i].value;
        }

        var previousAngle = Mathf.PI * 0.5f;
        for (int i = 0; i < data.Length; ++i)
        {
            var description = data[i];
            var sectorValue = description.value / value;
            var sectorAngle = previousAngle + Mathf.PI * 2f * sectorValue;
            output.Add(new CircleSector()
            {
                planePercents = sectorValue,
                alphaMin = previousAngle,
                alphaMax = sectorAngle,
                color1 = description.color1,
                color2 = description.color2,
                radius = description.radius
            });
            previousAngle = sectorAngle;
        }

        return output;
    }

    private void Awake()
    {
        mDynamicMesh = new Mesh() { name = "circleDiagram_proceduralMesh" };
        mDynamicMesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mDynamicMesh;

        var renderer = GetComponent<MeshRenderer>();
        if (renderer)
        {
            mMaterialInstance = renderer.material;
        }
    }

    private void OnDestroy()
    {
        DestroyImmediate(mDynamicMesh);
        if (mMaterialInstance)
        {
            DestroyImmediate(mMaterialInstance);
            mMaterialInstance = null;
        }
    }
}
