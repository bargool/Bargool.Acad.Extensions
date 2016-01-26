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
        public static bool CheckValid(this ObjectId id)
        {
            return !id.IsNull && id.IsValid && !id.IsErased && !id.IsEffectivelyErased;
        }

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

        // Opens a DBObject in the given mode (kaefer @ TheSwamp)
        public static T GetObject<T>(this ObjectId id, OpenMode mode, bool openErased) where T : DBObject
        {
            return id.GetObject(mode, openErased) as T;
        }

        // Opens a DBObject in the given mode (kaefer @ TheSwamp)
        public static T GetObject<T>(this ObjectId id, OpenMode mode, bool openErased, bool forceOpenOnLockedLayer) where T : DBObject
        {
            return id.GetObject(mode, openErased, forceOpenOnLockedLayer) as T;
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
    }
}
