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
    [CustomPropertyDrawer(typeof(SubModuleParticleEffects))]
    public class SubModuleParticleEffectsDrawer : PropertyDrawer
    {
        private SubModuleParticleEffects _subModuleParticleEffects;
        
        
        private SerializedProperty particleClassesProperty;
        private readonly ListView particleClasses = new();
        


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var listIndex = PGPropertyDrawerUtility.GetDrawingListIndex(property);
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            var objList = obj as List<SubModuleBase>;
            _subModuleParticleEffects = (SubModuleParticleEffects) objList[listIndex];

            FindAndBindProperties(property);
            VisualizeProperties();

            CreateParticleClasses();

            container.Add(particleClasses);


            return container;
        }

        private void FindAndBindProperties(SerializedProperty property)
        {
            particleClassesProperty = property.FindPropertyRelative(nameof(SubModuleParticleEffects.particleClasses));
        }

        private void VisualizeProperties()
        {
            particleClasses.tooltip = "Particle Effects.";
        }

        private void CreateParticleClasses()
        {
            particleClasses.showBoundCollectionSize = false; // Important to avoid OutOfRangeException in BindItem.
            particleClasses.name = "particleClasses";
            particleClasses.itemsSource = _subModuleParticleEffects.particleClasses;
            
            particleClasses.PGObjectListViewStyle();
            
            particleClasses.makeItem = MakeItem;
            particleClasses.bindItem = BindItem;
        }
        
        private VisualElement MakeItem()
        {
            var item = new VisualElement();
            VisualElement SpawnTypeWrapper = new VisualElement();
            SpawnTypeWrapper.style.flexDirection = FlexDirection.Row;
            var spawnType = new EnumField();
            spawnType.name = "spawnType";
            spawnType.style.width = 152f;
            var particle = new ObjectField();
            particle.style.flexGrow = 1f;
            particle.name = "particle";
            var effect = new ObjectField();
            effect.style.flexGrow = 1f;
            effect.name = "effect";

            VisualElement AutoDespawnWrapper = new VisualElement();
            AutoDespawnWrapper.style.flexDirection = FlexDirection.Row;
            var autoDespawn = new Toggle("Auto Despawn");
            autoDespawn.name = "autoDespawn";
            autoDespawn.PGToggleStyleDefault();
            var autoDespawnTimer = new FloatField("Seconds");
            autoDespawnTimer.name = "autoDespawnTimer"; 
            autoDespawnTimer.PGClampValue();
            autoDespawnTimer.style.width = 100f;
            autoDespawnTimer.PGRemoveLabelSpace();
            autoDespawnTimer.style.marginLeft = 15f;


            var PositionWrapper = new VisualElement();
            PositionWrapper.name = "PositionWrapper";
            var position = new EnumField("Position");
            position.name = "position";

            var ExplosionPartsWrapper = new VisualElement();
            ExplosionPartsWrapper.name = "ExplosionPartsWrapper";
            ExplosionPartsWrapper.style.marginLeft = 12f;
            var maxPosition = new IntegerField("Max. Amount");
            maxPosition.PGClampValue();
            var setParentExplosionParts = new Toggle("Set Parent");
            setParentExplosionParts.name = "setParentExplosionParts";
            setParentExplosionParts.tooltip = "Parent the spawned effects to the detached parts.";
            
            var setParent = new EnumField("Set Parent");
            setParent.name = "setParent";
            

            if (_subModuleParticleEffects.addedType == nameof(GoreModuleCut))
                PositionWrapper.style.display = DisplayStyle.None;

            spawnType.tooltip = "Type of object being spawned.\n" +
                                "Particles will be played automatically.";

            autoDespawn.tooltip = "Despawn particles automatically after the effect duration or GameObjects after a specified time.";
            autoDespawnTimer.tooltip = "Time until the GameObject gets released into the pool.";
            
            position.tooltip = "Spawn Position of the particle.\n" + "\n" +
                               "Center: Character mesh center.\n" +
                               "Method: Position specified in the explosion method.\n" +
                               "Parts: One spawn in the center of each explosion part.";

            maxPosition.tooltip = "Maximum amount of particles spawned per execution. Can not be higher than the amount of explosion parts.";
            
            var rotation = new EnumField("Rotation");
            rotation.name = "rotation";
            rotation.tooltip = "Rotation of the Particle System (Z+ is forward).";

            setParent.tooltip = "Sets the parent of the particle after spawning. If Both is selected, two particles will be instantiated.";

            if (_subModuleParticleEffects.addedType == nameof(GoreModuleCut))
            {
                rotation.tooltip += "\n" + "Note that Cut Direction does not apply to multi-cutted mesh.";
            }
            
            
            particle.objectType = typeof(ParticleSystem);
            effect.objectType = typeof(GameObject);
            
            SpawnTypeWrapper.Add(spawnType);
            SpawnTypeWrapper.Add(particle);
            SpawnTypeWrapper.Add(effect);
            item.Add(SpawnTypeWrapper);
            AutoDespawnWrapper.Add(autoDespawn);
            AutoDespawnWrapper.Add(autoDespawnTimer);
            item.Add(AutoDespawnWrapper);
            PositionWrapper.Add(position);
            ExplosionPartsWrapper.Add(maxPosition);
            ExplosionPartsWrapper.Add(setParentExplosionParts);
            PositionWrapper.Add(ExplosionPartsWrapper);
            item.Add(PositionWrapper);
            item.Add(rotation);
            item.Add(setParent);
            
            return item;
        }
        
        private void BindItem(VisualElement item, int index)
        {
            particleClassesProperty.serializedObject.Update();
                
            var spawnType = item.Q<EnumField>("spawnType"); 
            var particle = item.Q<ObjectField>("particle");
            var effect = item.Q<ObjectField>("effect");
            var autoDespawn = item.Q<Toggle>("autoDespawn");
            var autoDespawnTimer = item.Q<FloatField>("autoDespawnTimer");
            var PositionWrapper = item.Q<VisualElement>("PositionWrapper");
            var position = PositionWrapper.Q<EnumField>("position");
            var maxPosition = PositionWrapper.Q<IntegerField>();
            var setParentExplosionParts = PositionWrapper.Q<Toggle>("setParentExplosionParts");
            var rotation = item.Q<EnumField>("rotation");
            var setParent = item.Q<EnumField>("setParent");

            var listClassProperty = particleClassesProperty.GetArrayElementAtIndex(index);
            
            var spawnTypeProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.spawnType));
            spawnType.BindProperty(spawnTypeProperty);
            var particleProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.particle));
            particle.BindProperty(particleProperty);
            var effectProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.effect));
            effect.BindProperty(effectProperty);
            var autoDespawnProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.autoDespawn));
            autoDespawn.BindProperty(autoDespawnProperty);
            var autoDespawnTimerProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.autoDespawnTimer));
            autoDespawnTimer.BindProperty(autoDespawnTimerProperty);
            var positionProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.positionExpl));
            position.BindProperty(positionProperty);
            var maxPositionProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.maxPosition));
            maxPosition.BindProperty(maxPositionProperty);
            setParentExplosionParts.PGSetupBindPropertyRelative(listClassProperty, nameof(SubModuleParticleEffects.ParticleWrapperClass.setParentExplosionParts));
            var rotationProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.rotationExpl));
            if (_subModuleParticleEffects.addedType == nameof(GoreModuleCut))
                rotationProperty = listClassProperty.FindPropertyRelative(nameof(SubModuleParticleEffects.ParticleWrapperClass.rotationCut));
            rotation.BindProperty(rotationProperty);
            setParent.PGSetupBindPropertyRelative(listClassProperty, nameof(SubModuleParticleEffects.ParticleWrapperClass.setParent));
            
            
            SpawnTypeDisplay();
            spawnType.RegisterValueChangedCallback(evt =>
            {
                SpawnTypeDisplay();
                AutoDespawnDisplay();
            });
            void SpawnTypeDisplay()
            {
                effect.PGDisplayStyleFlex(_subModuleParticleEffects.particleClasses[index].spawnType == Enums.SpawnType.Gameobject);
                particle.PGDisplayStyleFlex(_subModuleParticleEffects.particleClasses[index].spawnType == Enums.SpawnType.ParticleSystem);
            }

            autoDespawn.RegisterValueChangedCallback(evt =>
            {
                AutoDespawnDisplay();
            });
            AutoDespawnDisplay();
            void AutoDespawnDisplay()
            {
                autoDespawnTimer.PGDisplayStyleFlex(_subModuleParticleEffects.particleClasses[index].spawnType == Enums.SpawnType.Gameobject &&
                                                    _subModuleParticleEffects.particleClasses[index].autoDespawn);
            }
            
            PositionDisplay();
            position.RegisterValueChangedCallback(evt => PositionDisplay());
            void PositionDisplay()
            {
                maxPosition.PGDisplayStyleFlex(_subModuleParticleEffects.particleClasses[index].positionExpl == Enums.ParticlePositionExpl.Parts);
                setParentExplosionParts.PGDisplayStyleFlex(_subModuleParticleEffects.particleClasses[index].positionExpl == Enums.ParticlePositionExpl.Parts);
            }

            rotation.RegisterValueChangedCallback(evt => SetParentDisplay());
            SetParentDisplay();
            void SetParentDisplay()
            {
                if (_subModuleParticleEffects.addedType != nameof(GoreModuleCut))
                {
                    setParent.style.display = DisplayStyle.None;
                }
                else
                {
                    setParent.PGDisplayStyleFlex(_subModuleParticleEffects.particleClasses[index].rotationCut == Enums.ParticleRotationCut.CutDirection);
                }

            }
            
        }
    }
}