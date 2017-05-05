Shader "Unlit/RayShader"
{
  Properties
  {
  }
  SubShader
  {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

    Pass
    {
      Blend SrcAlpha OneMinusSrcAlpha
      ZWrite Off

      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        float3 cam_position;
        float ray_alpha;

        struct v2f
        {
          fixed3 worldNormal : TEXCOORD0;
          float3 worldPosition : TEXCOORD1;
          half4 position : SV_POSITION;
        };

        v2f vert (float4 position : POSITION, float3 normal : NORMAL)
        {
          v2f o;
          o.position = UnityObjectToClipPos(position);
          o.worldNormal = UnityObjectToWorldNormal(normal);
          o.worldNormal *= -1;
          o.worldPosition = mul(unity_ObjectToWorld, position).xyz;
          return o;
        }
        fixed4 frag (v2f i) : SV_Target
        {
          float x2;
          float y2;
          float z2;
          fixed4 color = 0;

          float3 frag_pt = i.worldPosition;

          x2 = frag_pt.x-cam_position.x;
          x2 *= x2;
          y2 = frag_pt.y-cam_position.y;
          y2 *= y2;
          z2 = frag_pt.z-cam_position.z;
          z2 *= z2;
          float frag_cam_dist = sqrt(x2+y2+z2);

          float alpha = min(ray_alpha,frag_cam_dist/10);

          color.rgba = float4(1,1,1,alpha);

          return color;
        }
        ENDCG
    }
  }
}
