using UnityEngine;

[ExecuteInEditMode]
public class FixPerspectiveScale : MonoBehaviour {
#if UNITY_EDITOR
    public Vector3 Scale = new Vector3(1f, 1f, 1f);
    public FixPerspectiveScaleConfig PerspectiveScaleConfig;
    private Transform _transform;
    private Camera _cam;
    private float _scaleMult;
    private float _frustrumInnerAngle;
    private float _camFrustWidthShouldBe;
    private float _dist;

    private void Awake() {
        _transform = GetComponent<Transform>();
        _cam = Camera.main;
        PerspectiveScaleConfig = FindObjectOfType<FixPerspectiveScaleConfig>();
    }

    private void OnEnable() {
        PerspectiveScaleConfig = FindObjectOfType<FixPerspectiveScaleConfig>();
    }

    private void Update() {
        if (!PerspectiveScaleConfig)
            return;
        _frustrumInnerAngle = (180f - Camera.main.fieldOfView) / 2f * Mathf.PI / 180f;

        _camFrustWidthShouldBe = (float) PerspectiveScaleConfig.ReferenceResolutionHeight /
                                 PerspectiveScaleConfig.PixelPerUnit;
        _dist = Mathf.Tan(_frustrumInnerAngle) * (_camFrustWidthShouldBe / 2);
        _dist /= PerspectiveScaleConfig.PixelScale;
        _scaleMult = _transform.position.z / _dist;
        _transform.localScale =
            new Vector3(_scaleMult * Scale.x, _scaleMult * Scale.y, _scaleMult * Scale.z);
    }

#endif
}