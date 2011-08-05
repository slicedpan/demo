using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace phystest
{
    public class LPPRenderer
    {

        #region fields

        private QuadRenderer _quadRenderer;

        private Effect _clearGBuffer;
        private Effect _basicLighting;
        private Effect _shadowLighting;
        private Effect _postProcess;
        private Effect _edgeDetect;
        private Effect _glowEffect;
        private Effect _clearShadow;
        private Effect _drawLights;

        public static int meshCountGBuffer = 0;
        public static int meshCountReconstructed = 0;

        private BlendState lightBlend;

        private ContentManager _contentManager;
        private GraphicsDevice _graphicsDevice;

        private RenderTarget2D _lightBuffer;
        private RenderTarget2D _depthBuffer;
        private RenderTarget2D _normalBuffer;
        private RenderTarget2D _outputTexture;
        private RenderTarget2D _preProcess;
        private RenderTarget2D _glowBuffer;
        private RenderTarget2D _edgeMask;
        private RenderTarget2D _shadowBuffer;
        private RenderTarget2D _glowBuffer2;
        Texture2D drawTex;
       
        private Vector3[] _cornersViewSpace = new Vector3[8];
        private Vector3[] _cornersWorldSpace = new Vector3[8];
        private Vector3[] _currentFrustumCorners = new Vector3[8];

        private List<LPPMesh> _currentOccluders = new List<LPPMesh>();

        private Vector2 pixelSize;

        public const int ShadowMapSize = 1024;

        private bool _highlight;
        private LPPMesh _mesh;


        public ContentManager ContentManager
        {
            get
            {
                return _contentManager;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _graphicsDevice;
            }
        }

        private int _width;
        private int _height;

        #endregion

        public LPPRenderer(GraphicsDevice graphicsDevice, ContentManager contentManager, int width, int height)
        {
            _quadRenderer = new QuadRenderer();
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;
            _width = width;
            _height = height;

            CreateGBuffer();
            LoadShaders();

            lightBlend = new BlendState()
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };
            pixelSize = new Vector2(1.0f / (float)width, 1.0f / (float)height);
        }
        private void LoadShaders()
        {
            _clearGBuffer = ContentManager.Load<Effect>("shaders/ClearGBuffer");
            _basicLighting = ContentManager.Load<Effect>("shaders/LightingLPP");
            _shadowLighting = ContentManager.Load<Effect>("shaders/DPSLightingLPP");
            _edgeDetect = ContentManager.Load<Effect>("shaders/EdgeDetect");
            _postProcess = ContentManager.Load<Effect>("shaders/PostProcess");
            _glowEffect = ContentManager.Load<Effect>("shaders/Glow");
            _clearShadow = ContentManager.Load<Effect>("shaders/ClearShadow");
            _drawLights = ContentManager.Load<Effect>("shaders/Showlights");

        }
        private void CreateGBuffer()
        {
            _depthBuffer = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Single, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            _normalBuffer = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Rgba1010102, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            _lightBuffer = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Rgba64, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _glowBuffer = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Rgba64, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            _glowBuffer2 = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Rgba64, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            _outputTexture = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            _preProcess = new RenderTarget2D(GraphicsDevice, _width, _height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);
            _edgeMask = new RenderTarget2D(GraphicsDevice, _width / 2, _height / 2, false, SurfaceFormat.Rg32, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            _shadowBuffer = new RenderTarget2D(GraphicsDevice, ShadowMapSize, ShadowMapSize * 2, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
        }

        public void Highlight(LPPMesh mesh)
        {
            _highlight = true;
            _mesh = mesh;
        }
            
        public RenderTarget2D Render(Camera camera, List<Light> lights, List<LPPMesh> meshes, List<LPPMesh> occludingMeshes)
        {
            ComputeFrustumCorners(camera);
            meshCountGBuffer = 0;
            meshCountReconstructed = 0;
            //first of all, we must bind our GBuffer and reset all states
            _graphicsDevice.SetRenderTargets(_normalBuffer, _depthBuffer);
            _graphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1.0f, 0);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;

            //bind the effect that outputs the default GBuffer values
            _clearGBuffer.CurrentTechnique.Passes[0].Apply();
            //draw a full screen quad for clearing our GBuffer
            _quadRenderer.RenderQuad(_graphicsDevice, -Vector2.One, Vector2.One, pixelSize);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //now, render all our objects

            RenderToGBuffer(camera, meshes);

            if (Console.GetBool("drawdepth"))
                return _depthBuffer;
            if (Console.GetBool("drawnormal"))
                return _normalBuffer;

            _graphicsDevice.SetRenderTarget(_edgeMask);

            _edgeDetect.Parameters["normal"].SetValue(_normalBuffer);
            _edgeDetect.Parameters["depth"].SetValue(_depthBuffer);
            _edgeDetect.Parameters["invScreenWidth"].SetValue(1.0f / _width);
            _edgeDetect.Parameters["invScreenHeight"].SetValue(1.0f / _height);
            _edgeDetect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);

            //resolve our GBuffer and render the lights
            //clear the light buffer with black
            
            _graphicsDevice.SetRenderTarget(_lightBuffer);
            //dont be fooled by Color.Black, as its alpha is 255 (or 1.0f)
            _graphicsDevice.Clear(new Color(0, 0, 0, 0));

            //dont use depth/stencil test...we dont have a depth buffer, anyway
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //draw using additive blending. 
            //At first I was using BlendState.additive, but it seems to use alpha channel for modulation, 
            //and as we use alpha channel as the specular intensity, we have to create our own blend state here
            RenderLights(camera, lights, occludingMeshes);
      
            AddGlow();
            
            if (Console.GetBool("drawshadowmap"))
                return _shadowBuffer;
            if (Console.GetBool("drawedgemask"))            
                return _edgeMask;            
            if (Console.GetBool("drawlightbuffer"))
            {
                _graphicsDevice.SetRenderTarget(_outputTexture);
                _drawLights.CurrentTechnique.Passes[0].Apply();
                _drawLights.Parameters["lightBuffer"].SetValue(_lightBuffer);
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);
                return _outputTexture;
            }

            //reconstruct each object shading, using the light texture as input (and another specific parameters too)
            _graphicsDevice.SetRenderTarget(_outputTexture);            
            _graphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, Color.Black, 1.0f, 0);
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            ReconstructShading(camera, meshes);

            PostProcess(camera);

            //unbind our final buffer and return it
            _graphicsDevice.SetRenderTarget(null);

            return _outputTexture;
        }

        private void GenerateDPSShadowMap(Camera camera, Light light, List<LPPMesh> occludingMeshes, RenderTarget2D target)
        {
            _currentOccluders.Clear();            

            foreach (LPPMesh occluder in occludingMeshes)
            {
                if (occluder.BoundingSphere.Intersects(light.BoundingSphere))
                    _currentOccluders.Add(occluder);
            }
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.SetRenderTarget(target);
            _clearShadow.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize / 2);

            foreach (LPPMesh occluder in _currentOccluders)
            {
                occluder.DrawOcclusion(camera, GraphicsDevice, light);
                //Game1.occluders++;
            }
            _graphicsDevice.SetRenderTarget(target);
            _clearShadow.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize / 2);

            foreach (LPPMesh occluder in _currentOccluders)
            {
                occluder.DrawOcclusion(camera, GraphicsDevice, light, false);
            }
            Game1.shadowmaps++;
            _graphicsDevice.SetRenderTarget(null);
        }

        private void RenderLights(Camera camera, List<Light> lights, List<LPPMesh> occludingMeshes)
        {
            Vector3[] camCorners = camera.Frustum.GetCorners();
            Vector3 camCentre = Vector3.Zero;
            foreach (Vector3 vec in camCorners)
            {
                camCentre += vec;
            }
            camCentre /= camCorners.Length;
            Effect _lighting;
            foreach (Light light in lights)
            {
                if (light.LightType == "DynamicShadow")
                {
                    light.LightView = Matrix.CreateLookAt(light.Transform.Translation, light.Transform.Translation - Vector3.UnitY, Vector3.UnitZ);
                    light.LightViewProj = light.LightView * Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.0f, 0.01f, light.Radius);
                    GenerateDPSShadowMap(camera, light, occludingMeshes, _shadowBuffer);
                    _lighting = _shadowLighting;
                    _lighting.Parameters["InvViewLightViewProj"].SetValue(Matrix.Invert(camera.EyeTransform) * light.LightViewProj);
                    _lighting.Parameters["InvView"].SetValue(Matrix.Invert(camera.EyeTransform));
                    _lighting.Parameters["LightView"].SetValue(light.LightView);
                    _graphicsDevice.SetRenderTarget(_lightBuffer);
                    _lighting.Parameters["ShadowMap"].SetValue(_shadowBuffer);
                    _lighting.Parameters["texOffset"].SetValue(Game1.texOffset);
                }
                else if (light.LightType == "BakedShadow")
                {
                    _lighting = _shadowLighting;
                    
                    if (light.shadowMap == null)
                    {
                        light.LightView = Matrix.CreateLookAt(light.Transform.Translation, light.Transform.Translation - Vector3.UnitY, Vector3.UnitZ);
                        light.LightViewProj = light.LightView * Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), 1.0f, 0.01f, light.Radius);
                        var tmpBuffer = new RenderTarget2D(GraphicsDevice, ShadowMapSize, ShadowMapSize * 2, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
                        GenerateDPSShadowMap(camera, light, occludingMeshes, tmpBuffer);
                        light.shadowMap = tmpBuffer;
                    }
                    drawTex = light.shadowMap;
                    _lighting.Parameters["InvViewLightViewProj"].SetValue(Matrix.Invert(camera.EyeTransform) * light.LightViewProj);
                    _lighting.Parameters["InvView"].SetValue(Matrix.Invert(camera.EyeTransform));
                    _lighting.Parameters["LightView"].SetValue(light.LightView);
                    _graphicsDevice.SetRenderTarget(_lightBuffer);
                    _lighting.Parameters["ShadowMap"].SetValue(light.shadowMap);
                    _lighting.Parameters["texOffset"].SetValue(Game1.texOffset);

                }
                else
                {
                    _graphicsDevice.SetRenderTarget(_lightBuffer);
                    _lighting = _basicLighting;
                }

                _graphicsDevice.BlendState = lightBlend;
                _graphicsDevice.DepthStencilState = DepthStencilState.None;                
                _lighting.Parameters["texOffset"].SetValue(Game1.texOffset);
                _lighting.Parameters["GBufferPixelSize"].SetValue(new Vector2(0.5f / _width, 0.5f / _height));
                _lighting.Parameters["DepthBuffer"].SetValue(_depthBuffer);
                _lighting.Parameters["NormalBuffer"].SetValue(_normalBuffer);
                _lighting.Parameters["fudge"].SetValue(Game1.mix);
                _lighting.Parameters["FarClip"].SetValue(camera.FarClip);
                _lighting.Parameters["Radius"].SetValue(light.Radius);
                _lighting.Parameters["LightViewRay"].SetValue(-light.Transform.Translation);

                //convert light position into viewspace
                Vector3 viewSpaceLPos = Vector3.Transform(light.Transform.Translation,
                                                            camera.EyeTransform);
                _lighting.Parameters["LightPosition"].SetValue(viewSpaceLPos);
                Vector3 lightColor = light.Color.ToVector3() * light.Intensity;
                _lighting.Parameters["LightColor"].SetValue(lightColor);
                float invRadiusSqr = 1.0f / (light.Radius * light.Radius);
                _lighting.Parameters["InvLightRadiusSqr"].SetValue(invRadiusSqr);

                Vector2 bottomLeftCorner, topRightCorner, size;
                //compute a screen-space quad that fits the light's bounding sphere

                camera.ProjectBoundingSphereOnScreen(light.BoundingSphere, out bottomLeftCorner, out size);
                bottomLeftCorner.Y = -bottomLeftCorner.Y - size.Y;
                //
                topRightCorner = bottomLeftCorner + size;

                //clamp them
                bottomLeftCorner.X = Math.Max(bottomLeftCorner.X, -1);
                bottomLeftCorner.Y = Math.Max(bottomLeftCorner.Y, -1);
                topRightCorner.X = Math.Min(topRightCorner.X, 1);
                topRightCorner.Y = Math.Min(topRightCorner.Y, 1);

                //apply our frustum corners to this effect. Use the computed view-space rect
                ApplyFrustumCorners(_lighting, bottomLeftCorner, topRightCorner);

                _lighting.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderQuad(_graphicsDevice, bottomLeftCorner, topRightCorner, pixelSize);                

            }

        }
        private void PostProcess(Camera camera)
        {
            if (_highlight)
            {
                _graphicsDevice.SetRenderTarget(_outputTexture);
                _graphicsDevice.BlendState = BlendState.AlphaBlend;
                _mesh.DrawHighlight(camera, GraphicsDevice, _edgeMask);
            }
            _highlight = false;

            if (Console.GetBool("blur"))
            {
                _graphicsDevice.SetRenderTarget(_preProcess);

                _postProcess.CurrentTechnique = _postProcess.Techniques["EdgeBlur"];

                _postProcess.Parameters["inputtexture"].SetValue(_outputTexture);
                _postProcess.Parameters["edgemask"].SetValue(_edgeMask);
                _postProcess.Parameters["invScreenHeight"].SetValue(1.0f / _height);
                _postProcess.Parameters["invScreenWidth"].SetValue(1.0f / _width);

                _postProcess.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);

                _graphicsDevice.SetRenderTarget(_outputTexture);

                _postProcess.Parameters["inputtexture"].SetValue(_preProcess);

                _postProcess.CurrentTechnique.Passes[1].Apply();
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);
            }

           
        }

        private void AddGlow()
        {
            if (Console.GetBool("glow"))
            {
                _graphicsDevice.SetRenderTarget(_glowBuffer);
                _glowEffect.Parameters["hdrPower"].SetValue(Console.GetFloat("hdrpower"));
                _graphicsDevice.Clear(new Color(0, 0, 0, 0));
                _glowEffect.Parameters["lightBuffer"].SetValue(_lightBuffer);
                _glowEffect.Parameters["invScreenWidth"].SetValue(1.0f / (float)_width);
                _glowEffect.Parameters["invScreenHeight"].SetValue(1.0f / (float)_height);
                _glowEffect.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);

                _graphicsDevice.SetRenderTarget(_glowBuffer2);
                _graphicsDevice.Clear(ClearOptions.Target, new Color(0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0);
                _glowEffect.Parameters["lightBuffer"].SetValue(_glowBuffer);
                _glowEffect.CurrentTechnique.Passes[1].Apply();
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);

                _graphicsDevice.SetRenderTarget(_glowBuffer);
                _graphicsDevice.Clear(ClearOptions.Target, new Color(0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0);
                _glowEffect.Parameters["lightBuffer"].SetValue(_glowBuffer2);
                _glowEffect.CurrentTechnique.Passes[2].Apply();
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);

                _graphicsDevice.SetRenderTarget(_lightBuffer);   
                _graphicsDevice.BlendState = BlendState.Additive;
                _glowEffect.Parameters["lightBuffer"].SetValue(_glowBuffer);
                _glowEffect.CurrentTechnique.Passes[3].Apply();
                _quadRenderer.RenderQuad(GraphicsDevice, -Vector2.One, Vector2.One, pixelSize);
            }
        }

        private void RenderToGBuffer(Camera cam, List<LPPMesh> meshes)
        {
            foreach (LPPMesh mesh in meshes)
            {
                meshCountGBuffer++;
                mesh.RenderToGBuffer(cam, GraphicsDevice);
            }
        }

        private void ReconstructShading(Camera cam, List<LPPMesh> meshes)
        {
            RenderTarget2D currentLightBuffer = Console.GetBool("glow") ? _glowBuffer : _lightBuffer;
            foreach (LPPMesh mesh in meshes)
            {
                meshCountReconstructed++;
                mesh.ReconstructShading(cam, GraphicsDevice, currentLightBuffer);
            }
        }

        private void ComputeFrustumCorners(Camera camera)
        {
            camera.Frustum.GetCorners(_cornersWorldSpace);
            Matrix matView = camera.EyeTransform; //this is the inverse of our camera transform
            Vector3.Transform(_cornersWorldSpace, ref matView, _cornersViewSpace); //put the frustum into view space
            for (int i = 0; i < 4; i++) //take only the 4 farthest points
            {
                _currentFrustumCorners[i] = _cornersViewSpace[i + 4];
            }
            Vector3 temp = _currentFrustumCorners[3];
            _currentFrustumCorners[3] = _currentFrustumCorners[2];
            _currentFrustumCorners[2] = temp;
        }

        /// <summary>
        /// This method computes the frustum corners applied to a quad that can be smaller than
        /// our screen. This is useful because instead of drawing a full-screen quad for each
        /// point light, we can draw smaller quads that fit the light's bounding sphere in screen-space,
        /// avoiding unecessary pixel shader operations
        /// </summary>
        /// <param name="effect">The effect we want to apply those corners</param>
        /// <param name="topLeftVertex"> The top left vertex, in screen space [-1..1]</param>
        /// <param name="bottomRightVertex">The bottom right vertex, in screen space [-1..1]</param>
        private void ApplyFrustumCorners(Effect effect, Vector2 topLeftVertex, Vector2 bottomRightVertex)
        {
            float dx = _currentFrustumCorners[1].X - _currentFrustumCorners[0].X;
            float dy = _currentFrustumCorners[0].Y - _currentFrustumCorners[2].Y;

            Vector3[] _localFrustumCorners = new Vector3[4];
            _localFrustumCorners[0] = _currentFrustumCorners[2];
            _localFrustumCorners[0].X += dx * (topLeftVertex.X * 0.5f + 0.5f);
            _localFrustumCorners[0].Y += dy * (bottomRightVertex.Y * 0.5f + 0.5f);

            _localFrustumCorners[1] = _currentFrustumCorners[2];
            _localFrustumCorners[1].X += dx * (bottomRightVertex.X * 0.5f + 0.5f);
            _localFrustumCorners[1].Y += dy * (bottomRightVertex.Y * 0.5f + 0.5f);

            _localFrustumCorners[2] = _currentFrustumCorners[2];
            _localFrustumCorners[2].X += dx * (topLeftVertex.X * 0.5f + 0.5f);
            _localFrustumCorners[2].Y += dy * (topLeftVertex.Y * 0.5f + 0.5f);

            _localFrustumCorners[3] = _currentFrustumCorners[2];
            _localFrustumCorners[3].X += dx * (bottomRightVertex.X * 0.5f + 0.5f);
            _localFrustumCorners[3].Y += dy * (topLeftVertex.Y * 0.5f + 0.5f);

            effect.Parameters["FrustumCorners"].SetValue(_localFrustumCorners);
        }
    }
}
