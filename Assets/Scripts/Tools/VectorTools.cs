using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    public static  class VectorTools 
    {
        public static int WherePointZY(Vector3 vectorPoint1, Vector3 vectorPoint2, Vector3 point)
        {
            var s = (vectorPoint2.z - vectorPoint1.z) * (point.y - vectorPoint1.y) - (vectorPoint2.y - vectorPoint1.y) * (point.z - vectorPoint1.z);
            //var s = (vectorPoint1.y*vectorPoint2.z - vectorPoint1.z* vectorPoint2.y) * point.x 
            //        + (vectorPoint1.z*vectorPoint2.x - vectorPoint1.x* vectorPoint2.z) * point.y
            //        + (vectorPoint1.x * vectorPoint2.y - vectorPoint1.y* vectorPoint2.x) * point.z;
            if (Math.Abs(vectorPoint1.y - vectorPoint2.y) < 0.001 && Math.Abs(vectorPoint2.y - point.y) < 0.001 
                || Math.Abs(vectorPoint1.z - vectorPoint2.z) < 0.001 && Math.Abs(vectorPoint2.z - point.z) < 0.001)
            {
                return 0;
            }
            if (s < -0.0001) return 1;
            else if (s > 0.0001) return -1;
            return 0;
        }

        public static bool IsVectorsCrossZY(Vector3 vector1Point1, Vector3 vector1Point2, Vector3 vector2Point1, Vector3 vector2Point2)
        {
            var where1Point1 = WherePointZY(vector1Point1, vector1Point2, vector2Point1);
            var where1Point2 = WherePointZY(vector1Point1, vector1Point2, vector2Point2);
            var where2Point1 = WherePointZY(vector2Point1, vector2Point2, vector1Point1);
            var where2Point2 = WherePointZY(vector2Point1, vector2Point2, vector1Point2);
            if (where1Point1 == 0 && where1Point2 == 0)//лежит ли точка на отрезке.
            {
                if (!PointsInSegment(vector1Point1, vector1Point2, vector2Point1, vector2Point2))
                {
                    if (MainManager.EnableLogging) Debug.Log($" Not Cross : {vector1Point1} {vector1Point2} - {vector2Point1} {vector2Point2}");
                    return false;
                }
                else
                {
                    if (MainManager.EnableLogging) Debug.Log($" Cross : {vector1Point1} {vector1Point2} - {vector2Point1} {vector2Point2}");
                    return true;
                }
            }
            var condition = (where1Point1 != where1Point2 && where2Point1 != where2Point2
                                                          && where1Point2 != 0 && where1Point1 != 0 && where2Point2 != 0 && where2Point1 != 0) /*|| (wherePoint2 == 0 && wherePoint1 == 0)*/;
            if (MainManager.EnableLogging)
            {
                Debug.Log(condition
                    ? $" Cross : {vector1Point1} {vector1Point2} - {vector2Point1} {vector2Point2}"
                    : $" Not Cross : {vector1Point1} {vector1Point2} - {vector2Point1} {vector2Point2}");
            }

            return condition;

        }

        public static bool PointsInSegment(Vector3 segment1, Vector3 segment2, Vector3 point1, Vector3 point2)
        {
            if (Mathf.Min(point1.x, point2.x) >= Mathf.Max(segment1.x, segment2.x) || Mathf.Max(point1.x, point2.x) <= Mathf.Min(segment1.x, segment2.x))
                return false;
            if (Mathf.Min(point1.y, point2.y) >= Mathf.Max(segment1.y, segment2.y) || Mathf.Max(point1.y, point2.y) <= Mathf.Min(segment1.y, segment2.y))
                return false;
            if (Mathf.Min(point1.z, point2.z) >= Mathf.Max(segment1.z, segment2.z) || Mathf.Max(point1.z, point2.z) <= Mathf.Min(segment1.z, segment2.z))
                return false;


            return true;
        }

        public static bool HasVectorCrossWithTrianglesZY(List<int> triangles, List<Vector3> vertices, Vector3 point1, Vector3 point2)
        { 
            for (var i = 0; i < triangles.Count - 2; i += 3)
            {
                if (IsVectorsCrossZY(vertices[triangles[i]], vertices[triangles[i + 1]], point1, point2))
                    return true;
                if (IsVectorsCrossZY(vertices[triangles[i + 1]], vertices[triangles[i + 2]], point1, point2))
                    return true;
                if (IsVectorsCrossZY(vertices[triangles[i + 2]], vertices[triangles[i]], point1, point2))
                    return true;
            }
            return false;
        }

        public static bool HasEqualsTriangle(List<int> triangles, List<Vector3> vertices, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            for (var i = 0; i < triangles.Count - 2; i += 3)
            {
                if (CompareTriangle(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]], point1, point2, point3))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CompareTriangle(Vector3 point11, Vector3 point12, Vector3 point13, Vector3 point21, Vector3 point22, Vector3 point23 )
        {
            if ((point11.Equals(point21) || point11.Equals(point22) || (point11.Equals(point23)))
                &&(point12.Equals(point21) || point12.Equals(point22) || point12.Equals(point23))
                &&(point13.Equals(point21) || point13.Equals(point22) || point13.Equals(point23)))
            {
                return true;
            }
            return false;
        }

    }
}
