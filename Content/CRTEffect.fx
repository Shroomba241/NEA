sampler2D SpriteTexture : register(s0);
float2 Resolution; 

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 originalColor = tex2D(SpriteTexture, texCoord);
    
    float pixelY = texCoord.y * Resolution.y;
    
    float modY = fmod(pixelY, 6.0);

    if (modY < 3.0)
    {
        return float4(originalColor.rgb * 0.5, originalColor.a);
    }
    else
    {
        return originalColor;
    }
}

technique DarkenScanlineWholeScreen
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
