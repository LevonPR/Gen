Shader "MicroEvolution/SoftCell"
{
    Properties
    {
        _Color ("Color", Color) = (0.35, 0.8, 1, 0.72)
        _RimColor ("Rim Color", Color) = (0.7, 1, 1, 1)
        _InnerColor ("Inner Glow", Color) = (0.4, 0.95, 0.55, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.2
        _Gloss ("Core Gloss", Range(0, 2)) = 0.95
        _FresnelBoost ("Fresnel Boost", Range(0, 3)) = 1.6
        _Iridescence ("Iridescence", Range(0, 1)) = 0.35
        _Thickness ("Thickness", Range(0, 2)) = 1.1
        _Distort ("View Distort", Range(0, 0.2)) = 0.04
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

            fixed4 _Color;
            fixed4 _RimColor;
            fixed4 _InnerColor;
            float _RimPower;
            float _Gloss;
            float _FresnelBoost;
            float _Iridescence;
            float _Thickness;
            float _Distort;

            v2f vert (appdata v)
            {
                v2f o;
                // Subtle organic wobble in vertex stage
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                float wobble = sin(wp.x * 3.0 + _Time.y * 2.0) * cos(wp.y * 2.5 + _Time.y) * 0.015;
                float4 vtx = v.vertex;
                vtx.xyz += v.normal * wobble;
                o.pos = UnityObjectToClipPos(vtx);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, vtx).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.localPos = vtx.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.worldNormal);
                float3 v = normalize(i.viewDir);
                // Fake refraction nudge on normal
                n = normalize(n + v * _Distort);
                float ndotv = saturate(dot(n, v));
                float fresnel = pow(1.0 - ndotv, _RimPower) * _FresnelBoost;
                float fresnel2 = pow(1.0 - ndotv, _RimPower * 0.45) * 0.35;

                float core = saturate(1.0 - length(i.localPos) * (1.35 * _Thickness));
                float gloss = pow(core, 1.8) * _Gloss;

                // Iridescent shift along rim
                float3 iri = float3(
                    sin(fresnel * 6.28 + 0.0) * 0.5 + 0.5,
                    sin(fresnel * 6.28 + 2.1) * 0.5 + 0.5,
                    sin(fresnel * 6.28 + 4.2) * 0.5 + 0.5);

                fixed4 col = _Color;
                col.rgb = col.rgb * (0.45 + gloss * 0.85)
                        + _InnerColor.rgb * gloss * 0.55
                        + _RimColor.rgb * fresnel
                        + iri * fresnel2 * _Iridescence;
                col.a = saturate(_Color.a * (0.5 + fresnel * 0.5 + gloss * 0.25));
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
