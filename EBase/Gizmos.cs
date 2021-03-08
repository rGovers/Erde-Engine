using OpenTK;
using System;
using System.Collections.Generic;

namespace Erde
{
    public struct GizmoVertex
    {
        public Vector4 Position;
        public Vector4 Color;
    }

    // TODO: Make Thread Aware
    // Quick and dirty for now
    public static class Gizmos
    {
        static List<GizmoVertex> NextVertices;
        static List<GizmoVertex> Vertices;

        public static void DrawLine(Vector3 a_start, Vector3 a_end, float a_width, Vector4 a_color)
        {
            if (NextVertices == null)
            {
                NextVertices = new List<GizmoVertex>();
            }

            Vector3 forward = a_end - a_start;
            float dist = forward.Length;
            forward /= dist;

            Vector3 up = Vector3.UnitY;

            if (Math.Abs(Vector3.Dot(up, forward)) >= 0.95)
            {
                up = Vector3.UnitZ;
            }

            Vector3 right = Vector3.Cross(up, forward);

            GizmoVertex startRight = new GizmoVertex()
            {
                Position = new Vector4(a_start + right * a_width, 1.0f),
                Color = a_color
            };
            GizmoVertex startLeft = new GizmoVertex() 
            { 
                Position = new Vector4(a_start - right * a_width, 1.0f),
                Color = a_color
            };
            GizmoVertex endRight = new GizmoVertex() 
            { 
                Position = new Vector4(a_end + right * a_width, 1.0f),
                Color = a_color
            };
            GizmoVertex endLeft = new GizmoVertex()
            {
                Position = new Vector4(a_end - right * a_width, 1.0f),
                Color = a_color
            };

            NextVertices.Add(startRight);
            NextVertices.Add(endRight);
            NextVertices.Add(startLeft);

            NextVertices.Add(startLeft);
            NextVertices.Add(endRight);
            NextVertices.Add(endLeft);
        }

        public static void Clear()
        {
            Vertices = NextVertices;

            NextVertices = new List<GizmoVertex>();
        }

        public static GizmoVertex[] GetVertices()
        {
            if (Vertices != null)
            {
                return Vertices.ToArray(); 
            }

            return null;
        }
    }
}