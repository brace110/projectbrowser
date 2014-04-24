using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectBrowser
{
    
    // This class renders the current depth stream frame.

    public class DepthStreamRenderer : Object2D
    {
        // The child responsible for rendering the depth stream's skeleton.
        //private readonly SkeletonStreamRenderer skeletonStream;

        // The back buffer where the depth frame is scaled as requested by the Size.
        private RenderTarget2D backBuffer;

        // The last frame of depth data.
        private short[] depthData;

        // The depth frame as a texture.
        private Texture2D depthTexture;
        
        // This Xna effect is used to convert the depth to RGB color information.
        private Effect kinectDepthVisualizer;

        // Whether or not the back buffer needs updating.
        private bool needToRedrawBackBuffer = true;

        // Initializes a new instance of the DepthStreamRenderer class.
        public DepthStreamRenderer(Game game):base(game)
        {
            //this.skeletonStream = new SkeletonStreamRenderer(game, this.SkeletonToDepthMap);
        }

        // The update method where the new depth frame is retrieved.
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

            using (var frame = this.Chooser.Sensor.DepthStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (null == frame)
                {
                    return;
                }

                // Reallocate values if necessary
                if (null == this.depthData || this.depthData.Length != frame.PixelDataLength)
                {
                    this.depthData = new short[frame.PixelDataLength];
                    
                    this.depthTexture = new Texture2D(
                        Game.GraphicsDevice,
                        frame.Width,
                        frame.Height,
                        false,
                        SurfaceFormat.Bgra4444);

                    this.backBuffer = new RenderTarget2D(
                        Game.GraphicsDevice,
                        frame.Width,
                        frame.Height,
                        false,
                        SurfaceFormat.Color,
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount,
                        RenderTargetUsage.PreserveContents);
                }

                frame.CopyPixelDataTo(this.depthData);
                this.needToRedrawBackBuffer = true;
            }

            // Update the skeleton renderer
            //this.skeletonStream.Update(gameTime);
        }

        /// This method renders the color and skeleton frame.
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            // If we don't have a depth target, exit
            if (this.depthTexture == null)
            {
                return;
            }
            
            if (this.needToRedrawBackBuffer)
            {
                // Set the backbuffer and clear
                Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                this.depthTexture.SetData<short>(this.depthData);

                // Draw the depth image
                this.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, this.kinectDepthVisualizer);
                this.SharedSpriteBatch.Draw(this.depthTexture, Vector2.Zero, Color.White);
                this.SharedSpriteBatch.End();

                // Draw the skeleton
               // this.skeletonStream.Draw(gameTime);

                // Reset the render target and prepare to draw scaled image
                this.Game.GraphicsDevice.SetRenderTarget(null);

                // No need to re-render the back buffer until we get new data
                this.needToRedrawBackBuffer = false;
            }
                // Draw scaled image
                this.SharedSpriteBatch.Begin();
                this.SharedSpriteBatch.Draw(
                    this.backBuffer,
                    new Rectangle((int)Position.X, (int)Position.Y, (int)this.Size.X, (int)this.Size.Y),
                    null,
                    Color.White);
                this.SharedSpriteBatch.End();

            base.Draw(gameTime);
        }

        /// This method loads the Xna effect.
        protected override void LoadContent()
        {
            base.LoadContent();

            // This effect is used to convert depth data to color for display
            this.kinectDepthVisualizer = Game.Content.Load<Effect>("Kinect/Dept/KinectDepthVisualizer");
        }

        /// This method maps a SkeletonPoint to the depth frame.
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the depth frame.</returns>
        private Vector2 SkeletonToDepthMap(SkeletonPoint point)
        {
            if ((null != Chooser.Sensor) && (null != Chooser.Sensor.DepthStream))
            {
                // This is used to map a skeleton point to the depth image location
                var depthPt = Chooser.Sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(point, Chooser.Sensor.DepthStream.Format);
                return new Vector2(depthPt.X, depthPt.Y);
            }

            return Vector2.Zero;
        }
    }
}
