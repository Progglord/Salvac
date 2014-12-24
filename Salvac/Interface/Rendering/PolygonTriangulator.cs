// Salvac
// Copyright (C) 2014 Oliver Schmidt
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using DotSpatial.Topology;
using OpenTK;
using System.Threading.Tasks;

namespace Salvac.Interface.Rendering
{
    public sealed class PolygonTriangulator
    {
        public sealed class TriangulationResult
        {
            public Vector2[] Vertices
            { get; private set; }

            public int[] FillIndices
            { get; private set; }

            public int[] LineIndices
            { get; private set; }

            public TriangulationResult(Vector2[] vertices, int[] fillIndices, int[] lineIndices)
            {
                if (vertices == null) throw new ArgumentNullException("vertices");
                if (fillIndices == null) throw new ArgumentNullException("fillIndices");
                if (lineIndices == null) throw new ArgumentNullException("lineIndices");

                this.Vertices = vertices;
                this.FillIndices = fillIndices;
                this.LineIndices = lineIndices;
            }
        }

        private sealed class Vertex
        {
            public int Index;
            public bool IsActive, IsConvex, IsEar;
            public float Angle;
            public Vector2 Point;
            public Vertex Previous, Next;
        }

        #region Polygon Calculation Tools

        private static bool IsConvex(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float val = (p3.Y - p1.Y) * (p2.X - p1.X) - (p3.X - p1.X) * (p2.Y - p1.Y);
            return val > 0;
        }

        private static bool IsInside(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            if (IsConvex(p1, p, p2)) return false;
            if (IsConvex(p2, p, p3)) return false;
            if (IsConvex(p3, p, p1)) return false;
            return true;
        }

        private static bool IsCounterClockwise(params Vector2[] vertices)
        {
            float area = 0;
            int i2 = 0;
            for (int i1 = 0; i1 < vertices.Length; i1++)
            {
                i2 = i1 + 1;
                if (i2 == vertices.Length) i2 = 0;
                area += vertices[i1].X * vertices[i2].Y - vertices[i1].Y * vertices[i2].X;
            }

            if (area == 0)
            {
                Debug.Write("While triangulating polygon: Some vertices are not measurable oriented. Assuming clockwise order.");
                return false;
            }
            else
                return area > 0;
        }

        private static bool IsInCone(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool convex = IsConvex(p1, p2, p3);

            if (convex)
            {
                if (!IsConvex(p1, p2, p)) return false;
                if (!IsConvex(p2, p3, p)) return false;
                return true;
            }
            else
            {
                if (IsConvex(p1, p2, p)) return true;
                if (IsConvex(p2, p3, p)) return true;
                return false;
            }

        }

        private static bool Intersects(Vector2 p11, Vector2 p12, Vector2 p21, Vector2 p22)
        {
            if ((p11.X == p21.X) && (p11.Y == p21.Y)) return false;
            if ((p11.X == p22.X) && (p11.Y == p22.Y)) return false;
            if ((p12.X == p21.X) && (p12.Y == p21.Y)) return false;
            if ((p12.X == p22.X) && (p12.Y == p22.Y)) return false;

            Vector2 v1ort = new Vector2(p12.Y - p11.Y, p11.X - p12.X);
            Vector2 v2ort = new Vector2(p22.Y - p21.Y, p21.X - p22.X);

            Vector2 v = p21 - p11;
            float dot21 = Vector2.Dot(v, v1ort);
            v = p22 - p11;
            float dot22 = Vector2.Dot(v, v1ort);

            v = p11 - p21;
            float dot11 = Vector2.Dot(v, v2ort);
            v = p12 - p21;
            float dot12 = Vector2.Dot(v, v2ort);

            if (dot11 * dot12 > 0) return false;
            if (dot21 * dot22 > 0) return false;

            return true;
        }

        #endregion

        public static TriangulationResult Triangulate(Polygon polygon)
        {
            // Load coordinates and put them into counter-clockwise order
            List<Vector2> vertices = new List<Vector2>(polygon.Shell.Coordinates.Select(c => new Vector2((float)c.X, (float)c.Y)));
            vertices.RemoveAt(0);
            if (!IsCounterClockwise(vertices.ToArray()))
                vertices.Reverse();

            // Generate outline indices
            List<int> lineIndices = new List<int>(vertices.Count * 2);
            for (int i = 0; i < vertices.Count; i++)
            {
                lineIndices.Add(i);
                lineIndices.Add((i + 1) % vertices.Count);
            }

            // Triangulate
            List<int> fillIndices = PolygonTriangulator.TriangulateShell(vertices);
            return new TriangulationResult(vertices.ToArray(), fillIndices.ToArray(), lineIndices.ToArray());
        }

        public static async Task<TriangulationResult> TriangulateAsync(Polygon polygon)
        {
            return await Task.Run(() => PolygonTriangulator.Triangulate(polygon));
        }

        //private static IList<IList<Vector2>> RemoveHoles(IList<Vector2> shell, IList<IList<Vector2>> holes)
        //{
        //    if (holes.Count == 0)
        //        return (new IList<Vector2>[] { shell }).ToList();

        //    while (holes.Count != 0)
        //    {
        //        IList<Vector2> hole = null;
        //        int holePointIndex = -1;
        //        foreach (IList<Vector2> iHole in holes)
        //        {
        //            if (hole == null)
        //            {
        //                hole = iHole;
        //                holePointIndex = -1;
        //            }

