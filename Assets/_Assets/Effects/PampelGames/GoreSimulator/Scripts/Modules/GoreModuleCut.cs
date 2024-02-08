// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PampelGames.Shared.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PampelGames.GoreSimulator
{
    [Serializable]
    public class GoreModuleCut : GoreModuleBase
    {
        public override string ModuleName()
        {
            return "Cut";
        }

        public override string ModuleInfo()
        {
            return "Cut Module: Enables mesh cutting.\n" + "\n" +
                   "The suggested way to execute is via the IGoreObject interface attached to the bones, using IGoreObject.ExecuteCut().";
        }

        public override int imageIndex()
        {
            return 1;
        }

        public override void ClearSubmodules()
        {
            _goreSimulator.cutModules.Clear();
        }

        private readonly List<GameObject> currentPoolableObjects = new();
        private readonly List<GameObject> currentDestroyableObjects = new();
        
        /********************************************************************************************************************************/

        public override void Reset(List<BonesClass> bonesClasses)
        {
            if (!_goreSimulator.meshCutInitialized) return;
            base.Reset(bonesClasses);
            for (var i = 0; i < _goreSimulator.bonesClasses.Count; i++) bonesClasses[i].ResetCutted();
            for (var i = 0; i < _goreSimulator.cutModules.Count; i++) _goreSimulator.cutModules[i].Reset();
        }

        public override void FinalizeExecution()
        {
            base.FinalizeExecution();
            
            for (var k = 0; k < _goreSimulator.cutModules.Count; k++)
            {
                if(!_goreSimulator.cutModules[k].moduleActive) continue;
                _goreSimulator.cutModules[k].FinalizeExecution(
                    new List<GameObject>(currentPoolableObjects), new List<GameObject>(currentDestroyableObjects));
            }
        }

        /********************************************************************************************************************************/


        public string ExecuteCut(string boneName, Vector3 position, Vector3 force, out GameObject detachedObject)
        {
            detachedObject = null;
            currentPoolableObjects.Clear();
            currentDestroyableObjects.Clear();
            
            var boneAmount = 1;

            if (boneName == _goreSimulator.center.name)
            {
                var nearestChildIndex = CutUtility.FindNearestTransformIndex(_goreSimulator.centerBonesClass.firstChildren, position);
                boneName = _goreSimulator.centerBonesClass.firstChildren[nearestChildIndex].name;
            }

            if (!_goreSimulator.bonesDict.TryGetValue(boneName, out var bonesClassTuple)) return string.Empty;

            var currentMesh = _goreSimulator.smr.sharedMesh;
            _goreSimulator.smr.sharedMesh = _goreSimulator.originalMesh;
            _goreSimulator.smr.BakeMesh(_goreSimulator.bakedMesh);
            _goreSimulator.smr.sharedMesh = currentMesh;

            var bakedVertices = PGMeshUtility.CreateVertexList(_goreSimulator.bakedMesh);
            var cutIndex = CutUtility.GetCutIndex(_goreSimulator.smr.transform, bonesClassTuple.Item2, bakedVertices, position);


            var bonesClass = bonesClassTuple.Item1;
            var bonesStorageClass = bonesClassTuple.Item2;
            var cutIndexClass = bonesStorageClass.cutIndexClasses[cutIndex];

            var indexesStorageClass = bonesClass.indexesStorageClass;
            var chunkClasses = bonesClass.chunkClasses;
            var innerChunkClasses = new List<ChunkClass>();
            
            var executionCutClass = ExecutionClassesUtility.GetPoolExecutionCutClass();

            /********************************************************************************************************************************/
            // Logic if already cutted bones exist.


            if (bonesClass.cutted)
            {
                if (bonesClass.cuttedIndex == -1) return string.Empty;
                if (cutIndexClass.cutIndex >= bonesClass.cuttedIndex) return string.Empty;

                CutUtility.AddSameBoneHierarchy(executionCutClass, cutIndexClass.cutIndex, bonesStorageClass,
                    bonesClass.cuttedIndex, chunkClasses, innerChunkClasses, true, new List<string>());
            }

            else
            {
                var notCuttedChildren = new List<Tuple<BonesClass, BonesStorageClass>>();
                var cuttedChildren = new List<Tuple<BonesClass, BonesStorageClass>>();

                for (var i = 0; i < bonesClass.boneChildrenSel.Count; i++)
                {
                    var childBone = bonesClass.boneChildrenSel[i];
                    if (!_goreSimulator.bonesDict.TryGetValue(childBone.name, out var childTuple)) continue;
                    if (!childTuple.Item1.cutted) notCuttedChildren.Add(childTuple);
                    else cuttedChildren.Add(childTuple);
                }

                boneAmount += notCuttedChildren.Count;

                if (cuttedChildren.Count > 0)
                {
                    for (var i = 0; i < notCuttedChildren.Count; i++)
                    {
                        var childTuple = notCuttedChildren[i];
                        var cuttedIndex = childTuple.Item1.cuttedIndex;
                        if (childTuple.Item1.firstChildCutted) cuttedIndex = childTuple.Item2.cutIndexClasses.Count;
                        CutUtility.AddChildBoneHierarchy(executionCutClass, childTuple.Item2,
                            cuttedIndex, childTuple.Item1.chunkClasses, innerChunkClasses, true);
                    }

                    var cuttedChildrenNames = new List<string>(cuttedChildren.Count);
                    for (var i = 0; i < cuttedChildren.Count; i++)
                    {
                        var childTuple = cuttedChildren[i];
                        cuttedChildrenNames.Add(childTuple.Item1.bone.name);
                        if (childTuple.Item1.cuttedIndex == -1) continue;
                        CutUtility.AddChildBoneHierarchy(executionCutClass, childTuple.Item2,
                            childTuple.Item1.cuttedIndex, childTuple.Item1.chunkClasses, innerChunkClasses, true);
                    }

                    var usedCutIndex = bonesClass.cuttedIndex;
                    if (bonesClass.firstChildCutted) usedCutIndex = bonesStorageClass.cutIndexClasses.Count;

                    CutUtility.AddSameBoneHierarchy(executionCutClass, cutIndexClass.cutIndex, bonesStorageClass,
                        usedCutIndex, chunkClasses, innerChunkClasses, true, cuttedChildrenNames);
                }

                else // Add everything from the bonesClass down.
                {
                    executionCutClass.newIndexes.UnionWith(cutIndexClass.indexesCutSide);

                    CutUtility.AddSameBoneHierarchy(executionCutClass, cutIndexClass.cutIndex, bonesStorageClass,
                        -1, chunkClasses, innerChunkClasses, false, new List<string>());

                    for (var i = 0; i < bonesClass.boneChildrenSel.Count; i++)
                    {
                        var childBone = bonesClass.boneChildrenSel[i];

                        if (!_goreSimulator.bonesDict.TryGetValue(childBone.name, out var childTuple)) continue;

                        var cuttedIndex = -1;
                        if (childTuple.Item1.firstChildCutted) cuttedIndex = childTuple.Item2.cutIndexClasses.Count;

                        CutUtility.AddChildBoneHierarchy(executionCutClass, childTuple.Item2, cuttedIndex,
                            childTuple.Item1.chunkClasses, innerChunkClasses, false);
                    }
                }
            }

            CutUtility.RemoveUsedChildren(_goreSimulator, _goreSimulator.centerExecutionCutClass, bonesClass);

            bonesClass.cutted = true;
            bonesClass.cuttedIndex = cutIndexClass.cutIndex;
            for (var i = 0; i < bonesClass.boneChildrenSel.Count; i++)
            {
                if (!_goreSimulator.bonesDict.TryGetValue(bonesClass.boneChildrenSel[i].name, out var childTuple)) continue;
                childTuple.Item1.cutted = true;
                childTuple.Item1.cuttedIndex = -1;
            }

            if (cutIndexClass.cutIndex == 0 && bonesClass.parentExists)
                if (_goreSimulator.bonesDict.TryGetValue(bonesClass.firstParent.name, out var parentTuple))
                    parentTuple.Item1.firstChildCutted = true;

            /********************************************************************************************************************************/

            ExecutionUtility.AddCutMaterial(_goreSimulator);


            _goreSimulator.centerExecutionCutClass.newIndexes.ExceptWith(cutIndexClass.oppositeParentIndexes);
            _goreSimulator.usedBonesClasses.Add(new UsedBonesClass());
            _goreSimulator.usedBonesClasses[^1].AddItems(bonesClass.bone, cutIndexClass.cutIndex);

            executionCutClass.AddExecutionIndexes(cutIndexClass.cutIndexesParentSide, cutIndexClass.sewIndexesParentSide,
                cutIndexClass.sewTrianglesParentSide);

            _goreSimulator.centerExecutionCutClass.AddExecutionIndexes(cutIndexClass.cutIndexesParentSide, cutIndexClass.sewIndexesForParent,
                cutIndexClass.sewTrianglesForParent);


            MeshCutJobs.IndexesSnapshotCut(_goreSimulator.smr.transform, _goreSimulator.storage.meshDataClass, _goreSimulator.meshNativeDataClass,
                _goreSimulator.originalMesh,
                _goreSimulator.centerExecutionCutClass, _goreSimulator.centerMesh, false, out var cutCentersCenter);

            GoreMultiCut goreMultiCut;

            var children = BonesUtility.GetChildren(_goreSimulator.smrBones, bonesClass.bone, _goreSimulator.childrenEnum);
            foreach (var child in children) child.parent = null;

            
            var subModuleClass = ExecutionClassesUtility.GetPoolSubModuleClass();
            subModuleClass.children = children;
            subModuleClass.centerMesh = _goreSimulator.centerMesh;
            subModuleClass.force = force;
            
            // Skinned Children
            var detachedSkinnedChildren = SkinnedChildren.CreateSkinnedChildrenForBone(_goreSimulator, bonesClass.bone);
            subModuleClass.children.AddRange(detachedSkinnedChildren);

            var multiCutChildClasses = new List<MultiCutChildClass>();

            /********************************************************************************************************************************/
            // Sub Ragdoll
            if (ExecutionUtility.FindSubModule<SubModuleCutRagdoll>(_goreSimulator.cutModules) is SubModuleCutRagdoll moduleCut &&
                boneAmount >= moduleCut.minimumBoneAmount)
            {
                if (indexesStorageClass.cutMeshes[cutIndexClass.cutIndex] != null)
                    indexesStorageClass.cutMeshes[cutIndexClass.cutIndex].Clear();
                else
                    indexesStorageClass.cutMeshes[cutIndexClass.cutIndex] = new Mesh();
                
                MeshCutJobs.IndexesSnapshotCut(_goreSimulator.smr.transform, _goreSimulator.storage.meshDataClass, _goreSimulator.meshNativeDataClass, 
                    _goreSimulator.originalMesh, executionCutClass, indexesStorageClass.cutMeshes[cutIndexClass.cutIndex], false, out var cutCenters);
                
                subModuleClass.cutPosition = cutCenters[^1];

                indexesStorageClass.cutMeshes[cutIndexClass.cutIndex].RecalculateBounds();
                subModuleClass.cutDirection = CutUtility.CreatePlaneNormalFromCutIndexes(_goreSimulator.smr.transform, bakedVertices,
                    executionCutClass.cutIndexes[^1], indexesStorageClass.cutMeshes[cutIndexClass.cutIndex].bounds.center, subModuleClass.cutPosition);

                subModuleClass.subRagdoll = true;
                detachedObject = RagdollUtility.CreateRagdollObject(_goreSimulator.smr,
                    _goreSimulator.gameObject.name + " - " + bonesClass.bone.name + " - " + cutIndexClass.cutIndex + " - Ragdoll");

                goreMultiCut = detachedObject.gameObject.AddComponent<GoreMultiCut>();
                goreMultiCut.status = Enums.MultiCutStatus.Ragdoll;
                goreMultiCut._goreSimulator = _goreSimulator;
                goreMultiCut.bakedMesh = _goreSimulator.bakedMesh;

                moduleCut.ExecuteRagdollSubModule(detachedObject, indexesStorageClass.cutMeshes[cutIndexClass.cutIndex], cutIndexClass, bonesClass,
                    cutCenters, subModuleClass, goreMultiCut);

                var detachedGoreBones = detachedObject.GetComponentsInChildren<GoreBone>();
                for (var i = 0; i < detachedGoreBones.Length; i++)
                {
                    detachedGoreBones[i]._goreMultiCut = goreMultiCut;
                    detachedGoreBones[i].multiCut = true;
                }

                for (var i = 0; i < innerChunkClasses.Count; i++)
                    multiCutChildClasses.Add(new MultiCutChildClass
                    {
                        chunkClass = innerChunkClasses[i],
                        childObject = subModuleClass.subModuleObjectClasses[0].obj
                    });
                
            }
            /********************************************************************************************************************************/
            else
            {
                var materialsCopy = _goreSimulator.smr.materials
                    .Select(Object.Instantiate)
                    .ToArray();

                detachedObject = new GameObject(_goreSimulator.gameObject.name + " - " + bonesClass.bone.name + " - " + cutIndexClass.cutIndex);
                detachedObject.transform.SetPositionAndRotation(bonesClass.bone.position, _goreSimulator.smr.transform.rotation);
                goreMultiCut = detachedObject.AddComponent<GoreMultiCut>();
                goreMultiCut._goreSimulator = _goreSimulator;
                goreMultiCut.bakedMesh = _goreSimulator.bakedMesh;

                var mass = 1f;
                if (bonesClass.goreBone._rigidbody != null) mass = bonesClass.goreBone._rigidbody.mass;

                for (var i = 0; i < innerChunkClasses.Count; i++)
                {
                    var subModuleObjectClass = CutUtility.CreateSubModuleObjectClass(goreMultiCut, innerChunkClasses[i], _goreSimulator.smr.transform,
                        bakedVertices);
                    currentPoolableObjects.Add(subModuleObjectClass.obj);
                    subModuleObjectClass.obj.transform.SetParent(detachedObject.transform);
                    subModuleObjectClass.renderer.materials = materialsCopy;
                    subModuleObjectClass.mass = mass;
                    subModuleClass.subModuleObjectClasses.Add(subModuleObjectClass);
                }
                

                for (var i = 0; i < innerChunkClasses.Count; i++)
                    multiCutChildClasses.Add(new MultiCutChildClass
                    {
                        chunkClass = innerChunkClasses[i],
                        childObject = subModuleClass.subModuleObjectClasses[i].obj
                    });

                subModuleClass.cutPosition = subModuleClass.subModuleObjectClasses[0].cutCenters[0];

                var chunkVertices = PGMeshUtility.CreateVertexList(innerChunkClasses[0].mesh);
                innerChunkClasses[0].mesh.RecalculateBounds();
                subModuleClass.cutDirection = CutUtility.CreatePlaneNormalFromCutIndexes(detachedObject.transform, chunkVertices,
                    innerChunkClasses[0].indexClasses[0].cutIndexes, innerChunkClasses[0].mesh.bounds.center, subModuleClass.cutPosition);
            }
            
            /********************************************************************************************************************************/

            currentDestroyableObjects.Add(detachedObject);
            _goreSimulator.AddDestroyableObject(detachedObject);

            subModuleClass.parent = detachedObject;
            subModuleClass.centerBone = bonesClass.bone;
            
            goreMultiCut.bonesClass = bonesClass;
            goreMultiCut.CreateMultiCutChildClassDict(multiCutChildClasses);


            for (var k = 0; k < _goreSimulator.cutModules.Count; k++)
            {
                if(!_goreSimulator.cutModules[k].moduleActive) continue;
                _goreSimulator.cutModules[k].ExecuteModuleCut(subModuleClass);
            }

            for (var i = 0; i < bonesClass.directBoneChildren.Count; i++) bonesClass.directBoneChildren[i].gameObject.SetActive(false);

            _goreSimulator.smr.sharedMesh = _goreSimulator.centerMesh;

            ExecutionClassesUtility.ReleaseSubModuleClass(subModuleClass);
            ExecutionClassesUtility.ReleaseExecutionCutClass(executionCutClass);


            // GameObject parentOBJ = new GameObject(bonesClass.bone.name + "_" + "triangles");
            // for (int k = 0; k < triangles.Count; k++)
            // {
            //     var worldSpace = _goreSimulator.smr.transform.TransformPoint(_goreSimulator.bakedMesh.vertices[triangles[k]]);
            //     var sphere = PGInformationUtility.CreateSphere(worldSpace, 0.01f);
            //     sphere.transform.SetParent(parentOBJ.transform);
            // }
            
            return bonesClass.bone.name;
        }
    }
}