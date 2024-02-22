using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenFPS : MonoBehaviour
{
    public bool DisplayFramerate = false;

    private float _deltaTime = 0.0f;

    private void Update()
    {
        if (GameManager.Instance.GamePaused) return;
        if (DisplayFramerate)
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        }
    }

    void OnGUI()
    {
        if (DisplayFramerate)
        {
            int width = Screen.width, height = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, width, height * 2 / 100);
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = height * 2 / 100;
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            float msec = _deltaTime * 1000.0f;
            float fps = 1.0f / _deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}
