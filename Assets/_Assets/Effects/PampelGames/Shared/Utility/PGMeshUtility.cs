// ----------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PampelGames.Shared.Utility
{
    public static class PGMeshUtility
    {
        
        
        /// <summary>
        ///     Combines an array of meshes into one new single mesh.
        /// </summary>
        public static Mesh CombineMeshes(Mesh[] meshes, bool mergeSubmeshes)
        {
            return CombineMeshesInternal(meshes, mergeSubmeshes);
        }

        /// <summary>
        ///     Combines multiple mesh instances into a single mesh manually.
        ///     This works similar to Unity's mesh.CombineMeshes() method, but works with mal-formed mesh as well.
        /// </summary>
        /// <param name="combineInstances">The array of CombineInstance objects representing the meshes to combine.</param>
        /// <returns>The combined mesh.</returns>
        public static Mesh CombineMeshesManually(CombineInstance[] combineInstances)
        {
            return CombineMeshesManuallyInternal(combineInstances);
        }
        
        /// <summary>
        ///     Get all indexes that are within the radius from the pivot point.
        /// </summary>
        /// <param name="indexesInRadius">All indexes within the radius.</param>
        /// <param name="closestIndex">The closest index to the pivot point.</param>
        public static void GetIndexesInRadius(Mesh mesh, Vector3 pivot, float radius, out int[] indexesInRadius, out int closestIndex)
        {
            GetIndexesInRadiusInternal(mesh, pivot, radius, out indexesInRadius, out closestIndex);
        }

        /// <summary>
        ///     Equalizes the normals of vertices that share the same position in the mesh.
        /// </summary>
        public static void EqualizeNormals(Mesh mesh)
        {
            EqualizeNormalsInternal(mesh);
        }
        
        /* Bounds ***********************************************************************************************************************/
        
        /// <summary>
        /// Adapts a capsule collider to the mesh bounds.
        /// </summary>
        public static void MatchCapsuleColliderToBounds(Mesh mesh, CapsuleCollider capsuleCollider)
        {
            MatchCapsuleColliderToBoundsInternal(mesh, capsuleCollider);
        }
        

        /// <summary>
        ///     Transforms the bounds of the given CombineInstance from local space to world space.
        /// </summary>
        public static Bounds GetBoundsWorldSpace(CombineInstance combineInstance)
        {
            var boundsLocal = combineInstance.mesh.bounds;
            var matrix = combineInstance.transform;
            return TransformBoundsToWorldInternal(boundsLocal, matrix);
        }
        
        
        /* Vertices & UVs ****************************************************************************************************************/
        
        /// <summary>
        ///     Creates a new list from the current UVs.
        ///     Remember to use mesh.SetUVs(UVs); when done.
        /// </summary>
        public static List<Vector2> CreateUVList(Mesh mesh, int channel = 0)
        {
            var UVs = new List<Vector2>();
            mesh.GetUVs(channel, UVs);
            return UVs;
        }
        
        /// <summary>
        ///     Creates a new list from the current vertices which can be used for the transformation methods provided below.
        ///     Remember to use mesh.SetVertices(vertices); when done.
        /// </summary>
        public static List<Vector3> CreateVertexList(Mesh mesh)
        {
            var vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            return vertices;
        }
        
        /// <summary>
        ///     Translates all vertices.
        /// </summary>
        /// <param name="translation">Delta position of the vertices.</param>
        public static void PGTranslateVertices(List<Vector3> vertices, Vector3 translation)
        {
            for (var i = 0; i < vertices.Count; i++) vertices[i] += translation;
        }
        
        /// <summary>
        ///     Rotates all vertices around a pivot point.
        /// </summary>
        /// <param name="pivot">The pivot point around which the rotation occurs. For example mesh.bounds.center</param>
        public static void PGRotateVertices(List<Vector3> vertices, Quaternion rotation, Vector3 pivot)
        {
            for (var i = 0; i < vertices.Count; i++) vertices[i] = rotation * (vertices[i] - pivot) + pivot;
        }
        
        /// <summary>
        ///     Scales the vertices of the mesh by the specified scaleFactor around the given center point.
        /// </summary>
        /// <param name="scaleFactor">The scale factor to apply to the vertices.</param>
        /// <param name="center">The center point around which the scaling is performed.</param>
        public static void PGScaleVertices(List<Vector3> vertices, Vector3 scaleFactor, Vector3 center)
        {
            for (var i = 0; i < vertices.Count; i++)
            {
                var scaledPosition = vertices[i] - center;
                scaledPosition = new Vector3(scaledPosition.x * scaleFactor.x, scaledPosition.y * scaleFactor.y, scaledPosition.z * scaleFactor.z);
                vertices[i] = scaledPosition + center;
            }
        }
        
        public static void FlipNormals(Mesh mesh) 
        {
            FlipNormalsInternal(mesh);
        }
        
        
        /* Combine Instances ************************************************************************************************************/
        
        public static void PGTranslateCombine(this ref CombineInstance combineInstance, Vector3 translation)
        {
            if (translation == Vector3.zero) return;
            Matrix4x4 translationMatrix = Matrix4x4.Translate(translation);
            combineInstance.transform = translationMatrix * combineInstance.transform;
        }

        public static void PGRotateCombine(this ref CombineInstance combineInstance, Vector3 pivot, Quaternion rotation)
        {
            if (rotation == Quaternion.identity) return;
            Matrix4x4 translationToOrigin = Matrix4x4.Translate(-pivot);
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotation);
            Matrix4x4 translationBackToPivot = Matrix4x4.Translate(pivot);
            Matrix4x4 transform = translationBackToPivot * rotationMatrix * translationToOrigin;
            combineInstance.transform = transform * combineInstance.transform;
        }

        public static void PGRotateCombine(this ref CombineInstance combineInstance, Vector3 pivot, Vector3 eulerRotation)
        {
            if (eulerRotation == Vector3.zero) return;
            PGRotateCombine(ref combineInstance, pivot, Quaternion.Euler(eulerRotation));
        }

        public static void PGScaleCombine(this ref CombineInstance combineInstance, Vector3 pivot, Vector3 size)
        {
            if (size == Vector3.one) return;
            Matrix4x4 translationToOrigin = Matrix4x4.Translate(-pivot);
            Matrix4x4 scaleMatrix = Matrix4x4.Scale(size);
            Matrix4x4 translationBackToPivot = Matrix4x4.Translate(pivot);
            Matrix4x4 transform = translationBackToPivot * scaleMatrix * translationToOrigin;
            combineInstance.transform = transform * combineInstance.transform;
        }
        
        /********************************************************************************************************************************/

        /// <summary>
        ///     Combines the Mesh Renderers into a new one.
        /// </summary>
        /// <param name="rootObj">GameObject to attach the renderer to.</param>
        /// <param name="objects">List of GameObjects with MeshFilter and MeshRenderer to be combined.</param>
        /// <param name="mergeSubMeshes">If true, only one resulting sub-mesh will be created.</param>
        /// <param name="saveMesh">Save the Mesh into the project (Editor only).</param>
        public static bool CombineMeshes(GameObject rootObj, List<GameObject> objects, bool mergeSubMeshes, bool saveMesh)
        {
            if (mergeSubMeshes) return CombineMeshesMergeInternal(rootObj, objects, saveMesh);
            return CombineMeshesInternal(rootObj, objects, saveMesh);
        }

        /********************************************************************************************************************************/
        /********************************************************************************************************************************/


        private static Mesh CombineMeshesInternal(Mesh[] meshes, bool mergeSubmeshes)
        {
            var combinedInstances = new CombineInstance[meshes.Length];
            for (var i = 0; i < combinedInstances.Length; i++) combinedInstances[i].transform = Matrix4x4.identity;
            for (var i = 0; i < combinedInstances.Length; i++) combinedInstances[i].mesh = meshes[i];

            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combinedInstances, mergeSubmeshes, true);
            return combinedMesh;
        }

        private static Mesh CombineMeshesManuallyInternal(CombineInstance[] combineInstances)
        {
            Mesh finalMesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uv0 = new List<Vector2>();
            List<Vector2> uv1 = new List<Vector2>(); 
            List<Vector4> tangents = new List<Vector4>();
            List<Color> colors = new List<Color>();

            List<List<int>> subMeshTriangles = new List<List<int>>();

            int vertexOffset = 0;

            foreach (CombineInstance ci in combineInstances)
            {
                Mesh mesh = ci.mesh;
                Matrix4x4 transform = ci.transform;

                foreach (Vector3 vertex in mesh.vertices)
                    vertices.Add(transform.MultiplyPoint3x4(vertex));

                foreach (Vector3 normal in mesh.normals)
                    normals.Add(transform.MultiplyVector(normal));

                foreach (Vector4 tangent in mesh.tangents)
                {
                    Vector4 newTangent = transform.MultiplyPoint3x4(tangent);
                    newTangent.w = tangent.w;
                    tangents.Add(newTangent);
                }

                uv0.AddRange(mesh.uv);

                if (mesh.uv2.Length > 0)
                {
                    uv1.AddRange(mesh.uv2);
                }

                colors.AddRange(mesh.colors);

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    List<int> triangles = new List<int>();
                    mesh.GetTriangles(triangles, i);
                    for (int j = 0; j < triangles.Count; j++)
                        triangles[j] += vertexOffset;

                    subMeshTriangles.Add(triangles);
                }

                vertexOffset += mesh.vertexCount;
            }

            finalMesh.SetVertices(vertices);
            finalMesh.SetNormals(normals);
            finalMesh.SetUVs(0, uv0);

            if (uv1.Count > 0)
            {
                finalMesh.SetUVs(1, uv1);
            }

            finalMesh.SetTangents(tangents);
            if(vertices.Count == colors.Count) finalMesh.SetColors(colors);

            finalMesh.subMeshCount = subMeshTriangles.Count;
            for (int i = 0; i < subMeshTriangles.Count; i++)
                finalMesh.SetTriangles(subMeshTriangles[i], i);

            finalMesh.RecalculateBounds();

            return finalMesh;
        }
        
        private static void GetIndexesInRadiusInternal(Mesh mesh, Vector3 pivot, float radius, out int[] indexesInRadius, out int closestIndex)
        {
            HashSet<int> indexesWithinRadiusSet = new HashSet<int>();
            float closestDistanceSqr = Mathf.Infinity;
            closestIndex = -1;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int vertexIndex1 = triangles[i];
                int vertexIndex2 = triangles[i + 1];
                int vertexIndex3 = triangles[i + 2];

                Vector3 vertex1 = vertices[vertexIndex1];
                Vector3 vertex2 = vertices[vertexIndex2];
                Vector3 vertex3 = vertices[vertexIndex3];

                float distanceSqr1 = (vertex1 - pivot).sqrMagnitude;
                float distanceSqr2 = (vertex2 - pivot).sqrMagnitude;
                float distanceSqr3 = (vertex3 - pivot).sqrMagnitude;

                if (distanceSqr1 <= radius * radius)
                {
                    indexesWithinRadiusSet.Add(vertexIndex1);
                    if (distanceSqr1 < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr1;
                        closestIndex = vertexIndex1;
                    }
                }

                if (distanceSqr2 <= radius * radius)
                {
                    indexesWithinRadiusSet.Add(vertexIndex2);
                    if (distanceSqr2 < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr2;
                        closestIndex = vertexIndex2;
                    }
                }

                if (distanceSqr3 <= radius * radius)
                {
                    indexesWithinRadiusSet.Add(vertexIndex3);
                    if (distanceSqr3 < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr3;
                        closestIndex = vertexIndex3;
                    }
                }
            }

            indexesInRadius = indexesWithinRadiusSet.ToArray();
        }
        
        
        private static void EqualizeNormalsInternal(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int vertexCount = mesh.vertexCount;

            Dictionary<Vector3, List<int>> vertexMap = new Dictionary<Vector3, List<int>>();

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 vertexPosition = vertices[i];

                if (!vertexMap.ContainsKey(vertexPosition))
                    vertexMap[vertexPosition] = new List<int>();

                vertexMap[vertexPosition].Add(i);
            }

            foreach (KeyValuePair<Vector3, List<int>> entry in vertexMap)
            {
                List<int> indices = entry.Value;
                int count = indices.Count;
                if (count <= 1) continue;

                Vector3 averageNormal = Vector3.zero;
                foreach (int index in indices) averageNormal += normals[index];
                averageNormal /= count;

                foreach (int index in indices) normals[index] = averageNormal;
            }

            mesh.normals = normals;
        }
        
        private static void MatchCapsuleColliderToBoundsInternal(Mesh mesh, CapsuleCollider capsuleCollider)
        {
            Bounds bounds = mesh.bounds;
            Vector3 size = bounds.size;
            var lengths = new List<float>{ size.x, size.y, size.z };
            lengths.Sort();
            capsuleCollider.radius = lengths[1] / 2;
            capsuleCollider.height = lengths[2];
            capsuleCollider.center = bounds.center;
            int direction = 0;
            float maxLength = size.x;
            if (size.y > maxLength)
            {
                direction = 1;
                maxLength = size.y;
            }
            if (size.z > maxLength) direction = 2;
                    
            capsuleCollider.direction = direction;
        }

        private static Bounds TransformBoundsToWorldInternal(Bounds boundsLocal, Matrix4x4 matrix)
        {
            var center = matrix.MultiplyPoint3x4(boundsLocal.center);
            var extents = boundsLocal.extents;
            var boundsWorld = new Bounds(center, Vector3.zero);

            // Encapsulate all the possible world points to get full world size bounds
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, extents)));
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, -extents)));
            extents = -extents;
            extents.y *= -1;
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, extents)));
            extents.z *= -1;
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, extents)));
            extents = -extents;
            extents.x *= -1;
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, extents)));
            extents.y *= -1;
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, extents)));
            extents.z *= -1;
            boundsWorld.Encapsulate(matrix.MultiplyPoint3x4(PointFromBBox(boundsLocal.center, extents)));

            return boundsWorld;
        }

        private static Vector3 PointFromBBox(Vector3 center, Vector3 extents)
        {
            return new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
        }
        
        private static bool CombineMeshesInternal(GameObject rootObj, List<GameObject> objects, bool saveMesh)
        {
            List<MeshRenderer> renderers = new List<MeshRenderer>();
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            foreach (var obj in objects)
            {
                if(!obj.TryGetComponent<MeshRenderer>(out var renderer) ||
                   !obj.TryGetComponent<MeshFilter>(out var meshFilter)) continue;
                renderers.Add(renderer);
                meshFilters.Add(meshFilter);
            }
            
            var allBoneWeights = new List<BoneWeight>();
            var allCombineInstances = new List<CombineInstance>();
            var allBindPoses = new List<Matrix4x4>();
            var allMaterials = new List<Material>();

            int boneOffset = 0;

            for (var i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                var meshFilter = meshFilters[i];
                
                allMaterials.AddRange(renderer.sharedMaterials);

                // Adjust the boneweights
                BoneWeight[] boneWeights = meshFilter.sharedMesh.boneWeights;
                for (int j = 0; j < boneWeights.Length; j++)
                {
                    BoneWeight boneWeight = boneWeights[j];
                    boneWeight.boneIndex0 += boneOffset;
                    boneWeight.boneIndex1 += boneOffset;
                    boneWeight.boneIndex2 += boneOffset;
                    boneWeight.boneIndex3 += boneOffset;
                    allBoneWeights.Add(boneWeight);
                }
                
                // Adjust the bind poses
                for (int j = 0; j < meshFilter.sharedMesh.bindposes.Length; ++j)
                {
                    allBindPoses.Add(meshFilter.sharedMesh.bindposes[j] * renderer.transform.worldToLocalMatrix);
                }

                CombineInstance combineInstance = new CombineInstance
                {
                    mesh = meshFilter.sharedMesh,
                    transform = renderer.transform.localToWorldMatrix,
                };
                allCombineInstances.Add(combineInstance);
            }

            MeshRenderer combinedRenderer = rootObj.GetComponent<MeshRenderer>();
            if (combinedRenderer == null) combinedRenderer = rootObj.AddComponent<MeshRenderer>();

            MeshFilter combinedMeshFilter = rootObj.GetComponent<MeshFilter>();
            if (combinedMeshFilter == null) combinedMeshFilter = rootObj.AddComponent<MeshFilter>();
            
            /********************************************************************************************************************************/
            
            
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(allCombineInstances.ToArray(), false, true);
            mesh.boneWeights = allBoneWeights.ToArray();
            mesh.bindposes = allBindPoses.ToArray();
            mesh.RecalculateBounds();
            mesh.Optimize();

            combinedMeshFilter.sharedMesh = mesh;
            combinedRenderer.sharedMaterials = allMaterials.ToArray();
            
            
#if UNITY_EDITOR
            if (saveMesh)
            {
                if(!SaveMesh(mesh)) return false;
            }
#endif
            return true;
        }
        
        private static bool CombineMeshesMergeInternal(GameObject rootObj, List<GameObject> objects, bool saveMesh)
        {
            var firstObjRenderer = objects[0].GetComponent<MeshRenderer>();
            var materials = firstObjRenderer.sharedMaterials;
    
            var combineInstances = new List<CombineInstance>();

            foreach (var obj in objects)
            {
                var meshFilter = obj.GetComponent<MeshFilter>();
                for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
                {
                    combineInstances.Add(new CombineInstance
                    {
                        mesh = meshFilter.sharedMesh,
                        subMeshIndex = i,
                        transform = meshFilter.transform.localToWorldMatrix
                    });
                }
            }

            MeshRenderer combinedRenderer = rootObj.AddComponent<MeshRenderer>();
            MeshFilter combinedMeshFilter = rootObj.AddComponent<MeshFilter>();
            combinedMeshFilter.sharedMesh = new Mesh();
            combinedMeshFilter.sharedMesh.CombineMeshes(combineInstances.ToArray(), true);

            // use the materials array from the first object renderer's materials
            combinedRenderer.sharedMaterials = materials;

#if UNITY_EDITOR
            if (saveMesh)
            {
                if (!SaveMesh(combinedMeshFilter.sharedMesh)) return false;
            }
#endif

            return true;
        }
        
#if UNITY_EDITOR
        private static bool SaveMesh(Mesh mesh)
        {
            string defaultPath = "Assets/";
            string defaultName = "CombinedMesh.asset";
            string message = "Save Combined Mesh";
            string defaultExtension = "asset";

            string savePath = EditorUtility.SaveFilePanelInProject(message, defaultName, defaultExtension, 
                "Please enter a file name to save the mesh to.", defaultPath);
            if (string.IsNullOrEmpty(savePath)) return false;
            
            AssetDatabase.CreateAsset(mesh, savePath);
            AssetDatabase.SaveAssets();
            return true;

        }
#endif

        private static void FlipNormalsInternal(Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++) 
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3) 
                {
                    (triangles[i], triangles[i + 2]) = (triangles[i + 2], triangles[i]);
                }
                mesh.SetTriangles(triangles, m);
            }
        }
        
    }
}