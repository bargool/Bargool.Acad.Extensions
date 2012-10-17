/*
 * User: aleksey.nakoryakov
 * Date: 10.04.12
 * Time: 18:10
 */
using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Методы расширения для класса Curve
	/// </summary>
	public static class CurveExtensions
	{
		/// <summary>
		/// Метод определяет лежит ли точка на указанной кривой. Точка считается на кривой, если расстояние от неё до кривой не больше,
		/// чем значение Tolerance в данном чертеже
		/// </summary>
		/// <param name="curve">Кривая, на которой определяем точку</param>
		/// <param name="point">Точка, положение которой определяем</param>
		/// <returns>Если расстояние от данной точки до кривой меньше Tolerance, возвращает true</returns>
		public static bool IsPointOnCurve(this Curve curve, Point3d point)
		{
			return IsPointOnCurve(curve, point, Tolerance.Global);
		}
		
		public static bool IsPointOnCurve(this Curve curve, Point3d point, Tolerance tolerance)
		{
			try
			{
				Point3d pt = curve.GetClosestPointTo(point, false);
				return (pt - point).Length <= tolerance.EqualPoint;
			}
			catch 
			{ }
			return false;
		}
		
		/// <summary>
		/// Метод служит для определения длины кривой
		/// </summary>
		/// <param name="curve">Кривая, для которой вычисляется длина</param>
		/// <returns>Длину кривой. Если кривая Ray или Xline - возвращает double.PositiveInfinity</returns>
		/// <exception cref="@ArgumentNullException@">Если кривая == null</exception>
		public static double GetLength(this Curve curve)
		{
			if (curve == null)
				throw new ArgumentNullException("curve is null");
			if (curve is Ray || curve is Xline)
				return double.PositiveInfinity;
			return 
				curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
		}
	}
}
