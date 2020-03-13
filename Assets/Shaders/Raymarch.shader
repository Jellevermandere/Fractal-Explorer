Shader "Unlit/Raymarch"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

                  #define MAX_STEPS 100 // maximum raycast loops
                  #define MAX_DIST 10. // ray cannot go further than this distance
                  #define SURF_DIST .01 // how near to surface we should raycast to

     // ********** Basic Raymarcher *************** //
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };
            
            
            
            
            
            // returns distance to the scene objects (we have 2 objects here, sphere and ground plane)
            float GetDist(float3 p) 
            {
              
              float4 sphere = float4(0, 0, 0, 0.1); // x,y,z,radius
              float d = length(fmod(p,0.5) - sphere.xyz) - sphere.w;
              d = length(float2(length(fmod(p.xz, 1)) - 0.4, fmod(p.y,1))) - 0.1;
             
             return d;
            }


            // jump along the ray until we get close enough to some of our objects
            float RayMarch(float3 ro, float3 rd)
            {
              float dO = 0.;
              for (int i = 0; i < MAX_STEPS; i++) 
              {
                float3 p = ro + rd * dO;
                float dS = GetDist(p);
                dO += dS;
                if (dO > MAX_DIST || dS < SURF_DIST) break;
              }
              return dO;
            }

            float3 GetNormal(float3 p) 
            {
              float d = GetDist(p);
              float2 e = float2(.01, 0);
              float3 n = d - float3(GetDist(p - e.xyy),GetDist(p - e.yxy),GetDist(p - e.yyx));
              return normalize(n);
            }

           

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.ro = mul(unity_WorldToObject,float4( _WorldSpaceCameraPos,1));
                o.hitPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
              float2 uv = (i.uv - 0.5);
              float3 col = float3(0,0,0);

              float3 ro = i.ro;  //float3(0,0,-3);
              float3 rd = normalize(i.hitPos - ro);  //normalize(float3(uv.x, uv.y, 1));

              float d = RayMarch(ro, rd);
              
              if(d < MAX_DIST){
              
                float3 p = ro + rd * d;
                float3 n = GetNormal(p);

                col.rgb = n;

              } else discard;

              return float4(col,1);
            }
            ENDCG
        } // pass
    } // subshader
}
