// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PampelGames.Shared.Tools;
using PampelGames.Shared.Utility;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace PampelGames.GoreSimulator
{
    public class SubModuleParticleEffects : SubModuleBase
    {
        public override string ModuleName()
        {
            return "Spawn Effects";
        }

        public override string ModuleInfo()
        {
            return "Particle spawner using an automated, shared pool.";
        }

        public override int imageIndex()
        {
            return 7;
        }

        public override void ModuleAdded(Type type)
        {
            base.ModuleAdded(type);
            addedType = type.Name;
            particleClasses ??= new List<ParticleWrapperClass>();
            particleClasses.Add(new ParticleWrapperClass());
#if UNITY_EDITOR
            if (type == typeof(GoreModuleCut))
                particleClasses[^1].particle = _goreSimulator._defaultReferences.cutParticle;
            else if (type == typeof(GoreModuleExplosion)) particleClasses[^1].particle = _goreSimulator._defaultReferences.explosionParticle;
#endif
        }

        public override bool CompatibleRagdoll()
        {
            return false;
        }

        /********************************************************************************************************************************/

        public List<ParticleWrapperClass> particleClasses = new();
        public string addedType;
        
        internal readonly List<GameObject> activeSystems = new();
        
        [Serializable]
        public class ParticleWrapperClass
        {
            public Enums.SpawnType spawnType = Enums.SpawnType.ParticleSystem;
            public GameObject effect;
            public ParticleSystem particle;
            public bool autoDespawn = true;
            public float autoDespawnTimer = 5f;
            public Enums.ParticlePositionExpl positionExpl = Enums.ParticlePositionExpl.Center;
            public int maxPosition = 5;
            public bool setParentExplosionParts = true;
            public Enums.ParticleRotationExpl rotationExpl = Enums.ParticleRotationExpl.Method;
            public Enums.ParticleRotationCut rotationCut = Enums.ParticleRotationCut.CutDirection;
            public Enums.ParticleSetParent setParent = Enums.ParticleSetParent.Character;
        }
        
        

        /********************************************************************************************************************************/

        public override void Initialize()
        {
#if UNITY_EDITOR
            for (int i = 0; i < particleClasses.Count; i++)
            {
                if(particleClasses[i].spawnType == Enums.SpawnType.ParticleSystem && particleClasses[i].particle == null)
                {
                    Debug.LogError("Particle System missing on Gore Simulator: " + _goreSimulator.gameObject.name);
                    return;
                }
                if(particleClasses[i].spawnType == Enums.SpawnType.Gameobject && particleClasses[i].effect == null)
                {
                    Debug.LogError("Effect missing on Gore Simulator: " + _goreSimulator.gameObject.name);
                    return;
                }
            }
#endif

            base.Initialize();

            if(_goreSimulator._globalSettings.poolActive)
            {
                for (var i = 0; i < particleClasses.Count; i++)
                {
                    if (particleClasses[i].spawnType == Enums.SpawnType.ParticleSystem)
                    {
                        InitializePool(particleClasses[i].particle.gameObject, _goreSimulator._globalSettings.particlePreload,
                            _goreSimulator._globalSettings.particleLimited);
                    }
                    else
                    {
                        InitializePool(particleClasses[i].effect, _goreSimulator._globalSettings.particlePreload,
                            _goreSimulator._globalSettings.particleLimited);
                    }
                }
            }
            
        }
        
        
        /********************************************************************************************************************************/

        public override void ExecuteModuleCut(SubModuleClass subModuleClass)
        {

            var position = subModuleClass.cutPosition;
            
            for (var i = 0; i < particleClasses.Count; i++)
            {
                var rotation = GetRotationCut(particleClasses[i], subModuleClass);

                if (particleClasses[i].rotationCut == Enums.ParticleRotationCut.CutDirection)
                {
                    if(subModuleClass.multiCut) continue;

                    if (particleClasses[i].setParent == Enums.ParticleSetParent.None)
                    {
                        var pooledObj = GetSpawnObject(i, position, rotation);
                        ProcessSpawnedObject(pooledObj, i);
                    }
                    else if (particleClasses[i].setParent == Enums.ParticleSetParent.Both ||
                             particleClasses[i].setParent == Enums.ParticleSetParent.Character)
                    {
                        var pooledObj = GetSpawnObject(i, position, rotation);
                        pooledObj.transform.SetParent(subModuleClass.centerBone);  
                        ProcessSpawnedObject(pooledObj, i);
                    }
                    if (particleClasses[i].setParent == Enums.ParticleSetParent.Both ||
                        particleClasses[i].setParent == Enums.ParticleSetParent.DetachedPart)
                    {
                        Quaternion rotation2 = rotation;
                        if (subModuleClass.cutDirection != Vector3.zero) rotation2 = Quaternion.LookRotation(subModuleClass.cutDirection * -1);
                        var pooledObj2 = GetSpawnObject(i, position, rotation2);
                    
                        pooledObj2.transform.SetParent(subModuleClass.subRagdoll
                            ? ((SkinnedMeshRenderer) subModuleClass.subModuleObjectClasses[0].renderer).rootBone
                            : subModuleClass.parent.transform);
                        ProcessSpawnedObject(pooledObj2, i);
                    }
                }
                else
                {
                    var pooledObj = GetSpawnObject(i, position, rotation);
                    ProcessSpawnedObject(pooledObj, i);
                }
            }
        }

        public override void ExecuteModuleExplosion(SubModuleClass subModuleClass)
        {
            for (var i = 0; i < particleClasses.Count; i++)
                if (particleClasses[i].positionExpl == Enums.ParticlePositionExpl.Center)
                {
                    var rotation = GetRotationExplosion(particleClasses[i], subModuleClass.subModuleObjectClasses[0]);
                    var pooledObj = GetSpawnObject(i, subModuleClass.centerPosition, rotation);
                    ProcessSpawnedObject(pooledObj, i);
                }
                else if (particleClasses[i].positionExpl == Enums.ParticlePositionExpl.Method)
                {
                    var rotation = GetRotationExplosion(particleClasses[i], subModuleClass.subModuleObjectClasses[0]);
                    var pooledObj = GetSpawnObject(i, subModuleClass.position, rotation);
                    ProcessSpawnedObject(pooledObj, i);
                }
                else
                {
                    int maxAmount = particleClasses[i].maxPosition;
                    int stepSize = Math.Max(1, subModuleClass.subModuleObjectClasses.Count / maxAmount);

                    for (int j = 0; j < maxAmount && j * stepSize < subModuleClass.subModuleObjectClasses.Count; j++)
                    {
                        var subModuleObjClass = subModuleClass.subModuleObjectClasses[j * stepSize];
                        var rotation = GetRotationExplosion(particleClasses[i], subModuleObjClass);

                        var pooledObj = GetSpawnObject(i, subModuleObjClass.centerPosition, rotation);
                        
                        if (particleClasses[i].setParentExplosionParts)
                        {
                            pooledObj.transform.SetParent(subModuleObjClass.obj.transform);
                        }
                        ProcessSpawnedObject(pooledObj, i);
                    }
                }
        }

        /********************************************************************************************************************************/
        
        private GameObject GetSpawnObject(int index, Vector3 position, Quaternion rotation)
        {
            GameObject pooledObj = default;
            if(_goreSimulator._globalSettings.poolActive)
            {
                if (particleClasses[index].spawnType == Enums.SpawnType.ParticleSystem)
                    pooledObj = PGPool.Get(particleClasses[index].particle.gameObject);
                else
                    pooledObj = PGPool.Get(particleClasses[index].effect);
            }
            else
            {
                if (particleClasses[index].spawnType == Enums.SpawnType.ParticleSystem)
                {
                    pooledObj = Object.Instantiate(particleClasses[index].particle.gameObject);
                }
                else
                    pooledObj = Object.Instantiate(particleClasses[index].effect);
                
                pooledObj.AddComponent<PGEmptyComponent>();
            }
            
            pooledObj.transform.SetPositionAndRotation(position, rotation);
            activeSystems.Add(pooledObj);

            return pooledObj;
        }
        private void ProcessSpawnedObject(GameObject obj, int particleClassesIndex)
        {
            var particleWrapperClass = particleClasses[particleClassesIndex];
            
            if (particleWrapperClass.spawnType == Enums.SpawnType.ParticleSystem)
            {
                if(particleWrapperClass.autoDespawn)
                {
                    if (!obj.TryGetComponent<PGPoolableParticles>(out var pgPoolableParticles))
                    {
                        pgPoolableParticles = obj.AddComponent<PGPoolableParticles>();
                        if (pgPoolableParticles.TryGetComponent<ParticleSystem>(out var particleSystem))
                            pgPoolableParticles._particleSystem = particleSystem;
                    }

                    pgPoolableParticles.poolActive = _goreSimulator._globalSettings.poolActive;
                    pgPoolableParticles._particleSystem.Play();
                }
                else
                {
                    if (obj.TryGetComponent<ParticleSystem>(out var particleSystem))
                        particleSystem.Play();
                }
            }
            else
            {
                if (particleWrapperClass.autoDespawn)
                {
                    PGScheduler.ScheduleTime(obj.GetComponent<MonoBehaviour>(), particleWrapperClass.autoDespawnTimer, () =>
                    {
                        if (activeSystems.Contains(obj)) activeSystems.Remove(obj);
                        if(_goreSimulator._globalSettings.poolActive) PGPool.Release(obj);
                        else Object.Destroy(obj);
                    });
                }
            }
        }

        /********************************************************************************************************************************/
        
        public override void ExecuteModuleRagdoll(List<GoreBone> goreBones)
        {
        }

        public override void Reset()
        {
            base.Reset();
            ReleaseParticles();
            
            
        }

        internal void ReleaseParticles()
        {
            if (!_goreSimulator.gameObject.scene.isLoaded) return;
            
            if(_goreSimulator._globalSettings.poolActive)
            {
                for (var i = activeSystems.Count - 1; i >= 0; i--)
                {
                    if(!activeSystems[i].scene.isLoaded) continue;
                    PGPool.Release(activeSystems[i]);
                }
            } 
            else
            {
                for (var i = activeSystems.Count - 1; i >= 0; i--) Object.Destroy(activeSystems[i]);
            }
            activeSystems.Clear();
        }

        /********************************************************************************************************************************/

        private Quaternion GetRotationCut(ParticleWrapperClass particleClass, SubModuleClass subModuleClass)
        {
            if (particleClass.rotationCut == Enums.ParticleRotationCut.CutDirection)
            {
                if (subModuleClass.cutDirection == Vector3.zero) return Quaternion.identity; 
                return Quaternion.LookRotation(subModuleClass.cutDirection);
            }
            if (particleClass.rotationCut == Enums.ParticleRotationCut.Method)
            {
                if (subModuleClass.force == Vector3.zero) return Quaternion.identity; 
                return Quaternion.LookRotation(subModuleClass.force);
            }           
            if (particleClass.rotationCut == Enums.ParticleRotationCut.Default) return Quaternion.identity;
            return PGMathUtility.GetRandomRotation();
        }
        
        private Quaternion GetRotationExplosion(ParticleWrapperClass particleClass, SubModuleObjectClass subModuleObjectClass)
        {
            if (particleClass.rotationExpl == Enums.ParticleRotationExpl.Method)
            {
                if (subModuleObjectClass.force == Vector3.zero) return Quaternion.identity; 
                return Quaternion.LookRotation(subModuleObjectClass.force);
            }
            if (particleClass.rotationExpl == Enums.ParticleRotationExpl.Default) return Quaternion.identity;
            return PGMathUtility.GetRandomRotation();
        }


        /********************************************************************************************************************************/
        // Pool

        private static GameObject[] InitializePool(GameObject prefab, int preloadAmount, bool limited)
        {
            var pool = PGPool.TryGetExistingPool(prefab) ?? new ObjectPool<GameObject>(
                () => CreateSetup(prefab),
                GetSetup,
                ReleaseSetup,
                DestroySetup,
                true,
                preloadAmount);
            
            return PGPool.Preload(prefab, pool, preloadAmount, limited);
        }

        private static GameObject CreateSetup(GameObject prefab)
        {
            var obj = Object.Instantiate(prefab);
            return obj;
        }

        private static void GetSetup(GameObject obj)
        {
#if UNITY_EDITOR
            if (obj == null)
            {
                DebugHandler.EmptyPooledObject();
            }
            if (SO_GlobalSettings.Instance.hidePooledObjects) obj.hideFlags = HideFlags.None;
#endif
            obj.SetActive(true);
        }

        private static void ReleaseSetup(GameObject obj)
        {
#if UNITY_EDITOR
            if (obj == null)
            {
                DebugHandler.EmptyPooledObject();
            }
            if (SO_GlobalSettings.Instance.hidePooledObjects) obj.hideFlags = HideFlags.HideInHierarchy;
#endif
            obj.transform.SetParent(null);
            obj.SetActive(false);
            Object.DontDestroyOnLoad(obj);
        }

        private static void DestroySetup(GameObject obj)
        {
            Object.Destroy(obj);
        }
    }
}