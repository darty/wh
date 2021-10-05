using UnityEngine;



namespace PGLibrary.Helpers
{
	/// <summary>
	/// Use this as a base class to create a singleton gameobject
	/// </summary>
	public abstract class SingletonGameObject<T> : MonoBehaviour where T : Component
	{
		private static T instance;
        private static bool isDestroyed;

        //This is the public reference that other classes will use
        public static T Instance
		{
			get
			{
                if (isDestroyed)
                {
                    Debug.LogWarning("Trying to access a singleton that was destroyed");
                    return null;
                }

				//If _instance hasn't been set yet, we grab it from the scene!
				//This will only happen the first time this reference is used.
				if(instance == null)
					CreateInstance();
				return instance;
			}
			protected set
			{
				instance = value;
			}
		}


        protected virtual void Awake()
        {
            //Debug.Log("SingletonGameObject Awake " + this.transform.name, this.transform);
            isDestroyed = false;
            if (instance == null)
            {
                CreateInstance();
            }
            else
            {
                //there is already an instance assigned as singleton, destroy this one
                Debug.Log("SingletonGameObject Awake found existing instance, destroy this one");
                Destroy(this.gameObject);
            }
        }


        protected virtual void OnDestroy()
        {
            //Debug.Log("SingletonGameObject OnDestroy " + this.transform.name, this.transform);
            if (instance == this)
            {
                instance = null;
                isDestroyed = true;
            }
        }

		
		private static void CreateInstance () 
		{
            //Debug.Log("SingletonGameObject CreateInstance of type " + typeof(T).Name);

            //check if the singleton exists in the scene
            T[] instances = FindObjectsOfType<T>();
            
            if (instances == null || instances.Length < 1)
			{
                //no instances of T found, create one
                Debug.Log("SingletonGameObject no instances found, create new");
                GameObject go = new GameObject(typeof(T).Name);
				instance = go.AddComponent<T>();
			}
			else
			{
				//there are more than one instances found, throw error and try to recover
                if(instances.Length > 1)
                    Debug.LogError(string.Format("Multiple instances of this singleton ({0}) were present in the scene", instances[0].GetType().Name) , instances[0]);

                //destroy all instances except one
                instance = instances[0];
                for (int i = instances.Length-1; i>0; i--)
				{
					if(instances[i] != instance)
						Destroy(instances[i]);
				}
			}

		}




	}
}
