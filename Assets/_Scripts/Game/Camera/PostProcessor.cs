using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(PostProcessVolume))]
public class PostProcessor : MonoBehaviour
{
    [SerializeField] 
    private PostProcessVolume _postProcessVolume;
    [SerializeField] 
    private ColorGrading _colorGrading;

    private void OnEnable()
    {
        if (_postProcessVolume == null)
            _postProcessVolume = GetComponent<PostProcessVolume>();

        if (_colorGrading == null)
            _colorGrading = _postProcessVolume.profile.GetSetting<ColorGrading>();

        UpdateBlackAndWhiteEffect(true);
    }

    public void UpdateBlackAndWhiteEffect(bool enabled)
    {
        if (_colorGrading == null)
            return;

        //var grayscaleMode = enabled ? ColorGradientMode.HorizontalGradient : ColorGradingMode.None;
        //_colorGrading.colorFilter.active = grayscaleMode;
    }
}