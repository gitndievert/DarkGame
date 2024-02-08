// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    public interface IGoreObjectParent
    {
        public void ExecuteCut(string boneName, Vector3 position);
        public void ExecuteCut(string boneName, Vector3 position, Vector3 force);

        public void ExecuteCut(string boneName, Vector3 position, out GameObject detachedObject);
        public void ExecuteCut(string boneName, Vector3 position, Vector3 force, out GameObject detachedObject);
        
        public void SpawnCutParticles(Vector3 position, Vector3 direction);

        public void ExecuteExplosion();

        public void ExecuteExplosion(float radialForce);
        
        public void ExecuteExplosion(Vector3 position, float force);
        
        public void ExecuteExplosion(out List<GameObject> explosionParts);

        public void ExecuteExplosion(float radialForce, out List<GameObject> explosionParts);
        
        public void ExecuteExplosion(Vector3 position, float force, out List<GameObject> explosionParts);
        
        
    }
}
