Shader "Custom/Outline" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1) // Warna putih elegan
        _Outline ("Outline Width", Range (.002, 0.05)) = .008
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        Pass {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            float _Outline;
            float4 _OutlineColor;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 norm   = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(norm.xy);

                o.pos.xy += offset * o.pos.z * _Outline;
                o.color = _OutlineColor;
                return o;
            }

            half4 frag(v2f i) : SV_Target {
                return i.color;
            }
            ENDCG
        }
    }
}
