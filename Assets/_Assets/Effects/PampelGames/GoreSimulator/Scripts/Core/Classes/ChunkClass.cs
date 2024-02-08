// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using PampelGames.Shared.Tools;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    [Serializable]
    public class ChunkClass
    {
        public string boneName;
        public int cutIndexClassIndex;

        public SerializableMesh serializableMesh;

        // OrigToNewMap - used to find the precached vertices at runtime.
        public List<int> keys;
        public List<int> values;

        /// <summary>
        ///     Cut and Sew indexes of the new mesh.
        /// </summary>
        public List<ExplosionIndexClass> indexClasses;
        

        /********************************************************************************************************************************/
        
        internal Mesh mesh;
        internal Vector3 boundsSize = Vector3.zero;
        internal List<Vector3> cutCenters = new();

    }
}