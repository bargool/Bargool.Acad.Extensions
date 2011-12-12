/*
 * User: aleksey.nakoryakov
 * Date: 12.12.2011
 * Time: 17:17
 */
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.DatabaseServices;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Класс расширений для BlockTableRecordExtensions.
	/// </summary>
	public static class BlockTableRecordExtensions
	{
		public static Dictionary<string, List<ObjectId>> GetObjects(this BlockTableRecord btr)
		{
			Dictionary<string, List<ObjectId>> result = new Dictionary<string, List<ObjectId>>();
			foreach (ObjectId id in btr)
			{
				if (!result.ContainsKey(id.ObjectClass.Name))
					result.Add(id.ObjectClass.Name, new List<ObjectId>());
				result[id.ObjectClass.Name].Add(id);
			}
			return result;
		}
	}
}
