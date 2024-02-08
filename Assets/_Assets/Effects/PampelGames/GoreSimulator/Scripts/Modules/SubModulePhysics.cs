// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using PampelGames.Shared.Utility;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    public class SubModulePhysics : SubModuleBase
    {
        public override string ModuleName()
        {
            return "Physics";
        }

        public override string ModuleInfo()
        {
            return "Adds physics components to detached objects.";
        }

        public override int imageIndex()
        {
            return 2;
        }

        public override bool CompatibleRagdoll()
        {
            return false;
        }

        public override void ModuleAdded(Type type)
        {
            base.ModuleAdded(type);
#if UNITY_EDITOR
            layer = LayerMask.NameToLayer("Default");
#endif
        }

        /********************************************************************************************************************************/

        public Enums.Collider collider = Enums.Collider.None;
        public int vertexLimit = 2000;
        public Enums.FallbackCollider fallbackCollider = Enums.FallbackCollider.Box;
        public int layer;
        public string tag;
        public bool optimizeMesh;

        public bool rigidbody;
        public float drag;
        public float angularDrag = 0.05f;
        
        /********************************************************************************************************************************/

        public override void ExecuteModuleCut(SubModuleClass subModuleClass)
        {
            ExecuteModuleInternalChildren(subModuleClass);

            if (subModuleClass.subRagdoll)
            {
                SetLayerAndTag(subModuleClass.parent);
                return;
            }
            
            ExecuteModuleInternal(subModuleClass);

            if (!rigidbody) return;
            
            Rigidbody rigid;
            if (subModuleClass.parent.TryGetComponent<Rigidbody>(out var _rigidbody))
                rigid = _rigidbody;
            else
                rigid = subModuleClass.parent.AddComponent<Rigidbody>();   
            
            if (subModuleClass.subModuleObjectClasses.Count > 0)
                rigid.mass = subModuleClass.subModuleObjectClasses[^1].mass;
            
            rigid.drag = drag;
            rigid.angularDrag = angularDrag;
            if(subModuleClass.force != Vector3.zero) rigid.AddForce(subModuleClass.force);

        }

        public override void ExecuteModuleExplosion(SubModuleClass subModuleClass)
        {
            ExecuteModuleInternal(subModuleClass);
            ExecuteModuleInternalChildren(subModuleClass);

            if (!rigidbody) return;
            for (int i = 0; i < subModuleClass.subModuleObjectClasses.Count; i++)
            {
                var subModuleObjectClass = subModuleClass.subModuleObjectClasses[i];
                if(!subModuleObjectClass.obj.TryGetComponent(out Rigidbody rigid))
                    rigid = subModuleObjectClass.obj.AddComponent<Rigidbody>();
                
                rigid.mass = subModuleObjectClass.mass;
                rigid.drag = drag;
                rigid.angularDrag = angularDrag;
                
                if (subModuleObjectClass.force != Vector3.zero) rigid.AddForce(subModuleObjectClass.force);
            }
        }

        public override void ExecuteModuleRagdoll(List<GoreBone> goreBones)
        {
            
        }
        

        /********************************************************************************************************************************/

        private void ExecuteModuleInternal(SubModuleClass subModuleClass)
        {
            SetLayerAndTag(subModuleClass.parent);

            for (int i = 0; i < subModuleClass.subModuleObjectClasses.Count; i++)
            {
                var subModuleObjectClass = subModuleClass.subModuleObjectClasses[i];
                ExecuteInternal(subModuleObjectClass);
            }
        }
        
        private void ExecuteModuleInternalChildren(SubModuleClass subModuleClass)
        {
            if (optimizeMesh) _goreSimulator.meshOptimized = true;

            for (int i = 0; i < subModuleClass.children.Count; i++)
            {
                var child = subModuleClass.children[i].gameObject;
                if (child.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    var childMesh = meshFilter.mesh;
                    ExecuteInternalChild(child, childMesh);
                }
            }
        }
        
        private void ExecuteInternal(SubModuleObjectClass subModuleObjectClass)
        {
            var obj = subModuleObjectClass.obj;
            var mesh = subModuleObjectClass.mesh;
            var boundsSize = subModuleObjectClass.boundsSize;
            var renderer = subModuleObjectClass.renderer;
            
            SetLayerAndTag(obj);
            
            if (obj.TryGetComponent<Collider>(out var _collider)) return;
            
            if(optimizeMesh)
            {
                mesh.OptimizeReorderVertexBuffer();
                _goreSimulator.meshOptimized = true;
            }
            mesh.RecalculateBounds();

            var vertexCount = mesh.vertexCount;

            /********************************************************************************************************************************/
            // If bounds size is wrong (massively oversized), the pivot is not correct. 
            // Use the default bounds and calculate pivot point
            if (boundsSize.sqrMagnitude * Constants.OversizedColliderMultiplier < renderer.bounds.size.sqrMagnitude &&
                (collider == Enums.Collider.Box || collider == Enums.Collider.Capsule) || 
                (collider == Enums.Collider.Automatic && vertexCount > vertexLimit))
            {
                if (collider is Enums.Collider.Capsule || (collider == Enums.Collider.Automatic && fallbackCollider == Enums.FallbackCollider.Capsule))
                {
                    var capsuleCollider = obj.AddComponent<CapsuleCollider>();
                    MatchCapsuleColliderToBounds(mesh, boundsSize, capsuleCollider);
                    capsuleCollider.material = _goreSimulator.physicMaterial;
                }
                else
                {
                    var boxCollider = obj.AddComponent<BoxCollider>();
                    MatchBoxColliderToBounds(mesh, boundsSize, boxCollider);
                    boxCollider.material = _goreSimulator.physicMaterial;
                }
                return;
            }
            /********************************************************************************************************************************/
            
            if (collider == Enums.Collider.Box || (collider == Enums.Collider.Automatic && vertexCount > vertexLimit && fallbackCollider == Enums.FallbackCollider.Box))
            {
                var boxCollider = obj.AddComponent<BoxCollider>();
                boxCollider.material = _goreSimulator.physicMaterial;
            }
            else if (collider == Enums.Collider.Capsule || (collider == Enums.Collider.Automatic && vertexCount > vertexLimit && fallbackCollider == Enums.FallbackCollider.Capsule))
            {
                var capsuleCollider = obj.AddComponent<CapsuleCollider>();
                PGMeshUtility.MatchCapsuleColliderToBounds(mesh, capsuleCollider);
                capsuleCollider.material = _goreSimulator.physicMaterial;
            }
            else if (collider is Enums.Collider.Mesh or Enums.Collider.Automatic)
            {
                var meshCollider = obj.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.material = _goreSimulator.physicMaterial;
            }
        }

        private void ExecuteInternalChild(GameObject child, Mesh mesh)
        {
            
            if (!child.TryGetComponent<DetachedChild>(out var detachedChild))
            {
                SetLayerAndTag(child.gameObject);
                return;
            }
            
            if (collider != Enums.Collider.None && !child.TryGetComponent<Collider>(out var col))
            {
                if(optimizeMesh) mesh.OptimizeReorderVertexBuffer();
                mesh.RecalculateBounds();

                var vertexCount = mesh.vertexCount;
                
                if (collider == Enums.Collider.Box || (collider == Enums.Collider.Automatic && vertexCount > vertexLimit && fallbackCollider == Enums.FallbackCollider.Box))
                {
                    var boxCollider = child.gameObject.AddComponent<BoxCollider>();
                    boxCollider.material = _goreSimulator.physicMaterial;
                    detachedChild.RegisterComponent(boxCollider);
                }
                    
                else if (collider == Enums.Collider.Capsule || (collider == Enums.Collider.Automatic && vertexCount > vertexLimit && fallbackCollider == Enums.FallbackCollider.Capsule))
                {
                    var capsuleCollider = child.gameObject.AddComponent<CapsuleCollider>();
                    if(mesh != null) PGMeshUtility.MatchCapsuleColliderToBounds(mesh, capsuleCollider);
                    capsuleCollider.material = _goreSimulator.physicMaterial;
                    detachedChild.RegisterComponent(capsuleCollider);
                }
                    
                else if (collider is Enums.Collider.Mesh or Enums.Collider.Automatic)
                {
                    var meshCollider = child.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                    meshCollider.material = _goreSimulator.physicMaterial;
                    detachedChild.RegisterComponent(meshCollider);
                }
            }
            
            if (rigidbody)
            {
                var rigid = child.GetComponent<Rigidbody>();
                if (rigid == null) rigid = child.gameObject.AddComponent<Rigidbody>();
                detachedChild.RegisterComponent(rigid);
            }
            
        }
        
        /********************************************************************************************************************************/

        private void SetLayerAndTag(GameObject obj)
        {
            obj.layer = layer;
            if (!string.IsNullOrEmpty(tag) && tag != "Untagged") obj.tag = tag;
        }
        
        private void MatchCapsuleColliderToBounds(Mesh mesh, Vector3 boundsSize, CapsuleCollider capsuleCollider)
        {
            var lengths = new List<float>{ boundsSize.x, boundsSize.y, boundsSize.z };
            lengths.Sort();
            capsuleCollider.radius = lengths[1] / 2;
            capsuleCollider.height = lengths[2];
            int direction = 0;
            float maxLength = boundsSize.x;
            if (boundsSize.y > maxLength)
            {
                direction = 1;
                maxLength = boundsSize.y;
            }
            if (boundsSize.z > maxLength) direction = 2;
            capsuleCollider.direction = direction;

            // Calculating center
            var vertices = PGMeshUtility.CreateVertexList(mesh);
            Vector3 center = Vector3.zero;
            int division = vertices.Count / 4; // Only 4, less expensive.
            for (var j = 0; j < 4; j++)
            {
                int index = division * j;
                center += vertices[index];
            }
            capsuleCollider.center = center / 4;
        }
        
        private void MatchBoxColliderToBounds(Mesh mesh, Vector3 boundsSize, BoxCollider boxCollider)
        {
            // Set the size of the box collider to the bounding box size
            boxCollider.size = boundsSize;

            // Calculating center
            var vertices = PGMeshUtility.CreateVertexList(mesh);
            Vector3 center = Vector3.zero;
            int division = vertices.Count / 4; // Only 4, less expensive.
            for (var j = 0; j < 4; j++)
            {
                int index = division * j;
                center += vertices[index];
            }
            boxCollider.center = center / 4;
        }
    }
}