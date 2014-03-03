/*
 * User: aleksey
 * Date: 01.02.2014
 * Time: 9:35
 */
using System;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Description of EditorExtensions.
	/// </summary>
	public static class EditorExtensions
	{
		public static void WriteLine(this Editor ed, string message, params object[] parameters)
		{
			string s = string.Format(message, parameters);
			ed.WriteMessage("\n" + s + "\n");
		}
		
		/// <summary>
		/// Borrowed from http://www.theswamp.org/index.php?topic=46442.msg514605#msg514605
		/// </summary>
		public static void Zoom(this Editor ed, Extents3d ext)
		{
			if (ed == null)
				throw new ArgumentNullException("ed");
			
			using (ViewTableRecord view = ed.GetCurrentView())
			{
				ext.TransformBy(view.WorldToEye());
				view.Width = ext.MaxPoint.X - ext.MinPoint.X;
				view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
				view.CenterPoint = new Point2d(
					(ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
					(ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
				ed.SetCurrentView(view);
			}
		}
		
		/// <summary>
		/// Borrowed from http://www.theswamp.org/index.php?topic=46442.msg514605#msg514605
		/// </summary>
		/// <param name="ed"></param>
		public static void ZoomExtents(this Editor ed)
		{
			if (ed == null)
				throw new ArgumentNullException("ed");
			
			Database db = ed.Document.Database;
			Extents3d ext = (short)Application.GetSystemVariable("cvport") == 1 ?
				new Extents3d(db.Pextmin, db.Pextmax) :
				new Extents3d(db.Extmin, db.Extmax);
			ed.Zoom(ext);
		}
		
		/// <summary>
		/// Returns the transformation matrix from the ViewportTableRecord DCS to WCS.
		/// </summary>
		/// <remarks>Borrowed from http://www.acadnetwork.com/index.php?topic=232.msg406#msg406</remarks>
		/// <param name="view">The ViewportTableRecord instance this method applies to.</param>
		/// <returns>The DCS to WCS transformation matrix.</returns>
		public static Matrix3d EyeToWorld(this AbstractViewTableRecord view)
		{
			return
				Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
				Matrix3d.Displacement(view.Target - Point3d.Origin) *
				Matrix3d.PlaneToWorld(view.ViewDirection);
		}

		/// <summary>
		/// Returns the transformation matrix from the ViewportTableRecord WCS to DCS.
		/// </summary>
		/// <remarks>Borrowed from http://www.acadnetwork.com/index.php?topic=232.msg406#msg406</remarks>
		/// <param name="view">The ViewportTableRecord instance this method applies to.</param>
		/// <returns>The WCS to DCS transformation matrix.</returns>
		public static Matrix3d WorldToEye(this AbstractViewTableRecord view)
		{
			return view.EyeToWorld().Inverse();
		}
	}
}
