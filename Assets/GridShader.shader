Shader "Unlit/GridShader"
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
      ZWrite Off

      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        float3 cam_position;
        float3 lazy_origin_ray;
        float snapped_lazy_origin_pitch;
        float snapped_lazy_origin_yaw;
        float snapped_goal_origin_pitch;
        float snapped_goal_origin_yaw;
        float grid_resolution;
        float grid_alpha;

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
          float frag_origin_pitch_display_threshhold = 0.99;
          float frag_origin_yaw_display_threshhold = 0.999;
          float frag_origin_pitch_notches = 90/grid_resolution;
          float frag_origin_yaw_notches = 360/grid_resolution;
          float window_r = 2;

          fixed4 color = 0;
          float band = 0;
          float lazy_shade = 1;
          float goal_shade = 1;
          float x2;
          float y2;
          float z2;

          float3 frag_pt = i.worldPosition;
          float3 frag_origin_ray = normalize(frag_pt);

          x2 = frag_origin_ray.x;
          x2 *= x2;
          y2 = frag_origin_ray.y;
          y2 *= y2;
          z2 = frag_origin_ray.z;
          z2 *= z2;
          float frag_plane_origin_dist = sqrt(x2+z2);
          float frag_origin_dist = sqrt(x2+y2+z2);

          x2 = frag_origin_ray.x-cam_position.x;
          x2 *= x2;
          y2 = frag_origin_ray.y-cam_position.y;
          y2 *= y2;
          z2 = frag_origin_ray.z-cam_position.z;
          z2 *= z2;
          float frag_plane_cam_dist = sqrt(x2+z2);
          float frag_cam_dist = sqrt(x2+y2+z2);

          x2 = lazy_origin_ray.x;
          x2 *= x2;
          y2 = lazy_origin_ray.y;
          y2 *= y2;
          z2 = lazy_origin_ray.z;
          z2 *= z2;
          float lazy_plane_origin_dist = sqrt(x2+z2);
          float lazy_origin_dist = sqrt(x2+y2+z2); //should always be 1

          float frag_origin_pitch = atan2(frag_origin_ray.y,frag_plane_origin_dist); //pitch is angle of elevation
          float frag_origin_pitch_band = cos(frag_origin_pitch*4*frag_origin_pitch_notches); //pitch oscillates n times between straight ahead and straight up
          frag_origin_pitch_band = (frag_origin_pitch_band+1)/2; //pitch oscillates between 0 and 1
          frag_origin_pitch_band = (frag_origin_pitch_band-frag_origin_pitch_display_threshhold)/(1-frag_origin_pitch_display_threshhold); //pitch is - below threshhold, 0 at threshhold, and 1 at 1
          frag_origin_pitch_band = max(0,frag_origin_pitch_band);

          float frag_origin_yaw = atan2(frag_origin_ray.z,frag_origin_ray.x); //yaw is angle of cardinal direction
          float frag_origin_yaw_band = cos(frag_origin_yaw*frag_origin_yaw_notches); //yaw oscillates n times around circle
          frag_origin_yaw_band = (frag_origin_yaw_band+1)/2; //yaw oscillates between 0 and 1
          frag_origin_yaw_band = (frag_origin_yaw_band-frag_origin_yaw_display_threshhold)/(1-frag_origin_yaw_display_threshhold); //yaw is - below threshhold, 0 at threshhold, and 1 at 1
          frag_origin_yaw_band = max(0,frag_origin_yaw_band);

          lazy_shade = abs(snapped_lazy_origin_yaw-frag_origin_yaw)+abs(snapped_lazy_origin_pitch-frag_origin_pitch);
          lazy_shade = clamp(lazy_shade,0,1);

          goal_shade = abs(snapped_goal_origin_yaw-frag_origin_yaw)+abs(snapped_goal_origin_pitch-frag_origin_pitch);
          goal_shade = clamp(goal_shade,0,1);

          float from_min;
          float from_max;
          float to_min;
          float to_max;
          float v;

          from_min = 10;
          from_max = 5;
          to_min = 5.5;
          to_max = 31;
          v = grid_resolution;
          v = ((v-from_min)/(from_max-from_min))*(to_max-to_min)+to_min;

               if(grid_resolution > 7.5)  v = 5.5; //10
          else if(grid_resolution > 2.5)  v = 11;  //5
          else if(grid_resolution > 0.45) v = 111; //0.5
          else v = 1000000;                        //0.5

          lazy_shade = min(1,lazy_shade*v);
          lazy_shade *= lazy_shade;
          lazy_shade *= lazy_shade;
          lazy_shade *= lazy_shade;

          goal_shade = min(1,goal_shade*v);
          goal_shade *= goal_shade;
          goal_shade *= goal_shade;
          goal_shade *= goal_shade;

          frag_pt = normalize(frag_pt);
          x2 = frag_pt.x-lazy_origin_ray.x;
          x2 *= x2;
          y2 = frag_pt.y-lazy_origin_ray.y;
          y2 *= y2;
          z2 = frag_pt.z-lazy_origin_ray.z;
          z2 *= z2;
          float frag_lazy_dist = sqrt(x2+y2+z2);

          band = clamp(frag_origin_pitch_band+frag_origin_yaw_band,0,1);

          from_min = 10;
          from_max = 5;
          to_min = 5;
          to_max = 20;
          v = grid_resolution;
          v = ((v-from_min)/(from_max-from_min))*(to_max-to_min)+to_min;

               if(grid_resolution > 7.5)  v = 5;  //10
          else if(grid_resolution > 2.5)  v = 11; //5
          else if(grid_resolution > 0.45) v = 51; //1
          else v = 50;                            //0.5

          band *= (window_r-frag_lazy_dist*v)/window_r;
          color.rgba = float4(goal_shade,lazy_shade,1,band*grid_alpha);

          return color;
        }
        ENDCG
    }
  }
}
