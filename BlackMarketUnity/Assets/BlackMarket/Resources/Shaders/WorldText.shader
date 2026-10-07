Shader "BlackMarket/WorldText" {
 Properties { _MainTex ("Font atlas", 2D) = "white" {} _Color ("Color", Color) = (1,1,1,1) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   Cull Off
   ZWrite Off
   ZTest LEqual
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);half4 _Color;
   V vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color*_Color;return o;}
   half4 frag(V i):SV_Target{half4 c=i.color;c.a*=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a;return c;}
   ENDHLSL
  }
 }
}
