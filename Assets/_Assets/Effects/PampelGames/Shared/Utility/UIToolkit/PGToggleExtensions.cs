// ---------------------------------------------------
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ---------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;

namespace PampelGames.Shared.Utility
{
    public static class PGToggleExtensions
    {

        /// <summary>
        ///     Corrects the wrong alignment of the default Toggle.
        /// </summary>
        public static void PGToggleStyleDefault(this Toggle toggle)
        {
            var label = toggle.Q<Label>();
            label.RemoveFromClassList(PGConstantsUSS.ToggleLabel);
            label.AddToClassList(PGConstantsUSS.BaseTextFieldLabel);
            label.AddToClassList(PGConstantsUSS.FloatFieldLabel);
            label.AddToClassList(PGConstantsUSS.BaseFieldLabelWithDragger);
            
            using var visualChildrenIterator = toggle.Children().GetEnumerator();
            while (visualChildrenIterator.MoveNext())
            {
                if (visualChildrenIterator.Current is not ({ } element and not Label)) continue;
                element.style.marginTop = 2f;
                break;
            }
        }
        

        
        
    }
}
