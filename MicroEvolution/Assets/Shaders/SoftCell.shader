Shader "MicroEvolution/SoftCell"
{
    Properties
    {
        _Color ("Color", Color) = (0.35, 0.8, 1, 0.72)
        _RimColor ("Rim Color", Color) = (0.7, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _Gloss ("Core Gloss", Range(0, 2)) = 0.85
        _FresnelBoost ("Fresnel Boost", Range(0, 3)) = 1.4
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
            float _RimPower;
            float _Gloss;
            float _FresnelBoost;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.localPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.worldNormal);
                float3 v = normalize(i.viewDir);
                float ndotv = saturate(dot(n, v));
                float fresnel = pow(1.0 - ndotv, _RimPower) * _FresnelBoost;

                // Fake thickness / core light from local position
                float core = saturate(1.0 - length(i.localPos) * 1.6);
                float gloss = pow(core, 2.0) * _Gloss;

                fixed4 col = _Color;
                col.rgb = col.rgb * (0.55 + gloss * 0.7) + _RimColor.rgb * fresnel;
                col.a = saturate(_Color.a * (0.55 + fresnel * 0.45 + gloss * 0.2));
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
