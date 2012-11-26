/*
 * User: aleksey.nakoryakov
 * Date: 12.12.2011
 * Time: 17:17
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Класс расширений для BlockTableRecordExtensions.
	/// </summary>
	public static class BlockTableRecordExtensions
	{
		public static Dictionary<string, List<ObjectId>> GetEntities(this BlockTableRecord btr)
		{
			return btr.GetEntities(false, false);
		}
		public static Dictionary<string, List<ObjectId>> GetEntities(this BlockTableRecord btr, bool EvalOffLayers, bool EvalFrozenLayers)
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
		
		/// <summary>
		/// Синхронизация вхождений блоков с их определением
		/// via http://sites.google.com/site/bushmansnetlaboratory/moi-zametki/attsynch
		/// </summary>
		/// <param name="btr">Запись таблицы блоков, принятая за определение блока</param>
		/// <param name="directOnly">Следует ли искать только на верхнем уровне, или же нужно
		/// анализировать и вложенные вхождения, т.е. следует ли рекурсивно обрабатывать блок в блоке:
		/// true - только верхний; false - рекурсивно проверять вложенные блоки.</param>
		/// <param name="removeSuperfluous">
		/// Следует ли во вхождениях блока удалять лишние атрибуты (те, которых нет в определении блока).</param>
		/// <param name="setAttDefValues">
		/// Следует ли всем атрибутам, во вхождениях блока, назначить текущим значением значение по умолчанию.</param>
		public static void AttSync(this BlockTableRecord btr, bool directOnly, bool removeSuperfluous, bool setAttDefValues) {
			Database db = btr.Database;
//			using (Bushman.AutoCAD.DatabaseServices.WorkingDatabaseSwitcher wdb = new Bushman.AutoCAD.DatabaseServices.WorkingDatabaseSwitcher(db)) {
			using (Transaction t = db.TransactionManager.StartTransaction()) {
				BlockTable bt = (BlockTable) t.GetObject(db.BlockTableId, OpenMode.ForRead);
				
				//Получаем все определения атрибутов из определения блока
				IEnumerable<AttributeDefinition> attdefs = btr.Cast<ObjectId>()
					.Where(n => n.ObjectClass.Name == "AcDbAttributeDefinition")
					.Select(n => (AttributeDefinition) t.GetObject(n, OpenMode.ForRead))
					.Where(n => !n.Constant);//Исключаем константные атрибуты, т.к. для них AttributeReference не создаются.
				
				//В цикле перебираем все вхождения искомого определения блока
				foreach (ObjectId brId in btr.GetBlockReferenceIds(directOnly, false)) {
					BlockReference br = (BlockReference) t.GetObject(brId, OpenMode.ForWrite);
					
					//Проверяем имена на соответствие. В том случае, если вхождение блока "A" вложено в определение блока "B",
					//то вхождения блока "B" тоже попадут в выборку. Нам нужно их исключить из набора обрабатываемых объектов
					//- именно поэтому проверяем имена.
					if (br.Name != btr.Name)
						continue;
					
					//Получаем все атрибуты вхождения блока
					IEnumerable<AttributeReference> attrefs = br.AttributeCollection.Cast<ObjectId>()
						.Select(n => (AttributeReference) t.GetObject(n, OpenMode.ForWrite));
					
					//Тэги существующих определений атрибутов
					IEnumerable<string> dtags = attdefs.Select(n => n.Tag);
					//Тэги существующих атрибутов во вхождении
					IEnumerable<string> rtags = attrefs.Select(n => n.Tag);
					
					//Если требуется - удаляем те атрибуты, для которых нет определения
					//в составе определения блока
					if (removeSuperfluous)
						foreach (AttributeReference attref in attrefs.Where(n => rtags
						                                                    .Except(dtags).Contains(n.Tag)))
							attref.Erase(true);
					
					//Свойства существующих атрибутов синхронизируем со свойствами их определений
					foreach (AttributeReference attref in attrefs.Where(n => dtags
					                                                    .Join(rtags, a => a, b => b, (a, b) => a).Contains(n.Tag))) {
						AttributeDefinition ad = attdefs.First(n => n.Tag == attref.Tag);
						
						//Метод SetAttributeFromBlock, используемый нами далее в коде, сбрасывает
						//текущее значение многострочного атрибута. Поэтому запоминаем это значение,
						//чтобы восстановить его сразу после вызова SetAttributeFromBlock.
						string value = attref.TextString;
						attref.SetAttributeFromBlock(ad, br.BlockTransform);
						//Восстанавливаем значение атрибута
						attref.TextString = value;
						
						if (attref.IsMTextAttribute) {
							
						}
						
						//Если требуется - устанавливаем для атрибута значение по умолчанию
						if (setAttDefValues)
							attref.TextString = ad.TextString;
						attref.AdjustAlignment(db);
					}
					
					//Если во вхождении блока отсутствуют нужные атрибуты - создаём их
					IEnumerable<AttributeDefinition> attdefsNew = attdefs.Where(n => dtags
					                                                            .Except(rtags).Contains(n.Tag));
					
					foreach (AttributeDefinition ad in attdefsNew) {
						AttributeReference attref = new AttributeReference();
						attref.SetAttributeFromBlock(ad, br.BlockTransform);
						attref.AdjustAlignment(db);
						br.AttributeCollection.AppendAttribute(attref);
						t.AddNewlyCreatedDBObject(attref, true);
					}
				}
				btr.UpdateAnonymousBlocks();
				t.Commit();
			}
			//Если это динамический блок
			if (btr.IsDynamicBlock) {
				using (Transaction t = db.TransactionManager.StartTransaction()) {
					foreach (ObjectId id in btr.GetAnonymousBlockIds()) {
						BlockTableRecord _btr = (BlockTableRecord) t.GetObject(id, OpenMode.ForWrite);
						
						//Получаем все определения атрибутов из оригинального определения блока
						IEnumerable<AttributeDefinition> attdefs = btr.Cast<ObjectId>()
							.Where(n => n.ObjectClass.Name == "AcDbAttributeDefinition")
							.Select(n => (AttributeDefinition) t.GetObject(n, OpenMode.ForRead));
						
						//Получаем все определения атрибутов из определения анонимного блока
						IEnumerable<AttributeDefinition> attdefs2 = _btr.Cast<ObjectId>()
							.Where(n => n.ObjectClass.Name == "AcDbAttributeDefinition")
							.Select(n => (AttributeDefinition) t.GetObject(n, OpenMode.ForWrite));
						//Определения атрибутов анонимных блоков следует синхронизировать
						//с определениями атрибутов основного блока
						//Тэги существующих определений атрибутов
						IEnumerable<string> dtags = attdefs.Select(n => n.Tag);
						IEnumerable<string> dtags2 = attdefs2.Select(n => n.Tag);
						//1. Удаляем лишние
						foreach (AttributeDefinition attdef in attdefs2.Where(n => !dtags.Contains(n.Tag))) {
							attdef.Erase(true);
						}
						//2. Синхронизируем существующие
						foreach (AttributeDefinition attdef in attdefs.Where(n => dtags
						                                                     .Join(dtags2, a => a, b => b, (a, b) => a).Contains(n.Tag))) {
							AttributeDefinition ad = attdefs2.First(n => n.Tag == attdef.Tag);
							ad.Position = attdef.Position;
							#if ACAD2009
							ad.TextStyle = attdef.TextStyle;
							#else
							ad.TextStyleId = attdef.TextStyleId;
							#endif
							//Если требуется - устанавливаем для атрибута значение по умолчанию
							if (setAttDefValues)
								ad.TextString = attdef.TextString;
							ad.Tag = attdef.Tag;
							ad.Prompt = attdef.Prompt;
							ad.LayerId = attdef.LayerId;
							ad.Rotation = attdef.Rotation;
							ad.LinetypeId = attdef.LinetypeId;
							ad.LineWeight = attdef.LineWeight;
							ad.LinetypeScale = attdef.LinetypeScale;
							ad.Annotative = attdef.Annotative;
							ad.Color = attdef.Color;
							ad.Height = attdef.Height;
							ad.HorizontalMode = attdef.HorizontalMode;
							ad.Invisible = attdef.Invisible;
							ad.IsMirroredInX = attdef.IsMirroredInX;
							ad.IsMirroredInY = attdef.IsMirroredInY;
							ad.Justify = attdef.Justify;
							ad.LockPositionInBlock = attdef.LockPositionInBlock;
							ad.MaterialId = attdef.MaterialId;
							ad.Oblique = attdef.Oblique;
							ad.Thickness = attdef.Thickness;
							ad.Transparency = attdef.Transparency;
							ad.VerticalMode = attdef.VerticalMode;
							ad.Visible = attdef.Visible;
							ad.WidthFactor = attdef.WidthFactor;
							ad.CastShadows = attdef.CastShadows;
							ad.Constant = attdef.Constant;
							ad.FieldLength = attdef.FieldLength;
							ad.ForceAnnoAllVisible = attdef.ForceAnnoAllVisible;
							ad.Preset = attdef.Preset;
							ad.Prompt = attdef.Prompt;
							ad.Verifiable = attdef.Verifiable;
							ad.AdjustAlignment(db);
						}
						//3. Добавляем недостающие
						foreach (AttributeDefinition attdef in attdefs.Where(n => !dtags2.Contains(n.Tag))) {
							AttributeDefinition ad = new AttributeDefinition();
							ad.SetDatabaseDefaults();
							ad.Position = attdef.Position;
							#if ACAD2009
							ad.TextStyle = attdef.TextStyle;
							#else
							ad.TextStyleId = attdef.TextStyleId;
							#endif
							ad.TextString = attdef.TextString;
							ad.Tag = attdef.Tag;
							ad.Prompt = attdef.Prompt;
							ad.LayerId = attdef.LayerId;
							ad.Rotation = attdef.Rotation;
							ad.LinetypeId = attdef.LinetypeId;
							ad.LineWeight = attdef.LineWeight;
							ad.LinetypeScale = attdef.LinetypeScale;
							ad.Annotative = attdef.Annotative;
							ad.Color = attdef.Color;
							ad.Height = attdef.Height;
							ad.HorizontalMode = attdef.HorizontalMode;
							ad.Invisible = attdef.Invisible;
							ad.IsMirroredInX = attdef.IsMirroredInX;
							ad.IsMirroredInY = attdef.IsMirroredInY;
							ad.Justify = attdef.Justify;
							ad.LockPositionInBlock = attdef.LockPositionInBlock;
							ad.MaterialId = attdef.MaterialId;
							ad.Oblique = attdef.Oblique;
							ad.Thickness = attdef.Thickness;
							ad.Transparency = attdef.Transparency;
							ad.VerticalMode = attdef.VerticalMode;
							ad.Visible = attdef.Visible;
							ad.WidthFactor = attdef.WidthFactor;
							ad.CastShadows = attdef.CastShadows;
							ad.Constant = attdef.Constant;
							ad.FieldLength = attdef.FieldLength;
							ad.ForceAnnoAllVisible = attdef.ForceAnnoAllVisible;
							ad.Preset = attdef.Preset;
							ad.Prompt = attdef.Prompt;
							ad.Verifiable = attdef.Verifiable;
							_btr.AppendEntity(ad);
							t.AddNewlyCreatedDBObject(ad, true);
							ad.AdjustAlignment(db);
						}
						//Синхронизируем все вхождения данного анонимного определения блока
						_btr.AttSync(directOnly, removeSuperfluous, setAttDefValues);
					}
					//Обновляем геометрию определений анонимных блоков, полученных на основе
					//этого динамического блока
					btr.UpdateAnonymousBlocks();
					t.Commit();
				}
			}
//			}
		}
		
		/// <summary>
		/// Синхронизация вхождений блоков с их определением
		/// via http://sites.google.com/site/bushmansnetlaboratory/moi-zametki/attsynch
		/// Вхождения обрабатываются рекурсивно, в них удаляются лишние атрибуты,
		/// значения по умолчанию не назначаются
		/// </summary>
		/// <param name="btr">Запись таблицы блоков, принятая за определение блока</param>
		public static void AttSync(this BlockTableRecord btr)
		{
			btr.AttSync(false, true, false);
		}
		
		/// <summary>
		/// Возвращает ObjectId всех вхождений блока. В том числе если блок динамический
		/// </summary>
		/// <param name="btr">BlockTableRecord блока, чьи вхождения надо искать</param>
		/// <returns>Коллекцию вхождений блока, в том числе динамических</returns>
		public static IEnumerable<ObjectId> GetAllBlockReferenceIds(this BlockTableRecord btr, bool directOnly)
		{
			Database db = btr.Database;
			IEnumerable<ObjectId> brefIds = btr
				.GetBlockReferenceIds(directOnly, false)
				.Cast<ObjectId>()
				.Concat(
					btr.GetAnonymousBlockIds()
					.Cast<ObjectId>()
					.SelectMany(
						n => n.GetObject<BlockTableRecord>()
						.GetBlockReferenceIds(directOnly, false)
						.Cast<ObjectId>()));
			return brefIds;
		}
	}
}
