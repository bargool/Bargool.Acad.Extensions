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
		/// <summary>
		/// Возвращает эффективное имя блока (т.е. если блок анонимный, полученный из динамического
		/// - вернёт имя динамического блока)
		/// </summary>
		/// <param name="o">Вхождение блока</param>
		/// <returns>Эффективное имя блока</returns>
		public static string GetEffectiveName(this BlockReference o)
		{
				BlockTableRecord btr = (BlockTableRecord)o.DynamicBlockTableRecord.GetObject(OpenMode.ForRead);
				return btr.Name;
		}
	}
}
