// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    public class GoreMultiCut : MonoBehaviour, IGoreObjectParent
    {
        
        public GoreSimulator _goreSimulator;
        public BonesClass bonesClass;
        public Mesh bakedMesh;
        
        internal Enums.MultiCutStatus status = Enums.MultiCutStatus.Mesh;
        private Dictionary<string, List<MultiCutChildClass>> childrenDict;


        public void ExecuteCut(string boneName, Vector3 position)
        {
            ExecuteCut(boneName, position, Vector3.zero);
        }
        
        public void ExecuteCut(string boneName, Vector3 position, Vector3 force)
        {
            ExecuteCutInternal(boneName, position, force, out var detachedObject);
        }

        public void ExecuteCut(string boneName, Vector3 position, out GameObject detachedObject)
        {
            ExecuteCut(boneName, position, Vector3.zero, out detachedObject);
        }

        public void ExecuteCut(string boneName, Vector3 position, Vector3 force, out GameObject detachedObject)
        {
            ExecuteCutInternal(boneName, position, force, out detachedObject);
        }

        public void ExecuteExplosion()
        {
            PrepareRagdollExplosion();
            var centerPosition = CalculateCenterPosition();
            ExecuteExplosionInternal(Vector3.zero, 0, centerPosition);
        }

        public void ExecuteExplosion(float radialForce)
        {
            PrepareRagdollExplosion();
            var centerPosition = CalculateCenterPosition();
            ExecuteExplosionInternal(centerPosition, radialForce, centerPosition);
        }

        public void ExecuteExplosion(Vector3 position, float force)
        {
            PrepareRagdollExplosion();
            var centerPosition = CalculateCenterPosition();
            ExecuteExplosionInternal(position, force, centerPosition);
        }

        public void ExecuteExplosion(out List<GameObject> explosionParts)
        {
            PrepareRagdollExplosion();
            var centerPosition = CalculateCenterPosition();
            explosionParts = ExecuteExplosionInternal(Vector3.zero, 0, centerPosition);
        }

        public void ExecuteExplosion(float radialForce, out List<GameObject> explosionParts)
        {
            PrepareRagdollExplosion();
            var centerPosition = CalculateCenterPosition();
            explosionParts = ExecuteExplosionInternal(centerPosition, radialForce, centerPosition);
        }

        public void ExecuteExplosion(Vector3 position, float force, out List<GameObject> explosionParts)
        {
            PrepareRagdollExplosion();
            var centerPosition = CalculateCenterPosition();
            explosionParts = ExecuteExplosionInternal(position, force, centerPosition);
        }

        public void SpawnCutParticles(Vector3 position, Vector3 direction)
        {
            _goreSimulator.SpawnCutParticles(position, direction);
        }

        /********************************************************************************************************************************/

        private void PrepareRagdollExplosion()
        {
            if (status == Enums.MultiCutStatus.Ragdoll)
            {
                CreateChildObjects();
            }
        }

        private List<GameObject> ExecuteExplosionInternal(Vector3 position, float force, Vector3 centerPosition)
        {
            List<GameObject> detachedObjects = new List<GameObject>();
            
            var subModuleClass = ExecutionClassesUtility.GetPoolSubModuleClass();
            subModuleClass.multiCut = true;
            subModuleClass.cutPosition = position;
            subModuleClass.centerPosition = centerPosition;
            subModuleClass.parent = gameObject;
            subModuleClass.position = position;
            subModuleClass.subModuleObjectClasses.Clear();
            
            if (status == Enums.MultiCutStatus.Ragdoll)
            {
                subModuleClass.multiCut = false;
                subModuleClass.subRagdoll = true;
                status = Enums.MultiCutStatus.Mesh;
            }

            float mass = 1f;
            if (gameObject.TryGetComponent<Rigidbody>(out var rigid)) mass = rigid.mass;
            
            foreach (KeyValuePair<string, List<MultiCutChildClass>> kvp in childrenDict)
            {
                foreach (MultiCutChildClass child in kvp.Value)
                {
                    SubModuleObjectClass subModuleObjectClass = new SubModuleObjectClass();
                    subModuleObjectClass.obj = child.childObject;
                    detachedObjects.Add(subModuleObjectClass.obj);
                    subModuleObjectClass.renderer = child.childObject.GetComponent<Renderer>();
                    subModuleObjectClass.mesh = child.chunkClass.mesh;
                    Vector3 worldCenter = subModuleObjectClass.obj.transform.TransformPoint(subModuleObjectClass.mesh.bounds.center);
                    subModuleObjectClass.centerPosition = worldCenter;
                    if (force != 0)
                    {
                        subModuleObjectClass.force = (worldCenter - position).normalized * force;    
                    }

                    subModuleObjectClass.mass = mass;
                    subModuleClass.subModuleObjectClasses.Add(subModuleObjectClass);
                    if(child.childObject.TryGetComponent<GoreMesh>(out var goreMesh)) Destroy(goreMesh);
                    child.childObject.transform.SetParent(null);
                }
            }

            for (var k = 0; k < _goreSimulator.explosionModules.Count; k++)
            {
                if(!_goreSimulator.explosionModules[k].moduleActive) continue;
                _goreSimulator.explosionModules[k].ExecuteModuleExplosion(subModuleClass);
            }

            ExecutionClassesUtility.ReleaseSubModuleClass(subModuleClass);
            Destroy(gameObject);
            return detachedObjects;
        }
        
        
        private void ExecuteCutInternal(string boneName, Vector3 position, Vector3 force, out GameObject detachedObject)
        {
            detachedObject = null;
            if (childrenDict.Count == 0) return;

            var subModuleClass = ExecutionClassesUtility.GetPoolSubModuleClass();
            subModuleClass.multiCut = true;
            subModuleClass.cutPosition = position;
            subModuleClass.force = force;
            
            if (status == Enums.MultiCutStatus.Ragdoll)
            {
                CreateChildObjects();
                status = Enums.MultiCutStatus.Mesh;
                subModuleClass.multiCut = false;
            }
            
            subModuleClass.centerMesh = bakedMesh;
            

            List<MultiCutChildClass> children01 = new List<MultiCutChildClass>();
            List<MultiCutChildClass> children02 = new List<MultiCutChildClass>();

            if (!childrenDict.TryGetValue(boneName, out var multiCutChildClassList)) return;
            if (multiCutChildClassList.Count == 0) return;
            childrenDict.Remove(boneName);

            var copiedDict = new Dictionary<string, List<MultiCutChildClass>>(childrenDict);
            
            /********************************************************************************************************************************/
            
            MultiCutChildClass closestMultiCut = null;
            float closestDistance = float.MaxValue;

            foreach (var multiCutChildClass in multiCutChildClassList) 
            {
                var world = multiCutChildClass.childObject.transform.TransformPoint(multiCutChildClass.chunkClass.mesh.bounds.center);
                float distance = (world - position).sqrMagnitude; 

                if (distance < closestDistance)
                {
                    closestMultiCut = multiCutChildClass;
                    closestDistance = distance;
                }
                
                children01.Add(multiCutChildClass);
            }
            
            AddChunkChildren(children01, boneName, childrenDict);
            children02.AddRange(childrenDict.SelectMany(pair => pair.Value));
            
            /********************************************************************************************************************************/
            // Going through all the possibilities for assigning children01 and children02.

            if (children01.Count == 1) return;
            
            if (children02.Count == 0)
            {
                if (children01.Count == 2)
                {
                    children02.Add(children01[1]);
                    children01.RemoveAt(1);
                }
                else if (multiCutChildClassList.Count > 1 && closestMultiCut.chunkClass.cutIndexClassIndex == 0)
                {
                    children02.Add(closestMultiCut);
                    children01.Remove(closestMultiCut);
                }
                else if (multiCutChildClassList.Count > 1 && closestMultiCut.chunkClass.cutIndexClassIndex != 0)
                {
                    for (int i = children01.Count - 1; i >= 0; i--)
                    {
                        if(children01[i] == closestMultiCut) continue;
                        if (children01[i].chunkClass.boneName == boneName)
                        {
                            if (children01[i].chunkClass.cutIndexClassIndex < closestMultiCut.chunkClass.cutIndexClassIndex)
                            {
                                children02.Add(children01[i]);
                                children01.RemoveAt(i);
                            }
                        }
                    }
                }
                else // Going through children, detach the nearest one.
                {
                    MultiCutChildClass closestChildMultiCut = null;
                    float closestChildDistance = float.MaxValue;

                    for (int i = 0; i < children01.Count; i++)
                    {
                        if(children01[i].chunkClass.boneName == boneName) continue;
                        
                        var world = children01[i].childObject.transform.TransformPoint(children01[i].chunkClass.mesh.bounds.center);
                        float distance = (world - position).sqrMagnitude; 

                        if (distance < closestChildDistance)
                        {
                            closestChildMultiCut = children01[i];
                            closestChildDistance = distance;
                        }
                    }

                    if (!copiedDict.TryGetValue(closestChildMultiCut.chunkClass.boneName, out var newMultiCutChildClassList)) return;
                    if (newMultiCutChildClassList.Count == 0) return;
                    
                    children02.AddRange(newMultiCutChildClassList);
                    AddChunkChildren(children02, closestChildMultiCut.chunkClass.boneName, copiedDict);

                    for (int i = 0; i < children02.Count; i++) children01.Remove(children02[i]);
                }
            }
            
            /********************************************************************************************************************************/
            
            if (children01.Count < children02.Count)
            {
                (children01, children02) = (children02, children01);
            }

            if (children02.Count > 0)
            {
                var worldBounds01 = children01[0].childObject.transform.TransformDirection(children01[0].chunkClass.mesh.bounds.center);
                var worldBounds02 = children02[0].childObject.transform.TransformDirection(children02[0].chunkClass.mesh.bounds.center);
                subModuleClass.cutDirection = worldBounds02 - worldBounds01;
            }
            
            /********************************************************************************************************************************/
            // New 
            
            float mass = 1f;
            if (gameObject.TryGetComponent<Rigidbody>(out var rigid)) mass = rigid.mass;
            
            var thisGameObject = gameObject;

            if(children02.Count > 0)
            {
                detachedObject = new GameObject(thisGameObject.name + " - Cut");
                detachedObject.transform.SetPositionAndRotation(thisGameObject.transform.position, thisGameObject.transform.rotation);
                subModuleClass.parent = detachedObject;

                var goreMultiCut = detachedObject.AddComponent<GoreMultiCut>();
                goreMultiCut._goreSimulator = _goreSimulator;
                goreMultiCut.bonesClass = bonesClass;
                goreMultiCut.bakedMesh = bakedMesh;
                goreMultiCut.CreateMultiCutChildClassDict(children02);

                for (var i = 0; i < children02.Count; i++)
                {
                    if (children02[i].childObject.TryGetComponent<GoreMesh>(out var goreMesh)) goreMesh._goreMultiCut = goreMultiCut;
                    SubModuleObjectClass subModuleObjectClass = new SubModuleObjectClass();
                    subModuleObjectClass.obj = children02[i].childObject;
                    subModuleObjectClass.renderer = children02[i].childObject.GetComponent<Renderer>();
                    subModuleObjectClass.mesh = children02[i].chunkClass.mesh;
                    subModuleObjectClass.obj.transform.SetParent(detachedObject.transform);
                    subModuleObjectClass.boundsSize = children02[i].chunkClass.boundsSize;
                    subModuleObjectClass.mass = mass;
                    subModuleClass.subModuleObjectClasses.Add(subModuleObjectClass);
                }

                for (var k = 0; k < _goreSimulator.cutModules.Count; k++)
                {
                    if(!_goreSimulator.cutModules[k].moduleActive) continue;
                    _goreSimulator.cutModules[k].ExecuteModuleCut(subModuleClass);
                }

                List<GameObject> destroyableObjects = new List<GameObject>();
                destroyableObjects.Add(detachedObject);
                List<GameObject> poolableObjects = new List<GameObject>();
                for (var k = 0; k < _goreSimulator.cutModules.Count; k++)
                {
                    if(!_goreSimulator.cutModules[k].moduleActive) continue;
                    _goreSimulator.cutModules[k].FinalizeExecution(poolableObjects, destroyableObjects);
                }
                
                _goreSimulator.AddDestroyableObject(detachedObject);
            }
            
            /********************************************************************************************************************************/
            // Existing
            
            subModuleClass.parent = gameObject;
            subModuleClass.subModuleObjectClasses.Clear();
            for (var i = 0; i < children01.Count; i++)
            {
                SubModuleObjectClass subModuleObjectClass = new SubModuleObjectClass();
                subModuleObjectClass.obj = children01[i].childObject;
                subModuleObjectClass.renderer = children01[i].childObject.GetComponent<Renderer>();
                subModuleObjectClass.mesh = children01[i].chunkClass.mesh;
                subModuleObjectClass.mesh.RecalculateNormals();
                subModuleObjectClass.boundsSize = children01[i].chunkClass.boundsSize;
                subModuleObjectClass.mass = mass;
                subModuleClass.subModuleObjectClasses.Add(subModuleObjectClass);
            }
            
            for (var k = 0; k < _goreSimulator.cutModules.Count; k++)
            {
                _goreSimulator.cutModules[k].ExecuteModuleCut(subModuleClass);
            }
            
            CreateMultiCutChildClassDict(children01);
            
            ExecutionClassesUtility.ReleaseSubModuleClass(subModuleClass);
            
            detachedObject = subModuleClass.parent;
        }
        
        /********************************************************************************************************************************/

        private void CreateChildObjects()
        {
            // ElementAt returns random value from the dict. Used when created with sub-ragdoll.
            var randomItem = childrenDict.ElementAt(0);
            var childRenderer = randomItem.Value[0].childObject.GetComponent<SkinnedMeshRenderer>();
            var childRendererTransform = childRenderer.transform;

            childRenderer.sharedMesh = _goreSimulator.originalMesh;
            childRenderer.BakeMesh(bakedMesh);
            var bakedVertices = PGMeshUtility.CreateVertexList(bakedMesh);

            var transform1 = transform;
            List<GameObject> currentChildren = new List<GameObject>(transform1.childCount);
            foreach (Transform child in transform) currentChildren.Add(child.gameObject);

            foreach (KeyValuePair<string, List<MultiCutChildClass>> kvp in childrenDict)
            {
                foreach (MultiCutChildClass multiCutChildClass in kvp.Value)
                {
                    MeshCutJobs.IndexesSnapshotExplosion(transform, multiCutChildClass.chunkClass, bakedVertices);

                    var detachedChild = ObjectCreationUtility.CreateMeshObject(_goreSimulator, multiCutChildClass.chunkClass.mesh,
                        gameObject.name + " - " + multiCutChildClass.chunkClass.boneName + " - " + multiCutChildClass.chunkClass.cutIndexClassIndex);
                    detachedChild.transform.SetPositionAndRotation(childRendererTransform.position, childRendererTransform.rotation);
                    if (detachedChild.TryGetComponent<Renderer>(out var _renderer)) _renderer.materials = childRenderer.materials;
                    detachedChild.transform.SetParent(transform1);
                    multiCutChildClass.childObject = detachedChild;
                    multiCutChildClass.chunkClass.mesh.RecalculateBounds();
                    var goreMesh = detachedChild.AddComponent<GoreMesh>();
                    goreMesh._boneName = multiCutChildClass.chunkClass.boneName;
                    goreMesh._goreMultiCut = this;
                }
            }

            foreach (var currentChild in currentChildren) Destroy(currentChild);
        }

        private void AddChunkChildren(List<MultiCutChildClass> multiCutChildClasses, string boneName, Dictionary<string, List<MultiCutChildClass>> dictionary)
        {
            if(!_goreSimulator.bonesDict.TryGetValue(boneName, out var tuple)) return;
            for (int i = 0; i < tuple.Item1.boneChildrenSel.Count; i++)
            {
                if (!dictionary.TryGetValue(tuple.Item1.boneChildrenSel[i].name, out var childMultiCutChildClasses)) continue;
                multiCutChildClasses.AddRange(childMultiCutChildClasses);
                dictionary.Remove(tuple.Item1.boneChildrenSel[i].name);
            }
        }

        private Vector3 CalculateCenterPosition()
        {
            List<Vector3> worldCenters = new List<Vector3>();
            foreach (KeyValuePair<string, List<MultiCutChildClass>> kvp in childrenDict)
            {
                foreach (MultiCutChildClass child in kvp.Value)
                {
                    child.chunkClass.mesh.RecalculateBounds();
                    var localBoundsCenter = child.chunkClass.mesh.bounds.center;
                    worldCenters.Add(child.childObject.transform.TransformPoint(localBoundsCenter));
                }
            }

            if (worldCenters.Count == 0) return Vector3.zero;
            var avgX = worldCenters.Average(pos => pos.x);
            var avgY = worldCenters.Average(pos => pos.y);
            var avgZ = worldCenters.Average(pos => pos.z);

            return new Vector3(avgX, avgY, avgZ);
        }
        
        
        /********************************************************************************************************************************/

        internal void CreateMultiCutChildClassDict(List<MultiCutChildClass> multiCutChildClasses)
        {
            childrenDict = new Dictionary<string, List<MultiCutChildClass>>();

            for (int i = 0; i < multiCutChildClasses.Count; i++)
            {
                var currentKey = multiCutChildClasses[i].chunkClass.boneName;
                if (childrenDict.ContainsKey(currentKey))
                {
                    childrenDict[currentKey].Add(multiCutChildClasses[i]);
                }
                else
                {
                    childrenDict[currentKey] = new List<MultiCutChildClass> { multiCutChildClasses[i] };
                }
            }
        }
        
        
    }
}