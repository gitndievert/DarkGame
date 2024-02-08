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
    public class MeshParts : MonoBehaviour
    {
        public List<GameObject> meshParts = new();

        public Vector2 seperationDirection = new(1f, 1f);
        public float seperationSlider;


        public List<Vector3> localBoundsPositions = new();
        public float maxDistanceX;
        public float maxDistanceY;

        /********************************************************************************************************************************/
        public void ApplySeperation()
        {
            if (localBoundsPositions.Count != meshParts.Count)
                CreateLocalBoundsPositions();

            for (var i = 0; i < meshParts.Count; i++)
            {
                var percentageX = Mathf.Abs(localBoundsPositions[i].x) / maxDistanceX * seperationSlider;
                var percentageY = Mathf.Abs(localBoundsPositions[i].y / maxDistanceY) * seperationSlider;

                var xDistance = Mathf.Lerp(0f, seperationDirection.x, percentageX);
                var yDistance = Mathf.Lerp(0f, seperationDirection.y, percentageY);

                var localCenter = localBoundsPositions[i];
                if (localCenter.x < 0) xDistance *= -1;

                meshParts[i].transform.localPosition = new Vector3(xDistance, yDistance, 0);
            }
        }

        /********************************************************************************************************************************/

        private void CreateLocalBoundsPositions()
        {
            localBoundsPositions.Clear();
            for (var i = 0; i < meshParts.Count; i++)
            {
                var part = meshParts[i];
                var meshRenderer = part.GetComponent<MeshRenderer>();

                if (meshRenderer == null) continue;

                var localBoundsPosition = part.transform.InverseTransformPoint(meshRenderer.bounds.center);
                localBoundsPositions.Add(localBoundsPosition);
            }

            GetMaxDistance();
        }

        private void GetMaxDistance()
        {
            if (localBoundsPositions.Count == 0) return;

            maxDistanceX = localBoundsPositions.Max(pos => Mathf.Abs(pos.x));
            maxDistanceY = localBoundsPositions.Max(pos => Mathf.Abs(pos.y));
        }
    }
}