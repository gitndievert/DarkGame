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
    public class SO_ColorKeywords : ScriptableObject
    {
        public List<string> colorKeywords;
        
        public List<int> ComponentColorKeywordIDs()
        {
            return colorKeywords.Select(Shader.PropertyToID).ToList();
        }
    }
}
