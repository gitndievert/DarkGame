// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using PampelGames.Shared.Editor;
using PampelGames.Shared.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.GoreSimulator.Editor
{
    [CustomEditor(typeof(MeshParts))]
    public class MeshPartsInspector : UnityEditor.Editor
    {
        protected VisualElement container;
        public VisualTreeAsset _visualTree;
        
        private MeshParts _meshParts;

        private Vector2Field seperationDirection;
        private Slider seperationSlider;
        
        private ToolbarToggle showMeshParts;
        private ListView meshPartsListView;
        
        protected void OnEnable()
        {
            container = new VisualElement();
            _visualTree.CloneTree(container);
            _meshParts = target as MeshParts;

            FindElements();
            BindElements();
            VisualizeElements();
        }

        private void FindElements()
        {
            showMeshParts = container.Q<ToolbarToggle>(nameof(showMeshParts));
            meshPartsListView = container.Q<ListView>(nameof(meshPartsListView));
            
            seperationDirection = container.Q<Vector2Field>(nameof(seperationDirection));
            seperationSlider = container.Q<Slider>(nameof(seperationSlider));
        }

        private void BindElements()
        {
            seperationDirection.PGSetupBindProperty(serializedObject, nameof(MeshParts.seperationDirection));
            seperationSlider.PGSetupBindProperty(serializedObject, nameof(MeshParts.seperationSlider));
        }

        private void VisualizeElements()
        {
            
        }
        
        /********************************************************************************************************************************/
        
        public override VisualElement CreateInspectorGUI()
        {
            CreateSeperation();
            CreateMeshPartsListView();

            return container;
        }
        
        /********************************************************************************************************************************/

        private void CreateSeperation()
        {
            seperationSlider.RegisterValueChangedCallback(evt =>
            {
                _meshParts.ApplySeperation();
                SceneView.RepaintAll();
            });
        }
        
        private void CreateMeshPartsListView()
        {
            var skinnedChildrenProperty = serializedObject.FindProperty(nameof(_meshParts.meshParts));
            meshPartsListView.PGSetupObjectListView(skinnedChildrenProperty, _meshParts.meshParts);
            meshPartsListView.PGObjectListViewStyle();

            showMeshParts.RegisterValueChangedCallback(evt =>
            {
                meshPartsListView.PGDisplayStyleFlex(showMeshParts.value);
            });
        }
        
        
        
    }
}
