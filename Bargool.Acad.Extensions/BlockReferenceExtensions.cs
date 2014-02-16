/*
 * User: aleksey.nakoryakov
 * Date: 31.01.2012
 * Time: 17:37
 */
using System;
using System.Globalization;
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
			if (db == null)
				throw new ArgumentNullException("BlockReference didn't added to database");
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
		
		public static void SetDynamicParameterValue(this BlockReference block, string parameterName, object parameterValue)
		{
			if (block.IsDynamicBlock)
			{
				DynamicBlockReferencePropertyCollection pc = block.DynamicBlockReferencePropertyCollection;
				DynamicBlockReferenceProperty prop = pc
					.Cast<DynamicBlockReferenceProperty>()
					.FirstOrDefault(p => p.PropertyName.Equals(parameterName, StringComparison.InvariantCulture));
				if (prop != null)
				{
					if (prop.PropertyTypeCode == (short)DynamicPropertyTypes.Distance)
					{
						prop.Value = parameterValue;
					}
					else if (prop.PropertyTypeCode == (short)DynamicPropertyTypes.Visibility)
					{
						object val = prop.GetAllowedValues()
							.First(n => n == parameterValue);
						prop.Value = val;
					}
				}
				else
					throw new ArgumentException("No parameter " + parameterName);
			}
		}
		
		public static void SetDynamicParameterValue(this BlockReference block, string parameterName, string parameterValue)
		{
			// TODO: Этот метод надо удалить - он лишний
			if (block.IsDynamicBlock)
			{
				DynamicBlockReferencePropertyCollection pc = block.DynamicBlockReferencePropertyCollection;
				DynamicBlockReferenceProperty prop = pc
					.Cast<DynamicBlockReferenceProperty>()
					.FirstOrDefault(p => p.PropertyName.Equals(parameterName, StringComparison.InvariantCulture));
				if (prop != null)
				{
					if (prop.PropertyTypeCode == (short)DynamicPropertyTypes.Distance)
					{
						prop.Value = double.Parse(parameterValue, CultureInfo.InvariantCulture);
					}
					else if (prop.PropertyTypeCode == (short)DynamicPropertyTypes.Visibility)
					{
						object val = prop.GetAllowedValues()
							.First(n => n.ToString().Equals(parameterValue, StringComparison.InvariantCulture));
						prop.Value = val;
					}
				}
				else
					throw new ArgumentException("No parameter " + parameterName);
			}
		}
		
		/// <summary>
		/// Текущее значение параметра видимости динамического блока
		/// </summary>
		/// <param name="block"></param>
		/// <returns>Текущее значение параметра видиомсти,
		/// если параметра видимости нет - пустая строка</returns>
		public static string GetCurrentVisibilityValue(this BlockReference block)
		{
			if (block.IsDynamicBlock)
			{
				DynamicBlockReferencePropertyCollection pc = block.DynamicBlockReferencePropertyCollection;
				foreach (DynamicBlockReferenceProperty property in pc)
				{
					if (property.PropertyTypeCode == 5)
					{
						return property.Value.ToString();
					}
				}
			}
			return string.Empty;
		}
		
		/// <summary>
		/// Получение возможный значений параметра видимости
		/// </summary>
		/// <param name="block"></param>
		/// <returns>Массив названий видимости. Если параметра видимости нет - null</returns>
		public static string[] GetVisibilityValues(this BlockReference block)
		{
			if (block.IsDynamicBlock)
			{
				DynamicBlockReferencePropertyCollection pc = block.DynamicBlockReferencePropertyCollection;
				foreach (DynamicBlockReferenceProperty property in pc)
				{
					if (property.PropertyTypeCode == 5)
						return property.GetAllowedValues().Select(n => n.ToString()).ToArray();
				}
			}
			return null;
		}
		
		public static object GetDynamicPropertyValue(this BlockReference block, string propertyname)
		{
			if (block.IsDynamicBlock)
			{
				DynamicBlockReferencePropertyCollection pc = block.DynamicBlockReferencePropertyCollection;
				foreach (DynamicBlockReferenceProperty property in pc)
				{
					if (property.PropertyName.Equals(propertyname, StringComparison.InvariantCulture))
					{
						return property.Value;
					}
				}
			}
			return null;
		}
	}
}
