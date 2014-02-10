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
	/// Класс для методов перегрузки различных Prompt results
	/// </summary>
	public static class PromptExtensions
	{
		/// <summary>
		/// Получение ObjectId выделенных элементов из PromptSelectionResult
		/// </summary>
		/// <param name="res"></param>
		/// <returns>Если PromptStatus.OK - возвращается ObjectId выбранных элементов, иначе - null</returns>
		public static IEnumerable<ObjectId> GetSelectedObjectIds(this PromptSelectionResult res)
		{
			if (res.Status == PromptStatus.OK)
				return res.Value.Cast<SelectedObject>().Select(so => so.ObjectId);
			
			return null;
		}
	}
}
