/*
 * User: aleksey.nakoryakov
 * Date: 13.12.2011
 * Time: 13:25
 */
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.DatabaseServices;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Description of DatabaseExtensions.
	/// </summary>
	public static class DatabaseExtensions
	{
//		public static Dictionary<string, List<ObjectId>> GetEntities(this Database db)
//		{
//			return db.GetEntities(false, false, false);
//		}
//
//		public static Dictionary<string, List<ObjectId>> GetEntities(this Database db,
//		                                                             bool EvalOffLayers,
//		                                                             bool EvalFrozenLayers)
//		{
//			return db.GetEntities(EvalOffLayers, EvalFrozenLayers, false);
//		}
//		public static Dictionary<string, List<ObjectId>> GetEntities(this Database db,
//		                                                             bool EvalOffLayers,
//		                                                             bool EvalFrozenLayers,
//		                                                             bool EvalAnonymBlocks)
//		{
//			return db.GetEntities(EvalOffLayers, EvalFrozenLayers, EvalAnonymBlocks, false);
//		}
		
		public static Dictionary<string, List<ObjectId>> GetEntities(this Database db,
		                                                             bool EvalOffLayers,
		                                                             bool EvalFrozenLayers,
		                                                             bool EvalAnonymBlocks,
		                                                             bool EvalXrefs,
		                                                             bool EvalLayouts)
		{
			Dictionary<string, List<ObjectId>> result = new Dictionary<string, List<ObjectId>>();
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
				foreach (ObjectId btrId in bt)
				{
					BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
					if (btr.IsFromExternalReference&&!EvalXrefs)
						continue;
					if (btr.IsAnonymous&&!EvalAnonymBlocks)
						continue;
					if (btr.IsLayout&&!EvalLayouts)
						continue;
					foreach (KeyValuePair<string, List<ObjectId>> kvp in btr.GetObjects(EvalOffLayers, EvalFrozenLayers))
					{
						if (!result.ContainsKey(kvp.Key))
							result.Add(kvp.Key, new List<ObjectId>());
						result[kvp.Key].AddRange(kvp.Value);
					}
				}
			}
			return result;
		}
	}
}
