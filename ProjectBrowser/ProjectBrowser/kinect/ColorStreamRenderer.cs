using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace ProjectBrowser
{
    public class ColorStreamRenderer : Object2D
    {
        // This child responsible for rendering the color stream's skeleton.
        //private readonly SkeletonStreamRenderer skeletonStream;
        
        // The last frame of color data.
        private byte[] colorData;

        // The color frame as a texture.
        private Texture2D colorTexture;

        // The back buffer where color frame is scaled as requested by the Size.
        private RenderTarget2D backBuffer;
        
        // This Xna effect is used to swap the Red and Blue bytes of the color stream data.
        private Effect kinectColorVisualizer;

        // Whether or not the back buffer needs updating.
        private bool needToRedrawBackBuffer = true;

        public ColorStreamRenderer(Game game):base(game)
        {
            //this.skeletonStream = new SkeletonStreamRenderer(game, this.SkeletonToColorMap);
        }

        public override void Initialize()
        {
            base.Initialize();
            //this.Size = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        }

        // The update method where the new color frame is retrieved.
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If the sensor is not found, not running, or not connected, stop now
            if (null == this.Chooser.Sensor ||
                false == this.Chooser.Sensor.IsRunning ||
                KinectStatus.Connected != this.Chooser.Sensor.Status)
            {
                return;
            }
            
            using (var frame = this.Chooser.Sensor.ColorStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (frame == null)
                {
                    return;
                }

                // Reallocate values if necessary
                if (this.colorData == null || this.colorData.Length != frame.PixelDataLength)
                {
                    this.colorData = new byte[frame.PixelDataLength];

                    this.colorTexture = new Texture2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color);

                    this.backBuffer = new RenderTarget2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color, 
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount, 
                        RenderTargetUsage.PreserveContents);            
                }

                frame.CopyPixelDataTo(this.colorData);
                this.needToRedrawBackBuffer = true;
            }

            // Update the skeleton renderer
            //this.skeletonStream.Update(gameTime);
        }

        // This method renders the color and skeleton frame.
        public override void Draw(GameTime gameTime)
        {
            // If we don't have the effect load, load it
            if (null == this.kinectColorVisualizer)
            {
                this.LoadContent();
            }

            // If we don't have a target, don't try to render
            if (null == this.colorTexture)
            {
                return;
            }
            
            if (this.needToRedrawBackBuffer)
            {
                // Set the backbuffer and clear
                this.Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                this.Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                this.colorTexture.SetData<byte>(this.colorData);

                // Draw the color image
                this.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, this.kinectColorVisualizer);
                this.SharedSpriteBatch.Draw(this.colorTexture, Vector2.Zero, Color.White);
                this.SharedSpriteBatch.End();

                // Draw the skeleton
                //this.skeletonStream.Draw(gameTime);

                // Reset the render target and prepare to draw scaled image
                this.Game.GraphicsDevice.SetRenderTargets(null);

                // No need to re-render the back buffer until we get new data
                this.needToRedrawBackBuffer = false;
            }
                // Draw the scaled texture
                this.SharedSpriteBatch.Begin();
                this.SharedSpriteBatch.Draw(
                    this.backBuffer,
                    new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y),
                    null,
                    Color.White);
                this.SharedSpriteBatch.End();
            

            base.Draw(gameTime);
        }

        // This method loads the Xna effect.
        protected override void LoadContent()
        {
            base.LoadContent();

            // This effect is necessary to remap the BGRX byte data we get
            // to the XNA color RGBA format.
            this.kinectColorVisualizer = Game.Content.Load<Effect>("Kinect/Color/KinectColorVisualizer");
        }

        /// This method is used to map the SkeletonPoint to the color frame.
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the color frame.</returns>
        private Vector2 SkeletonToColorMap(SkeletonPoint point)
        {
            if ((null != Chooser.Sensor) && (null != Chooser.Sensor.ColorStream))
            {
                // This is used to map a skeleton point to the color image location
                var colorPt = Chooser.Sensor.CoordinateMapper.MapSkeletonPointToColorPoint(point, Chooser.Sensor.ColorStream.Format);
                return new Vector2(colorPt.X, colorPt.Y);
            }

            return Vector2.Zero;
        }
    }
}
