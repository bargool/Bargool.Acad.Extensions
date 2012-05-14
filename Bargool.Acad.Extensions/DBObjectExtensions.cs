/*
 * User: aleksey.nakoryakov
 * Date: 29.11.2011
 * Time: 15:11
 */
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
		/// <param name="rewrite">Необязательный параметр. Перезаписывать Xrecord? Если false - будем добавлять в конец</param>
		/// <param name="addDuplicates">Необязательный параметр. Добавлять дубликаты? Если false - дубликаты не добавлять</param>
		public static void WriteXrecord(this DBObject o, string xrecordName, ResultBuffer buffer, bool rewrite, bool addDuplicates)
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
		/// <param name="rewrite">Необязательный параметр. Перезаписывать Xrecord? Если false - будем добавлять в конец</param>
		/// <param name="addDuplicates">Необязательный параметр. Добавлять дубликаты? Если false - дубликаты не добавлять</param>
		public static void WriteXrecord(this DBObject o, string xrecordName, TypedValue tvalue, bool rewrite, bool addDuplicates)
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
			bool delDict = false;
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
						if (dict.Count == 0)
							delDict = true;
					}
				}
				if (delDict)
					o.ReleaseExtensionDictionary();
			}
		}
		
		/// <summary>
		/// Метод через Reflection копирует значения свойств с аналогичными именами и типами
		/// </summary>
		/// <param name="o">Текущий объект</param>
		/// <param name="source">Объект-источник, откуда копируются значения свойств. Может быть другого типа, чем o</param>
		public static void CopyPropertiesFrom(this DBObject o, DBObject source)
		{
			PropertyInfo[] sourceProps = source.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance);
			PropertyInfo[] thisProps = o.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance);
			foreach (PropertyInfo sourcePI in sourceProps)
			{
				PropertyInfo thisPI = thisProps.FirstOrDefault(p =>
				                                               p.Name == sourcePI.Name &&
				                                               p.PropertyType == sourcePI.PropertyType &&
				                                               p.CanRead && p.CanWrite);
				if (sourcePI.CanRead&&sourcePI.CanWrite &&
				    sourcePI.PropertyType.Name!="Point3d" &&
				    thisPI!=null)
				{
					try
					{
						thisPI.SetValue(o, sourcePI.GetValue(source, null), null);
					}
					catch (System.Reflection.TargetInvocationException ex)
					{
						Debug.WriteLine(ex.InnerException.Message);
					}
				}
			}
		}
	}
}