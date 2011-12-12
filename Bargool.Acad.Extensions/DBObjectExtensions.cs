/*
 * User: aleksey.nakoryakov
 * Date: 29.11.2011
 * Time: 15:11
 */
using System;
using System.Linq;

using acad = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.DatabaseServices;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Класс расширений для DBObject
	/// </summary>
	public static class DBObjectExtensions
	{
		
		/// <summary>
		/// Добавляет Xrecord к объекту
		/// </summary>
		/// <param name="o">Объект для добавления, куда добавляется Xrecord</param>
		/// <param name="xrecordName">Имя записи</param>
		/// <param name="buffer">Данные для добавления</param>
		/// <param name="rewrite">Необязательный параметр. Перезаписывать Xrecord? Если false - будем добавлять в конец. По умолчанию true</param>
		/// <param name="addDuplicates">Необязательный параметр. Добавлять дубликаты? Если false - дубликаты не добавлять. По умолчанию false</param>
		public static void WriteXrecord(this DBObject o, string xrecordName, ResultBuffer buffer, bool rewrite = true, bool addDuplicates = false)
		{
			if (xrecordName == null)
				throw new ArgumentNullException("xrecordName is null");
			if (buffer == null)
				throw new ArgumentNullException("buffer is null");
			if (o.ExtensionDictionary==ObjectId.Null || o.ExtensionDictionary.IsErased) {
				o.CreateExtensionDictionary();
			}
			using (DBDictionary dict = (DBDictionary)o.ExtensionDictionary.GetObject(OpenMode.ForWrite))
			{
				Xrecord xrecord = new Xrecord();
				if (dict.Contains(xrecordName)) {
					xrecord = (Xrecord)dict.GetAt(xrecordName).GetObject(OpenMode.ForWrite);
				}
				else {
					dict.SetAt(xrecordName, xrecord);
					o.Database.TransactionManager.AddNewlyCreatedDBObject(xrecord, true);
				}
				if (rewrite) {
					xrecord.Data = buffer;
				}
				else {
					TypedValue[] tempBuffer = xrecord.Data.AsArray();
					if (addDuplicates) {
						xrecord.Data = new ResultBuffer(tempBuffer
						                                .Concat(buffer.AsArray())
						                                .ToArray());
					}
					else {
						xrecord.Data = new ResultBuffer(tempBuffer
						                                .Union(buffer.AsArray())
						                                .ToArray());
					}
				}
			}
		}
		
		/// <summary>
		/// Добавляет Xrecord к объекту
		/// </summary>
		/// <param name="o">Объект для добавления, куда добавляется Xrecord</param>
		/// <param name="xrecordName">Имя записи</param>
		/// <param name="tvalue">Данные для добавления</param>
		/// <param name="rewrite">Необязательный параметр. Перезаписывать Xrecord? Если false - будем добавлять в конец. По умолчанию true</param>
		/// <param name="addDuplicates">Необязательный параметр. Добавлять дубликаты? Если false - дубликаты не добавлять. По умолчанию false</param>
		public static void WriteXrecord(this DBObject o, string xrecordName, TypedValue tvalue, bool rewrite = true, bool addDuplicates = false)
		{
			o.WriteXrecord(xrecordName, new ResultBuffer(
				new TypedValue[]{tvalue}), rewrite, addDuplicates);
		}
		
		/// <summary>
		/// Возвращает содержимое Xrecord
		/// </summary>
		/// <param name="o">Объект, из которого извлекается содержимое Xrecord</param>
		/// <param name="xrecordName">Имя записи</param>
		/// <returns>Содержимое Xrecord, либо null, если такого Xrecord нет</returns>
		public static ResultBuffer GetXrecord(this DBObject o, string xrecordName)
		{
			if (xrecordName == null)
				throw new ArgumentNullException("xrecordName is null");
			if (o.ExtensionDictionary != ObjectId.Null &&
			   !o.ExtensionDictionary.IsErased)
			{
				using (DBDictionary dict = (DBDictionary)o.ExtensionDictionary.GetObject(OpenMode.ForRead))
				{
					if (dict.Contains(xrecordName))
					{
						Xrecord xrecord = dict.GetAt(xrecordName).GetObject(OpenMode.ForRead) as Xrecord;
						if (xrecord != null)
							return xrecord.Data;
					}
				}
			}
			return null;
		}
		
		/// <summary>
		/// Удаляет Xrecord с заданным именем, если после удаления остается пустой словарь - он также удаляется
		/// </summary>
		/// <param name="o">Объект, у которого удаляется Xrecord</param>
		/// <param name="xrecordName">Имя записи</param>
		public static void DeleteXrecord(this DBObject o, string xrecordName)
		{
			if (xrecordName == null)
				throw new ArgumentNullException("xrecordName is null");
			if (o.ExtensionDictionary != ObjectId.Null &&
			   !o.ExtensionDictionary.IsErased)
			{
				using (DBDictionary dict = (DBDictionary)o.ExtensionDictionary.GetObject(OpenMode.ForRead))
				{
					if (dict.Contains(xrecordName))
					{
						dict.UpgradeOpen();
					    dict.Remove(xrecordName);
					    if (dict.Count==0)
					    {
//					    	o.ExtensionDictionary.IsEffectivelyErased
					    	dict.Erase(true);
					    }
					}
				}
//		    	AuditInfo ai = (AuditInfo)AuditInfo.Create(typeof(AuditInfo), o.UnmanagedObject, true);
//		    	o.Audit(ai);
			}
		}
	}
}
