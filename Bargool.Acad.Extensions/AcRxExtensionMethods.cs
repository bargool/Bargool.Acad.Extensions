/*
 * User: aleksey.nakoryakov
 * Date: 26.02.13
 * Time: 11:50
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Bargool.Acad.Extensions
{
	/// <summary>
	/// Description of AcRxExtensionMethods.
	/// </summary>
	public static class AcRxExtensionMethods
	{
		// ObjectId.IsTypeOf<T>() extension method:
		//
		// returns true if the managed wrapper returned
		// by opening the given ObjectId is an instance
		// of the generic argument type or a derived type.
		//
		// Example: Tells if the DBObject referenced by
		// the given ObjectId is an instance of a Curve:
		//
		//   ObjectId id = GetSomeObjectId();
		//
		//   bool isCurve = id.IsTypeOf<Curve>();
		//
		// The advantage in the above use of IsTypeOf<T>() is
		// that it doesn't require the RXClass for Curve to be
		// fetched on each call, because it is already cached
		// in a static member of the generic RXClass<Curve> type.
		//
		// The other benefit is that IsTypeOf<T>() can be
		// used anywhere, and without a transaction, since
		// it doesn't need to open the database object.
		//
		// The same can also be done more directly,
		// but less-intuitively:
		//
		//   bool isCurve = id.ObjectClass.IsAssignableTo<Curve>();
		//
		//
		// And of course, without using any of these helper APIs,
		// one would need to do:
		//
		//   bool isCurve = id.ObjectClass.IsDerivedFrom(
		//                       RXClass.GetClass( typeof( Curve ) )
		//                  );
		//
		
		public static bool IsTypeOf<T>( this ObjectId id )
			where T: DBObject
		{
			return RXClass<T>.IsAssignableFrom( id.ObjectClass );
		}
		
		// Like IsTypeOf<T>, except ids whose opened managed wrapper
		// type are derived from T, but are not T itself, do not match.
		//
		// Use this method when you require all ids to be instances
		// of concrete types (e.g., BlockReference, DBText, etc.), and
		// there is no chance that any objects will be derived from
		// the given type. Using abstract types as the generic argument
		// (e.g., Curve, Entity, DBObject, etc.) is not valid, as the
		// result will always be false:
		//
		// This method is faster than IsTypeOf<T>.
		
		public static bool IsInstanceOf<T>( this ObjectId id )
			where T: DBObject
		{
			return RXClass<T>.unmanagedObject == id.ObjectClass.UnmanagedObject;
		}
		
		// returns true, if a managed wrapper with the given
		// RXClass can be cast to a variable of type T, where
		// T is DBObject or any derived type:
		
		public static bool IsAssignableTo<T>( this RXClass rxclass )
			where T: DBObject
		{
			return RXClass<T>.IsAssignableFrom( rxclass );
		}
		
		// IEnumerable<ObjectId>.OfType<T>() Extension:
		//
		// The OfType<T> extension method is the plural form
		// of IsTypeOf<T>, that reduces an IEnumerable<ObjectId>
		// sequence to only those ids whose managed wrapper
		// type is an instance of the generic argument type.
		//
		// Here we reduce an ObjectIdCollection to a sequence
		// containing only the ObjectIds of Circle entities:
		//
		//   ObjectIdCollection entityIds = GetSomeObjectIdCollection();
		//
		//   var circleIds = entityIds.OfType<Circle>();
		
		// Can be invoked on ObjectId[] arrays or List<ObjectId>:
		
		public static IEnumerable<ObjectId> OfType<T>( this IEnumerable<ObjectId> source )
			where T: DBObject
		{
			ObjectId[] array = source as ObjectId[];
			if( array != null )
				return OfTypeIterator( array );
			else
				return src.Where( IsTypeOf<T> );
		}
		
		// Overload for ObjectIdCollections:
		
		public static IEnumerable<ObjectId> OfType<T>( this ObjectIdCollection ids )
			where T: DBObject
		{
			return OfTypeIterator<T>( ids );
		}
		
		// Overload for BlockTableRecord:
		
		public static IEnumerable<ObjectId> OfType<T>( this BlockTableRecord btr, bool includingErased )
			where T: DBObject
		{
			return OfTypeIterator<T>( includingErased ? btr.IncludingErased : btr );
		}
		
		static IEnumerable<ObjectId> OfTypeIterator<T>( this IEnumerable<ObjectId> ids )
			where T: DBObject
		{
			foreach( ObjectId id in ids )
			{
				if( RXClass<T>.IsAssignableFrom( id.ObjectClass ))
				   yield return id;
				}
		}
		
		// Specialize the case for array and ObjectIdCollection
		// sources, both of which can be iterated faster using
		// their indexers, verses their IEnumerator:
		
		static IEnumerable<ObjectId> OfTypeIterator<T>( this ObjectId[] ids )
			where T: DBObject
		{
			int cnt = ids.Length;
			for( int i = 0; i < cnt; i++ )
			{
				ObjectId id = ids[i];
				if( RXClass<T>.IsAssignableFrom( id.ObjectClass ) )
					yield return id;
			}
		}
		
		static IEnumerable<ObjectId> OfTypeIterator<T>( this ObjectIdCollection ids )
			where T: DBObject
		{
			int cnt = ids.Count;
			for( int i = 0; i < cnt; i++ )
			{
				ObjectId id = ids[i];
				if( RXClass<T>.IsAssignableFrom( id.ObjectClass ) )
					yield return id;
			}
		}
		
		////////////////////////////////////////////////////////////////////////////
		// IEnumerable<ObjectId>.AreAnyOfType<T>():
		//
		// Return true if at least one ObjectId in the source references a
		// database object whose opened managed wrapper type is an instance of T:
		
		public static bool AreAnyOfType<T>( this IEnumerable<ObjectId> ids )
			where T: DBObject
		{
			return ids.Any( IsTypeOf<T> );
		}
		
		public static bool AreAnyOfType<T>( this ObjectIdCollection ids )
			where T: DBObject
		{
			int cnt = ids.Count;
			for( int i = 0; i < cnt; i++ )
			{
				if( ids[i].IsTypeOf<T>() )
					return true;
			}
			return false;
		}
		
		////////////////////////////////////////////////////////////////////////////
		// IEnumerable<ObjectId>.AreAllOfType<T>():
		//
		// Return true if all ObjectIds in the source reference database
		// objects whose opened managed wrapper type is an instance of T.
		//
		// This method is useful for validating the contents of
		// an ObjectIdCollection or array of Object[], passed as
		// an argument to an API.
		//
		
		public static bool AreAllOfType<T>( this IEnumerable<ObjectId> ids)
			where T: DBObject
		{
			return ids.All( IsTypeOf<T> );
		}
		
		public static bool AreAllOfType<T>( this ObjectIdCollection ids )
			where T: DBObject
		{
			int cnt = ids.Count;
			for( int i = 0; i < cnt; i++ )
			{
				if( ! ids[i].IsTypeOf<T>() )
					return false;
			}
			return true;
		}
		
		// Returns true if the given RXClass is the RXClass of the
		// managed wrapper of type T:
		
		public static bool EqualsObjectClass<T>( this RXClass rxclass )
			where T: DBObject
		{
			return rxclass.UnmanagedObject == RXClass<T>.unmanagedObject;
		}
		
		// RXClass<T> Class:
		//
		// RXClass<T> caches the RXClass and value of its UnmanagedObject
		// property for each managed wrapper type T, allowing extension
		// methods of this class to avoid the boxing operation incurrred
		// by using the DisposableWrapper's == operator. A bool indicating
		// if the RXClass has children (derived RXClasses) is cached and
		// updated whenever the ObjectARX runtime class tree is rebuilt.
		//
		// This class handles the ModuleLoaded/unload events, so that it
		// can update its hasChildren member whenever the ObjectARX runtime
		// class hierarchy is rebuilt, which could result in the addition
		// or deletion of child RXClasses. Tracking whether this RXClass has
		// derived/child RXClasses allows us to avoid a pointless call to
		// RXClass.IsDerivedFrom() on RXClasses we compare to this RXClass.
		//
		// Caching the UnmanagedObject property value of each RXClass
		// also exploits a limitation that precludes methods of types
		// deriving from MarshalByRefObject from being inlined.
		
		static class RXClass<T> where T: DBObject
		{
			public static RXClass instance;
			public static IntPtr unmanagedObject;
			public static bool hasChildren;
			
			static RXClass()
			{
				Initialize();
				SystemObjects.DynamicLinker.ModuleLoaded += moduleLoadUnload;
				SystemObjects.DynamicLinker.ModuleUnloaded += moduleLoadUnload;
			}
			
			public static bool IsAssignableFrom( RXClass rxclass )
			{
				return rxclass.UnmanagedObject == unmanagedObject
					|| ( hasChildren && rxclass.IsDerivedFrom( instance ) );
			}
			
			public static bool Equals( RXClass rxclass )
			{
				return rxclass != null && rxclass.UnmanagedObject == unmanagedObject;
			}
			
			static void moduleLoadUnload( object sender, DynamicLinkerEventArgs e )
			{
				Initialize();
			}
			
			// The cost of this is high, and will happen every time
			// a module is loaded and unloaded, but the performance
			// gain resulting from caching this data is significant
			// and far outweighs the slight delay imposed whenever
			// a module is loaded or unloaded. It should be pointed
			// out that the above event handlers are not added until
			// the first use of code that uses IsTypeOf<T>, for each
			// distinct type used as the generic argument.
			
			static void Initialize()
			{
				instance = RXClass.GetClass( typeof( T ) );
				unmanagedObject = instance.UnmanagedObject;
				hasChildren = HasChildren();
			}
			
			
			// Returns a bool indicating if at least one RXClass is
			// directly derived from the RXClass instance member.
			//
			// If the result is false, this RXCLass has no children
			// (e.g., there are no RXClasses derived from this RXClass),
			// and hence, there is no point to calling IsDerivedFrom()
			// on any other RXClass to find out if it is derived from
			// this RXClass.
			//
			// In most cases where GetObjects<T> is used, the generic
			// argument type will be a concrete type of entity, like
			// BlockReference, Polyline, etc. Unless there are custom
			// objects that derive from those concrete types (which is
			// unlikely), there will be no runtime classes that derive
			// from the RXClass for the managed wrapper type used as
			// the generic argument, and we can avoid the cost of a
			// call to RXClass.IsDerivedFrom() on the ObjectClass of
			// every ObjectId in the sequence.
			
			static bool HasChildren()
			{
				foreach( DictionaryEntry e in SystemObjects.ClassDictionary )
				{
					RXClass rxclass = e.Value as RXClass;
					if( rxclass != null && Equals( rxclass.MyParent ) )
						return true;
				}
				return false;
			}
		}
	}
}
