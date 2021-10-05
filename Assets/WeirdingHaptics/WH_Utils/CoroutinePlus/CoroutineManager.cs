using UnityEngine;
using System.Collections;
using System;

namespace PGLibrary.Helpers
{
    public class CoroutineManager : SingletonGameObject<CoroutineManager>
    {

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }

        public CoroutinePlus StartCoroutinePlus(IEnumerator myIEnumerator)
        {
            return StartCoroutinePlus(this, myIEnumerator);
        }

        public CoroutinePlus StartCoroutinePlus(MonoBehaviour myMonobehaviour, IEnumerator myIEnumerator)
        {
            CoroutinePlus coroutine = new CoroutinePlus(myIEnumerator);
            myMonobehaviour.StartCoroutine(coroutine.GetEnumeratorWrapper());
            return coroutine;
        }


        public void StartCoroutinePlus(CoroutinePlus coroutinePlus)
        {
            StartCoroutinePlus(this, coroutinePlus);
        }


        public void StartCoroutinePlus(MonoBehaviour myMonobehaviour, CoroutinePlus coroutinePlus)
        {
            myMonobehaviour.StartCoroutine(coroutinePlus.GetEnumeratorWrapper());
        }




        public CoroutinePlus CreateCoroutine(IEnumerator myIEnumerator)
        {
            return new CoroutinePlus(myIEnumerator);
        }




        /// <summary>
        /// create an unique running instance of myCoroutinePlus. If a previous myCoroutinePlus is running, it gets stopped.
        /// </summary>
        /// <param name="myCoroutinePlus"></param>
        /// <param name="myMonobehaviour"></param>
        /// <param name="myIEnumerator"></param>
        /// <returns></returns>
        public CoroutinePlus StartUniqueCoroutine(ref CoroutinePlus myCoroutinePlus, MonoBehaviour myMonobehaviour, IEnumerator myIEnumerator)
        {
            if (myCoroutinePlus != null)
                myCoroutinePlus.Stop();
            return StartCoroutinePlus(myMonobehaviour, myIEnumerator);
        }


        public CoroutinePlus CreateUniqueCoroutine(ref CoroutinePlus myCoroutinePlus, IEnumerator myIEnumerator)
        {
            return StartUniqueCoroutine(ref myCoroutinePlus, this, myIEnumerator);
        }


        public void StopCoroutine(CoroutinePlus coroutinePlus)
        {
            if (coroutinePlus != null)
                coroutinePlus.Stop();
        }

        /*
    	public IEnumerator WaitForRealSeconds(float time)
    	{
    		float start = Time.realtimeSinceStartup;
    		while (Time.realtimeSinceStartup < start + time) 
    		{
    			yield return null;
    		}	
    	}*/


        public IEnumerator ExecuteAfterDelay(MonoBehaviour myMonobehaviour, float delaySeconds, Action action)
        {
            float waitUntill = Time.time + delaySeconds;
            while (Time.time < waitUntill)
                yield return null;
            action();
        }

    }
}