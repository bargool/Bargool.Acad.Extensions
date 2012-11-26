/*
 * User: aleksey.nakoryakov
 * Date: 31.01.2012
 * Time: 17:37
 */
using System;
using System.Linq;
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
		
		/// <summary>
		/// Метод добавляет к вхождению блока атрибуты, определённые в определении блока
		/// </summary>
		/// <param name="o">Вхождение блока</param>
		/// <param name="UseDefaultTexts">Использовать ли значения по умолчанию, установленные атрибутам</param>
		public static void AppendAttributes(this BlockReference o, bool UseDefaultTexts=true)
		{
			Database db = o.Database;
			ObjectId blockDefinitionId = o.IsDynamicBlock ? o.DynamicBlockTableRecord : o.BlockTableRecord;
			BlockTableRecord blockDefinition = blockDefinitionId.GetObject<BlockTableRecord>();
			if (blockDefinition == null)
				throw new System.Exception("BlockTableRecord missed");
			if (blockDefinition.HasAttributeDefinitions)
			{
				using (Transaction tr = db.TransactionManager.StartTransaction())
				{
					var attDefinitions = blockDefinition.Cast<ObjectId>()
						.Where(n => n.ObjectClass.Name == "AcDbAttributeDefinition")
						.Select(n => n.GetObject<AttributeDefinition>(OpenMode.ForRead));
					foreach (AttributeDefinition attdef in attDefinitions)
					{
						AttributeReference attref = new AttributeReference();
						attref.SetAttributeFromBlock(attdef, o.BlockTransform);
						if (UseDefaultTexts)
						{
							attref.TextString = attdef.TextString;
						}
						o.AttributeCollection.AppendAttribute(attref);
						tr.AddNewlyCreatedDBObject(attref, true);
					}
					tr.Commit();
				}
			}
		}
		
		/// <summary>
		/// Метод добавляет к вхождению блока атрибуты, определённые в определении блока.
		/// Атрибутам присваиваются значения по умолчанию
		/// </summary>
		/// <param name="o">Вхождение блока</param>
//		public static void AppendAttributes(this BlockReference o)
//		{
//			AppendAttributes(o, true);
//		}
	}
}
