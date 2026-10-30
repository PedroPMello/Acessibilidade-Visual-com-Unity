using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class ColorBlindnessPass : ScriptableRenderPass
{
    private Material m_Material;

    public ColorBlindnessPass(Material material)
    {
        m_Material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        requiresIntermediateTexture = true;
    }

    public void Setup(Material material)
    {
        m_Material = material;
    }

    public override void RecordRenderGraph(
        RenderGraph renderGraph,
        ContextContainer frameData)
    {

        if (m_Material == null)
        {
            Debug.LogError("MATERIAL NULL");
            return;
        }

        UniversalResourceData resourceData =
            frameData.Get<UniversalResourceData>();

        TextureHandle source =
            resourceData.activeColorTexture;

        if (!source.IsValid())
        {
            Debug.LogError("SOURCE INVALID");
            return;
        }

        TextureDesc sourceDesc =
            renderGraph.GetTextureDesc(source);

        sourceDesc.name = "ColorBlindnessOutput";
        sourceDesc.clearBuffer = false;
        sourceDesc.depthBufferBits = 0;

        TextureHandle destination =
            renderGraph.CreateTexture(sourceDesc);

        RenderGraphUtils.BlitMaterialParameters parameters =
            new RenderGraphUtils.BlitMaterialParameters(
                source,
                destination,
                m_Material,
                0
            );

        renderGraph.AddBlitPass(
            parameters,
            "Color Blindness Filter"
        );

        resourceData.cameraColor = destination;

    }
}