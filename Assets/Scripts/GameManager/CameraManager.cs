using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum CameraFilter
{
    NONE,
    RAIN,
    DUST_STORM,
    OLD,
    SHYRON,
    SHADOW_MEDAL
}

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public CinemachineVirtualCamera defaultVirtualCamera;
    private CinemachineBasicMultiChannelPerlin defaultCameraNoise;
    public Camera defaultCamera;

    public CameraFilter filter;

    public float defaultZoom;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeCameraNoise();
    }

    private void Start()
    {
        defaultZoom = defaultVirtualCamera.m_Lens.OrthographicSize;
    }

    public CameraFilter GetFilter()
    {
        return this.filter;
    }

    public void SetFilter(CameraFilter filter)
    {
        this.filter = filter;
        UpdateFilter();
    }

    public void UpdateFilter()
    {
        switch (filter)
        {
            case CameraFilter.NONE:
                SetChromaticAberrationEffect(0, 0.5f, defaultCamera);
                ChangeCameraColor(Color.white, 0.5f, defaultCamera);
                SetVignetteEffect(0f, 0f, 0.5f, defaultCamera);
                SetFilmGrainEffect(0f, 0.5f);
                SetBloomEffect(0f, 0.5f);
                SetLensDistortionEffect(0f, 0.5f);
                SetDepthOfFieldEffect(0f, 0f, 0.5f);
                break;
            case CameraFilter.RAIN:
                ChangeCameraColor(new Color(117 / 255f, 129 / 255f, 255 / 255f), .5f, defaultCamera);
                break;
            case CameraFilter.DUST_STORM:
                SetChromaticAberrationEffect(0.05f, 0.5f, defaultCamera); // quasi rien
                ChangeCameraColor(new Color(220f / 255f, 180f / 255f, 100f / 255f), 0.5f, defaultCamera); // teinte sable/orange
                SetVignetteEffect(0.25f, 0.7f, 0.5f, defaultCamera); // vignette douce
                SetFilmGrainEffect(3f, 0.5f); // bruit moyen pour simuler poussière
                SetBloomEffect(0.4f, 0.5f); // légère diffusion lumineuse
                SetLensDistortionEffect(-0.1f, 0.5f); // très subtile
                SetDepthOfFieldEffect(0.7f, 8f, 0.5f); // vision moins nette
                break;
            case CameraFilter.OLD:
                SetChromaticAberrationEffect(0.3f, 1f, defaultCamera); // Aberration légère
                ChangeCameraColor(new Color(190f / 255f, 180f / 255f, 150f / 255f), 0.5f, defaultCamera); // Teinte terne
                SetVignetteEffect(0.6f, 0.9f, .2f, defaultCamera); // Vignettage prononcé
                SetFilmGrainEffect(10f, .2f); // Bruit fort pour effet granuleux
                SetBloomEffect(0.5f, .2f); // Lueur très douce
                SetLensDistortionEffect(-1f, .2f); // Légère distorsion de lentille
                SetDepthOfFieldEffect(1f, 7f, .2f); // Flou d’arrière-plan marqué
                break;
            case CameraFilter.SHYRON:
                SetChromaticAberrationEffect(0.5f, 0.5f, defaultCamera); // Aberration marquée
                ChangeCameraColor(new Color(100f / 255f, 80f / 255f, 110f / 255f), 0.5f, defaultCamera); // Teinte violette sombre
                SetVignetteEffect(0.3f, 0.9f, 0.5f, defaultCamera); // Vignettage fort
                SetFilmGrainEffect(5f, 0.5f); // Bruit moyen
                SetBloomEffect(0.3f, 0.5f); // Lueur discrète
                SetLensDistortionEffect(-0.5f, 0.5f); // Distorsion subtile
                SetDepthOfFieldEffect(0.5f, 5f, 0.5f); // Léger flou d’arrière-plan
                break;
            case CameraFilter.SHADOW_MEDAL:
                ChangeCameraColor(new Color(206f / 255f, 0f / 255f, 204f / 255f), 1, defaultCamera);
                ShakeCamera(3, 3, 1f);
                SetVignetteEffect(.5f, .8f, 1f, defaultCamera);
                break;
            default:
                break;
        }
    }

    private void InitializeCameraNoise()
    {
        if (defaultVirtualCamera != null)
        {
            defaultCameraNoise = defaultVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (defaultCameraNoise == null)
            {
                Debug.LogError("CinemachineBasicMultiChannelPerlin component is missing on the CinemachineVirtualCamera.");
            }
        }
        else
        {
            Debug.LogError("Default Camera is not assigned.");
        }
    }

    public void SetLensDistortionEffect(float intensity, float transitionTime)
    {
        if (postProcessVolume != null)
        {
            VolumeProfile profile = postProcessVolume.profile;
            if (profile.TryGet<LensDistortion>(out var lensDistortion))
            {
                StartCoroutine(LensDistortionTransitionRoutine(lensDistortion, intensity, transitionTime));
            }
            else
            {
                Debug.LogWarning("LensDistortion component not found in VolumeProfile.");
            }
        }
    }

    public void SetDepthOfFieldEffect(float focusDistance, float aperture, float transitionTime)
    {
        if (postProcessVolume != null)
        {
            VolumeProfile profile = postProcessVolume.profile;
            if (profile.TryGet<DepthOfField>(out var dof))
            {
                StartCoroutine(DepthOfFieldTransitionRoutine(dof, focusDistance, aperture, transitionTime));
            }
            else
            {
                Debug.LogWarning("DepthOfField component not found in VolumeProfile.");
            }
        }
    }

    private IEnumerator DepthOfFieldTransitionRoutine(DepthOfField dof, float targetFocusDistance, float targetAperture, float transitionTime)
    {
        float startFocus = dof.focusDistance.value;
        float startAperture = dof.aperture.value;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            dof.focusDistance.value = Mathf.Lerp(startFocus, targetFocusDistance, t);
            dof.aperture.value = Mathf.Lerp(startAperture, targetAperture, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        dof.focusDistance.value = targetFocusDistance;
        dof.aperture.value = targetAperture;
    }


    private IEnumerator LensDistortionTransitionRoutine(LensDistortion lensDistortion, float targetIntensity, float transitionTime)
    {
        float startIntensity = lensDistortion.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        lensDistortion.intensity.value = targetIntensity;
    }


    public void SetBloomEffect(float intensity, float transitionTime)
    {
        if (postProcessVolume != null)
        {
            VolumeProfile profile = postProcessVolume.profile;
            if (profile.TryGet<Bloom>(out var bloom))
            {
                StartCoroutine(BloomTransitionRoutine(bloom, intensity, transitionTime));
            }
            else
            {
                Debug.LogWarning("Bloom component not found in VolumeProfile.");
            }
        }
    }

    private IEnumerator BloomTransitionRoutine(Bloom bloom, float targetIntensity, float transitionTime)
    {
        float startIntensity = bloom.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            bloom.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        bloom.intensity.value = targetIntensity;
    }


    public void SetFilmGrainEffect(float intensity, float transitionTime)
    {
        if (postProcessVolume != null)
        {
            VolumeProfile profile = postProcessVolume.profile;
            if (profile.TryGet<FilmGrain>(out var filmGrain))
            {
                StartCoroutine(FilmGrainTransitionRoutine(filmGrain, intensity, transitionTime));
            }
            else
            {
                Debug.LogWarning("FilmGrain component not found in VolumeProfile.");
            }
        }
    }

    private IEnumerator FilmGrainTransitionRoutine(FilmGrain filmGrain, float targetIntensity, float transitionTime)
    {
        float startIntensity = filmGrain.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            filmGrain.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        filmGrain.intensity.value = targetIntensity;
    }


    // Effet de shake
    public void ShakeCamera(float amplitude, float frequency, float duration, CinemachineVirtualCamera camera = null)
    {
        CinemachineBasicMultiChannelPerlin noise = GetCameraNoise(camera);
        if (noise != null)
        {
            StartCoroutine(ShakeRoutine(noise, amplitude, frequency, duration));
        }
        else
        {
            Debug.LogError("Camera noise component is not initialized.");
        }
    }

    // ShakeCamera Avec caméra par défaut
    public void ShakeCamera(float amplitude, float frequency, float duration)
    {
        CinemachineBasicMultiChannelPerlin noise = this.defaultCameraNoise;
        if (noise != null)
        {
            StartCoroutine(ShakeRoutine(noise, amplitude, frequency, duration));
        }
        else
        {
            Debug.LogError("Camera noise component is not initialized.");
        }
    }

    private IEnumerator ShakeRoutine(CinemachineBasicMultiChannelPerlin noise, float amplitude, float frequency, float duration)
    {
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
        yield return new WaitForSecondsRealtime(duration);
        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }

    // Zoom
    public void ZoomCamera(float targetOrthoSize, float zoomSpeed, CinemachineVirtualCamera camera = null)
    {
        StartCoroutine(ZoomRoutine(targetOrthoSize, zoomSpeed, camera));
    }

    public void ResetCameraZoom()
    {
        StartCoroutine(ZoomRoutine(defaultZoom, 1));
    }

    // Remplacez la méthode ZoomRoutine existante par celle-ci :

    private IEnumerator ZoomRoutine(float targetOrthoSize, float duration, CinemachineVirtualCamera camera = null)
    {
        if (camera == null) camera = defaultVirtualCamera;
        if (camera == null) yield break;

        float startOrthoSize = camera.m_Lens.OrthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            camera.m_Lens.OrthographicSize = Mathf.Lerp(startOrthoSize, targetOrthoSize, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        camera.m_Lens.OrthographicSize = targetOrthoSize;
    }


    // Dezoom (reset to default ortho size)
    public void DezoomCamera(float defaultOrthoSize, float zoomSpeed, CinemachineVirtualCamera camera = null)
    {
        ZoomCamera(defaultOrthoSize, zoomSpeed, camera);
    }

    // Référence publique pour le Volume
    public Volume postProcessVolume;

    public void ChangeCameraColor(Color targetColor, float transitionSpeed, Camera camera = null)
    {
        // Utiliser la caméra fournie ou chercher la caméra de la CinemachineVirtualCamera
        if (camera == null)
        {
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                camera = virtualCamera.GetComponent<Camera>();
            }
        }

        // Assurer que la caméra est définie
        if (camera != null)
        {
            // Utiliser la référence publique de Volume
            if (postProcessVolume != null)
            {
                VolumeProfile profile = postProcessVolume.profile;
                ColorAdjustments colorAdjustments;
                if (profile.TryGet(out colorAdjustments))
                {
                    StartCoroutine(ChangeColorRoutine(colorAdjustments, targetColor, transitionSpeed));
                }
                else
                {
                    Debug.LogWarning("ColorAdjustments component not found in VolumeProfile.");
                }
            }
            else
            {
                Debug.LogWarning("Volume is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("No camera provided or found.");
        }
    }

    private IEnumerator ChangeColorRoutine(ColorAdjustments colorAdjustments, Color targetColor, float transitionSpeed)
    {
        Color startColor = colorAdjustments.colorFilter.value;
        float elapsedTime = 0f;

        // Calcul de la durée de transition (1 / transitionSpeed)
        float transitionDuration = 1f / transitionSpeed;

        while (elapsedTime < transitionDuration)
        {
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        colorAdjustments.colorFilter.value = targetColor;
    }


    private CinemachineBasicMultiChannelPerlin GetCameraNoise(CinemachineVirtualCamera camera)
    {
        if (camera == null) camera = defaultVirtualCamera;
        return camera != null ? camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() : null;
    }


    public void SetVignetteEffect(float intensity, float smoothness, float transitionTime, Camera camera = null)
    {
        if (camera == null)
        {
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                camera = virtualCamera.GetComponent<Camera>();
            }
        }

        if (camera != null)
        {
            if (postProcessVolume != null)
            {
                VolumeProfile profile = postProcessVolume.profile;
                if (profile.TryGet<Vignette>(out var vignette))
                {
                    StartCoroutine(VignetteTransitionRoutine(vignette, intensity, smoothness, transitionTime));
                }
                else
                {
                    Debug.LogWarning("Vignette component not found in VolumeProfile.");
                }
            }
            else
            {
                Debug.LogWarning("PostProcessVolume is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("No camera provided or found.");
        }
    }

    private IEnumerator VignetteTransitionRoutine(Vignette vignette, float targetIntensity, float targetSmoothness, float transitionTime)
    {
        float startIntensity = vignette.intensity.value;
        float startSmoothness = vignette.smoothness.value;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float t = transitionTime > 0 ? elapsedTime / transitionTime : 1f;
            vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
            vignette.smoothness.value = Mathf.Lerp(startSmoothness, targetSmoothness, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        vignette.intensity.value = targetIntensity;
        vignette.smoothness.value = targetSmoothness;
    }

    public void SetChromaticAberrationEffect(float intensity, float transitionTime, Camera camera = null)
    {
        if (camera == null)
        {
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera != null)
            {
                camera = virtualCamera.GetComponent<Camera>();
            }
        }

        if (camera != null)
        {
            if (postProcessVolume != null)
            {
                VolumeProfile profile = postProcessVolume.profile;
                if (profile.TryGet<ChromaticAberration>(out var chromaticAberration))
                {
                    StartCoroutine(ChromaticAberrationTransitionRoutine(chromaticAberration, intensity, transitionTime));
                }
                else
                {
                    Debug.LogWarning("ChromaticAberration component not found in VolumeProfile.");
                }
            }
            else
            {
                Debug.LogWarning("PostProcessVolume is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("No camera provided or found.");
        }
    }

    private IEnumerator ChromaticAberrationTransitionRoutine(ChromaticAberration chromaticAberration, float targetIntensity, float transitionTime)
    {
        float startIntensity = chromaticAberration.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < transitionTime)
        {
            float t = transitionTime > 0 ? elapsedTime / transitionTime : 1f;
            chromaticAberration.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        chromaticAberration.intensity.value = targetIntensity;
    }

}
