// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PampelGames.Shared.Tools.PGInspector;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    /// <summary>
    ///     Gore Simulator Component.
    /// </summary>
    [AddComponentMenu("Pampel Games/Gore Simulator")]
    public class GoreSimulator : MonoBehaviour, IGoreObjectParent
    {
        public SO_DefaultReferences _defaultReferences;
        public SO_GlobalSettings _globalSettings;
        public SO_ColorKeywords _ColorKeywords;

        public SO_Storage storage;

        [SerializeReference] public List<GoreModuleBase> goreModules = new();
        [SerializeReference] public List<SubModuleBase> cutModules = new();
        [SerializeReference] public List<SubModuleBase> explosionModules = new();
        [SerializeReference] public List<SubModuleBase> ragdollModules = new();

        public Color componentColor;
        public Material cutMaterial;
        public bool cutMaterialAdded;
        public Material decalMaterial;
        public bool decalMaterialAdded;
        public PhysicMaterial physicMaterial;
        public bool destroyChildren = true;
        public Enums.Children childrenEnum = Enums.Children.Rendered;
        public List<SkinnedMeshRenderer> skinnedChildren = new();

        public SkinnedMeshRenderer smr;

        public bool meshCutInitialized;
        public bool ragdollInitialized;
        public bool colliderInitialized;

#if UNITY_EDITOR
        public bool setupMeshCut = true;
        public int meshesPerBone = 2;
        public bool setupRagdoll = true;
        public bool setupRagdollAnimator = true;
        public Animator ragdollAnimator;
        public float ragdollTotalMass = 20f;
        public PGEnums.AxisEnum jointOrientation = PGEnums.AxisEnum.XPlus;
        public bool inverseDirection;
        public bool createCollider = true;
        public List<BonesListClass> bonesListClasses; // Inspector Bones Tree View
#endif

        public Transform center;
        public BonesClass centerBonesClass;

        /// <summary>
        ///     Assigned bones.
        /// </summary>
        public List<Transform> bones;

        /// <summary>
        ///     Created from the assigned bones.
        /// </summary>
        public List<BonesClass> bonesClasses;

        /// <summary>
        ///     Components within the hierarchy of the assigned bones.
        /// </summary>
        internal List<GoreBone> goreBones;

        internal HashSet<Transform> smrBones;

        internal Mesh originalMesh;
        internal Mesh bakedMesh;
        internal Material[] originalMaterials;
        internal List<Mesh> skinnedChildrenBakedMeshes = new();

        internal List<Transform> nonBoneChildren;

        internal Dictionary<string, Tuple<BonesClass, BonesStorageClass>> bonesDict;

        internal MeshNativeDataClass meshNativeDataClass;

        internal readonly ExecutionCutClass centerExecutionCutClass = new();
        internal Mesh centerMesh;
        internal readonly List<UsedBonesClass> usedBonesClasses = new();

        internal bool updateWhenOffscreenDefault;
        internal bool meshOptimized;

        private Dictionary<string, GoreBone> goreBoneDict;

        private GameObject ragdollCutCopy;
        private List<BonesClass> ragdollCutBonesClasses;

        private bool exploded;
        private bool ragdollActive;

        private List<int> colorKeywordIDs;


        /// <summary>
        ///     GameObjects in the scene that that are released to the pool on reset.
        /// </summary>
        private readonly HashSet<GameObject> poolableObjects = new();

        /// <summary>
        ///     GameObjects in the scene that can be destroyed on reset.
        /// </summary>
        private readonly HashSet<GameObject> destroyableObjects = new();
        
        /// <summary>
        ///     Non-bone children (items etc.) that have been detached from the character.
        /// </summary>
        private readonly HashSet<GameObject> detachedChildren = new();

        private readonly List<HierarchyDataClass> hierarchyDataList = new();


        /* Public Delegates *************************************************************************************************************/

        public event Action OnExecute = delegate { };
        public event Action<string> OnExecuteCut = delegate { };
        public event Action OnExecuteExplosion = delegate { };
        public event Action OnExecuteRagdoll = delegate { };
        public event Action OnCharacterReset = delegate { };
        public event Action OnDeath = delegate { };
        public event Action OnDestroyAction = delegate { };

        /* Unity Events ****************************************************************************************************************/

        public PGEventClass onExecuteClass;
        public PGEventClass onExecuteCutClass;
        public PGEventClass onExecuteExplosionClass;
        public PGEventClass onExecuteRagdollClass;
        public PGEventClass onCharacterReset;
        public PGEventClass onDeath;
        public PGEventClass onDestroy;

        /********************************************************************************************************************************/


        private void Awake()
        {
            if (!meshCutInitialized && !ragdollInitialized) return;
            colorKeywordIDs = _ColorKeywords.ComponentColorKeywordIDs();
            goreBones = new List<GoreBone>();
            foreach (var bonesClass in bonesClasses) goreBones.Add(bonesClass.goreBone);
            for (var i = 0; i < goreModules.Count; i++) goreModules[i].Initialize();

            InitializeMeshCut();
            InitializedRagdoll();

            bonesDict = new Dictionary<string, Tuple<BonesClass, BonesStorageClass>>();
            for (var i = 0; i < bonesClasses.Count; i++)
                bonesDict.Add(bonesClasses[i].bone.name, new Tuple<BonesClass, BonesStorageClass>(bonesClasses[i], storage.bonesStorageClasses[i]));
            smrBones = new HashSet<Transform>(smr.bones);
            goreBoneDict = new Dictionary<string, GoreBone>();
            for (var i = 0; i < goreBones.Count; i++) goreBoneDict.Add(goreBones[i].gameObject.name, goreBones[i]);

            RecordHierarchy();
            GoreSimulatorAPI.AddGoreSimulator(this);
            SetComponentColor();
        }

        private void InitializeMeshCut()
        {
            if (!meshCutInitialized) return;
            centerMesh = new Mesh();
            cutMaterial = Instantiate(cutMaterial);
            decalMaterial = Instantiate(decalMaterial);
            originalMesh = smr.sharedMesh;
            originalMaterials = smr.materials;
            for (var i = 0; i < skinnedChildren.Count; i++) skinnedChildrenBakedMeshes.Add(new Mesh());
            bakedMesh = new Mesh();
            storage.meshDataClass.InitializeRuntimeMeshData(originalMesh);
            meshNativeDataClass = new MeshNativeDataClass();
            meshNativeDataClass.InitializeRuntimeMeshData(storage.meshDataClass.serializableMesh);
            InitializeChunkMeshes();
            centerExecutionCutClass.newIndexes.UnionWith(storage.meshDataClass.serializableMesh.indexes);
            Pool.InitializePool(_defaultReferences.pooledMesh, _globalSettings.cutPreload, _globalSettings.cutLimited);
            for (var i = 0; i < cutModules.Count; i++) cutModules[i].Initialize();
            for (var i = 0; i < explosionModules.Count; i++) explosionModules[i].Initialize();
        }

        private void InitializeChunkMeshes()
        {
            if (bonesClasses.Count == 0) return;
            if (bonesClasses[0].chunkClasses.Count > 0 && bonesClasses[0].chunkClasses[0].mesh != null) return; // Only used for prefab instantiation.

            for (var i = 0; i < storage.bonesStorageClasses.Count; i++)
            {
                if (storage.bonesStorageClasses[i].chunkClasses.Count == 0) continue;

                for (var j = 0; j < storage.bonesStorageClasses[i].chunkClasses.Count; j++)
                {
                    var editorChunkClass = storage.bonesStorageClasses[i].chunkClasses[j];
                    bonesClasses[i].chunkClasses.Add(new ChunkClass());
                    var newChunkClass = bonesClasses[i].chunkClasses[j];
                    var serializableMesh = editorChunkClass.serializableMesh;
                    var chunkMesh = serializableMesh.CreateMeshFromData();
                    newChunkClass.mesh = chunkMesh;
                    newChunkClass.boneName = editorChunkClass.boneName;
                    newChunkClass.cutIndexClassIndex = editorChunkClass.cutIndexClassIndex;
                    newChunkClass.keys = editorChunkClass.keys;
                    newChunkClass.values = editorChunkClass.values;
                    newChunkClass.indexClasses = editorChunkClass.indexClasses;
                    newChunkClass.boundsSize = chunkMesh.bounds.size;
                }
            }
        }

        private void InitializedRagdoll()
        {
            if (!ragdollInitialized) return;
            updateWhenOffscreenDefault = smr.updateWhenOffscreen;
            RagdollUtility.ToggleRagdoll(goreBones, false, smr, updateWhenOffscreenDefault);
            for (var i = 0; i < ragdollModules.Count; i++) ragdollModules[i].Initialize();
        }

        private void ResetCharacterInternal()
        {
            ResetMeshCut();
            ResetRagdoll();
            DespawnAllObjects();
            foreach (var bone in smr.bones) bone.gameObject.SetActive(true);
            for (var i = 0; i < goreModules.Count; i++) goreModules[i].Reset(bonesClasses);
            HierarchyDataUtility.RestoreHierarchy(hierarchyDataList);
            detachedChildren.Clear();
            RecordHierarchy();
            InvokeOnCharacterReset();
        }

        private void ResetMeshCut()
        {
            if (!meshCutInitialized) return;
            smr.sharedMesh = originalMesh;
            smr.materials = originalMaterials;
            centerExecutionCutClass.ClearIndexes();
            usedBonesClasses.Clear();
            centerExecutionCutClass.newIndexes.UnionWith(storage.meshDataClass.serializableMesh.indexes);
            smr.enabled = true;
            foreach (var skinnedChild in skinnedChildren) skinnedChild.enabled = true;
            cutMaterialAdded = false;
            decalMaterialAdded = false;
            exploded = false;
            for (var i = 0; i < cutModules.Count; i++) cutModules[i].moduleActive = true;
            for (var i = 0; i < explosionModules.Count; i++) explosionModules[i].moduleActive = true;
            if (meshOptimized)
            {
                meshOptimized = false;
                for (var i = 0; i < storage.bonesStorageClasses.Count; i++)
                {
                    if (storage.bonesStorageClasses[i].chunkClasses.Count == 0) continue;
                    for (var j = 0; j < storage.bonesStorageClasses[i].chunkClasses.Count; j++)
                    {
                        var editorChunkClass = storage.bonesStorageClasses[i].chunkClasses[j];
                        var newChunkClass = bonesClasses[i].chunkClasses[j];
                        var chunkMesh = editorChunkClass.serializableMesh.CreateMeshFromData();
                        newChunkClass.mesh = chunkMesh;
                    }
                }
            }
        }

        private void ResetRagdoll()
        {
            if (!ragdollInitialized) return;
            ragdollActive = false;
            for (var i = 0; i < ragdollModules.Count; i++) ragdollModules[i].moduleActive = true;
        }


        /********************************************************************************************************************************/

        internal void AddDetachedObject(GameObject obj)
        {
            poolableObjects.Add(obj);
        }

        internal void AddDestroyableObject(GameObject obj)
        {
            destroyableObjects.Add(obj);
        }

        internal void AddDetachedChild(GameObject obj)
        {
            detachedChildren.Add(obj);
        }

        internal void ReleasePooledObject(GameObject obj)
        {
            if (obj == null) return;
            if (!poolableObjects.Contains(obj)) return;
            poolableObjects.Remove(obj);
            Pool.Release(obj, GetInstanceID());
        }

        /********************************************************************************************************************************/
        /* Public ***********************************************************************************************************************/

        /// <summary>
        ///     Executes a cut at the bone nearest to the given position.
        ///     Please note that this method incurs some overhead due to the bone-finding process.
        /// </summary>
        /// <param name="position">The position at which the cut should be executed.</param>
        public void ExecuteCut(Vector3 position)
        {
            var boneIndex = CutUtility.FindNearestTransformIndex(bones, position);
            ExecuteCut(bones[boneIndex].name, position);
        }

        /// <summary>
        ///     Executes a cut on the mesh for the specified bone.
        /// </summary>
        /// <param name="boneName">Name of the bone that should be cut.</param>
        /// <param name="position">Position of the cut. Finds the nearest possible mesh part of that bone.</param>
        public void ExecuteCut(string boneName, Vector3 position)
        {
            ExecuteCut(boneName, position, Vector3.zero);
        }

        /// <summary>
        ///     Executes a cut at the bone nearest to the given position.
        ///     Please note that this method incurs some overhead due to the bone-finding process.
        /// </summary>
        /// <param name="position">The position at which the cut should be executed.</param>
        /// <param name="force">Force to be applied to the cut mesh.</param>
        public void ExecuteCut(Vector3 position, Vector3 force)
        {
            var boneIndex = CutUtility.FindNearestTransformIndex(bones, position);
            ExecuteCut(bones[boneIndex].name, position, force);
        }

        /// <summary>
        ///     Executes a cut on the mesh for the specified bone.
        /// </summary>
        /// <param name="boneName">Name of the bone that should be cut.</param>
        /// <param name="position">Position of the cut. Finds the nearest possible mesh part of that bone.</param>
        /// <param name="force">Force to be applied to the cut mesh.</param>
        public void ExecuteCut(string boneName, Vector3 position, Vector3 force)
        {
            if (!meshCutInitialized) return;
            if (exploded) return;
            if (ExecutionUtility.FindGoreModule<GoreModuleCut>(goreModules) is not GoreModuleCut moduleCut) return;
            var cutBoneName = moduleCut.ExecuteCut(boneName, position, force, out var detachedObject);
            if (cutBoneName == string.Empty) return;
            moduleCut.FinalizeExecution();
            InvokeOnExecuteCut(cutBoneName);
            if (goreBoneDict.TryGetValue(cutBoneName, out var goreBone))
                if (goreBone.onDeath)
                    InvokeOnDeath();
        }

        /// <summary>
        ///     Executes a cut at the bone nearest to the given position.
        ///     Please note that this method incurs some overhead due to the bone-finding process.
        /// </summary>
        /// <param name="position">The position at which the cut should be executed.</param>
        /// <param name="detachedObject">
        ///     The detached object in the scene which has the different mesh parts as its children.
        ///     This object should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteCut(Vector3 position, out GameObject detachedObject)
        {
            var boneIndex = CutUtility.FindNearestTransformIndex(bones, position);
            ExecuteCut(bones[boneIndex].name, position, out detachedObject);
        }

        /// <summary>
        ///     Executes a cut on the mesh for the specified bone.
        /// </summary>
        /// <param name="boneName">Name of the bone that should be cut.</param>
        /// <param name="position">Position of the cut. Finds the nearest possible mesh part of that bone.</param>
        /// <param name="detachedObject">
        ///     The detached object in the scene which has the different mesh parts as its children.
        ///     This object should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteCut(string boneName, Vector3 position, out GameObject detachedObject)
        {
            ExecuteCut(boneName, position, Vector3.zero, out detachedObject);
        }

        /// <summary>
        ///     Executes a cut at the bone nearest to the given position.
        ///     Please note that this method incurs some overhead due to the bone-finding process.
        /// </summary>
        /// <param name="position">The position at which the cut should be executed.</param>
        /// <param name="force">Force to be applied to the cut mesh.</param>
        /// <param name="detachedObject">
        ///     The detached object in the scene which has the different mesh parts as its children.
        ///     This object should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteCut(Vector3 position, Vector3 force, out GameObject detachedObject)
        {
            var boneIndex = CutUtility.FindNearestTransformIndex(bones, position);
            ExecuteCut(bones[boneIndex].name, position, force, out detachedObject);
        }

        /// <summary>
        ///     Executes a cut on the mesh for the specified bone.
        /// </summary>
        /// <param name="boneName">Name of the bone that should be cut.</param>
        /// <param name="position">Position of the cut. Finds the nearest possible mesh part of that bone.</param>
        /// <param name="force">Force to be applied to the cut mesh.</param>
        /// <param name="detachedObject">
        ///     The detached object in the scene which has the different mesh parts as its children.
        ///     This object should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteCut(string boneName, Vector3 position, Vector3 force, out GameObject detachedObject)
        {
            detachedObject = null;
            if (!meshCutInitialized) return;
            if (exploded) return;
            if (ExecutionUtility.FindGoreModule<GoreModuleCut>(goreModules) is not GoreModuleCut moduleCut) return;
            var cutBoneName = moduleCut.ExecuteCut(boneName, position, force, out detachedObject);
            if (cutBoneName == string.Empty) return;
            moduleCut.FinalizeExecution();
            InvokeOnExecuteCut(cutBoneName);
            if (goreBoneDict.TryGetValue(cutBoneName, out var goreBone))
                if (goreBone.onDeath)
                    InvokeOnDeath();
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        public void ExecuteExplosion()
        {
            ExecuteExplosion(Vector3.zero, 0);
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="radialForce">
        ///     Radial force to be added to the explosion parts, directed from the character center to each cutted part.
        ///     Note: Requires the physics submodule with rigidbody checked.
        /// </param>
        public void ExecuteExplosion(float radialForce)
        {
            var worldCenter = smr.transform.TransformPoint(smr.sharedMesh.bounds.center);
            ExecuteExplosion(worldCenter, radialForce);
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="position">Explosion center from where a spherical force is applied on the mesh parts.</param>
        /// <param name="force">
        ///     Force to be added to the explosion parts, directed from the position to each cutted part.
        ///     Note: Requires the physics submodule with rigidbody checked.
        /// </param>
        public void ExecuteExplosion(Vector3 position, float force)
        {
            if (!meshCutInitialized) return;
            if (exploded) return;
            if (ExecutionUtility.FindGoreModule<GoreModuleExplosion>(goreModules) is not GoreModuleExplosion goreModuleExplosion) return;
            goreModuleExplosion.ExecuteExplosion(position, force);
            exploded = true;
            goreModuleExplosion.FinalizeExecution();
            InvokeOnExecuteExplosion();
            InvokeOnDeath();
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="explosionParts">
        ///     A list of the explosion parts in the scene.
        ///     These parts should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteExplosion(out List<GameObject> explosionParts)
        {
            ExecuteExplosion(Vector3.zero, 0, out explosionParts);
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="radialForce">
        ///     Radial force to be added to the explosion parts, directed from the character center to each cutted part.
        ///     Note: Requires the physics submodule with rigidbody checked.
        /// </param>
        /// <param name="explosionParts">
        ///     A list of the explosion parts in the scene.
        ///     These parts should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteExplosion(float radialForce, out List<GameObject> explosionParts)
        {
            var worldCenter = smr.transform.TransformPoint(smr.sharedMesh.bounds.center);
            ExecuteExplosion(worldCenter, radialForce, out explosionParts);
        }

        /// <summary>
        ///     Executes an explosion on the mesh.
        /// </summary>
        /// <param name="position">Explosion center from where a spherical force is applied on the mesh parts.</param>
        /// <param name="force">
        ///     Force to be added to the explosion parts, directed from the position to each cutted part.
        ///     Note: Requires the physics submodule with rigidbody checked.
        /// </param>
        /// <param name="explosionParts">
        ///     A list of the explosion parts in the scene.
        ///     These parts should not be destroyed but returned to the pool using the ResetCharacter() or DespawnDetachedObjects() method.
        ///     Alternatively, the 'Auto Despawn' module could be used, or the character itself could simply be destroyed.
        /// </param>
        public void ExecuteExplosion(Vector3 position, float force, out List<GameObject> explosionParts)
        {
            explosionParts = null;
            if (!meshCutInitialized) return;
            if (exploded) return;
            if (ExecutionUtility.FindGoreModule<GoreModuleExplosion>(goreModules) is not GoreModuleExplosion goreModuleExplosion) return;
            explosionParts = goreModuleExplosion.ExecuteExplosion(position, force);
            exploded = true;
            goreModuleExplosion.FinalizeExecution();
            InvokeOnExecuteExplosion();
            InvokeOnDeath();
        }

        /// <summary>
        ///     Activates ragdoll mode for the character. Ensure that the Unity Animator component is deactivated and any
        ///     custom character controllers do not interfere with the physics-based ragdoll behavior.
        /// </summary>
        public void ExecuteRagdoll()
        {
            ExecuteRagdoll(Vector3.zero, string.Empty);
        }

        /// <summary>
        ///     Activates ragdoll mode for the character. Ensure that the Unity Animator component is deactivated and any
        ///     custom character controllers do not interfere with the physics-based ragdoll behavior.
        /// </summary>
        /// <param name="force">Adds a force in world space to the center bone.</param>
        public void ExecuteRagdoll(Vector3 force)
        {
            ExecuteRagdoll(force, string.Empty);
        }

        /// <summary>
        ///     Activates ragdoll mode for the character. Ensure that the Unity Animator component is deactivated and any
        ///     custom character controllers do not interfere with the physics-based ragdoll behavior.
        /// </summary>
        /// <param name="force">Adds a force in world space to the specified bone.</param>
        /// <param name="boneName">Name of the bone to add the force to.</param>
        public void ExecuteRagdoll(Vector3 force, string boneName)
        {
            if (!ragdollInitialized) return;
            if (exploded) return;
            if (ragdollActive) return;
            if (ExecutionUtility.FindGoreModule<GoreModuleRagdoll>(goreModules) is not GoreModuleRagdoll moduleRagdoll) return;
            moduleRagdoll.ExecuteRagdoll(goreBones);
            if (boneName == string.Empty) boneName = center.name;
            if (force != Vector3.zero && goreBoneDict.TryGetValue(boneName, out var goreBone)) goreBone._rigidbody.AddForce(force);
            ragdollActive = true;
            InvokeOnExecuteRagdoll();
        }

        /// <summary>
        ///     Activates ragdoll mode for the character and executes a cut at the specified position.
        /// </summary>
        /// <param name="boneName">Name of the bone that should be cut.</param>
        /// <param name="position">Position of the cut. Finds the nearest possible mesh part of that bone.</param>
        /// <param name="force">Force to be applied to the cut mesh and ragdoll bone.</param>
        public void ExecuteRagdollCut(string boneName, Vector3 position, Vector3 force)
        {
            ExecuteRagdoll(force, boneName);
            ExecuteCut(boneName, position, force);
        }

        /// <summary>
        ///     Resets the character back to its initial state.
        /// </summary>
        public void ResetCharacter()
        {
            ResetCharacterInternal();
        }

        /// <summary>
        ///     Destroys this Gore Simulator component and despawns associated pooled objects.
        ///     You can also call <see cref="GoreSimulatorAPI.SceneCleanup" /> to clean up all active Gore Simulator components at once.
        /// </summary>
        public void SceneCleanup()
        {
            Destroy(this);
        }

        /// <summary>
        ///     Despawns all detached mesh parts and particle effects that have been created by this component, recycling them into the pool.
        /// </summary>
        public void DespawnAllObjects()
        {
            DespawnParticles();
            DespawnDetachedObjects();
        }

        /// <summary>
        ///     Despawns all detached mech parts that have been created by this component, recycling them into the object pool.
        ///     Excluding particles.
        /// </summary>
        public void DespawnDetachedObjects()
        {
            if (!gameObject.scene.isLoaded) return;

            if (_globalSettings.poolActive)
                foreach (var detachedObject in poolableObjects)
                    Pool.Release(detachedObject, GetInstanceID());
            else
                foreach (var detachedObject in poolableObjects)
                {
                    if (detachedObject == null) continue;
                    if (detachedObject.TryGetComponent<GorePoolable>(out var poolable))
                        if (poolable.m_InstanceID == GetInstanceID())
                            Destroy(detachedObject);
                }

            foreach (var ragdollObject in destroyableObjects) Destroy(ragdollObject);
            poolableObjects.Clear();
            destroyableObjects.Clear();
        }

        /// <summary>
        ///     Despawns all active particles that have been created by this component, recycling them into the pool.
        /// </summary>
        public void DespawnParticles()
        {
            if (ExecutionUtility.FindSubModule<SubModuleParticleEffects>(cutModules) is SubModuleParticleEffects subModuleParticleEffectsCut)
                subModuleParticleEffectsCut.ReleaseParticles();
            if (ExecutionUtility.FindSubModule<SubModuleParticleEffects>(explosionModules) is SubModuleParticleEffects subModuleParticleEffectsExpl)
                subModuleParticleEffectsExpl.ReleaseParticles();
        }

        /// <summary>
        ///     Spawns particles from the Cut SubModuleParticleEffects module without executing a cut.
        /// </summary>
        public void SpawnCutParticles(Vector3 position, Vector3 direction)
        {
            if (!meshCutInitialized) return;
            if (ExecutionUtility.FindSubModule<SubModuleParticleEffects>(cutModules) is not SubModuleParticleEffects subModuleParticle) return;
            var subModuleClass = ExecutionClassesUtility.GetPoolSubModuleClass();
            subModuleClass.centerBone = gameObject.transform;
            subModuleClass.cutPosition = position;
            subModuleClass.cutDirection = direction;
            subModuleParticle.ExecuteModuleCut(subModuleClass);
            ExecutionClassesUtility.ReleaseSubModuleClass(subModuleClass);
        }

        /// <summary>
        ///     Records the current hierarchy.
        ///     Required if GameObjects added to the bone hierarchy after game start should be registered.
        /// </summary>
        public void RecordHierarchy()
        {
            nonBoneChildren = BonesUtility.GetChildren(smrBones, smr.rootBone, childrenEnum);
            HierarchyDataUtility.RecordHierarchy(hierarchyDataList, nonBoneChildren);
        }

        /// <summary>
        ///     Restores the last recorded hierarchy. By default, the hierarchy is being recorded once in Awake().
        ///     This method is also being called automatically with <see cref="ResetCharacter" />.
        /// </summary>
        public void RestoreHierarchy()
        {
            HierarchyDataUtility.RestoreHierarchy(hierarchyDataList);
        }

        /// <summary>
        ///     Get all active GameObjects in the scene that have been created by this component.
        /// </summary>
        public List<GameObject> GetCreatedObjects()
        {
            return poolableObjects.Concat(destroyableObjects).ToList();
        }
        
        /// <summary>
        ///     Get the children from the character hierarchy that have been detached and are active in the scene (items etc.).
        /// </summary>
        public List<GameObject> GetDetachedChildren()
        {
            return detachedChildren.Concat(destroyableObjects).ToList();
        }

        /// <summary>
        ///     Get all active Particle Systems in the scene that have been created by this component.
        /// </summary>
        public List<GameObject> GetActiveParticles()
        {
            var activeParticles = new List<GameObject>();
            activeParticles.AddRange(GetActiveCutParticles());
            activeParticles.AddRange(GetActiveExplosionParticles());
            return activeParticles;
        }

        /// <summary>
        ///     Get all active Particle Systems in the scene that have been created by the Cut Module.
        /// </summary>
        public List<GameObject> GetActiveCutParticles()
        {
            var activeParticles = new List<GameObject>();
            if (ExecutionUtility.FindSubModule<SubModuleParticleEffects>(cutModules) is SubModuleParticleEffects cutParticles)
                activeParticles.AddRange(cutParticles.activeSystems);
            return activeParticles;
        }

        /// <summary>
        ///     Get all active Particle Systems in the scene that have been created by the Explosion Module.
        /// </summary>
        public List<GameObject> GetActiveExplosionParticles()
        {
            var activeParticles = new List<GameObject>();
            if (ExecutionUtility.FindSubModule<SubModuleParticleEffects>(explosionModules) is SubModuleParticleEffects explosionParticles)
                activeParticles.AddRange(explosionParticles.activeSystems);
            return activeParticles;
        }

        /// <summary>
        ///     Get a main gore module from this component.
        /// </summary>
        public T GetGoreModule<T>() where T : GoreModuleBase
        {
            foreach (var goreModule in goreModules)
                if (goreModule is T module)
                    return module;
            return null;
        }

        /// <summary>
        ///     Get a cut sub-module from this component.
        /// </summary>
        public T GetSubModuleCut<T>() where T : SubModuleBase
        {
            foreach (var subModuleCut in cutModules)
                if (subModuleCut is T module)
                    return module;
            return null;
        }

        /// <summary>
        ///     Get an explosion sub-module from this component.
        /// </summary>
        public T GetSubModuleExplosion<T>() where T : SubModuleBase
        {
            foreach (var subModuleExplosion in explosionModules)
                if (subModuleExplosion is T module)
                    return module;
            return null;
        }

        /// <summary>
        ///     Get a ragdoll sub-module from this component.
        /// </summary>
        public T GetSubModuleRagdoll<T>() where T : SubModuleBase
        {
            foreach (var subModuleRagdoll in ragdollModules)
                if (subModuleRagdoll is T module)
                    return module;
            return null;
        }

        /// <summary>
        ///     Activates/deactivates a sub-module in the <see cref="GoreModuleCut" /> module.
        /// </summary>
        /// <param name="index">Index of the submodule.</param>
        /// <param name="active">Set active or inactive.</param>
        public void SetSubModuleCutActive(int index, bool active)
        {
            if (!meshCutInitialized) return;
            if (index < 0) return;
            if (index >= cutModules.Count) return;
            cutModules[index].moduleActive = active;
        }

        /// <summary>
        ///     Activates/deactivates a sub-module in the <see cref="GoreModuleCut" /> module.
        /// </summary>
        /// <param name="subModuleType">Type of the submodule.</param>
        /// <param name="active">Set active or inactive.</param>
        public void SetSubModuleCutActive(Type subModuleType, bool active)
        {
            if (!meshCutInitialized) return;
            var subModules = cutModules
                .Where(module => module.GetType() == subModuleType || module.GetType().IsSubclassOf(subModuleType))
                .ToList();
            foreach (var subModule in subModules) subModule.moduleActive = active;
        }

        /// <summary>
        ///     Activates/deactivates a sub-module in the <see cref="GoreModuleExplosion" /> module.
        /// </summary>
        /// <param name="index">Index of the submodule.</param>
        /// <param name="active">Set active or inactive.</param>
        public void SetSubModuleExplosionActive(int index, bool active)
        {
            if (!meshCutInitialized) return;
            if (index < 0) return;
            if (index >= explosionModules.Count) return;
            explosionModules[index].moduleActive = active;
        }

        /// <summary>
        ///     Activates/deactivates a sub-module in the <see cref="GoreModuleExplosion" /> module.
        /// </summary>
        /// <param name="subModuleType">Type of the submodule.</param>
        /// <param name="active">Set active or inactive.</param>
        public void SetSubModuleExplosionActive(Type subModuleType, bool active)
        {
            if (!meshCutInitialized) return;
            var subModules = explosionModules
                .Where(module => module.GetType() == subModuleType || module.GetType().IsSubclassOf(subModuleType))
                .ToList();
            foreach (var subModule in subModules) subModule.moduleActive = active;
        }

        /// <summary>
        ///     Activates/deactivates a sub-module in the <see cref="GoreModuleRagdoll" /> module.
        /// </summary>
        /// <param name="index">Index of the submodule.</param>
        /// <param name="active">Set active or inactive.</param>
        public void SetSubModuleRagdollActive(int index, bool active)
        {
            if (!ragdollInitialized) return;
            if (index < 0) return;
            if (index >= ragdollModules.Count) return;
            ragdollModules[index].moduleActive = active;
        }

        /// <summary>
        ///     Overwrites the material colors for this component.
        /// </summary>
        public void SetComponentColor()
        {
            if (componentColor.r == 0f && componentColor.g == 0f && componentColor.b == 0f) return;
            SetComponentColor(componentColor);
        }

        /// <summary>
        ///     Overwrites the material colors for this component.
        /// </summary>
        /// <param name="color">Color value.</param>
        public void SetComponentColor(Color color)
        {
            foreach (var keyword in colorKeywordIDs)
            {
                if (cutMaterial.HasProperty(keyword)) cutMaterial.SetColor(keyword, color);
                if (decalMaterial.HasProperty(keyword)) decalMaterial.SetColor(keyword, color);
            }
        }

        /********************************************************************************************************************************/
        /********************************************************************************************************************************/

        private void InvokeOnExecute()
        {
            OnExecute();
            if (onExecuteClass.activated) onExecuteClass.unityEvent.Invoke();
        }

        private void InvokeOnExecuteCut(string boneName)
        {
            OnExecuteCut(boneName);
            InvokeOnExecute();
            if (onExecuteCutClass.activated) onExecuteCutClass.unityEvent.Invoke();
        }

        private void InvokeOnExecuteExplosion()
        {
            OnExecuteExplosion();
            InvokeOnExecute();
            if (onExecuteExplosionClass.activated) onExecuteExplosionClass.unityEvent.Invoke();
        }

        private void InvokeOnExecuteRagdoll()
        {
            OnExecuteRagdoll();
            InvokeOnExecute();
            if (onExecuteRagdollClass.activated) onExecuteRagdollClass.unityEvent.Invoke();
        }

        private void InvokeOnCharacterReset()
        {
            OnCharacterReset();
            if (onCharacterReset.activated) onCharacterReset.unityEvent.Invoke();
        }

        internal void InvokeOnDeath()
        {
            OnDeath();
            if (onDeath.activated) onDeath.unityEvent.Invoke();
        }

        private void InvokeOnDestroy()
        {
            OnDestroyAction();
            if (onDestroy.activated) onDestroy.unityEvent.Invoke();
        }

        /********************************************************************************************************************************/

        private void OnDestroy()
        {
            for (var i = 0; i < goreModules.Count; i++) goreModules[i].Destroyed();
            OnDestroyMeshCut();
            OnDestroyRagdoll();
            DespawnAllObjects();
            if(destroyChildren) foreach(var child in detachedChildren) if(child != null) Destroy(child);
            GoreSimulatorAPI.RemoveGoreSimulator(this);
            InvokeOnDestroy();
        }

        private void OnDestroyMeshCut()
        {
            if (!meshCutInitialized) return;
            for (var i = 0; i < cutModules.Count; i++) cutModules[i].Destroyed();
            for (var i = 0; i < explosionModules.Count; i++) explosionModules[i].Destroyed();
            meshNativeDataClass.DisposeRuntimeMeshData();
        }

        private void OnDestroyRagdoll()
        {
            if (!ragdollInitialized) return;
            for (var i = 0; i < ragdollModules.Count; i++) ragdollModules[i].Destroyed();
        }
    }
}