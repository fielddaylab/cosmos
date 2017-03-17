Shader "Unlit/LabelShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		Pass
		{
			Cull front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float3 cam_position;
            float3 cam_ray;

            struct v2f {
                fixed3 worldNormal : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4 position : SV_POSITION;
            };

            v2f vert (float4 position : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                o.position = UnityObjectToClipPos(position);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.worldNormal *= -1;
                o.worldPosition = mul(unity_ObjectToWorld, position);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float window_r = 2;

                fixed4 color = 0;
                float alpha = 0;
                float x2;
                float y2;
                float z2;

                x2 = i.worldPosition.x-cam_position.x;
                x2 *= x2;
                y2 = i.worldPosition.y-cam_position.y;
                y2 *= y2;
                z2 = i.worldPosition.z-cam_position.z;
                z2 *= z2;
                float dist_from_cam = sqrt(x2+y2+z2);
                float3 proj_pt = cam_position+(cam_ray*dist_from_cam);

                x2 = i.worldPosition.x-proj_pt.x;
                x2 *= x2;
                y2 = i.worldPosition.y-proj_pt.y;
                y2 *= y2;
                z2 = i.worldPosition.z-proj_pt.z;
                z2 *= z2;
                float dist_from_proj = sqrt(x2+y2+z2);

                alpha = clamp((window_r-dist_from_proj)/window_r,0,1);

                color.rgba = float4(1,1,1,alpha);

                return color;
            }
            ENDCG
			
		}
	}
}
