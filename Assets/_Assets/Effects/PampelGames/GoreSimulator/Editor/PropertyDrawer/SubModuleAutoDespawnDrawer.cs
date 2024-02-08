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
    [CustomPropertyDrawer(typeof(SubModuleAutoDespawn))]
    public class SubModuleAutoDespawnDrawer : PropertyDrawer
    {
        private SubModuleAutoDespawn _subModuleAutoDespawn;

        private readonly FloatField despawnTimer = new("Despawn Timer");

        private VisualElement ShrinkWrapper = new();
        private Toggle shrink = new("Shrink");
        private readonly Slider startShrinking = new();

        private VisualElement MoveWrapper = new();
        private Toggle move = new("Move");
        private Vector3Field moveVector = new();
        private readonly Slider startMoving = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var listIndex = PGPropertyDrawerUtility.GetDrawingListIndex(property);
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            var objList = obj as List<SubModuleBase>;
            _subModuleAutoDespawn = (SubModuleAutoDespawn) objList[listIndex];

            FindAndBindProperties(property);
            VisualizeProperties();

            DrawModule();

            container.Add(despawnTimer);
            ShrinkWrapper.Add(shrink);
            ShrinkWrapper.Add(startShrinking);
            container.Add(ShrinkWrapper);
            MoveWrapper.Add(move);
            MoveWrapper.Add(moveVector);
            MoveWrapper.Add(startMoving);
            container.Add(MoveWrapper);

            return container;
        }

        private void FindAndBindProperties(SerializedProperty property)
        {
            despawnTimer.PGSetupBindPropertyRelative(property, nameof(SubModuleAutoDespawn.despawnTimer));
            shrink.PGSetupBindPropertyRelative(property, nameof(SubModuleAutoDespawn.shrink));
            startShrinking.PGSetupBindPropertyRelative(property, nameof(SubModuleAutoDespawn.startShrinking));
            
            move.PGSetupBindPropertyRelative(property, nameof(SubModuleAutoDespawn.move));
            moveVector.PGSetupBindPropertyRelative(property, nameof(SubModuleAutoDespawn.moveVector));
            startMoving.PGSetupBindPropertyRelative(property, nameof(SubModuleAutoDespawn.startMoving));
        }

        private void VisualizeProperties()
        {
            despawnTimer.tooltip = "Time until the detached mesh parts despawn after execution.";
            despawnTimer.PGClampValue();
            
            ShrinkWrapper.style.flexDirection = FlexDirection.Row;
            shrink.RegisterValueChangedCallback(evt => ShrinkDisplay());
            ShrinkDisplay();
            shrink.tooltip = "Reduces the scale of the objects to 0 before they despawn.";
            shrink.PGToggleStyleDefault();
            startShrinking.showInputField = true;
            startShrinking.tooltip = "Time until the mesh parts start to shrink (relative to the despawn time).";
            startShrinking.lowValue = 0f;
            startShrinking.highValue = 1f;
            startShrinking.style.flexGrow = 1f;
            
            MoveWrapper.style.flexDirection = FlexDirection.Row;
            move.RegisterValueChangedCallback(evt => MoveDisplay());
            MoveDisplay();
            move.tooltip = "Moves the objects before they despawn.";
            move.PGToggleStyleDefault();
            moveVector.tooltip = "Translation in world space.";
            moveVector.style.width = 165f;
            startMoving.showInputField = true;
            startMoving.tooltip = "Time until the mesh parts start to move (relative to the despawn time).";
            startMoving.lowValue = 0f;
            startMoving.highValue = 1f;
            startMoving.style.flexGrow = 1f;
            
        }

        private void ShrinkDisplay()
        {
            startShrinking.PGDisplayStyleFlex(_subModuleAutoDespawn.shrink);
        }
        private void MoveDisplay()
        {
            moveVector.PGDisplayStyleFlex(_subModuleAutoDespawn.move);
            startMoving.PGDisplayStyleFlex(_subModuleAutoDespawn.move);
        }
        

        private void DrawModule()
        {
        }
    }
}