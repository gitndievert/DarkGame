// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    public static class DebugHandler
    {

        public static void EmptyPooledObject()
        {
            Debug.LogError("Gore Simulator detected an Empty GameObject in the internal pool.\n" +
                           "Either call 'SceneCleanup' manually via API or GS component before closing a scene or deactivate the pool in the Global Settings.\n" +
                           "More information can be found in the documentation: 'Troubleshooting'.");
        }
    }
}
