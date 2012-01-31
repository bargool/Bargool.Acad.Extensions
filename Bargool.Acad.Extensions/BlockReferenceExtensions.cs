/*
 * User: aleksey.nakoryakov
 * Date: 31.01.2012
 * Time: 17:37
 */
using System;

using Autodesk.AutoCAD.DatabaseServices;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Класс расширений для BlockReference
	/// </summary>
	public static class BlockReferenceExtensions
	{
		public static string GetBlockName(this BlockReference o)
		{
//			if (o.IsDynamicBlock)
//			{
				BlockTableRecord btr = (BlockTableRecord)o.DynamicBlockTableRecord.GetObject(OpenMode.ForRead);
				return btr.Name;
//			}
//			else
//			{
//				return o.Name;
//			}
		}
	}
}
