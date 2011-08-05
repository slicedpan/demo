using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    public class QuadRenderer
    {
        //buffers for rendering the quad
        private VertexPositionTexture[] _vertexBuffer = null;
        private short[] _indexBuffer = null;

        public QuadRenderer()
        {
            _vertexBuffer = new VertexPositionTexture[4];
            _vertexBuffer[0] = new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0.0f, 0.0f));
            _vertexBuffer[1] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1.0f, 0.0f));
            _vertexBuffer[2] = new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0.0f, 1.0f));
            _vertexBuffer[3] = new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1.0f, 1.0f));

            _indexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void RenderQuad(GraphicsDevice graphicsDevice, Vector2 v1, Vector2 v2, Vector2 pixelSize)
        {
            _vertexBuffer[0].Position.X = v1.X - pixelSize.X;
            _vertexBuffer[0].Position.Y = v2.Y - pixelSize.Y;

            _vertexBuffer[1].Position.X = v2.X - pixelSize.X;
            _vertexBuffer[1].Position.Y = v2.Y - pixelSize.Y;

            _vertexBuffer[2].Position.X = v1.X - pixelSize.X;
            _vertexBuffer[2].Position.Y = v1.Y - pixelSize.Y;

            _vertexBuffer[3].Position.X = v2.X - pixelSize.X;
            _vertexBuffer[3].Position.Y = v1.Y - pixelSize.Y;

            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                (PrimitiveType.TriangleList, _vertexBuffer, 0, 4, _indexBuffer, 0, 2);
        }

    }
}
