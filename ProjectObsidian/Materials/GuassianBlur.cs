using System;
using Elements.Core;
using FrooxEngine;
using Obsidian.Shaders;

[Category(new string[] { "Assets/Materials/Obsidian/Effects" })]
public class GaussianBlurMaterial : SingleShaderMaterialProvider
{
    protected override Uri ShaderURL => ShaderInjection.GaussianBlur;

    [Range(1f, 10f, "0.00")]
    public readonly Sync<float> Iterations;

    public readonly Sync<float2> Spread;

    public readonly AssetRef<ITexture2D> SpreadTexture;

    [Range(0f, 0.1f, "0.00")]
    public readonly Sync<float> RefractionStrength;

    [Range(0.1f, 10f, "0.00")]
    public readonly Sync<float> DepthDivisor;

    public readonly AssetRef<ITexture2D> NormalMap;

    public readonly Sync<float> SrcBlend;
    public readonly Sync<float> DstBlend;
    public readonly Sync<float> ZWrite;
    public readonly Sync<float> Cull;

    [Range(0f, 255f, "0")]
    public readonly Sync<float> StencilComp;
    [Range(0f, 255f, "0")]
    public readonly Sync<float> Stencil;
    [Range(0f, 255f, "0")]
    public readonly Sync<float> StencilOp;
    [Range(0f, 255f, "0")]
    public readonly Sync<float> StencilWriteMask;
    [Range(0f, 255f, "0")]
    public readonly Sync<float> StencilReadMask;
    [Range(0f, 15f, "0")]
    public readonly Sync<float> ColorMask;

    private static MaterialProperty _Iterations = new MaterialProperty("_Iterations");
    private static MaterialProperty _Spread = new MaterialProperty("_Spread");
    private static MaterialProperty _SpreadTex = new MaterialProperty("_SpreadTex");
    private static MaterialProperty _RefractionStrength = new MaterialProperty("_RefractionStrength");
    private static MaterialProperty _DepthDivisor = new MaterialProperty("_DepthDivisor");
    private static MaterialProperty _NormalMap = new MaterialProperty("_NormalMap");
    private static MaterialProperty _SrcBlend = new MaterialProperty("_SrcBlend");
    private static MaterialProperty _DstBlend = new MaterialProperty("_DstBlend");
    private static MaterialProperty _ZWrite = new MaterialProperty("_ZWrite");
    private static MaterialProperty _Cull = new MaterialProperty("_Cull");
    private static MaterialProperty _StencilComp = new MaterialProperty("_StencilComp");
    private static MaterialProperty _Stencil = new MaterialProperty("_Stencil");
    private static MaterialProperty _StencilOp = new MaterialProperty("_StencilOp");
    private static MaterialProperty _StencilWriteMask = new MaterialProperty("_StencilWriteMask");
    private static MaterialProperty _StencilReadMask = new MaterialProperty("_StencilReadMask");
    private static MaterialProperty _ColorMask = new MaterialProperty("_ColorMask");

    [DefaultValue(-1)]
    public readonly Sync<int> RenderQueue;

    private static bool _propertiesInitialized;

    public override bool PropertiesInitialized
    {
        get => _propertiesInitialized;
        protected set => _propertiesInitialized = value;
    }

    protected override void UpdateMaterial(ref MaterialUpdateWriter material)
    {
        material.UpdateFloat(_Iterations, Iterations);
        material.UpdateFloat2(_Spread, Spread);
        material.UpdateTexture(_SpreadTex, SpreadTexture);
        material.UpdateFloat(_RefractionStrength, RefractionStrength);
        material.UpdateFloat(_DepthDivisor, DepthDivisor);
        material.UpdateTexture(_NormalMap, NormalMap);

        material.UpdateFloat(_SrcBlend, SrcBlend);
        material.UpdateFloat(_DstBlend, DstBlend);
        material.UpdateFloat(_ZWrite, ZWrite);
        material.UpdateFloat(_Cull, Cull);

        material.UpdateFloat(_StencilComp, StencilComp);
        material.UpdateFloat(_Stencil, Stencil);
        material.UpdateFloat(_StencilOp, StencilOp);
        material.UpdateFloat(_StencilWriteMask, StencilWriteMask);
        material.UpdateFloat(_StencilReadMask, StencilReadMask);
        material.UpdateFloat(_ColorMask, ColorMask);

        if (!RenderQueue.GetWasChangedAndClear()) return;
        var renderQueue = RenderQueue.Value;
        if (renderQueue == -1) renderQueue = 3000; // Transparent+500
        material.SetRenderQueue(renderQueue);
    }

    protected override void UpdateKeywords(ShaderKeywords keywords) { }

    protected override void OnAttach()
    {
        base.OnAttach();

        // Default values from shader
        Iterations.Value = 4f;
        Spread.Value = new float2(0.1f, 0.1f);
        RefractionStrength.Value = 0.01f;
        DepthDivisor.Value = 1f;

        SrcBlend.Value = 1f; // One
        DstBlend.Value = 0f; // Zero
        ZWrite.Value = 1f;
        Cull.Value = 2f; // Back

        StencilComp.Value = 8f;
        Stencil.Value = 0f;
        StencilOp.Value = 0f;
        StencilWriteMask.Value = 255f;
        StencilReadMask.Value = 255f;
        ColorMask.Value = 15f;
    }
}