// ----------------------------------------------------
// Gore Simulator
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System.Collections.Generic;
using PampelGames.Shared;
using PampelGames.Shared.Editor;
using PampelGames.Shared.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.GoreSimulator.Editor
{
    [CustomPropertyDrawer(typeof(SubModulePhysics))]
    public class SubModulePhysicsDrawer : PropertyDrawer
    {
        private SubModulePhysics _subModulePhysics;

        private VisualElement ColliderWrapper = new();
        private SerializedProperty layerMaskProperty;
        private readonly LayerField layerMask = new("Layer");
        private readonly TagField tag = new("Tag");
        private readonly Toggle optimizeMesh = new("Optimize");
        
        
        private SerializedProperty colliderProperty;
        private readonly EnumField collider = new("Collider");
        private readonly VisualElement automaticColliderWrapper = new();
        private readonly IntegerField vertexLimit = new("Vertex Limit");
        private readonly EnumField fallbackCollider = new("Fallback");
        
        private SerializedProperty rigidbodyProperty;
        private readonly Toggle rigidbody = new("Rigidbody");
        private VisualElement rigidbodyWrapper = new();
        private readonly FloatField drag = new("Drag");
        private readonly FloatField angularDrag = new("Angular Drag");
        
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var listIndex = PGPropertyDrawerUtility.GetDrawingListIndex(property);
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            var objList = obj as List<SubModuleBase>;
            _subModulePhysics = (SubModulePhysics) objList[listIndex];

            FindAndBindProperties(property);
            VisualizeProperties();
            
            RigidVisualization();
            MaterialVisualization();
            DrawModule();
            
            automaticColliderWrapper.Add(vertexLimit);
            automaticColliderWrapper.Add(fallbackCollider);
            
            ColliderWrapper.Add(automaticColliderWrapper);
            ColliderWrapper.Add(layerMask);
            ColliderWrapper.Add(tag);
            ColliderWrapper.Add(optimizeMesh);
            
            container.Add(collider);
            container.Add(ColliderWrapper);
            
            rigidbody.PGDrawTopLine(true);
            container.Add(rigidbody);
            rigidbodyWrapper.Add(drag);
            rigidbodyWrapper.Add(angularDrag);
            container.Add(rigidbodyWrapper);

            return container;
        }
        
        private void FindAndBindProperties(SerializedProperty property)
        {
            layerMaskProperty = property.FindPropertyRelative(nameof(SubModulePhysics.layer));
            layerMask.BindProperty(layerMaskProperty);
            
            tag.PGSetupBindPropertyRelative(property, nameof(SubModulePhysics.tag));
            optimizeMesh.PGSetupBindPropertyRelative(property, nameof(SubModulePhysics.optimizeMesh));

            colliderProperty = property.FindPropertyRelative(nameof(SubModulePhysics.collider));
            collider.BindProperty(colliderProperty);
            vertexLimit.PGSetupBindPropertyRelative(property, nameof(SubModulePhysics.vertexLimit));
            fallbackCollider.PGSetupBindPropertyRelative(property, nameof(SubModulePhysics.fallbackCollider));
            rigidbodyProperty = property.FindPropertyRelative(nameof(SubModulePhysics.rigidbody));
            rigidbody.BindProperty(rigidbodyProperty);
            drag.PGSetupBindPropertyRelative(property, nameof(SubModulePhysics.drag));
            angularDrag.PGSetupBindPropertyRelative(property, nameof(SubModulePhysics.angularDrag));
        }

        private void VisualizeProperties()
        {
            string meshColliderTooltip = "Mesh colliders are the most realistic but by far the most expensive to add.";
            collider.tooltip = "Add colliders to detached objects. " + meshColliderTooltip + "\n" + "\n" +
                               "The 'Automatic' option will add mesh colliders if the mesh vertex count is below the specified limit.";

            vertexLimit.tooltip = "If a detached part has a vertex count below this limit, a Mesh Collider will be added. " +
                                  "If you receive warnings in the console when adding a Mesh Collider, you should consider decreasing this value.";
            vertexLimit.PGClampValue();
            fallbackCollider.tooltip = "Collider to be added if the vertex limit is reached.";
            
            layerMask.tooltip = "Layer being used for attached colliders.";
            tag.tooltip = "Tag being used for detached parts.";
            optimizeMesh.tooltip = "Use this option to improve the accuracy of the generated colliders.\n" +
                                   "Does incur a CPU cost both for cutting and character reset.";

            rigidbody.tooltip = "Add rigidbodies to detached objects. The mass values are inherited from the original bone's rigidbody."; 
            string frictionTooltip = "You can also influence the behavior of detached parts via the physics material " +
                                     "('Material references' icon at the top).";
            drag.tooltip = "Drag can be used to slow down an object. The higher the drag the more the object slows down.\n" + "\n" +
                           frictionTooltip;
            angularDrag.tooltip = "Angular drag can be used to slow down the rotation of an object. " +
                                  "The higher the drag the more the rotation slows down. \n" + "\n" +
                                  frictionTooltip;
            drag.PGClampValue();
            angularDrag.PGClampValue();
            
            /********************************************************************************************************************************/
            // Use PGOffsetLabel() -> Only delayed because of shared update.
            var vertexLimitLabel = vertexLimit.Q<Label>();
            vertexLimitLabel.style.paddingLeft = 12f;
            var fallbackColliderLabel = fallbackCollider.Q<Label>();
            fallbackColliderLabel.style.paddingLeft = 12f;
            
            var draglabel = drag.Q<Label>();
            draglabel.style.paddingLeft = 12f;
            var angularDraglabel = angularDrag.Q<Label>();
            angularDraglabel.style.paddingLeft = 12f;
            /********************************************************************************************************************************/
        }

        private void DrawModule()
        {
            collider.RegisterValueChangedCallback(evt =>
            {
                MaterialVisualization();
            });
            rigidbody.RegisterValueChangedCallback(evt =>
            {
                RigidVisualization();
            });
        }

        private void MaterialVisualization()
        {
            if (_subModulePhysics.collider == Enums.Collider.None)
            {
                ColliderWrapper.style.display = DisplayStyle.None;
            }
            else
            {
                ColliderWrapper.style.display = DisplayStyle.Flex;
            }
            if (_subModulePhysics.collider == Enums.Collider.Automatic)
            {
                automaticColliderWrapper.style.display = DisplayStyle.Flex;
            }
            else
            {
                automaticColliderWrapper.style.display = DisplayStyle.None;
            }
        }

        private void RigidVisualization()
        {
            rigidbodyWrapper.style.display = _subModulePhysics.rigidbody ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}