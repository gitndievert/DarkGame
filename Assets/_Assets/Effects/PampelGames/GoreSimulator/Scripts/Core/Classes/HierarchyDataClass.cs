// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace PampelGames.GoreSimulator
{
    public class HierarchyDataClass
    {
        public bool active;
        public int layer;
        public string tag;
        
        public GameObject obj;
        public Transform originalParent;

        public Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;    
    }

    public static class HierarchyDataUtility
    {
        public static void RecordHierarchy(List<HierarchyDataClass> hierarchyDataList, List<Transform> nonBoneChildren)
        {
            hierarchyDataList.Clear();
            foreach (var obj in nonBoneChildren)
            {
                HierarchyDataClass data = new HierarchyDataClass();

                var gameObject = obj.gameObject;
                data.active = gameObject.activeInHierarchy;
                data.layer = gameObject.layer;
                data.tag = gameObject.tag;
                
                data.obj = gameObject;
                data.originalParent = obj.transform.parent;
                
                data.originalLocalPosition = obj.localPosition;
                data.originalLocalRotation = obj.localRotation;

                if (!data.obj.TryGetComponent<DetachedChild>(out var detachedChild))
                    data.obj.AddComponent<DetachedChild>();
                
                hierarchyDataList.Add(data);
            }
        }
        
        public static void RestoreHierarchy(List<HierarchyDataClass> hierarchyDataList)
        {
            foreach (var data in hierarchyDataList)
            {
                if (data.obj == null) continue;
                
                data.obj.SetActive(data.active);
                
                data.obj.layer = data.layer;
                data.obj.tag = data.tag;
                
                var objTransform = data.obj.transform;
                objTransform.SetParent(data.originalParent);
                
                objTransform.localPosition = data.originalLocalPosition;
                objTransform.localRotation = data.originalLocalRotation;
                
                if (!data.obj.TryGetComponent<DetachedChild>(out var detachedChild)) continue;
                detachedChild.detached = false;
                detachedChild.RemoveAddedComponents();
            }
        }
    }
}
