using System;
using UnityEngine;

namespace PGLibrary.Math
{
	public static class MathHelper 
	{

        /// <summary>
        /// Scales the value between minValue and maxValue, returning a value in 0 - 1 range (without clamping)
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="value">Value.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        public static float Normalize(float value, float minValue, float maxValue)
        {
            return (value - minValue) / (maxValue - minValue);
        }


        /// <summary>
        /// Scales the value between minValue and maxValue, returning a value in 0 - 1 range (without clamping)
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="value">Value.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        public static float NormalizeClamp(float value, float minValue, float maxValue)
        {
            return Mathf.Clamp(Normalize(value, minValue, maxValue), 0, 1);
        }

        /// <summary>
        /// Remap the specified value from the range fromMinValue - fromMaxValue
        /// to a new range toMinValue - toMaxValue.
        /// </summary>
        /// <returns>The remap.</returns>
        /// <param name="value">Value.</param>
        /// <param name="fromMinValue">From minimum value.</param>
        /// <param name="fromMaxValue">From max value.</param>
        /// <param name="toMinValue">To minimum value.</param>
        /// <param name="toMaxValue">To max value.</param>
        public static float Remap(float value, float fromMinValue, float fromMaxValue, float toMinValue, float toMaxValue)
        {
            return toMinValue + (value - fromMinValue) * (toMaxValue - toMinValue) / (fromMaxValue - fromMinValue);
        }

        /// <summary>
        /// Remap the specified value from the range fromMinValue - fromMaxValue
        /// to a new range toMinValue - toMaxValue.
        /// Clamp the resulting value between toMinValue and toMaxValue
        /// </summary>
        /// <returns>The remap.</returns>
        /// <param name="value">Value.</param>
        /// <param name="fromMinValue">From minimum value.</param>
        /// <param name="fromMaxValue">From max value.</param>
        /// <param name="toMinValue">To minimum value.</param>
        /// <param name="toMaxValue">To max value.</param>
        public static float RemapClamp(float value, float fromMinValue, float fromMaxValue, float toMinValue, float toMaxValue)
        {
            return Mathf.Clamp(Remap(value, fromMinValue, fromMaxValue, toMinValue, toMaxValue), toMinValue, toMaxValue);
        }



        /// <summary>
        /// Checks if type is a numeric type
        /// </summary>
        /// <returns><c>true</c>, if numeric type was ised, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        public static bool IsNumericType(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}




		/// <summary>
		/// Finds the intersection of the line L1 though by p1 and p2 and line L2 though by p3 and p4
		/// math by https://www.topcoder.com/community/data-science/data-science-tutorials/geometry-concepts-line-intersection-and-its-applications/
		/// </summary>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="p3">P3.</param>
		/// <param name="p4">P4.</param>
		/// <param name="intersection">Intersection.</param>
		public static bool GetLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
		{
			intersection = Vector2.zero;

			float A1 = p2.y - p1.y;
			float B1 = p1.x - p2.x;
			float C1 = A1*p1.x + B1*p1.y;

			float A2 = p4.y - p3.y;
			float B2 = p3.x - p4.x;
			float C2 = A2*p3.x + B2*p3.y;

			float det = A1*B2 - A2*B1;
			if(det == 0)
			{
				//Lines are parallel
				return false;
			}else
			{
				float x = (B2*C1 - B1*C2)/det;
				float y = (A1*C2 - A2*C1)/det;
				intersection = new Vector2(x, y);
				return true;
			}

		}

		/// <summary>
		/// Finds the intersection of the linesegment L1 from p1 to p2 and line L2 from p3 to p4
		/// </summary>
		/// <returns><c>true</c>, if line segment intersection was gotten, <c>false</c> otherwise.</returns>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="p3">P3.</param>
		/// <param name="p4">P4.</param>
		/// <param name="intersection">Intersection.</param>
		public static bool GetLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
		{
			bool intersect = GetLineIntersection(p1, p2, p3, p4, out intersection);
			float epsilon = 0.001f;

			if(intersect)
			{
				//check if intersectionpoint is on the segments (first check L1, afterwards check L2 if needed)
                bool onL1 = IsPointOnLineSegment(p1, p2, intersection, epsilon);
                bool onL2 = false;
                if(onL1)
                    onL2 = IsPointOnLineSegment(p3, p4, intersection, epsilon);

                return onL1 && onL2;
			}

			return intersect;
		}


        

        /// <summary>
        /// Pcheck if point lays on line segment
        /// </summary>
        /// <returns><c>true</c>, if point lays on line segment <c>false</c> otherwise.</returns>
        /// <param name="p1Segment">P1 segment.</param>
        /// <param name="p2Segment">P2 segment.</param>
        /// <param name="point">Point.</param>
        /// <param name="precision">Precision.</param>
        public static bool IsPointOnLineSegment(Vector2 p1Segment, Vector2 p2Segment, Vector2 point, float precision = 0.001f)
        {
            //check if intersectionpoint is on the segments (checking one segment is enough)
            float l1MinX = Mathf.Min(p1Segment.x, p2Segment.x);
            float l1MaxX = Mathf.Max(p1Segment.x, p2Segment.x);
            bool xOnSegment = (point.x >= l1MinX - precision && point.x <= l1MaxX + precision);

            float l1MinY = Mathf.Min(p1Segment.y, p2Segment.y);
            float l1MaxY = Mathf.Max(p1Segment.y, p2Segment.y);
            bool yOnSegment = (point.y >= l1MinY - precision && point.y <= l1MaxY + precision);

            return xOnSegment && yOnSegment;
        }




