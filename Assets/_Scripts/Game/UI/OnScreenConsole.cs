// ********************************************************************
// CONFIDENTIAL - DO NOT DISTRIBUTE
// COPYRIGHT 2019-2024 Wacky Potato Games, LLC. All Rights Reserved.
// 
// If you send, receive, or use this file for any purpose other than
// internal use by Wacky Potato Games, it is without permission and an act of theft.
// Report any misuse of this file immediately to contact@wackypotato.com
// Misuse or failure to report misuse will subject you to legal action.
// 
// The intellectual and technical concepts contained herein are
// proprietary and are protected by trade secret and/or copyright law.
// Dissemination or reproduction of this material is forbidden.
// ********************************************************************


using UnityEngine;

public class OnScreenConsole : MonoBehaviour
{
    public bool ShowConsole = true;

    private string _logText = "";
    private Vector2 _scrollPosition = Vector2.zero;
    private const int _maxLogLines = 15;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {            
            string[] lines = _logText.Split('\n');         
            if (lines.Length > _maxLogLines)
            {
                int startIndex = lines.Length - _maxLogLines + 1;
                _logText = string.Join("\n", lines, startIndex, _maxLogLines - 1);
            }

            _logText += logString + "\n";
        }
    }

    private void OnGUI()
    {
        if (ShowConsole)
        {            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height / 4));
            GUILayout.Label(_logText);
            GUILayout.EndScrollView();
        }
    }
}