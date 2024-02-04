using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dark.Utility;
public class CamShake : PSingle<CamShake>
{
    [SerializeField]
    private float _defaultDuration = 1f;
    [SerializeField]
    private float _defaultIntensity = 2f;

    protected override void PDestroy()
    {
        StopAll();
    }

    public void StopAll()
    {
        StopAllCoroutines();
    }

    public void Shake()
    {
        StartCoroutine(ShakeCamera(_defaultIntensity, _defaultDuration));
    }

    public void Shake(float intensity)
    {
        StartCoroutine(ShakeCamera(intensity, _defaultDuration));
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeCamera(intensity, duration));
    }

    private IEnumerator ShakeCamera(float intensity, float duration)
    {
        Vector2 origPos = transform.localPosition;

        for (float t = 0.0f; t < duration; t += Time.deltaTime * intensity)
        {
            Vector2 tempVec = origPos + Random.insideUnitCircle / intensity;
            transform.localPosition = tempVec;
            yield return null;
        }

        transform.localPosition = origPos;
    }
}
