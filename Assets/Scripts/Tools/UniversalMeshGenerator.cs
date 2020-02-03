using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Tools.VectorTools;

namespace Assets.Scripts.Tools
{
    public static class UniversalMeshGenerator
    {

        public static List<int> Generate2D_ByPoints_ZPlane(List<int> pointNumbers, List<Vector3> vertices, bool inverseNormal=false)
        {
            if (pointNumbers.Count < 3)
            {
                if (MainManager.EnableLogging) Debug.LogError("Can't generate triangles from less 3 vertices.");
                return null;
            }
            var triangles = new List<int>();

            var sorted = pointNumbers.OrderBy(el => vertices[el].z).ThenBy(el => vertices[el].y).ToList();//sort by z
            var number1 = sorted[0];
            var number2 = sorted[1];

            safe = 0;
            var condition = vertices[number1].y < vertices[number2].y;
            Find3Point(condition ? number1: number2, condition?number2: number1, sorted, vertices, triangles, inverseNormal);
            
            return triangles;
        }

        private static int safe;
        private static void Find3Point(int number_1, int number_2, List<int> pointNumbers, List<Vector3> vertices, List<int> triangles, bool inverseNormal = false)
        {
            safe++;
            if (safe > 15) return;

            if (MainManager.EnableLogging) Debug.Log($"Start Search 3 point for {vertices[number_1]}({number_1}) {vertices[number_2]}({number_2})");

            var finds = pointNumbers.Where(el => CheckTriangleConditions(triangles, vertices, vertices[number_1], vertices[number_2], vertices[el]))
                .OrderBy(el => Vector3.Distance(vertices[number_1], vertices[el]) + Vector3.Distance(vertices[number_2], vertices[el]))
                .ToList();//Сортировка не всегда будет  предоставлять лучший вариант, но лучше чем ничего.

            if (finds.Count <= 0) return;//end Searching.

            var number_3 = finds.First();
            triangles.AddRange(new List<int> { number_1, inverseNormal? number_3 : number_2, inverseNormal ? number_2 : number_3 });
            if (MainManager.EnableLogging) Debug.Log($"_________________________________");
            if (MainManager.EnableLogging) Debug.Log($"___ tr {vertices[number_1].ToString()} {vertices[number_2].ToString()} {vertices[number_3].ToString()}  ({number_1} {number_2} {number_3})");
            var angle = Vector3.SignedAngle((vertices[number_2] - vertices[number_1]).normalized,
                (vertices[number_3] - vertices[number_1]).normalized, Vector3.right);
            if (MainManager.EnableLogging) Debug.Log($"angle {angle}");
                  
            if (angle > 0)
            {
                Find3Point(number_3, number_2, pointNumbers, vertices, triangles, inverseNormal);
                Find3Point(number_1, number_3, pointNumbers, vertices, triangles, inverseNormal);
            }
            else
            {
                Find3Point(number_2, number_3, pointNumbers, vertices, triangles, inverseNormal);
                Find3Point(number_3, number_1, pointNumbers, vertices, triangles, inverseNormal);
            }
            Find3Point(number_2, number_1, pointNumbers, vertices, triangles);
        }

