#ifndef COMMON_LIBRARY_INCLUDED
#define COMMON_LIBRARY_INCLUDED

#include "UnityCG.cginc"
#define White half3(1,1,1)
#define Black half3(0,0,0)

half _ToonThreshold;
half _ToonSmoothness;
half _ToonGloss;
half _ToonGlossSmoothness;
half3 _ToonShadowColor;
half3 _ToonGlossColor;
float2 _UV;



void InitLightingToon(half threshold, half smoothness, half3 shadowColor, half highlightSize, half highlightSmoothness, half3 highlightColor, float2 uv_map)
{
    _ToonThreshold = threshold;
    _ToonSmoothness = smoothness;
    _ToonShadowColor = shadowColor;
    _ToonGloss = highlightSize;
    _ToonGlossSmoothness = highlightSmoothness;
    _ToonGlossColor = highlightColor;
    _UV = uv_map;
}


half4 LightingToon(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
    // Create attenuation bands
    //half attenCeil = atten * 25;
    //attenCeil = round(attenCeil);
    //attenCeil /= 25;
    half attenCeil = atten;
    
    //Per-light attenuation! 
    // *****************Must double check/figure out why it's not really working rn
 //   #ifndef USING_DIRECTIONAL_LIGHT
 //       lightDir = normalize(lightDir);
 //       #if defined(POINT) || defined(POINT_COOKIE)
	//		atten = step(_ToonThreshold, attenCeil) * attenCeil;
	//	#elif defined(SPOT) || defined(SPOT_COOKIE)
	//		atten = step(_ToonThreshold, attenCeil) * attenCeil;
 //       #endif
	//#else
	//		atten = step(_ToonThreshold, attenCeil) * attenCeil;
 //   #endif
    
    
    
    half d = pow(dot(s.Normal, lightDir) * 0.5 + 0.5, max(0.8, _ToonThreshold));
	half shadow = smoothstep(0.5, _ToonSmoothness, d);
	half3 shadowColor = lerp(_ToonShadowColor, half3(1, 1, 1), shadow);

    half f = pow(dot(s.Normal, lightDir), max(30, _ToonGloss));
    half highlight = 1 - smoothstep(0.5, max(0.5, _ToonGlossSmoothness), f);
    half3 highlightColor = lerp(_ToonGlossColor, half3(1, 1, 1), highlight);

    

 

    
    half4 c;
    c.rgb = s.Albedo * d * _LightColor0.rgb * attenCeil * shadowColor * highlightColor;
	c.a = s.Alpha;
    return c;
} 







#endif