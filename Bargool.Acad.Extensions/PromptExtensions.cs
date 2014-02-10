/*
 * Created by SharpDevelop.
 * User: alexey.nakoryakov
 * Date: 10.02.2014
 * Time: 12:12
 */
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Description of PromptExtensions.
	/// </summary>
	public static class PromptExtensions
	{
		public static IEnumerable<ObjectId> GetSelectedObjectIds(this PromptSelectionResult res)
		{
			return res.Value.Cast<SelectedObject>().Select(so => so.ObjectId);
		}
	}
}
