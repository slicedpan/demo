using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using bepuData;
using BEPUphysics;
using BEPUphysicsDrawer.Models;
using BEPUphysicsDrawer.Lines;
using TomShane.Neoforce.Controls;

namespace phystest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public int scrheight = 720;
        public int scrwidth = 1280;

        int frameCounter = 0;
        int frameRate = 60;
        TimeSpan elapsedTime = TimeSpan.Zero;

        public static bool support;
        public static bool traction;

        public ModelDrawer ModelDrawer;
        public LineDrawer ConstraintDrawer;
        public LightDrawer LightDrawer;

        public static Effect NormalEffect;      //Make sure these are cloned!
        public static Effect TexturedEffect;
        public static Effect BasicEffect;
        public static Effect MainEffect;

        CharacterController characterController;
        CharacterControllerInput characterInput;

        public static float stretch;
        public static Color lightColor = Color.White;

        private static Game1 _game1;
        public static int shadowmaps;
        public static bool blur = true;
        public static bool drawshadowmap = false;
        public static bool drawlightbuffer = false;
        public static int occluders = 0;
        public static Vector2 texOffset = new Vector2(0.5f, 0.5f);
        public static Vector2 texScale = Vector2.One;
        public static Game1 thegame
        {
            get
            {
                return _game1;
            }
        }
        public static Model boxmodel, cylindermodel, spheremodel, capsulemodel;
        public static Model lppbox, lppcylinder, lppsphere, lppcapsule;

        public static float mix = 0.035f;
        public Model floormodel;
        public static Camera cam;
        public static Matrix projmatrix;
        public static Random rand;
        public static Space space;
        Component fallingBox;
        Component immovableBox;
        int counter = 10;
        PhysicsInfo boxdata;
        public static GameTime time;
        public static bool physdebug = false;
        public static bool drawconstraints = false;
        public static bool drawedgemask = false;
        public static bool drawdepth = false;
        public static bool drawnormal = false;
        public static bool drawall = false;
        public static bool drawspheres = false;
        public static KeyboardBuffer kbBuffer;
        Window window;
        Lift lift;
        TextBox textBox;
        Manager Manager;
#region blah
        bool isGrabbing = false;
        KeyboardState laststate = new KeyboardState();
        MouseState lastmouse = new MouseState();
        public bool minimized = false;
        BlendState bs;
        CameraController camcontrol;
        public static ActorContainer Actors;
        List<LPPMesh> meshes = new List<LPPMesh>();
        List<Light> visibleLights = new List<Light>();
        List<LPPMesh> occludingMeshes = new List<LPPMesh>();
        LPPRenderer lppRenderer;
        SpriteFont font;
        public static Texture2D circleimg;
        public static Effect ShadowEffect;
        public SphereDrawer SphereDrawer;

        int onScreenObj;
        int onScreenLights;
        public static bool drawmodels = true;

