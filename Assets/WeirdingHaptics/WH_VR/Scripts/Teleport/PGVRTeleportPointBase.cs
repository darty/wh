using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PGLibrary.PGVR
{
    /// <summary>
    /// base on Valve.VR.InteractionSystem.TeleportMarkerBase
    /// </summary>
    public abstract class PGVRTeleportPointBase : MonoBehaviour
    {
        public bool locked = false;
        protected bool isHighlighted = false;

        [SerializeField]
        protected PGVRTeleportPointSettings settings;


        #region teleport


        public void Initialize()
        {

        }

        public void SetLocked(bool locked)
        {
            this.locked = locked;

            UpdateVisuals();
        }


        public virtual bool ShowIfCurrentTeleportPoint()
        {
            return false;
        }

        /// <summary>
        /// Triggered by PGVRTeleport when player left this teleportPoint
        /// </summary>
        public virtual void PlayerEntered()
        {

        }


        /// <summary>
        /// Triggered by PGVRTeleport when player entered this teleportPoint
        /// </summary>
        public virtual void PlayerLeft()
        {

        }

        #endregion

        #region graphics

        public void StartHighlight()
        {
            isHighlighted = true;
            UpdateVisuals();
        }

        public void StartLockedHighlight()
        {
            isHighlighted = true;
            UpdateVisuals();
        }

        public void StopHighlight()
        {
            isHighlighted = false;
            UpdateVisuals();
        }

        public virtual bool ShowPositionReticle()
        {
             return false;
        }

        public abstract void SetAlpha(float alpha);
        public abstract void UpdateVisuals();
        #endregion

    }
}
