using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PGLibrary.Helpers
{
    public class CoroutinePlus : CustomYieldInstruction
    {
        private IEnumerator enumerator;
        private MonoBehaviour monobehaviour;
        private bool running = true;
    	private bool pauzed = false;


        public CoroutinePlus(IEnumerator myIEnumerator)
        {
            pauzed = false;
            running = true;
            enumerator = myIEnumerator;
        }

        public IEnumerator GetEnumeratorWrapper()
        {
            return IEnumeratorWrapper();
        }


        #region implemented abstract members of CustomYieldInstruction
        public override bool keepWaiting
        {
            get
            {
                return running;
            }
        }
        #endregion

        public bool IsRunning()
        {
            return running;
        }

        public void Stop()
        {
            running = false;
        }

    	public void Pauze()
    	{
    		pauzed = true;
    	}

    	public void Resume()
    	{
    		pauzed = false;
    	}


        IEnumerator IEnumeratorWrapper()
        {
            while (running)
            {
    			if (enumerator != null)
    			{
    				if(pauzed)
    				{
    					yield return null;
    				}
    				else
    				{
    					if(enumerator.MoveNext())
    						yield return enumerator.Current;
    					else
    						running = false;
    				}
    			}
    			else
    			{
    				running = false;
    			}
            }
        }
    }
}