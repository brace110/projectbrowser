using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace ProjectBrowser
{
    // This class will pick a kinect sensor if available.

    public class KinectChooser : DrawableGameComponent
    {
        // The status to string mapping.
        private readonly Dictionary<KinectStatus, string> statusMap = new Dictionary<KinectStatus, string>();

        // The requested color image format.
        private readonly ColorImageFormat colorImageFormat;

        // The requested depth image format.
        private readonly DepthImageFormat depthImageFormat;

        // The chooser background texture.
        private Texture2D chooserBackground;
        
        // The SpriteBatch used for rendering.
        private SpriteBatch spriteBatch;
        
        // The font for rendering the state text.
        private SpriteFont font;

        private Game1 maingame;
        private SkeletonStreamProcessor sklClass;

        public KinectChooser(Game1 game, ColorImageFormat colorFormat, DepthImageFormat depthFormat, SkeletonStreamProcessor _sklClass)
            : base(game)
        {
            this.colorImageFormat = colorFormat;
            this.depthImageFormat = depthFormat;
            this.maingame = game;
            this.sklClass = _sklClass;

            KinectSensor.KinectSensors.StatusChanged += this.KinectSensors_StatusChanged;
            this.DiscoverSensor();

            this.statusMap.Add(KinectStatus.Connected, string.Empty);
            this.statusMap.Add(KinectStatus.DeviceNotGenuine, "Device Not Genuine");
            this.statusMap.Add(KinectStatus.DeviceNotSupported, "Device Not Supported");
            this.statusMap.Add(KinectStatus.Disconnected, "Required");
            this.statusMap.Add(KinectStatus.Error, "Error");
            this.statusMap.Add(KinectStatus.Initializing, "Initializing...");
            this.statusMap.Add(KinectStatus.InsufficientBandwidth, "Insufficient Bandwidth");
            this.statusMap.Add(KinectStatus.NotPowered, "Not Powered");
            this.statusMap.Add(KinectStatus.NotReady, "Not Ready");
        }

        // Gets the selected KinectSensor.
        public KinectSensor Sensor { get; private set; }

        // Gets the last known status of the KinectSensor.
        public KinectStatus LastStatus { get; private set; }

        /// This method initializes necessary objects.
        public override void Initialize()
        {
            base.Initialize();

            this.spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        // This method loads the textures and fonts.
        protected override void LoadContent()
        {
            base.LoadContent();

            this.chooserBackground = Game.Content.Load<Texture2D>("Kinect/Chooser/ChooserBackground");
            this.font = Game.Content.Load<SpriteFont>("Kinect/Chooser/Segoe16");
        }

        // This method renders the current state of the KinectChooser.
        public override void Draw(GameTime gameTime)
        {
            
            // If the spritebatch is null, call initialize
            if (this.spriteBatch == null)
            {
                this.Initialize();
            }

            // If the background is not loaded, load it now
            if (this.chooserBackground == null)
            {
                this.LoadContent();
            }

            // If we don't have a sensor, or the sensor we have is not connected
            // then we will display the information text
            
                if (this.Sensor == null || this.LastStatus != KinectStatus.Connected)
                {
                        this.spriteBatch.Begin();

                        // Render the background
                        this.spriteBatch.Draw(
                            this.chooserBackground,
                            new Vector2(Game.GraphicsDevice.Viewport.Width / 2, (Game.GraphicsDevice.Viewport.Height / 2) + 250),
                            null,
                            Color.White,
                            0,
                            new Vector2(this.chooserBackground.Width / 2, this.chooserBackground.Height / 2),
                            1,
                            SpriteEffects.None,
                            0);
                        
                        // Determine the text
                        string txt = "Required";
                        if (this.Sensor != null)
                        {
                            txt = this.statusMap[this.LastStatus];
                        }

                        // Render the text
                        Vector2 size = this.font.MeasureString(txt);
                        this.spriteBatch.DrawString(
                            this.font,
                            txt,
                            new Vector2((Game.GraphicsDevice.Viewport.Width - size.X) / 2, ((Game.GraphicsDevice.Viewport.Height / 2) + size.Y) + 250),
                            Color.White);

                        this.spriteBatch.End();
                }

            base.Draw(gameTime);
        }

        // This method ensures that the KinectSensor is stopped before exiting.
        protected override void UnloadContent()
        {
            base.UnloadContent();

            // Always stop the sensor when closing down
            if (this.Sensor != null)
            {
                this.Sensor.Stop();
            }
        }

        /// This method will use basic logic to try to grab a sensor.
        /// Once a sensor is found, it will start the sensor with the
        /// requested options.
        private void DiscoverSensor()
        {
            
            // Grab any available sensor
            this.Sensor = KinectSensor.KinectSensors.FirstOrDefault();

            if (this.Sensor != null)
            {
                this.LastStatus = this.Sensor.Status;

                // If this sensor is connected, then enable it
                if (this.LastStatus == KinectStatus.Connected)
                {
                    this.Sensor.SkeletonStream.Enable();
                    this.Sensor.ColorStream.Enable(this.colorImageFormat);
                    this.Sensor.DepthStream.Enable(this.depthImageFormat);

                    this.Sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    // Use seated tracking.
                    // this.Sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                    this.Sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sklClass.kinect_AllFramesReady);

                    try
                    {
                        this.Sensor.Start();
                        Sensor.ElevationAngle = 10;
                    }
                    catch (IOException)
                    {
                        // sensor is in use by another application
                        // will treat as disconnected for display purposes
                        this.Sensor = null;
                    }
                }
            }
            else
            {
                this.LastStatus = KinectStatus.Disconnected;
            }
        }

        /// This wires up the status changed event to monitor for 
        /// Kinect state changes.  It automatically stops the sensor
        /// if the device is no longer available.
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // If the status is not connected, try to stop it
            if (e.Status != KinectStatus.Connected)
            {
                e.Sensor.Stop();
            }

            this.LastStatus = e.Status;
            this.DiscoverSensor();
        }
    }
}
