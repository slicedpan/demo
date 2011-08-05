using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace phystest
{
    public class LPPMesh : Mesh
    {
        public float Shininess;
        public float SpecularPower;
        
        Matrix[] m;

        public LPPMesh()
        {
            Shininess = 1.0f;
            SpecularPower = 1.0f;
        }
        public LPPMesh(Model model)
        {
            _model = model;
            m = new Matrix[_model.Bones.Count];
            GenerateBoundingSphere();
        }
        public virtual void RenderToGBuffer(Camera camera, GraphicsDevice graphicsDevice)
        {            
            if (Draw)
            {
                _model.CopyAbsoluteBoneTransformsTo(m);                
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    m[mesh.ParentBone.Index] *= Premult * Transform;
                    if (camera.Frustum.Intersects(Helpers.TransformSphere(mesh.BoundingSphere, m[mesh.ParentBone.Index])))
                    {
                        foreach (EffectMaterial effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = effect.Techniques[0];
                            //our first pass is responsible for rendering into GBuffer
                            effect.Parameters["SpecularPower"].SetValue(SpecularPower);
                            effect.Parameters["Shininess"].SetValue(Shininess);
                            effect.Parameters["World"].SetValue(m[mesh.ParentBone.Index]);
                            effect.Parameters["View"].SetValue(camera.EyeTransform);
                            effect.Parameters["Projection"].SetValue(camera.ProjectionTransform);
                            effect.Parameters["WorldView"].SetValue(m[mesh.ParentBone.Index] * camera.EyeTransform);
                            effect.Parameters["WorldViewProjection"].SetValue(m[mesh.ParentBone.Index] * camera.EyeProjectionTransform);
                            effect.Parameters["FarClip"].SetValue(camera.FarClip);
                            mesh.Draw();
                        }
                    }
                }
            }
        }
        public virtual void DrawHighlight(Camera camera, GraphicsDevice graphicsDevice, RenderTarget2D edgemask)
        {            
            foreach (ModelMesh mesh in _model.Meshes)
            {
                if (camera.Frustum.Intersects(Helpers.TransformSphere(mesh.BoundingSphere, m[mesh.ParentBone.Index])))
                {
                    foreach (EffectMaterial effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["DrawHighlight"];
                        effect.Parameters["WorldViewProjection"].SetValue(m[mesh.ParentBone.Index] * camera.EyeProjectionTransform);                        
                        effect.Parameters["edgemask"].SetValue(edgemask);
                        mesh.Draw();
                    }
                }
            }
        }
        public virtual void ReconstructShading(Camera camera, GraphicsDevice graphicsDevice, Texture2D lightBuffer)
        {
            float glowval = (float)Math.Sin(Game1.time.TotalGameTime.TotalSeconds * 2.0d) * 0.3f;            
            if (Draw)
            {
                foreach (ModelMesh mesh in _model.Meshes)
                {
                    if (camera.Frustum.Intersects(Helpers.TransformSphere(mesh.BoundingSphere, m[mesh.ParentBone.Index])))
                    {
                        foreach (EffectMaterial effect in mesh.Effects)
                        {
                            //this pass uses the light accumulation texture and reconstructs the mesh's shading                            
                            effect.CurrentTechnique = effect.Techniques[1];
                            effect.Parameters["World"].SetValue(m[mesh.ParentBone.Index]);
                            effect.Parameters["View"].SetValue(camera.EyeTransform);
                            effect.Parameters["Projection"].SetValue(camera.ProjectionTransform);
                            effect.Parameters["WorldView"].SetValue(m[mesh.ParentBone.Index] * camera.EyeTransform);
                            effect.Parameters["WorldViewProjection"].SetValue(m[mesh.ParentBone.Index] * camera.EyeProjectionTransform);
                            if (Highlight)
                            {
                                effect.Parameters["Highlight"].SetValue(1.5f + glowval);
                                effect.Parameters["HighlightMin"].SetValue(0.4f);
                            }
                            else
                            {
                                effect.Parameters["Highlight"].SetValue(1.0f);
                                effect.Parameters["HighlightMin"].SetValue(0.003f);
                            }
                            effect.Parameters["LightBuffer"].SetValue(lightBuffer);
                            effect.Parameters["LightBufferPixelSize"].SetValue(new Vector2(0.5f / lightBuffer.Width, 0.5f / lightBuffer.Height));

                            //effect.CurrentTechnique.Passes[0].Apply();
                            mesh.Draw();
                        }
                    }
                }
            }
        }
        public virtual void DrawOcclusion(Camera camera, GraphicsDevice graphicsDevice, Light light, bool front = true)
        {
            if (Draw)
            {
                Vector3 LightPos = light.Transform.Translation; 
                BoundingBox bb;
                float val;
                if (front)
                {
                    bb = new BoundingBox(LightPos - ((Vector3.UnitZ + Vector3.UnitX) * light.Radius), LightPos + (Vector3.One * light.Radius));
                    val = 1.0f;
                }
                else
                {
                    bb = new BoundingBox(LightPos - (Vector3.One * light.Radius), LightPos + ((Vector3.UnitZ + Vector3.UnitX) * light.Radius));
                    val = -1.0f;
                }

                foreach (ModelMesh mesh in _model.Meshes)
                {                    
                    if (bb.Intersects(Helpers.TransformSphere(mesh.BoundingSphere, m[mesh.ParentBone.Index])) || Game1.drawall)
                    {
                        foreach (EffectMaterial effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = effect.Techniques["ShadowMap"];
                            effect.Parameters["Dir"].SetValue(val);
                            effect.Parameters["LightWorldView"].SetValue(m[mesh.ParentBone.Index] * light.LightView);
                            effect.Parameters["LightWorldViewProjection"].SetValue(m[mesh.ParentBone.Index] * light.LightViewProj);
                            effect.Parameters["LightPosition"].SetValue(LightPos);
                            effect.Parameters["LightClip"].SetValue(light.Radius);
                            mesh.Draw();
                            Game1.occluders++;
                        }
                    }
                }
            }
        }
    }
}
