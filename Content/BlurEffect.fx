sampler2D TextureSampler : register(s0);
float2 Resolution;

float4 BlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pixel = 3.0 / Resolution;
    
    float4 color =
          tex2D(TextureSampler, texCoord + float2(-pixel.x, -pixel.y))
        + tex2D(TextureSampler, texCoord + float2(0.0f, -pixel.y))
        + tex2D(TextureSampler, texCoord + float2(pixel.x, -pixel.y))
        + tex2D(TextureSampler, texCoord + float2(-pixel.x, 0.0f))
        + tex2D(TextureSampler, texCoord)
        + tex2D(TextureSampler, texCoord + float2(pixel.x, 0.0f))
        + tex2D(TextureSampler, texCoord + float2(-pixel.x, pixel.y))
        + tex2D(TextureSampler, texCoord + float2(0.0f, pixel.y))
        + tex2D(TextureSampler, texCoord + float2(pixel.x, pixel.y));
    
    color /= 9.0;
    return color;
}

technique BlurTechnique
{
    pass P0
    {
        PixelShader = compile ps_2_0 BlurPS();
    }
}
