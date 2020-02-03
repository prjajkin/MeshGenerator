using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    public static  class MeshCutter
    {
        private static bool isRunning;

        private static List<int> leftTriangles;
        private static List<int> rightTriangles;
        private static List<int> leftCutPointsNumbers;
        private static List<int> rightCutPointsNumbers;

        public static void CutMesh(MeshFilter meshFilter, Plane cutPlane)
        {
            var mesh = meshFilter.mesh;
            if (isRunning)return;
            isRunning = true;

            var vertices = new List<Vector3>(mesh.vertexCount); 
            for (var i = 0; i < mesh.vertexCount; i++)
            {
                vertices.Add(meshFilter.transform.TransformPoint(mesh.vertices[i]));
            }

            var originalVerticesCount = vertices.Count;
          
            var triangles = mesh.triangles;
            leftTriangles = new List<int>();
            rightTriangles = new List<int>(); 
            leftCutPointsNumbers = new List<int>();
            rightCutPointsNumbers = new List<int>();

            var maxPoint = vertices.Max(el => el.x);
            var minPoint = vertices.Min(el => el.x);
            var checkPoint = cutPlane.ClosestPointOnPlane(vertices[0]).x;
            if (maxPoint <= checkPoint || minPoint >= checkPoint)
            {
                Debug.Log("Miss");
                isRunning = false;
                return;
            }

            for (var i = 0; i < triangles.Length; i+=3)
            {
                var vertexNumber1 = triangles[i];
                var vertexNumber2 = triangles[i + 1];
                var vertexNumber3 = triangles[i + 2];
                var point1 = vertices[vertexNumber1];
                var point2 = vertices[vertexNumber2];
                var point3 = vertices[vertexNumber3];
                var side1 = cutPlane.GetSide(point1);
                var side2 = cutPlane.GetSide(point2);
                var side3 = cutPlane.GetSide(point3);

                if (side1 && side2 && side3)
                {
                    rightTriangles.AddRange(new List<int>{vertexNumber1, vertexNumber2, vertexNumber3});
                }
                else if (!side1 && !side2 && !side3)
                {
                    leftTriangles.AddRange(new List<int> { vertexNumber1, vertexNumber2, vertexNumber3});
                }
                else//Треугольник режится плейном.
                {

                    if (side1 == side2)
                    {
                        CutTriangle(vertexNumber3, vertexNumber1, vertexNumber2, vertices, cutPlane);
                    }
                    else if(side1 == side3)
                    {
                        CutTriangle(vertexNumber2, vertexNumber3, vertexNumber1, vertices, cutPlane);
                    }
                    else
                    {
                        CutTriangle(vertexNumber1, vertexNumber2, vertexNumber3, vertices, cutPlane);
                    }
                }
            }

            //moving
            for (var i = 0; i < originalVerticesCount; i++)
            { 
                if (cutPlane.GetSide(vertices[i]))
                {
                    vertices[i] = MoveVertices(vertices[i], Vector3.right, MainManager.Settings.R);
                }
                else
                {
                    vertices[i] = MoveVertices(vertices[i], Vector3.left, MainManager.Settings.R);
                }
            }
            foreach (var number in rightCutPointsNumbers)
            {
                vertices[number] = MoveVertices(vertices[number], Vector3.right, MainManager.Settings.R);
            }
            foreach (var number in leftCutPointsNumbers)
            {
                vertices[number] = MoveVertices(vertices[number], Vector3.left, MainManager.Settings.R);
            }

            //Generate split triangles.
            leftTriangles.AddRange(UniversalMeshGenerator.Generate2D_ByPoints_ZPlane(leftCutPointsNumbers, vertices));
            rightTriangles.AddRange(UniversalMeshGenerator.Generate2D_ByPoints_ZPlane(rightCutPointsNumbers, vertices, true));

            //final
            mesh.Clear();
            var verticesToLocal = new List<Vector3>(vertices.Count);
            for (var i = 0; i < vertices.Count; i++)
            {
                verticesToLocal.Add(meshFilter.transform.InverseTransformPoint(vertices[i]));
            }
            mesh.SetVertices(verticesToLocal);
            leftTriangles.AddRange(rightTriangles);
            mesh.triangles = leftTriangles.ToArray();
            mesh.RecalculateNormals();

            isRunning = false;
        }

        private static void CutTriangle(int firstSideVertexNumber1, int secondSideVertexNumber2, int secondSideVertexNumber3, List<Vector3> vertices,  Plane cutPlane)
        {
            var point1 = vertices[firstSideVertexNumber1];
            var point2 = vertices[secondSideVertexNumber2];
            var point3 = vertices[secondSideVertexNumber3];

            var planeX = cutPlane.ClosestPointOnPlane(point1).x;
            var newPoint1 = GetCrossPointByPlane(point1, point2, planeX);
            var newPoint2 = GetCrossPointByPlane(point1, point3, planeX);
         
            if (point1.x < point2.x)
            {
                //first side left
                var number1 = GetOrAddPointNumber( newPoint1, leftCutPointsNumbers, vertices);
                var number2 = GetOrAddPointNumber( newPoint2, leftCutPointsNumbers, vertices); 
                leftTriangles.AddRange(new List<int> { firstSideVertexNumber1, number1, number2 });

                //second side right
                number1 = GetOrAddPointNumber( newPoint1, rightCutPointsNumbers, vertices);
                number2 = GetOrAddPointNumber( newPoint2, rightCutPointsNumbers, vertices);
                rightTriangles.AddRange(new List<int> { secondSideVertexNumber2, secondSideVertexNumber3, number2 });
                rightTriangles.AddRange(new List<int> { number2, number1, secondSideVertexNumber2 });
            }
            else
            {
                //first side right
                var number1 = GetOrAddPointNumber( newPoint1, rightCutPointsNumbers, vertices);
                var number2 = GetOrAddPointNumber( newPoint2, rightCutPointsNumbers, vertices);
                rightTriangles.AddRange(new List<int> { firstSideVertexNumber1, number1, number2 });

                //second side left
                number1 = GetOrAddPointNumber( newPoint1, leftCutPointsNumbers, vertices);
                number2 = GetOrAddPointNumber( newPoint2, leftCutPointsNumbers, vertices);
                leftTriangles.AddRange(new List<int> { secondSideVertexNumber2, secondSideVertexNumber3, number2 });
                leftTriangles.AddRange(new List<int> { number2, number1, secondSideVertexNumber2 });
            }
        }

        private static int GetOrAddPointNumber(/*int addedNumberLength,*/ Vector3 newPoint, ICollection<int> cutPointsNumbers,List<Vector3> points)
        {
            int number;
            var findsResult = cutPointsNumbers/*.Select(el => el- addedNumberLength)*/.Where(p => Vector3.Distance(points[p],newPoint)<=0.001f).ToList();
            if (findsResult.Count>0)
            {
                number = findsResult.First();
            }
            else
            {
                points.Add(newPoint);
                number = points.Count - 1;
                cutPointsNumbers.Add(number);
            }

            return number;
        }

        private static Vector3 MoveVertices( Vector3 vertex, Vector3 direction, float count)
        {
            return vertex +  direction * count;
        }

        private static Vector3 GetCrossPointByPlane(Vector3 point1, Vector3 point2, float planeX)
        {
            var sameCoefficient1 = (planeX - point1.x) / (point2.x - point1.x);
            var y1 = (point2.y - point1.y) * sameCoefficient1 + point1.y;
            var z1 = (point2.z - point1.z) * sameCoefficient1 + point1.z;
            return new Vector3(planeX, y1, z1);
        }
    


    }
}