#endregion

        protected override void OnDeactivated(object sender, System.EventArgs args)
        {
            minimized = true;
            base.OnDeactivated(sender, args);
        }

        protected override void OnActivated(object sender, System.EventArgs args)
        {
            minimized = false;
            base.OnActivated(sender, args);
        }

        public Game1()
        {
            IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = scrheight;
            graphics.PreferredBackBufferWidth = scrwidth;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;            
            graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";      
            _game1 = this;
            rand = new Random();
            SphereDrawer = new SphereDrawer(this);
            kbBuffer = new KeyboardBuffer(this.Window.Handle);            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Manager = new Manager(this, graphics, "Default");
            Manager.AutoCreateRenderTarget = true;
            Manager.Initialize();

            ModelDrawer = new BruteModelDrawer(this);
            ConstraintDrawer = new LineDrawer(this);
            LightDrawer = new LightDrawer(this);
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0.0f, -10.0f, 0.0f);
            Actors = new ActorContainer(space, ModelDrawer, ConstraintDrawer, LightDrawer);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            

            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region models

            boxmodel = Content.Load<Model>("primitives/box");
            lppbox = Content.Load<Model>("primitives/lit/box");
            lppsphere = Content.Load<Model>("primitives/lit/sphere");
            lppcapsule = Content.Load<Model>("primitives/lit/capsule");
            lppcylinder = Content.Load<Model>("primitives/lit/cylinder");
            cylindermodel = Content.Load<Model>("primitives/cylinder");
            spheremodel = Content.Load<Model>("primitives/sphere");
            capsulemodel = Content.Load<Model>("primitives/capsule");
            floormodel = Content.Load<Model>("floor/floor");
            boxdata = Content.Load<PhysicsInfo>("walls/roomphys");
            circleimg = Content.Load<Texture2D>("textures/circle");
            Model blockman = Content.Load<Model>("floor/floor");
            //Model skullmodel = Content.Load<Model>("skull/skull");
            Model wallmodel = Content.Load<Model>("room/room");
            PhysicsInfo skullphys = Content.Load<PhysicsInfo>("skull/skullphys");
            ShadowEffect = Content.Load<Effect>("shaders/Shadow");
            Model sceneModel = Content.Load<Model>("scene/scenetest");
            PhysicsInfo scenePhys = Content.Load<PhysicsInfo>("scene/scenetestphys");

            #endregion

            #region Effects
            NormalEffect = Content.Load<Effect>("shaders/LPPNormalEffect");
            BasicEffect = Content.Load<Effect>("shaders/LPPBasicEffect");
            TexturedEffect = Content.Load<Effect>("shaders/LPPTexturedEffect");
            MainEffect = Content.Load<Effect>("shaders/LPPMainEffect");
            #endregion
            Vector3 lowerButton = new Vector3(134.988f, 61.178f, 126.411f) * (2.54f / 64.0f);
            Vector3 upperButton = new Vector3(134.988f, 64.803f, 126.411f) * (2.54f / 64.0f);
            float convFactor = 2.54f / 64.0f;
            
            Console.Parse("physdebug false");
            Console.Parse("drawconstraints false");
            Console.Parse("drawedgemask false");
            Console.Parse("drawdepth false");
            Console.Parse("drawnormal false");
            Console.Parse("drawall false");
            Console.Parse("drawspheres false");
            Console.Parse("blur true");
            Console.Parse("drawshadowmap false");
            Console.Parse("drawlightbuffer false");
            Console.Parse("showPosition true");
           
            Console.LoadContent(Content);

            /*boxdata = new PhysicsInfo();
            boxdata.CreateBox(new Vector3(-1, -1, -1), Matrix.Identity, Vector3.One);
            boxdata.CreateBox(new Vector3(1, 1, 1), Matrix.Identity, Vector3.One);
            boxdata.CreateSphere(new Vector3(1, 3, 1), 1);*/


            // TODO: use this.Content to load your game content here
            projmatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight, 0.01f, 2500.0f);
            /*
            fallingBox = new Component(new Vector3(3.6f, 7.5f, -2.4f), Quaternion.Identity, Vector3.One / 2.0f);
            fallingBox.SetCollision(skullphys, false);
            fallingBox.Mesh = new LPPMesh(skullmodel);
            fallingBox.Mesh.Occlude = true;
            Actors.Add(new Actor(fallingBox));*/
            

            /*immovableBox = new TriangleMeshActor(floormodel, Vector3.Zero, Vector3.One / 8.0f);
            immovableBox.Mesh = new LPPMesh(floormodel);
            immovableBox.Mesh.Occlude = true;            

            Entities.Add(new Entity(immovableBox));*/

            immovableBox = new TriangleMeshComponent(wallmodel, Vector3.Zero, Vector3.One / (64.0f / 2.54f));
            var lppmesh = new LPPMesh(wallmodel);

            var scene = new Component(new LPPMesh(sceneModel), new Vector3(3.8f, 7.5f, -3.0f), Quaternion.Identity, Vector3.One / 64.0f);
            scene.SetCollision(scenePhys, true);
            Actors.Add(new Actor(scene));
            Actors.Add(new LiftButton(upperButton, Vector3.One / 64.0f, "lift up"));
            Actors.Add(new LiftButton(lowerButton, Vector3.One / 64.0f, "lift down"));

            lppmesh.SpecularPower = 1.0f;
            lppmesh.Shininess = 1.0f;

            immovableBox.Mesh = lppmesh;

            Actors.Add(new Actor(immovableBox)); 



            Actors.Add(new CeilingLamp(new Vector3(-1.5f, 11.9f, 1.68f), 0.75f));
            Actors.Add(new Ladder(new Vector3(13.735f, 5.0f, -2.32f), 1.0f, 3.5f, 0.5f));
            lift = new Lift(Vector3.Zero, 1 / 64.0f);
            Actors.Add(lift);
            
            Actors.Add(new PulsingLight(new Vector3(12.86f, 3.43f, 5.84f), Color.Red, 7.5f, 1.0f, 0.2f, 3.0d));            
            Actors.Add(new BareBulb(new Vector3(-14.5f, 11.4f, 3f), Vector3.One / 64.0f, Color.Wheat, 10.0f, 0.6f));
            font = Content.Load<SpriteFont>("font");
            
            RasterizerState rs = RasterizerState.CullCounterClockwise;
            GraphicsDevice.RasterizerState = rs;
            
            bs = GraphicsDevice.BlendState;

            cam = new Camera();
            cam.Aspect = (float)scrwidth / (float)scrheight;
            cam.Viewport = new Viewport(0, 0, scrwidth, scrheight);
            cam.NearClip = 0.01f;
            cam.FarClip = 100.0f;
            cam.Transform = Matrix.Identity;

            characterController = new CharacterController(new Vector3(-1.0f, 2.0f, 7.311f), 3.0f, 0.75f, 0.1f, 0.2f);
            characterInput = new CharacterControllerInput(space, cam);
            characterInput.CharacterController = characterController;
            characterInput.Activate();

            camcontrol = new CameraController(cam, Vector3.Zero, Vector2.Zero);
            Actors.Add(new Actor(new FollowLight(12.5f, 1.0f, Color.White, cam)));
            lppRenderer = new LPPRenderer(GraphicsDevice, Content, scrwidth, scrheight);

            for (int i = /*200*/0; i > 0; --i)
            {
                space.Update(0.0166f);
            }

            window = new Dialog(Manager);
            window.SetPosition(0, 0);
            window.SetSize(512, 512);
            window.Text = "Console";
            textBox = new TextBox(Manager);
            textBox.SetPosition(32, 32);
            window.Add(textBox, false);
            Manager.Add(window);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            frameCounter++;
            elapsedTime += gameTime.ElapsedGameTime;
            time = gameTime;
            KeyboardState curState;
            window.Visible = Console.GetBool("consoleActive");
            if (Console.GetBool("consoleActive"))
            {
                Console.HandleInput(Keyboard.GetState());
                curState = new KeyboardState();
            }
            else
            {
                curState = Keyboard.GetState();
            }
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            camcontrol.HandleInput(curState, Mouse.GetState());

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            space.Update(dt);

            characterInput.Update(dt, laststate, curState, GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.One));

            //if (!minimized) cam.HandleInput(Keyboard.GetState(), Mouse.GetState());

            base.Update(gameTime);

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && lastmouse.LeftButton == ButtonState.Released)
            {
                if (characterController.Grabbing)
                {
                    characterController.ReleaseEntity();
                }
                else if (characterController.LastTouched != null)
                {
                    EntityTag tag = characterController.LastTouched.Tag as EntityTag;
                    if (tag != null)
                    {
                        if (tag.useType == EntityTag.UseType.Grab)
                        {
                            characterController.GrabEntity();
                        }
                        else if (tag.useType == EntityTag.UseType.Use)
                        {
                            tag.ParentActor.Use();
                        }
                    }
                }

            }

            if (curState.IsKeyDown(Keys.Up))
            {
                lift.LiftUp();
            }
            if (curState.IsKeyDown(Keys.Down))
            {
                lift.LiftDown();
            }
            if (curState.IsKeyDown(Keys.Left))
            {
                lift.LiftStop();
            }
            if (curState.IsKeyDown(Keys.Right))
            {
                texOffset.Y -= 0.01f;
            }

            if (curState.IsKeyDown(Keys.B) && !laststate.IsKeyDown(Keys.B))
            {
                for (int i = 2000; i > 0; i--)
                {
                    space.Update(0.0166f);
                }
            }
            if (curState.IsKeyDown(Keys.J) && !laststate.IsKeyDown(Keys.J))
            {
                characterController.NoClip = !characterController.NoClip;
            }
            if (curState.IsKeyDown(Keys.P) && !laststate.IsKeyDown(Keys.P))
            {
                physdebug = !physdebug;
            }

            if (curState.IsKeyDown(Keys.O) && !laststate.IsKeyDown(Keys.O))
            {
                drawconstraints = !drawconstraints;
            }
            if (curState.IsKeyDown(Keys.T) && !laststate.IsKeyDown(Keys.T))
            {
                drawmodels = !drawmodels;
            }
            if (curState.IsKeyDown(Keys.F) && !laststate.IsKeyDown(Keys.F))
            {
                characterController.isFlying = !characterController.isFlying;
            }
            if (curState.IsKeyDown(Keys.Q) && !laststate.IsKeyDown(Keys.Q))
            {
                Console.Toggle("consoleActive");
            }
            if (curState.IsKeyDown(Keys.N) && !laststate.IsKeyDown(Keys.N))
            {
                Vector3 forwardvel = cam.Transform.Forward;
                BoxComponent.Spawn(cam.Transform.Translation + (forwardvel * 5.0f), forwardvel * 30.0f);
            }
            if (counter == 10) counter = 0;
            else
                counter++;

            laststate = curState;
            lastmouse = Mouse.GetState();

            Actors.Update(gameTime);
            Manager.Update(gameTime);

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            onScreenLights = 0;
            onScreenObj = 0;
            shadowmaps = 0;

            Manager.Draw(gameTime);
            RenderTarget2D overlay = Manager.RenderTarget;

            if (physdebug)
            {
                if (drawmodels)
                    ModelDrawer.Draw(cam.EyeTransform, cam.ProjectionTransform);
                if (drawconstraints)
                    ConstraintDrawer.Draw(cam.EyeTransform, cam.ProjectionTransform);
                LightDrawer.Draw(cam.EyeTransform, cam.ProjectionTransform);
                if (drawspheres)
                    SphereDrawer.Draw(cam.EyeTransform, cam.ProjectionTransform);
            }
            else
            {

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                meshes.Clear();
                visibleLights.Clear();
                occludingMeshes.Clear();

                foreach (LPPMesh mesh in Actors.LPPMeshes)
                {                   
                    if (mesh.Highlight)
                    {
                        lppRenderer.Highlight(mesh);
                    }
                    meshes.Add(mesh);
                    onScreenObj++;
                    
                    if (mesh.Occlude)
                    {
                        occludingMeshes.Add(mesh);
                    }
                }

                foreach (Component component in Actors.Components)
                {
                    if (component.Lights.Count > 0)
                    {
                        foreach (Light light in component.Lights)
                        {
                            if (cam.Frustum.Intersects(light.BoundingSphere))
                            {
                                visibleLights.Add(light);
                                onScreenLights++;
                            }
                        }
                    }
                    if (visibleLights.Count == 0)
                    {
                        visibleLights.Add(Light.Dummy());
                    }
                }

                RenderTarget2D output = lppRenderer.Render(cam, visibleLights, meshes, occludingMeshes);

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

               
                String mystr = String.Format("fps: {0}", frameRate);
                if (Console.GetBool("texInfo"))
                    mystr += String.Format("yoffs: {0}, yscale: {1}", texOffset.X, texOffset.Y);
                if (Console.GetBool("occluderInfo"))
                    mystr += String.Format(", occluders: {0}", occluders);
                if (Console.GetBool("meshCountInfo"))
                    mystr += String.Format("GBuffer Meshes: {0}, Reconstructed Meshes: {1}", LPPRenderer.meshCountGBuffer, LPPRenderer.meshCountReconstructed);
                if (Console.GetBool("showPosition"))
                    mystr += String.Format(", pos: {0}", characterController.Body.Position);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

                spriteBatch.Draw(output, new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                spriteBatch.DrawString(font, mystr, new Vector2(5, 5), Color.White);
                //spriteBatch.Draw(overlay, new Microsoft.Xna.Framework.Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Color(255,255,255,255));
                Console.Draw(spriteBatch);

                spriteBatch.End();

               

                occluders = 0;
            }
            base.Draw(gameTime);
        }
    }
}
