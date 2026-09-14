using System.Collections.Generic;
using NightCourier.World;
using UnityEngine;

namespace NightCourier.Prototype.Vehicles.Sport
{
    /// <summary>Cab-forward coupe with wheel openings integrated into a closed monocoque.</summary>
    internal static class SportBodyBuilder
    {
        public const float FrontZ = 2.84f, RearZ = -2.84f;
        public static GameObject Build(Transform parent, CityPalette p) => Build(parent,p.CarPaint,p.Glass,p.Metal,p.DarkWindow);
        public static GameObject Build(Transform parent, Material paint, Material glass, Material metal, Material black)
        {
            var root=new GameObject("Two-seat sport shell");
            root.transform.SetParent(parent,false);
            Transform t=root.transform;
            MeshObject(t,"Sculpted monocoque",Body(),paint);
            Quad(t,"Sport windscreen",glass,new Vector3(-.84f,.4f,1.23f),new Vector3(.84f,.4f,1.23f),
                new Vector3(.68f,.99f,.47f),new Vector3(-.68f,.99f,.47f),Vector3.forward);
            Quad(t,"Sport fastback glass",glass,new Vector3(-.68f,.98f,-.34f),new Vector3(.68f,.98f,-.34f),
                new Vector3(.79f,.47f,-.95f),new Vector3(-.79f,.47f,-.95f),Vector3.back);
            MeshObject(t,"Cambered painted canopy",Roof(),paint);
            foreach(int side in new[]{-1,1})
            {
                Quad(t,side<0?"Sport left side glass":"Sport right side glass",glass,
                    new Vector3(side*.845f,.405f,1.18f),new Vector3(side*.685f,.982f,.46f),
                    new Vector3(side*.685f,.977f,-.33f),new Vector3(side*.80f,.46f,-.91f),Vector3.right*side);
                Beam(t,"Slim A pillar",new Vector3(side*.86f,.40f,1.23f),new Vector3(side*.70f,1.005f,.47f),.055f,paint);
                Beam(t,"Rear canopy pillar",new Vector3(side*.70f,.995f,-.34f),new Vector3(side*.83f,.46f,-.99f),.075f,paint);
                Beam(t,"Roof rail",new Vector3(side*.70f,1.005f,.47f),new Vector3(side*.70f,.995f,-.34f),.045f,paint);
                Quad(t,"Side power-unit intake",black,
                    new Vector3(side*1.12f,-.12f,-.38f),new Vector3(side*1.11f,.28f,-.27f),
                    new Vector3(side*1.185f,.43f,-.98f),new Vector3(side*1.185f,-.12f,-1.06f),Vector3.right*side);
                Beam(t,"Intake upper blade",new Vector3(side*1.115f,.31f,-.29f),new Vector3(side*1.19f,.465f,-.99f),.045f,paint);
                Beam(t,"Carbon sill",new Vector3(side*1.075f,-.235f,1.24f),new Vector3(side*1.14f,-.235f,-1.08f),.07f,metal);
                Beam(t,"Flush door handle",new Vector3(side*1.106f,.26f,.06f),new Vector3(side*1.106f,.26f,-.13f),.022f,metal);
                Beam(t,"Mirror stalk",new Vector3(side*.87f,.49f,.92f),new Vector3(side*1.04f,.51f,.83f),.035f,metal);
                Part(t,"Aero mirror",new Vector3(side*1.065f,.52f,.83f),new Vector3(.19f,.075f,.24f),paint,PrimitiveType.Sphere);
                Part(t,"Headlight recess",new Vector3(side*.72f,.245f,2.807f),new Vector3(.62f,.15f,.065f),black);
                Part(t,"Front intake",new Vector3(side*.7f,-.075f,2.82f),new Vector3(.57f,.20f,.035f),black);
                Beam(t,"Flying buttress",new Vector3(side*.72f,.89f,-.40f),new Vector3(side*.98f,.48f,-1.43f),.115f,paint);
            }
            Part(t,"Central intake",new Vector3(0,-.105f,2.838f),new Vector3(.60f,.14f,.035f),black);
            Part(t,"Front carbon splitter",new Vector3(0,-.27f,2.73f),new Vector3(2.08f,.045f,.30f),metal);
            Part(t,"Rear diffuser",new Vector3(0,-.225f,-2.75f),new Vector3(2.06f,.11f,.30f),black);
            foreach(float x in new[]{-.72f,-.36f,0,.36f,.72f})
                Part(t,"Diffuser strake",new Vector3(x,-.285f,-2.68f),new Vector3(.025f,.14f,.40f),metal);
            Part(t,"Rear lamp recess",new Vector3(0,.31f,-2.827f),new Vector3(1.95f,.14f,.04f),black);
            Part(t,"Integrated rear lip",new Vector3(0,.475f,-2.69f),new Vector3(2.08f,.045f,.20f),paint);
            for(int i=0;i<7;i++)
                Part(t,"Rear motor deck louvre",new Vector3(0,.405f,-1.15f-i*.16f),new Vector3(1.10f,.022f,.065f),black);
            Part(t,"Cabin floor",new Vector3(0,.02f,.05f),new Vector3(1.4f,.06f,1.55f),black);
            foreach(float x in new[]{-.37f,.37f})
            {
                Part(t,"Bucket seat",new Vector3(x,.19f,.01f),new Vector3(.47f,.17f,.55f),black);
                var seat=Part(t,"Bucket backrest",new Vector3(x,.40f,-.24f),new Vector3(.45f,.52f,.12f),black);
                seat.transform.localRotation=Quaternion.Euler(12,0,0);
            }
            return root;
        }
        private static float Width(float z) => 1.04f+.105f*Mathf.Exp(-Mathf.Pow((z-1.8f)/.7f,2))
            +.16f*Mathf.Exp(-Mathf.Pow((z+1.65f)/.9f,2))+.035f*Mathf.Exp(-Mathf.Pow(z/.9f,2));
        private static float ArchBottom(float z)
        {
            float d=Mathf.Min(Mathf.Abs(z-1.8f),Mathf.Abs(z+1.65f));
            return d<.455f ? -.065f+Mathf.Sqrt(.455f*.455f-d*d) : -.25f;
        }
        private static Mesh Body()
        {
            const int rows=145,ring=12;
            var v=new List<Vector3>(); var tris=new List<int>();
            for(int i=0;i<rows;i++)
            {
                float z=Mathf.Lerp(RearZ,FrontZ,i/(float)(rows-1)),w=Width(z),bottom=ArchBottom(z);
                float shoulder=.39f+.15f*Mathf.Exp(-Mathf.Pow((z-1.8f)/.6f,2))+.17f*Mathf.Exp(-Mathf.Pow((z+1.65f)/.7f,2));
                shoulder-=.10f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.25f,2.84f,z));
                float deck=z<-.85f?.39f:.34f;
                v.AddRange(new[]{
                    new Vector3(-w*.73f,-.25f,z),new Vector3(w*.73f,-.25f,z),
                    new Vector3(w,bottom,z),new Vector3(w,Mathf.Max(bottom+.03f,shoulder-.10f),z),
                    new Vector3(w*.96f,shoulder,z),new Vector3(w*.74f,shoulder-.02f,z),
                    new Vector3(w*.57f,deck,z),new Vector3(-w*.57f,deck,z),
                    new Vector3(-w*.74f,shoulder-.02f,z),new Vector3(-w*.96f,shoulder,z),
                    new Vector3(-w,Mathf.Max(bottom+.03f,shoulder-.10f),z),new Vector3(-w,bottom,z)});
            }
            for(int row=0;row<rows-1;row++)
            for(int edge=0;edge<ring;edge++)
            {
                int a=row*ring+edge,b=row*ring+(edge+1)%ring,c=b+ring,d=a+ring;
                tris.AddRange(new[]{a,b,c,a,c,d});
            }
            // Independent cap vertices retain sharp front and rear edges.
            int back=v.Count;for(int e=0;e<ring;e++)v.Add(v[e]);
            int front=v.Count;for(int e=0;e<ring;e++)v.Add(v[(rows-1)*ring+e]);
            for(int e=1;e<ring-1;e++)
            {
                tris.AddRange(new[]{back,back+e+1,back+e});
                tris.AddRange(new[]{front,front+e,front+e+1});
            }
            var m=new Mesh{name="APEX E2 closed body"};m.SetVertices(v);m.SetTriangles(tris,0);
            m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        private static Mesh Roof()
        {
            var v=new List<Vector3>();var tri=new List<int>();
            for(int z=0;z<2;z++)for(int x=0;x<=12;x++)
            {
                float u=x/12f*2-1;
                v.Add(new Vector3(u*.70f,1.005f+.05f*(1-u*u),z==0?.47f:-.34f));
            }
            for(int x=0;x<12;x++)tri.AddRange(new[]{x,x+1,x+14,x,x+14,x+13});
            var m=new Mesh{name="Cambered canopy"};m.SetVertices(v);m.SetTriangles(tri,0);
            m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        private static void Quad(Transform t,string name,Material mat,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 outward)
        {
            var m=new Mesh{name=name};m.vertices=new[]{a,b,c,d};
            m.triangles=Vector3.Dot(Vector3.Cross(b-a,c-a),outward)>0?new[]{0,1,2,0,2,3}:new[]{0,2,1,0,3,2};
            m.RecalculateNormals();m.RecalculateBounds();MeshObject(t,name,m,mat);
        }
        private static void MeshObject(Transform t,string name,Mesh mesh,Material mat)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(t,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;
        }
        private static void Beam(Transform t,string name,Vector3 a,Vector3 b,float thickness,Material mat)
        {
            var go=Part(t,name,(a+b)*.5f,new Vector3(thickness,thickness,(b-a).magnitude),mat);
            go.transform.localRotation=Quaternion.LookRotation(b-a);
        }
        private static GameObject Part(Transform t,string name,Vector3 p,Vector3 s,Material mat,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(t,false);
            go.transform.localPosition=p;go.transform.localScale=s;go.GetComponent<Renderer>().sharedMaterial=mat;
            Object.Destroy(go.GetComponent<Collider>());return go;
        }
    }
}
