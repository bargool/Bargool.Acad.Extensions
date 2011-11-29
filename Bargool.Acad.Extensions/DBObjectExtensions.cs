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
	/// Класс расширений DBObject
	/// </summary>
	public static class DBObjectExtensions
	{
		/// <summary>
		/// Добавляет Xrecord к объекту
		/// </summary>
		/// <param name="o">Объект для добавления</param>
		/// <param name="xrecordName">Имя записи</param>
		/// <param name="buffer">Данные для записи</param>
		/// <param name="rewrite">Перезаписывать Xrecord? Если false - будем добавлять</param>
		/// <param name="addDuplicates">Добавлять дубликаты? Если false - дубликаты не добавлять</param>
		public static void WriteXrecord(this DBObject o, string xrecordName, ResultBuffer buffer, bool rewrite, bool addDuplicates)
		{
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
	}
}
