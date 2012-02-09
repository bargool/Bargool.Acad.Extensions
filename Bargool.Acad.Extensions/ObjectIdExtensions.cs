/*
 * User: aleksey
 * Date: 08.02.2012
 * Time: 23:25
 */
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.DatabaseServices;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Класс расширений для ObjectId
	/// </summary>
	public static class ObjectIdExtensions
	{
		// Opens a DBObject in ForRead mode (kaefer @ TheSwamp)
		public static T GetObject<T>(this ObjectId id) where T : DBObject
		{
			return id.GetObject<T>(OpenMode.ForRead);
		}

		// Opens a DBObject in the given mode (kaefer @ TheSwamp)
		public static T GetObject<T>(this ObjectId id, OpenMode mode) where T : DBObject
		{
			return id.GetObject(mode) as T;
		}
		
		// Opens a collection of DBObject in ForRead mode (kaefer @ TheSwamp)
		public static IEnumerable<T> GetObjects<T>(this IEnumerable<ObjectId> ids) where T : DBObject
		{
			return ids.GetObjects<T>(OpenMode.ForRead);
		}

		// Opens a collection of DBObject in the given mode (kaefer @ TheSwamp)
		public static IEnumerable<T> GetObjects<T>(this IEnumerable<ObjectId> ids, OpenMode mode) where T : DBObject
		{
			return ids
				.Cast<ObjectId>()
				.Select(id => id.GetObject<T>(mode))
				.Where(res => res != null);
		}
		
		// Applies the given Action to each element of the collection (mimics the F# Seq.iter function).
		public static void Iterate<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (T item in collection) action(item);
		}

		// Applies the given Action to each element of the collection (mimics the F# Seq.iteri function).
		// The integer passed to the Action indicates the index of element.
		public static void Iterate<T>(this IEnumerable<T> collection, Action<T, int> action)
		{
			int i = 0;
			foreach (T item in collection) action(item, i++);
		}
	}
}
