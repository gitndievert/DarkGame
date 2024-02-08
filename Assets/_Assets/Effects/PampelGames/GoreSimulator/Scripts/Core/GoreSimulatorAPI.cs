// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    /// <summary>
    ///     Global API for <see cref="GoreSimulator" />.
    /// </summary>
    public static class GoreSimulatorAPI
    {
        private static readonly HashSet<GoreSimulator> activeGoreSimulators = new();

        /* Internal *******************************************************************************************************************************/

        public static void AddGoreSimulator(GoreSimulator goreSimulator)
        {
            activeGoreSimulators.Add(goreSimulator);
        }

        public static void RemoveGoreSimulator(GoreSimulator goreSimulator)
        {
            activeGoreSimulators.Remove(goreSimulator);
        }

        /* Informational **********************************************************************************************************************/

        /// <summary>
        ///     Get a list of all active <see cref="GoreSimulator" />s.
        /// </summary>
        public static List<GoreSimulator> GetActiveGoreSimulators()
        {
            return activeGoreSimulators.ToList();
        }

        /// <summary>
        ///     Returns the total number of active <see cref="GoreSimulator" />s in the hierarchy.
        /// </summary>
        public static int ActiveGoreSimulators()
        {
            return activeGoreSimulators.Count;
        }

        /* Operational ***********************************************************************************************************************/


        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        public static void ExecuteExplosionAll()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.ExecuteExplosion();
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="radialForce">
        ///     Radial force to be added to the explosion parts, directed from the character center to each cutted part.
        ///     Note: Requires the physics submodule with rigidbody checked.
        /// </param>
        public static void ExecuteExplosionAll(float radialForce)
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.ExecuteExplosion(radialForce);
        }


        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="position">Explosion center from where a spherical force is applied on the mesh parts.</param>
        /// <param name="force">
        ///     Force to be added to the explosion parts, directed from the position to each cutted part.
        ///     Note: Requires the physics submodule with rigidbody checked.
        /// </param>
        public static void ExecuteExplosionAll(Vector3 position, float force)
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.ExecuteExplosion(position, force);
        }

        /// <summary>
        ///     Activates ragdoll mode for the character. Ensure that the Unity Animator component is deactivated and any
        ///     custom character controllers do not interfere with the physics-based ragdoll behavior.
        /// </summary>
        public static void ExecuteRagdollAll()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.ExecuteRagdoll();
        }


        /// <summary>
        ///     Cleans up all active Gore Simulator components and associated pooled objects in every scene.
        /// </summary>
        public static void SceneCleanup()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.SceneCleanup();
        }

        /// <summary>
        ///     Despawns all detached mesh parts and particle effects that have been created by Gore Simulators, recycling them into the pool.
        /// </summary>
        public static void DespawnAllObjects()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.DespawnAllObjects();
        }

        /// <summary>
        ///     Despawns all detached mech parts that have been created by Gore Simulators, recycling them into the object pool.
        ///     Excluding particles.
        /// </summary>
        public static void DespawnAllDetachedObjects()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.DespawnDetachedObjects();
        }

        /// <summary>
        ///     Despawns all active particles that have been created by Gore Simulators, recycling them into the pool.
        /// </summary>
        public static void DespawnAllParticles()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.DespawnParticles();
        }

        /// <summary>
        ///     Resets all characters back to their initial state.
        /// </summary>
        public static void ResetCharacters()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.ResetCharacter();
        }

        /// <summary>
        ///     Records the current hierarchy of all Gore Simulators.
        ///     Required if GameObjects added to the bone hierarchy after game start should be registered.
        /// </summary>
        public static void RecordHierarchies()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.RecordHierarchy();
        }

        /// <summary>
        ///     Restores the last recorded hierarchy of all Gore Simulators. By default, the hierarchy is being recorded once in Awake().
        ///     This method is also being called automatically with <see cref="ResetCharacter" />.
        /// </summary>
        public static void RestoreHierarchies()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.RestoreHierarchy();
        }

        /// <summary>
        ///     Get all active GameObjects in the scene that have been created by Gore Simulator components.
        /// </summary>
        public static List<GameObject> GetAllCreatedObjects()
        {
            var createdObjects = new List<GameObject>();
            foreach (var goreSimulator in activeGoreSimulators) createdObjects.AddRange(goreSimulator.GetCreatedObjects());
            return createdObjects;
        }

        /// <summary>
        ///     Retrieves all detached children of active Gore Simulators.
        /// </summary>
        public static List<GameObject> GetAllDetachedChildren()
        {
            var detachedChildren = new List<GameObject>();
            foreach (var goreSimulator in activeGoreSimulators) detachedChildren.AddRange(goreSimulator.GetDetachedChildren());
            return detachedChildren;
        }

        /// <summary>
        ///     Get all active Particle Systems in the scene that have been created by Gore Simulators.
        /// </summary>
        public static List<GameObject> GetAllActiveParticles()
        {
            var activeParticles = new List<GameObject>();
            foreach (var goreSimulator in activeGoreSimulators) activeParticles.AddRange(goreSimulator.GetActiveParticles());
            return activeParticles;
        }

        /// <summary>
        ///     Get all active Particle Systems in the scene that have been created by the Cut Modules from Gore Simulators.
        /// </summary>
        public static List<GameObject> GetAllActiveCutParticles()
        {
            var activeParticles = new List<GameObject>();
            foreach (var goreSimulator in activeGoreSimulators) activeParticles.AddRange(goreSimulator.GetActiveCutParticles());
            return activeParticles;
        }

        /// <summary>
        ///     Get all active Particle Systems in the scene that have been created by the Explosion Module from Gore Simulators.
        /// </summary>
        public static List<GameObject> GetActiveExplosionParticles()
        {
            var activeParticles = new List<GameObject>();
            foreach (var goreSimulator in activeGoreSimulators) activeParticles.AddRange(goreSimulator.GetActiveExplosionParticles());
            return activeParticles;
        }

        /// <summary>
        ///     Overwrites the material colors for all Gore Simulators.
        /// </summary>
        public static void SetComponentColors()
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.SetComponentColor();
        }

        /// <summary>
        ///     Overwrites the material colors for all Gore Simulators.
        /// </summary>
        /// <param name="color">Color value.</param>
        public static void SetComponentColors(Color color)
        {
            foreach (var goreSimulator in activeGoreSimulators) goreSimulator.SetComponentColor(color);
        }
    }
}