        //            for (int i = 0; i < iHole.Count; i++)
        //            {
        //                if (iHole[i].X > hole[holePointIndex].X)
        //                {
        //                    hole = iHole;
        //                    holePointIndex = i;
        //                }
        //            }
        //        }

        //        Vector2 holePoint = hole[holePointIndex];
        //        bool pointFound = false;
        //        Vector2 bestPolyPoint = default(Vector2);
        //        for (int i = 0; i < shell.Count; i++)
        //        {
        //            if (shell[i].X < holePoint.X) continue;
        //            if (!IsInCone(shell[(i + shell.Count - 1) % shell.Count], shell[i], shell[(i + 1) % shell.Count], holePoint))
        //                continue;

        //            Vector2 polyPoint = shell[i];
        //            if (pointFound)
        //            {
        //                Vector2 v1 = polyPoint - holePoint, v2 = bestPolyPoint - holePoint;
        //                v1.Normalize();
        //                v2.Normalize();
        //                if (v2.X > v1.X) continue;
        //            }

        //            bool pointVisible = true;
        //            for (int j = 0; j < shell.Count; j++)
        //            {
        //                Vector2 p1 = shell[j];
        //                Vector2 p2 = shell[(j + 1) % shell.Count];
        //                if (Intersects(holePoint, polyPoint, p1, p2))
        //                {
        //                    pointVisible = false;
        //                    break;
        //                }
        //            }

        //            if (pointVisible)
        //            {
        //                pointFound = true;
        //                bestPolyPoint = polyPoint;
        //            }
        //        }
        //    }
        //}

        private static List<int> TriangulateShell(List<Vector2> coordinates)
        {
            if (coordinates.Count < 3) throw new ArgumentException("There are not enough coordinates to make a polygon.", "coordinates");
            else if (coordinates.Count == 3)
            {
                if (IsCounterClockwise(coordinates.ToArray()))
                    return new List<int>((coordinates as IEnumerable<int>).Reverse().Select((v, i) => i));
                else
                    return coordinates.Select((v, i) => i).ToList();
            }

            // Load vertices
            List<Vertex> vertices = new List<Vertex>(coordinates.Count);
            for (int i = 0; i < coordinates.Count; i++)
            {
                vertices.Add(new Vertex()
                {
                    IsActive = true,
                    Index = i,
                    Point = coordinates[i],
                    Previous = ((i != 0) ? vertices[i - 1] : null),
                    Next = null
                });

                if (i != 0)
                    vertices[i - 1].Next = vertices[i];
            }
            vertices[vertices.Count - 1].Next = vertices[0];
            vertices[0].Previous = vertices[vertices.Count - 1];

            foreach (Vertex vertex in vertices) UpdateVertex(vertex, vertices);

            // Search for ears
            List<int> triangles = new List<int>();
            for (int i = 0; i < vertices.Count - 3; i++)
            {
                Vertex ear = null;
                for (int j = 0; j < vertices.Count; j++)
                {
                    if (!vertices[j].IsActive) continue;
                    if (!vertices[j].IsEar) continue;
                    if (ear == null)
                        ear = vertices[j];
                    else
                    {
                        if (vertices[j].Angle > ear.Angle)
                            ear = vertices[j];
                    }
                }
                if (ear == null)
                    throw new ArgumentException("Could not find any ear.", "coordinates");

                // Add triangle in clockwise order
                if (IsCounterClockwise(ear.Previous.Point, ear.Point, ear.Next.Point))
                {
                    triangles.Add(ear.Next.Index);
                    triangles.Add(ear.Index);
                    triangles.Add(ear.Previous.Index);
                }
                else
                {
                    triangles.Add(ear.Previous.Index);
                    triangles.Add(ear.Index);
                    triangles.Add(ear.Next.Index);
                }

                ear.IsActive = false;
                ear.Previous.Next = ear.Next;
                ear.Next.Previous = ear.Previous;

                if (i == vertices.Count - 4) break;

                UpdateVertex(ear.Previous, vertices);
                UpdateVertex(ear.Next, vertices);
            }

            // Add left triangles
            foreach (Vertex vertex in vertices)
            {
                if (vertex.IsActive)
                {
                    // Add triangle in clockwise order
                    if (IsCounterClockwise(vertex.Previous.Point, vertex.Point, vertex.Next.Point))
                    {
                        triangles.Add(vertex.Next.Index);
                        triangles.Add(vertex.Index);
                        triangles.Add(vertex.Previous.Index);
                    }
                    else
                    {
                        triangles.Add(vertex.Previous.Index);
                        triangles.Add(vertex.Index);
                        triangles.Add(vertex.Next.Index);
                    }
                    break;
                }
            }

            return triangles;
        }

        private static void UpdateVertex(Vertex vertex, IList<Vertex> vertices)
        {
            vertex.IsConvex = IsConvex(vertex.Previous.Point, vertex.Point, vertex.Next.Point);

            Vector2 v1 = vertex.Previous.Point - vertex.Point;
            Vector2 v2 = vertex.Next.Point - vertex.Point;
            v1.Normalize();
            v2.Normalize();
            vertex.Angle = Vector2.Dot(v1, v2);

            if (vertex.IsConvex)
            {
                vertex.IsEar = true;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (vertices[i].Point == vertex.Previous.Point || vertices[i].Point == vertex.Point || vertices[i].Point == vertex.Next.Point)
                        continue;
                    else if (IsInside(vertex.Previous.Point, vertex.Point, vertex.Next.Point, vertices[i].Point))
                    {
                        vertex.IsEar = false;
                        break;
                    }
                }
            }
            else
                vertex.IsEar = false;
        }
    }
}
