using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Visual polish and visitor HUD for the NPC museum scene.</summary>
public sealed class MuseumSceneEnhancements : MonoBehaviour
{
    public static int ViewedPaintCount { get; private set; }
    public static int ViewedStatueCount { get; private set; }
    public static int TotalPaintCount { get; private set; }
    public static int TotalStatueCount { get; private set; }
    private static readonly Color[] Wardrobe =
    {
        new Color(.24f,.36f,.58f), new Color(.48f,.19f,.16f), new Color(.18f,.42f,.34f),
        new Color(.56f,.39f,.17f), new Color(.38f,.25f,.48f), new Color(.16f,.39f,.48f),
        new Color(.55f,.28f,.22f), new Color(.33f,.39f,.22f)
    };
    private static readonly Color[] SkinTones =
    {
        new Color(.78f,.55f,.39f), new Color(.57f,.35f,.23f), new Color(.88f,.68f,.52f),
        new Color(.69f,.44f,.30f), new Color(.83f,.61f,.43f)
    };
    private readonly HashSet<string> viewedPaintings = new HashSet<string>();
    private readonly HashSet<string> viewedStatues = new HashSet<string>();
    [SerializeField, Min(60f)] private float dayToSunsetCycleSeconds = 360f;
    private Light sun;
    private Renderer skyRenderer;
    private Material skyMaterial;
    private float nextDaylightUpdate;
    private Paint focusedPaint;
    private StatueInteractable focusedStatue;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindAnyObjectByType<MuseumSceneEnhancements>() != null) return;
        var host = new GameObject("Museum Scene Enhancements");
        host.AddComponent<MuseumSceneEnhancements>();
    }

    private void Awake()
    {
        ViewedPaintCount = 0;
        ViewedStatueCount = 0;
        TotalPaintCount = FindObjectsByType<Paint>(FindObjectsSortMode.None).Length;
        TotalStatueCount = FindObjectsByType<StatueInteractable>(FindObjectsSortMode.None).Length;
        ColorVisitors();
        SetupArtwork();

       // SetupStatues();

        SetupCeramicAccents();
        SetupPostProcessing();
        var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights) if (light.type == LightType.Directional) { sun = light; break; }
        var sky = GameObject.Find("EsferaConSky");
        if (sky != null && sky.TryGetComponent(out skyRenderer)) skyMaterial = skyRenderer.material;
    }

    private void Update()
    {
        var cam = Camera.main;
        IInteractablePrompt next = null;
        if (cam != null && Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 3f))
            next = (IInteractablePrompt)hit.collider.GetComponentInParent<Paint>() ?? hit.collider.GetComponentInParent<StatueInteractable>();
        var nextPaint = next as Paint;
        var nextStatue = next as StatueInteractable;
        if (focusedPaint != nextPaint || focusedStatue != nextStatue)
        {
            if (focusedPaint != null) SetArtworkGlow(focusedPaint, false);
            focusedPaint = nextPaint;
            focusedStatue = nextStatue;
            if (focusedPaint != null) SetArtworkGlow(focusedPaint, true);
        }
        MuseumRuntimeUI.SetFocus(next != null ? next.PromptMessage : null);
        if (Time.time >= nextDaylightUpdate)
        {
            UpdateDaylight();
            nextDaylightUpdate = Time.time + .5f;
        }
    }

    public static void RegisterViewed(Paint paint)
    {
        var ui = FindAnyObjectByType<MuseumSceneEnhancements>();
        if (ui != null && paint != null && ui.viewedPaintings.Add(paint.name)) ViewedPaintCount = ui.viewedPaintings.Count;
    }

    public static void RegisterViewed(StatueInteractable statue)
    {
        var ui = FindAnyObjectByType<MuseumSceneEnhancements>();
        if (ui != null && statue != null && ui.viewedStatues.Add(statue.name)) ViewedStatueCount = ui.viewedStatues.Count;
    }

    private void ColorVisitors()
    {
        var visitors = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        for (int n = 0; n < visitors.Length; n++)
        {
            var renderers = visitors[n].GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                Color skinTone = SkinTones[n % SkinTones.Length];
                Color clothingTone = Wardrobe[n % Wardrobe.Length];
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    if (mat == null) continue;
                    string key = (mat.name + " " + renderer.name).ToLowerInvariant();
                    Color color = key.Contains("skin") || key.Contains("body") || key.Contains("face")
                        ? skinTone : clothingTone;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                }
                renderer.materials = materials;
            }
        }
    }

    private void SetupArtwork()
    {
        foreach (var paint in FindObjectsByType<Paint>(FindObjectsSortMode.None))
        {
            Transform anchor = paint.transform.Find("MuseumGlow");
            if (anchor == null)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "MuseumGlow";
                marker.transform.SetParent(paint.transform, false);
                marker.transform.localPosition = new Vector3(0f, .62f, -.05f);
                marker.transform.localScale = Vector3.one * .11f;
                var collider = marker.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                var renderer = marker.GetComponent<Renderer>();
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = new Color(1f, .72f, .34f);
                if (mat.HasProperty("_EmissionColor")) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", new Color(1f,.45f,.12f)*1.6f); }
                renderer.material = mat;
                anchor = marker.transform;
            }
        }
    }

    private void SetArtworkGlow(Paint paint, bool active)
    {
        var marker = paint.transform.Find("MuseumGlow");
        if (marker != null)
        {
            marker.localScale = Vector3.one * (active ? .16f : .11f);
            var renderer = marker.GetComponent<Renderer>();
            var material = renderer != null ? renderer.material : null;
            if (material != null && material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", new Color(1f,.45f,.12f) * (active ? 2.8f : 1.0f));
        }
    }

    private void SetupCeramicAccents()
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            string name = t.name.ToLowerInvariant();
            if (!(name.Contains("ceram") || name.Contains("vasija") || name.Contains("jarr"))) continue;
            if (t.GetComponentInChildren<Light>() != null) continue;
            var go = new GameObject("Ceramic Accent Light");
            var renderers = t.GetComponentsInChildren<Renderer>(true);
            Vector3 position = t.position + Vector3.up * 1.8f;
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                position = new Vector3(bounds.center.x, bounds.max.y + .3f, bounds.center.z);
            }
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, .79f, .55f);
            light.intensity = .55f;
            light.range = 3f;
            light.spotAngle = 65f;
            light.shadows = LightShadows.None;
        }
    }

    //This is a temporary solution to make the statues look better in the museum scene. Ideally, the statues should be modeled with proper pedestals and colliders.
    private void SetupStatues()
    {
        const float galleryFloor = 12.9f;
        foreach (var statue in FindObjectsByType<StatueInteractable>(FindObjectsSortMode.None))
        {
            
            var renderers = statue.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) continue;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            statue.transform.position += Vector3.up * (galleryFloor - bounds.min.y);
            
            if (statue.GetComponentInChildren<Collider>() == null)
            {
                var collider = statue.gameObject.AddComponent<BoxCollider>();
                Vector3 localCenter = statue.transform.InverseTransformPoint(bounds.center);
                Vector3 localSize = statue.transform.InverseTransformVector(bounds.size);
                collider.center = localCenter;
                collider.size = new Vector3(Mathf.Max(.25f, Mathf.Abs(localSize.x)), Mathf.Max(.35f, Mathf.Abs(localSize.y)), Mathf.Max(.25f, Mathf.Abs(localSize.z)));
            }
            
            string pedestalName = statue.name + " · Pedestal";
            if (GameObject.Find(pedestalName) != null) continue;
            float diameter = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) * .55f, .7f, 1.6f);
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = pedestalName;
            pedestal.transform.position = new Vector3(bounds.center.x, galleryFloor + .14f, bounds.center.z);
            pedestal.transform.localScale = new Vector3(diameter, .14f, diameter);
            
            var pedestalCollider = pedestal.GetComponent<Collider>();
            if (pedestalCollider != null) Destroy(pedestalCollider);
            var pedestalRenderer = pedestal.GetComponent<Renderer>();
            pedestalRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            pedestalRenderer.receiveShadows = false;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Color stone = new Color(.38f,.32f,.25f);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor",stone);
            if (material.HasProperty("_Color")) material.SetColor("_Color",stone);
            pedestalRenderer.material = material;
        }
    }

    private void SetupPostProcessing()
    {
        var volume = FindAnyObjectByType<Volume>();
        if (volume == null) return;
        var profile = volume.profile;
        if (profile == null) return;
        if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
        bloom.active = true; bloom.threshold.Override(1.35f); bloom.intensity.Override(.12f); bloom.scatter.Override(.35f); bloom.highQualityFiltering.Override(false);
        if (!profile.TryGet(out Vignette vignette)) vignette = profile.Add<Vignette>(true);
        vignette.active = true; vignette.intensity.Override(.08f); vignette.smoothness.Override(.45f);
        if (!profile.TryGet(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
        color.active = true; color.postExposure.Override(.05f); color.contrast.Override(3f); color.saturation.Override(4f);
        if (profile.TryGet(out MotionBlur motionBlur)) motionBlur.active = false;
    }

    private void UpdateDaylight()
    {
        float phase = Mathf.PingPong(Time.time / Mathf.Max(60f, dayToSunsetCycleSeconds), 1f);
        float sunset = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.64f, 1f, phase));
        Color day = new Color(.77f,.86f,1f);
        Color warm = new Color(1f,.53f,.28f);
        if (sun != null)
        {
            sun.color = Color.Lerp(Color.white, new Color(1f,.68f,.45f), sunset);
            sun.intensity = Mathf.Lerp(1.0f,.72f,sunset);
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(48f,12f,sunset), 35f, 0f);
        }
        if (skyMaterial != null)
        {
            Color tint = Color.Lerp(day,warm,sunset);
            if (skyMaterial.HasProperty("_BaseColor")) skyMaterial.SetColor("_BaseColor",tint);
            if (skyMaterial.HasProperty("_Color")) skyMaterial.SetColor("_Color",tint);
        }
    }
}



