/*
 * User: aleksey
 * Date: 01.02.2014
 * Time: 9:35
 */
using System;
using System.Text;
using Autodesk.AutoCAD.EditorInput;

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
	}
}