        /// <summary>
        /// Crossing Number, also referred to as odd-even test.
        /// The function will return YES if the point x,y is inside the polygon, or
        ///  NO if it is not.  If the point is exactly on the edge of the polygon,
        ///  then the function may return YES or NO.
        /// </summary>
        /// <returns><c>true</c>, if in point is inside polygon, <c>false</c> otherwise.</returns>
        /// <param name="p">Pointto be checkt</param>
        /// <param name="polyCorners">Poly corners which form the 2d polygon</param>
        public static bool IsPointInPolygon2D(Vector2 p, Vector2[] polyCorners) 
        {
            int   i, j=polyCorners.Length-1 ;
            bool  oddNodes=false      ;

            for (i=0; i<polyCorners.Length; i++) 
            {
                if ((polyCorners[i].y< p.y && polyCorners[j].y>=p.y
                ||   polyCorners[j].y< p.y && polyCorners[i].y>=p.y)
                &&  (polyCorners[i].x<=p.x || polyCorners[j].x<=p.x)) 
                {
                    if (polyCorners[i].x+(p.y-polyCorners[i].y)/(polyCorners[j].y-polyCorners[i].y)*(polyCorners[j].x-polyCorners[i].x)<p.x) 
                    {
                        oddNodes=!oddNodes; 
                    }
                }
                j=i; 
            }

            return oddNodes; 
        }


        /// <summary>
        /// Rounds the value to the nearest multiplication of factor.
        /// </summary>
        /// <returns>The to nearest factor.</returns>
        /// <param name="value">Value.</param>
        /// <param name="factor">Factor.</param>
        public static float RoundToNearestFactor(float value, float factor)
        {
            return Mathf.Round(value / factor) * factor;
        }


        /// <summary>
        /// Projects the point on a line formed by linepoint and linevec
        /// </summary>
        /// <returns>The point on line.</returns>
        /// <param name="linePoint">Line point.</param>
        /// <param name="lineDir">Line vec.</param>
        /// <param name="point">Point.</param>
        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineDir, Vector3 point)
        {
            lineDir.Normalize();

            //get vector from point on line to point in space
            Vector3 linePointToPoint = point - linePoint;

            float dot = Vector3.Dot(linePointToPoint, lineDir);

            return linePoint + lineDir * dot;
        }


        public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 vector = linePoint2 - linePoint1;
     
            Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);
     
            int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);
     
            //The projected point is on the line segment
            if(side == 0)
                return projectedPoint;
     
            if(side == 1)
                return linePoint1;
    
            if(side == 2)
                return linePoint2;
     
            //output is invalid
            return Vector3.zero;
        }  



        /// <summary>
        /// Determine on which side point is locatied on the linesegment formed by linePoint1 and linePoint2
        /// Returns 0 if point is on the line segment.
        /// Returns 1 if point is outside of the line segment and located on the side of linePoint1.
        /// Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        /// </summary>
        /// <returns>The on which side of line segment.</returns>
        /// <param name="linePoint1">Line point1.</param>
        /// <param name="linePoint2">Line point2.</param>
        /// <param name="point">Point.</param>
        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {

            Vector3 lineVec = linePoint2 - linePoint1;
            Vector3 pointVec = point - linePoint1;

            float dot = Vector3.Dot(pointVec, lineVec);

            //point is on side of linePoint2, compared to linePoint1
            if (dot > 0)
            {

                //point is on the line segment
                if (pointVec.magnitude <= lineVec.magnitude)
                {

                    return 0;
                }

                //point is not on the line segment and it is on the side of linePoint2
                else
                {

                    return 2;
                }
            }

            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else
            {

                return 1;
            }
        }


        /// <summary>
        /// Transform point from local space (defined by pos, rot, sca) to world space
        /// </summary>
        /// <param name="point">Point in local space</param>
        /// <param name="position">position of the local space origin</param>
        /// <param name="rotation">rotation of the local space origin</param>
        /// <param name="scale">scale of the local space origin</param>
        public static Vector3 TransformPoint(Vector3 point, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
            Vector3 worldPos = m.MultiplyPoint3x4(point);
            return worldPos;
        }



        /// <summary>
        /// Transform point from world space to local space (defined by pos, rot, sca)
        /// </summary>
        /// <param name="point">Point in world space</param>
        /// <param name="position">position of the local space origin</param>
        /// <param name="rotation">rotation of the local space origin</param>
        /// <param name="scale">scale of the local space origin</param>
        public static Vector3 InverseTransformPoint(Vector3 point, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 m = Matrix4x4.TRS(position, rotation, scale);
            Vector3 localPos = m.inverse.MultiplyPoint3x4(point);
            return localPos;
        }



    }
}
