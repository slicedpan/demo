using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using Microsoft.Xna.Framework.Input;

namespace phystest
{
    public class CharacterControllerInput
    {
        /// <summary>
        /// Camera to use for input.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Current offset from the position of the character to the 'eyes.'
        /// </summary>
        public Vector3 CameraOffset = new Vector3(0, .7f, 0);

        /// <summary>
        /// Physics representation of the character.
        /// </summary>
        /// 
        private CharacterController _charControl;
        public CharacterController CharacterController
        {
            set
            {
                _charControl = value;
                Space.Add(_charControl);
                CameraOffset = new Vector3(0.0f, _charControl.Body.Length * 0.45f, 0.0f);
                _charControl.headOffset = CameraOffset;
                Deactivate();
            }
            get
            {
                return _charControl;
            }
        }

        /// <summary>
        /// Whether or not to use the character controller's input.
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// Owning space of the character.
        /// </summary>
        public Space Space;


        /// <summary>
        /// Constructs the character and internal physics character controller.
        /// </summary>
        /// <param name="owningSpace">Space to add the character to.</param>
        /// <param name="CameraToUse">Camera to attach to the character.</param>
        public CharacterControllerInput(Space owningSpace, Camera CameraToUse)
        {            
            Space = owningSpace;  
            Camera = CameraToUse;
        }

        /// <summary>
        /// Gives the character control over the Camera and movement input.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;                
                CharacterController.Activate();
                Camera.Transform = Matrix.CreateTranslation(CharacterController.Body.Position);
            }
        }

        /// <summary>
        /// Returns input control to the Camera.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                CharacterController.Deactivate();
            }
        }


        /// <summary>
        /// Handles the input and movement of the character.
        /// </summary>
        /// <param name="dt">Time since last frame in simulation seconds.</param>
        /// <param name="previousKeyboardInput">The last frame's keyboard state.</param>
        /// <param name="keyboardInput">The current frame's keyboard state.</param>
        /// <param name="previousGamePadInput">The last frame's gamepad state.</param>
        /// <param name="gamePadInput">The current frame's keyboard state.</param>
        public void Update(float dt, KeyboardState previousKeyboardInput, KeyboardState keyboardInput, GamePadState previousGamePadInput, GamePadState gamePadInput)
        {
            if (IsActive)
            {
                //Note that the character controller's update method is not called here; this is because it is handled within its owning space.
                //This method's job is simply to tell the character to move around based on the Camera and input.

                //Puts the Camera at eye level.
                Vector3 scale;
                Quaternion quat;
                Vector3 trans;
                Camera.Transform.Decompose(out scale, out quat, out trans);
                Camera.Transform = Matrix.CreateFromQuaternion(quat) * Matrix.CreateTranslation(CharacterController.Body.Position + CameraOffset);
                Vector2 totalMovement = Vector2.Zero;
#if !WINDOWS
                Vector3 forward = Camera.WorldMatrix.Forward;
                forward.Y = 0;
                forward.Normalize();
                Vector3 right = Camera.WorldMatrix.Right;
                totalMovement += gamePadInput.ThumbSticks.Left.Y * new Vector2(forward.X, forward.Z);
                totalMovement += gamePadInput.ThumbSticks.Left.X * new Vector2(right.X, right.Z);
                CharacterController.MovementDirection = totalMovement;

                //Jumping
                if (previousGamePadInput.IsButtonUp(Buttons.LeftStick) && gamePadInput.IsButtonDown(Buttons.LeftStick))
                {
                    CharacterController.Jump();
                }
#else


                //Collect the movement impulses.

                Vector3 movementDir = Vector3.Zero;
                Vector3 total3DMovement = Vector3.Zero;

                if (CharacterController.ladder || CharacterController.isFlying)
                {
                    if (keyboardInput.IsKeyDown(Keys.W))
                    {
                        movementDir = Camera.Transform.Forward;
                        total3DMovement += movementDir;
                    }
                    if (keyboardInput.IsKeyDown(Keys.S))
                    {
                        movementDir = Camera.Transform.Forward;
                        total3DMovement -= movementDir;
                    }
                    if (keyboardInput.IsKeyDown(Keys.A))
                    {
                        movementDir = Camera.Transform.Left;
                        total3DMovement += movementDir;
                    }
                    if (keyboardInput.IsKeyDown(Keys.D))
                    {
                        movementDir = Camera.Transform.Right;
                        total3DMovement += movementDir;
                    }
                    if (keyboardInput.IsKeyDown(Keys.Space))
                    {
                        movementDir = Vector3.UnitY;
                        total3DMovement += movementDir;
                    }
                    if (keyboardInput.IsKeyDown(Keys.LeftControl))
                    {
                        movementDir = Vector3.UnitY;
                        total3DMovement -= movementDir;
                    }
                    if (total3DMovement.Length() > 0.001f)
                    {
                        CharacterController.InjectVelocity(Vector3.Normalize(total3DMovement));
                    }
                    else
                    {
                        CharacterController.InjectVelocity(Vector3.Zero);
                    }
                }
                else
                {
                    if (keyboardInput.IsKeyDown(Keys.W))
                    {
                        movementDir = Camera.Transform.Forward;
                        totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                    }
                    if (keyboardInput.IsKeyDown(Keys.S))
                    {
                        movementDir = Camera.Transform.Forward;
                        totalMovement -= Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                    }
                    if (keyboardInput.IsKeyDown(Keys.A))
                    {
                        movementDir = Camera.Transform.Left;
                        totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                    }
                    if (keyboardInput.IsKeyDown(Keys.D))
                    {
                        movementDir = Camera.Transform.Right;
                        totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
                    }
                    if (totalMovement == Vector2.Zero)
                        CharacterController.MovementDirection = Vector2.Zero;
                    else
                        CharacterController.MovementDirection = Vector2.Normalize(totalMovement);



                    //Jumping
                    if (keyboardInput.IsKeyDown(Keys.Space) && previousKeyboardInput.IsKeyUp(Keys.Space))
                    {
                        CharacterController.Jump();
                    }
                }

                CharacterController.orientation = quat;
                CharacterController.ladder = false;
#endif
            }
        }
    }
}