        private static bool CheckTriangleConditions(List<int> triangles, List<Vector3> vertices, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            //определяем в какую сторону от вектора искать тетью точку.
            if (WherePointZY(point1, point2, point3) != 1)
            {
                if(MainManager.EnableLogging) Debug.Log($"{point3} other side");
                return false;
            }

            // Проверка на такой триугольник.
            if (HasEqualsTriangle(triangles, vertices, point1, point2, point3))
            {
                if (MainManager.EnableLogging) Debug.Log($"{point3} already has point");
                return false;
            }

            //Проверка на пересечение уже с существующими триугольниками
            if (HasVectorCrossWithTrianglesZY(triangles, vertices, point1,
                    point3) //Проверка на пересечение уже с существующими триугольниками
                || HasVectorCrossWithTrianglesZY(triangles, vertices, point2, point3))
            {
                if (MainManager.EnableLogging) Debug.Log($"{point3} has cross");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Делает поточкам выпуклый многоугольник и добавляя глубину делает меш для 3д модели.
        /// </summary>
        /// <param name="vertices2D"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static Mesh GenerateMeshFrom2dTo3d(List<Vector3> vertices2D, float depth)
        {

            if (vertices2D.Count < 3)
            {
                Debug.LogError("Can't generate mesh from less 3 vertices.");
                return null;
            }

            var mesh = new Mesh();
            var triangles = new List<int>();
            var half = vertices2D.Count;
            var graph = new int[half, half];

            vertices2D = vertices2D.OrderBy(x => x.x).ToList(); //sort by x
            //Add Depth
            var vertices3D = new List<Vector3>(vertices2D);
            vertices3D.AddRange(vertices3D.Select(x =>
            {
                var outPoint = x;
                outPoint.z += depth;
                return outPoint;
            }).ToList());

            //triangles
            for (var i = 0; i < half - 2; i++)
            {
                float angle;

                //первая точка. проверка какая грань будет смежной от предыдущего триугольника.
                int point1=i;
                if(i>0)
                {
                    point1 = ((vertices3D[i + 2].y > vertices3D[i + 1].y) ^ (vertices3D[i - 1].y > vertices3D[i].y))
                    ? i
                    : i - 1;
                }

                angle = Vector3.SignedAngle((vertices2D[i + 1] - vertices2D[point1]).normalized,
                    (vertices2D[i + 2] - vertices2D[point1]).normalized, Vector3.forward);
                //Вторая точка. Определяем ротацию.
                var point2 = (angle < 0 ) ? i + 1 : i + 2;
                //Третья точка. Определяем ротацию.
                var point3 = (angle < 0 ) ? i + 2 : i + 1;

                //font
                triangles.AddRange(new List<int> {point1, point2, point3});
                //back.инверсия ротации.
                triangles.AddRange(new List<int> {point1 + half, point3 + half, point2 + half});


                graph[point1, point2] = 1;
                graph[point1, point3] = 1;
                graph[point2, point1] = 1;
                graph[point2, point3] = 1;
                graph[point3, point1] = 1;
                graph[point3, point2] = 1;

            }

            //triangles sides
            var counter = 0;
            var frame = new List<int>();
            CalculateFrame(0, ref frame, graph, vertices2D, ref counter);
             
            for (var i = 0; i < frame.Count; i++)
            {
                var point = frame[i];
                var iMinusOne = (i == 0 ? frame.Count - 1 : i - 1);
                var iPlusOne = (i == frame.Count - 1 ? 0 : i + 1);

                var vector1 = (vertices3D[point + half] - vertices3D[point]).normalized;
                var vector2 = (vertices3D[frame[iMinusOne] + half] - vertices3D[point]).normalized;
                var crossAngle = Vector3.Cross(vector1, vector2);
                var angle = Vector3.SignedAngle(vector1, vector2, crossAngle);
                var point2 = (angle > 0) ? frame[iMinusOne] + half : point + half;
                var point3 = (angle > 0) ? point + half : frame[iMinusOne] + half;

                //тут по два треугольника на вершину.Прверяем в какую сторону крутить.
                triangles.AddRange(new List<int>
                {
                    point,
                    point2,
                    point3
                });

                vector1 = (vertices3D[frame[iPlusOne]] - vertices3D[point]).normalized;
                vector2 = (vertices3D[point + half] - vertices3D[point]).normalized;
                crossAngle = Vector3.Cross(vector1, vector2);
                angle = Vector3.SignedAngle(vector1, vector2, crossAngle);
                point2 = (angle > 0) ? point + half : frame[iPlusOne];
                point3 = (angle > 0) ? frame[iPlusOne] : point + half;
                triangles.AddRange(new List<int>
                {
                    point,
                    point2,
                    point3
                });
            }

            Centralize(vertices3D, depth);
            mesh.vertices = vertices3D.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void Centralize(List<Vector3> vertices3D, float depth)
        {
            var minX = vertices3D.Min(i => i.x);
            var maxX = vertices3D.Max(i => i.x);
            var shiftX = -minX - (maxX - minX) / 2f;
            var minY = vertices3D.Min(i => i.y);
            var maxY = vertices3D.Max(i => i.y);
            var shiftY = -minY - (maxY - minY) / 2f;
            for (var index = 0; index < vertices3D.Count; index++)
            {
                vertices3D[index] += new Vector3(shiftX, shiftY, -depth / 2f);
            }
        }


        //Рассчитать внешний контур для постройки граней.
        //Сдесь реализован упращенный алгоритм. 
        private static void CalculateFrame(int currentPoint, ref List<int> frameOut, int[,] graph, List<Vector3> vertices2D, ref int counter)
        {
            int previousPoint;
            if (frameOut.Count == 0)
            {
                frameOut.Add(currentPoint); //Первый элемент контура.
                previousPoint = -1;
            }
            else
            {
                previousPoint = frameOut[frameOut.Count - 2];
            }

            var firstPoint = frameOut[0];

            float minAngle = 361;
            var minAnglePoint = -1;
            for (var nextPoint = 0; nextPoint < graph.GetLength(1); nextPoint++)
            {
                if (graph[currentPoint, nextPoint] != 1 /* || nextPoint == currentPoint || nextPoint == previousPoint*/) continue;

                if (frameOut.Exists(x => x == nextPoint && x != firstPoint)) continue;

                var angle = Vector3.SignedAngle(Vector3.up, (vertices2D[nextPoint] - vertices2D[currentPoint]).normalized, Vector3.back);

                if (angle < 0) angle = 360 + angle;


                if (angle <= minAngle)
                {
                    minAngle = angle;
                    minAnglePoint = nextPoint;
                }
            }


            if (minAnglePoint == -1 || /*minAnglePoint == previousPoint ||*/ minAnglePoint == frameOut[0]) return; //проверка на конец обхода.
            frameOut.Add(minAnglePoint);
            counter++;
            if (counter > 500) //на всякий случай. мало ли.
            {
                Debug.LogError("рекурсия зациклена.Проверьте алгоритм расчета внешнего контура.");
                return;
            }

            CalculateFrame(minAnglePoint, ref frameOut, graph, vertices2D, ref counter);
        }
    }
}
