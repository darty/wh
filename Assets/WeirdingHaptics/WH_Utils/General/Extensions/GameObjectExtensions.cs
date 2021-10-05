using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace UnityEngine
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Removes the component (if there is one).
		/// </summary>
		/// <param name="go">Go.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void RemoveComponent<T>(this GameObject go) where T:Component
		{
			T comp = go.GetComponent<T>();
			if(comp != null)
			{
				GameObject.Destroy(comp);
			}
		}


		/// <summary>
		/// Adds the component if the gameobject hasn't already got one of this type.
		/// </summary>
		/// <returns>The added or already present component.</returns>
		/// <param name="go">Go.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T AddComponentIfMissing<T>(this GameObject go) where T:Component
		{
			T comp = go.GetComponent<T>();
			if(comp == null)
			{
				comp = go.AddComponent<T>();
			}
			return comp;
		}


		/// <summary>
		/// Gets all components of type T in the children of the gameobject (excluding the parent).
		/// Normal GetComponentsInChildren also searches the gameobject itself.
		/// </summary>
		/// <returns>The component in children only.</returns>
		/// <param name="go">Go.</param>
		/// <param name="includeInactive">If set to <c>true</c> include inactive.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		//public static T[] GetComponentsInChildrenOnly<T>(this GameObject go, bool includeInactive = false) where T:Component
		public static T[] GetComponentsInChildrenOnly<T>(this GameObject go, bool includeInactive) where T:Component
		{
			//loop each child and get components in their children (which will include itself)
			//another method would be to get all components with GetComponentsInChildren and filter the parent component with LINQ, but this is x2 slower than this method!
			List<T> tList = new List<T>();
			go.GetComponentsInChildrenOnly<T>(tList, includeInactive);
			return tList.ToArray();
		}



		/// <summary>
		/// Gets all components of type T in the children of the gameobject (excluding the parent).
		/// Normal GetComponentsInChildren also searches the gameobject itself.
		/// </summary>
		/// <param name="go">Go.</param>
		/// <param name="tList">T list.</param>
		/// <param name="includeInactive">If set to <c>true</c> include inactive.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		//public static void GetComponentsInChildrenOnly<T>(this GameObject go, List<T> tList, bool includeInactive = false) where T:Component
		public static void GetComponentsInChildrenOnly<T>(this GameObject go, List<T> tList, bool includeInactive) where T:Component
		{
			//loop each child and get components in their children (which will include itself)
			//another method would be to get all components with GetComponentsInChildren and filter the parent component with LINQ, but this is x2 slower than this method!
			foreach(Transform child in go.transform) 
			{
				tList.AddRange(child.GetComponentsInChildren<T>(includeInactive));
			}
		}




        /// <summary>
        /// Sets the layer of this gameobject and all of it's children.
        /// </summary>
        /// <param name="go">Go.</param>
        /// <param name="layer">Layer.</param>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }


        /// <summary>
        /// Gets a list containing the gameobject and all child galmeobjects.
        /// </summary>
        /// <returns>The child transform list.</returns>
        /// <param name="go">GameObject.</param>
        /// <param name="list">List.</param>
        public static List<GameObject> GetChildGameObjectsList(this GameObject go, List<GameObject> list)
        {
            list.Add(go);

            foreach (Transform t in go.transform)
            {
                t.gameObject.GetChildGameObjectsList(list);
            }
            return list;
        }



    } 
}