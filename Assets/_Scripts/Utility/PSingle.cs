// ********************************************************************
// CONFIDENTIAL - DO NOT DISTRIBUTE
// COPYRIGHT 2019-2024 Wacky Potato Games, LLC. All Rights Reserved.
// 
// If you send, receive, or use this file for any purpose other than
// internal use by Wacky Potato Games, it is without permission and an act of theft.
// Report any misuse of this file immediately to contact@wackypotato.com
// Misuse or failure to report misuse will subject you to legal action.
// 
// The intellectual and technical concepts contained herein are
// proprietary and are protected by trade secret and/or copyright law.
// Dissemination or reproduction of this material is forbidden.
// ********************************************************************

using UnityEngine;

namespace Dark.Utility
{
    /// <summary>
    /// Persistant Singletons through Unity Scenes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PSingle<T> : MonoBehaviour where T : PSingle<T>
    {
        private static T _instance;

        public static T Instance
        {
            get { return _instance; }
        }

        void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            if (gameObject.transform.parent == null)
                DontDestroyOnLoad(gameObject);
            _instance = (T)this;
            PAwake();
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
            PDestroy();
        }

        protected virtual void PAwake()
        {
        }

        protected virtual void PDestroy()
        {
        }
    }
}
