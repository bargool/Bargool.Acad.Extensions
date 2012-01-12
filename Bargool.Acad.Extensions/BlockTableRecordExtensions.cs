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
			return btr.GetObjects(false, false);
		}
		public static Dictionary<string, List<ObjectId>> GetObjects(this BlockTableRecord btr, bool EvalOffLayers, bool EvalFrozenLayers)
		{
			Dictionary<string, List<ObjectId>> result = new Dictionary<string, List<ObjectId>>();
			foreach (ObjectId id in btr)
			{
				if (id.IsValid&&!id.IsErased&&!id.IsNull)
				{
					Entity ent = id.GetObject(OpenMode.ForRead) as Entity;
					if (ent!=null)
					{
						LayerTableRecord ltr = (LayerTableRecord)ent.LayerId.GetObject(OpenMode.ForRead);
						bool eval = true;
						if (!EvalOffLayers&&ltr.IsOff)
							eval = false;
						if (!EvalFrozenLayers&&ltr.IsFrozen)
							eval = false;
						if (eval)
						{
							if (!result.ContainsKey(id.ObjectClass.Name))
								result.Add(id.ObjectClass.Name, new List<ObjectId>());
							result[id.ObjectClass.Name].Add(id);
						}
					}
				}
			}
			return result;
		}
	}
}
