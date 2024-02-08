// ----------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using UnityEngine;

namespace PampelGames.Shared.Utility
{
    /// <summary>
    ///     <see cref="Transform"/> helpers.
    /// </summary>
    public class PGTransformUtility : MonoBehaviour
    {
        
        public static void RotateAroundPivot(Transform transform, Vector3 pivot, Vector3 eulerRotation)
        {
            Quaternion rotation = Quaternion.Euler(eulerRotation);
            RotateAroundPivot(transform, pivot, rotation);
        }
        
        /// <summary>
        ///     Rotates a transform around a pivot point.
        /// </summary>
        public static void RotateAroundPivot(Transform transform, Vector3 pivot, Quaternion rotation)
        {
            Vector3 direction = transform.position - pivot;
            direction = rotation * direction;
            transform.position = pivot + direction;
            transform.rotation = rotation * transform.rotation;
        }
        
    }
}
