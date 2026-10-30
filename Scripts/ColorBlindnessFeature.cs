using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ColorBlindnessFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
    }

    public Settings settings = new Settings();
    private ColorBlindnessPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new ColorBlindnessPass(settings.material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material != null)
        {
            m_ScriptablePass.Setup(settings.material);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}