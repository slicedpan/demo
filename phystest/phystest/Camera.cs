//-----------------------------------------------------------------------------
// Camera.cs
//
// Jorge Adriano Luna 2010
// http://jcoluna.wordpress.com
//-----------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    /// <summary>
    /// Class that holds all camera needed parameters. Its a model class, like in MVC pattern.
    /// An extern controller should change its parameters.
    /// </summary>
    public class Camera
    {
        private Matrix _transform = Matrix.Identity;
        private Matrix _eyeTransform = Matrix.Identity;
        private Matrix _projectionTransform = Matrix.Identity;
        private Matrix _eyeProjectionTransform = Matrix.Identity;
        private Viewport _viewport;
        private BoundingFrustum _frustum = new BoundingFrustum(Matrix.Identity);
        private float _farClip = 100;
        private float _nearClip = 1;
        private float _fovYDegrees = 45;
        private float _aspect = 1;

        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                Matrix.Invert(ref _transform, out _eyeTransform);
                Matrix.Multiply(ref _eyeTransform, ref _projectionTransform, out _eyeProjectionTransform);
                _frustum.Matrix = _eyeTransform * _projectionTransform;
            }
        }

        public Matrix EyeTransform
        {
            get { return _eyeTransform; }
        }

        public Matrix ProjectionTransform
        {
            get { return _projectionTransform; }
        }

        public Viewport Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        public BoundingFrustum Frustum
        {
            get { return _frustum; }
        }

        public float FarClip
        {
            get { return _farClip; }
            set
            {
                _farClip = value;
                ComputeProjectionTransform();
            }
        }

        public float NearClip
        {
            get { return _nearClip; }
            set
            {
                _nearClip = value;
                ComputeProjectionTransform();
            }
        }

        public float FovYDegrees
        {
            get { return _fovYDegrees; }
            set
            {
                _fovYDegrees = value;
                ComputeProjectionTransform();
            }
        }

        public float Aspect
        {
            get { return _aspect; }
            set
            {
                _aspect = value;
                ComputeProjectionTransform();
            }
        }

        public Matrix EyeProjectionTransform
        {
            get { return _eyeProjectionTransform; }
        }

        public Camera()
        {
            ComputeProjectionTransform();
        }

        public void ComputeProjectionTransform()
        {
            Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(_fovYDegrees),
                        Aspect, _nearClip, _farClip, out _projectionTransform);
            Matrix.Multiply(ref _eyeTransform, ref _projectionTransform, out _eyeProjectionTransform);
            _frustum.Matrix = _eyeTransform * _projectionTransform;
        }

        /// <summary>
        /// Project bounding sphere on our screen: topLeft in post-projection coordinates [-1..1] 
        /// and size in range [0..2]
        // http://www.gamasutra.com/view/feature/2942/the_mechanics_of_robust_stencil_.php?page=6
        // This method was taken from
        // //////////////////////////////////////////////////////////////////////////////////////////
        //	Point Light/CalculateScissorRectangle.cpp
        //	Calculate the scissor rectangle for a point light
        //	Downloaded from: www.paulsprojects.net
        //	Created:	14th December 2002
        //
        //	Copyright (c) 2006, Paul Baker
        //	Distributed under the New BSD Licence. (See accompanying file License.txt or copy at
        //	http://www.paulsprojects.net/NewBSDLicense.txt)
        // ///////////////////////////////////////////////////////////////////////////////////////	
        /// </summary>
        /// <param name="boundingSphere"></param>
        /// <param name="topLeft"></param>
        /// <param name="size"></param>
        /// 
        public void ProjectBoundingSphereOnScreen(BoundingSphere boundingSphere, out Vector2 topLeft, out Vector2 size)
        {
            //l is the bounding sphere's position in eye space
            Vector3 l = Vector3.Transform(boundingSphere.Center, _eyeTransform);

            //Store the coordinates of the scissor rectangle
            //Start by setting them to the outside of the screen
            float scissorLeft = -1.0f;
            float scissorRight = 1.0f;

            float scissorBottom = -1.0f;
            float scissorTop = 1.0f;

            //r is the radius of the bounding sphere
            float r = boundingSphere.Radius;


            //halfNearPlaneHeight is half the height of the near plane, i.e. from the centre to the top
            float halfNearPlaneHeight = _nearClip * (float)Math.Tan(MathHelper.ToRadians(_fovYDegrees * 0.5f));

            float halfNearPlaneWidth = halfNearPlaneHeight * _aspect;

            //All calculations in eye space


            //We wish to find 2 planes parallel to the Y axis which are tangent to the bounding sphere
            //of the light and pass through the origin (camera position)

            //plane normal. Of the form (x, 0, z)
            Vector3 normal;

            //Calculate the discriminant of the quadratic we wish to solve to find nx(divided by 4)
            float d = (l.Z * l.Z) * ((l.X * l.X) + (l.Z * l.Z) - r * r);

            //If d>0, solve the quadratic to get the normal to the plane
            if (d > 0.0f)
            {
                float rootD = (float)Math.Sqrt(d);

                //Loop through the 2 solutions
                for (int i = 0; i < 2; ++i)
                {
                    //Calculate the normal
                    if (i == 0)
                        normal.X = r * l.X + rootD;
                    else
                        normal.X = r * l.X - rootD;

                    normal.X /= (l.X * l.X + l.Z * l.Z);

                    normal.Z = r - normal.X * l.X;
                    normal.Z /= l.Z;

                    //We need to divide by normal.X. If ==0, no good
                    if (normal.X == 0.0f)
                        continue;


                    //p is the point of tangency
                    Vector3 p;

                    p.Z = (l.X * l.X) + (l.Z * l.Z) - r * r;
                    p.Z /= l.Z - ((normal.Z / normal.X) * l.X);

                    //If the point of tangency is behind the camera, no good
                    if (p.Z >= 0.0f)
                        continue;

                    p.X = -p.Z * normal.Z / normal.X;

                    //Calculate where the plane meets the near plane
                    //divide by the width to give a value in [-1, 1] for values on the screen
                    float screenX = normal.Z * _nearClip / (normal.X * halfNearPlaneWidth);

                    //If this is a left bounding value (p.X<l.X) and is further right than the
                    //current value, update
                    if (p.X < l.X && screenX > scissorLeft)
                        scissorLeft = screenX;

                    //Similarly, update the right value
                    if (p.X > l.X && screenX < scissorRight)
                        scissorRight = screenX;
                }
            }


            //Repeat for planes parallel to the x axis
            //normal is now of the form(0, y, z)
            normal.X = 0.0f;

            //Calculate the discriminant of the quadratic we wish to solve to find ny(divided by 4)
            d = (l.Z * l.Z) * ((l.Y * l.Y) + (l.Z * l.Z) - r * r);

            //If d>0, solve the quadratic to get the normal to the plane
            if (d > 0.0f)
            {
                float rootD = (float)Math.Sqrt(d);

                //Loop through the 2 solutions
                for (int i = 0; i < 2; ++i)
                {
                    //Calculate the normal
                    if (i == 0)
                        normal.Y = r * l.Y + rootD;
                    else
                        normal.Y = r * l.Y - rootD;

                    normal.Y /= (l.Y * l.Y + l.Z * l.Z);

                    normal.Z = r - normal.Y * l.Y;
                    normal.Z /= l.Z;

                    //We need to divide by normal.Y. If ==0, no good
                    if (normal.Y == 0.0f)
                        continue;


                    //p is the point of tangency
                    Vector3 p;

                    p.Z = (l.Y * l.Y) + (l.Z * l.Z) - r * r;
                    p.Z /= l.Z - ((normal.Z / normal.Y) * l.Y);

                    //If the point of tangency is behind the camera, no good
                    if (p.Z >= 0.0f)
                        continue;

                    p.Y = -p.Z * normal.Z / normal.Y;

                    //Calculate where the plane meets the near plane
                    //divide by the height to give a value in [-1, 1] for values on the screen
                    float screenY = normal.Z * _nearClip / (normal.Y * halfNearPlaneHeight);

                    //If this is a bottom bounding value (p.Y<l.Y) and is further up than the
                    //current value, update
                    if (p.Y < l.Y && screenY > scissorBottom)
                        scissorBottom = screenY;

                    //Similarly, update the top value
                    if (p.Y > l.Y && screenY < scissorTop)
                        scissorTop = screenY;
                }
            }

            //compute the width & height of the rectangle
            size.X = scissorRight - scissorLeft;
            size.Y = scissorTop - scissorBottom;

            topLeft.X = scissorLeft;
            topLeft.Y = -scissorBottom - size.Y;

        }
    }
}
