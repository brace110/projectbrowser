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
using Microsoft.Kinect;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;

namespace ProjectBrowser
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Dictionary with all the projects.
        Dictionary<Int32, Project> projects = new Dictionary<Int32, Project>();

        // Game state.
        public enum GameState
        {
            overview,
            detail 
        }

        public static GameState gameState = GameState.overview;

        // Screen size.
        public static Int32 screenWidth = 1000;
        public static Int32 screenHeight = 600;

        // Kinect.
        private KinectChooser chooser;
        private SkeletonStreamProcessor skeleton;
        private ColorStreamRenderer colorStreamerProcessor;
        private DepthStreamRenderer deptStreamerProcessor;

        // Database connection.
        MyDbWrapper db = new MyDbWrapper();

        private ParticleEngine particleEngine;
        public static Boolean particlesWereAdded;

        private MouseInput mouseInput;

        // Project location & hover vars.
        private Int32 xPos;
        private Int32 yPos;

        // Moving.
        private Boolean hovering = false;

        private Rectangle drophereRectangle;
        private Rectangle backbuttonRectangle;
        private Rectangle arrowLeftRectangle;
        private Rectangle arrowRightRectangle;

        // Font.
        private SpriteFont font;

        // Textures.
        private Dictionary<String, Texture2D> projectsTextures;
        private Dictionary<String, Texture2D> gameTextures;
        
        // List used to check which joints can control the menu etc..
        private List<JointType> allowedJoints;
        private int kinectCounter = 0;
        private bool hit;

        private Project hoveringProject;
        private Project detailsProject;
        private JointType hoveringWith;

        private Dictionary<Int32, Project> displayProjects = new Dictionary<int, Project>();
        private int startProject = 1;
        public static Texture2D pixel;
        private SpriteFont font2;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.graphics.PreferredBackBufferWidth = screenWidth;
            this.graphics.PreferredBackBufferHeight = screenHeight;

            // Needed for Kinect.
            this.graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            this.graphics.SynchronizeWithVerticalRetrace = true;

            // Makes the mouse visible.
            this.IsMouseVisible = true;

            // Fullscreen.
            graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            LoadInKinect();

            // Set which joints control the program.
            allowedJoints = new List<JointType>();
            allowedJoints.Add(JointType.HandLeft);
            allowedJoints.Add(JointType.HandRight);

            // Get all projects.
            projects = db.returnProjects();

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

            // Add spritebatch to services so the kinect can use it.
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            // Load in the Particle Engine.
            particleEngine = new ParticleEngine(this.Content);

            // Mouse input.
            mouseInput = new MouseInput();

            // Load in all textures.
            projectsTextures = TextureContent.LoadListContent<Texture2D>(this.Content, "projects");
            gameTextures = TextureContent.LoadListContent<Texture2D>(this.Content, "textures");

            pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White });

            // List<Int32> removeThese = new List<Int32>();

            // Load all textures from each project.
            foreach (KeyValuePair<Int32, Project> entry in projects)
            {
                // Load in the texture.
                if (projectsTextures.ContainsKey(entry.Value.textureName))
                {
                    projects[entry.Key].texture = projectsTextures[entry.Value.textureName];
                }
                else
                {
                    // No image available.
                    projects[entry.Key].texture = projectsTextures["no_image"];
                }
            }

            //foreach (Int32 item in removeThese)
            //{
            //    projects.Remove(item);
            //}

            initProjects();

            // Rectangles.
            drophereRectangle = new Rectangle((screenWidth / 2) - (150 / 2), 100, 150, 150);
            backbuttonRectangle = new Rectangle(35, screenHeight / 3, 75, 75);
            arrowLeftRectangle = new Rectangle(35, screenHeight / 3, 50, 50);
            arrowRightRectangle = new Rectangle(screenWidth - 100, screenHeight / 3, 50, 50);

            // Spritefont.
            font = Content.Load<SpriteFont>("font");
            font2 = Content.Load<SpriteFont>("font2");
        }

        private void initProjects()
        {
            displayProjects = new Dictionary<int, Project>();
            
            // Init the starting location for the projects (remember they are being drawn from the center).
            xPos = 175; 
            yPos = 110;

            Int32 i = 1;

            // Load all textures from each project.
            foreach (KeyValuePair<Int32, Project> entry in projects)
            {
                if (i >= startProject && i < (startProject + 6))
                {
                    // Set the rectangle to the right spot.
                    projects[entry.Key].locationRectangle = new Rectangle(xPos, yPos, entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);
                    // Set the rectangle to the right spot.
                    projects[entry.Key].place = entry.Value.locationRectangle;
                    // Change position for the next.
                    yPos += 145;

                    if (yPos > 450)
                    {
                        xPos += 500;
                        yPos = 110;
                    }

                    // Display project.
                    displayProjects[entry.Key] = entry.Value;
                }

                i++;
            }
        }

        private Boolean checkIfMoreProjecets()
        {
            if (projects.Count >= (startProject + 6))
            {
                return true;
            }

            return false;
        }

        private Boolean checkIfLessProjecets()
        {
            if (startProject > 1)
            {
                return true;
            }

            return false;
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
            // Allows the game to exit
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (keyboardState.IsKeyDown(Keys.Back))
            {
                gameState = GameState.overview;
            }

            // db.writeView(project);

            MouseState[] mouseState = mouseInput.refresh();
            Point mouseLocation = new Point(mouseState[0].X, mouseState[0].Y);

            Dictionary<JointType, Point> joints = skeleton.getSkeletonLocations(allowedJoints);

            switch (gameState)
            {
                case GameState.overview:
                    hit = false;

                    foreach (KeyValuePair<JointType, Point> item in joints)
                    {
                        foreach (KeyValuePair<Int32, Project> entry in displayProjects)
                        {
                            // foreach (KeyValuePair<JointType, Point> item in joints)
                            {
                                // Check if joint is over a texture.
                                if (entry.Value.locationRectangle.Contains(item.Value) && !hovering)
                                {
                                    // Joint is on texture.
                                    // Increase the counter.
                                    kinectCounter++;
                                    // Make sure it doesn't reset.
                                    hit = true;

                                    // If this has been going for 2.5 seconds.
                                    if (kinectCounter > 150)
                                    {
                                        // This project is hovering.
                                        entry.Value.hover = true;
                                        // Hovering project;
                                        hoveringProject = entry.Value;
                                        // Hovering active.
                                        hovering = true;
                                        // Reset.
                                        kinectCounter = 0;
                                        // Hovering with.
                                        hoveringWith = item.Key;

                                    }
                                }
                            }
                        }
                        
                        if (checkIfLessProjecets())
                        {
                            if (arrowLeftRectangle.Contains(item.Value))
                            {
                                kinectCounter++;
                                hit = true;

                                if (kinectCounter > 150)
                                {
                                    kinectCounter = 0;
                                    Console.WriteLine("Move Left");
                                    startProject -= 6;
                                    if (startProject < 1)
                                    {
                                        startProject = 1;
                                    }
                                    initProjects();
                                }
                            }
                        }
                        
                        if (checkIfMoreProjecets())
                        {
                            if (arrowRightRectangle.Contains(item.Value))
                            {
                                kinectCounter++;
                                hit = true;

                                if (kinectCounter > 150)
                                {
                                    kinectCounter = 0;
                                    Console.WriteLine("Move right");
                                    startProject += 6;
                                    initProjects();
                                }
                            }
                        }
                    }

                    if (!hit)
                    {
                        // No joint was on any texture.
                        kinectCounter = 0;
                    }

                    // Project is hovering.
                    if (hovering)
                    {
                        // Make it follow the mouse.
                        // entry.Value.locationRectangle = new Rectangle(mouseState[0].X - (entry.Value.locationRectangle.Width / 2), mouseState[0].Y - (entry.Value.locationRectangle.Height / 2), entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);

                        // If postion of hoverjoint is available.
                        if (joints.ContainsKey(hoveringWith))
                        {
                            // Make it follow the joint that selected it.
                            projects[hoveringProject.id].locationRectangle = new Rectangle(joints[hoveringWith].X - (projects[hoveringProject.id].locationRectangle.Width / 2), joints[hoveringWith].Y - (projects[hoveringProject.id].locationRectangle.Height / 2), projects[hoveringProject.id].locationRectangle.Width, projects[hoveringProject.id].locationRectangle.Height);
                        }

                        // Both hands are on the dropHereRectangle.
                        if (bothHandsOnLocation(drophereRectangle, joints))
                        {
                            // Put it back on it's place.
                            projects[hoveringProject.id].locationRectangle = projects[hoveringProject.id].place;

                            // Go the details.
                            detailsProject = projects[hoveringProject.id];

                            // Write a view to this project.
                            db.writeView(detailsProject);

                            // Get latest viewcount from db.
                            detailsProject.viewCount = db.updateView(detailsProject);

                            // Change gameState.
                            gameState = GameState.detail;

                            // Place it on the last known mouse location.
                            // entry.Value.locationRectangle = new Rectangle(mouseState[0].X - (entry.Value.locationRectangle.Width / 2), mouseState[0].Y - (entry.Value.locationRectangle.Height / 2), entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);

                            // Stop hovering.
                            projects[hoveringProject.id].hover = false;

                            // Stop hovering.
                            hovering = false;
                        }
                    }
                    break;

                case GameState.detail:
                    //if (backbuttonRectangle.Contains(mouseLocation))
                    //{
                    //    if (mouseState[0].LeftButton == ButtonState.Pressed && mouseState[1].LeftButton == ButtonState.Released)
                    //    {
                    //        mouseState[1] = mouseState[0];
                    //        gameState = GameState.overview;
                    //    }
                    //}

                    hit = false;

                    foreach (KeyValuePair<JointType, Point> item in joints)
                    {
                        if (backbuttonRectangle.Contains(item.Value))
                        {
                            kinectCounter++;
                            hit = true;

                            if (kinectCounter > 150)
                            {
                                kinectCounter = 0;
                                gameState = GameState.overview;
                            }
                        }
                    }

                    if (!hit)
                    {
                        // No joint was on any texture.
                        kinectCounter = 0;
                    }
                    
                    break;

                default:
                    break;
            }

            if (particlesWereAdded)
            {
                particleEngine.Update(true);
            }
            else
            {
                particleEngine.Update(false);
            }

            particlesWereAdded = false;

            base.Update(gameTime);
        }

        private Boolean bothHandsOnLocation(Rectangle drophereRectangle, Dictionary<JointType, Point> jointPositions)
        {
            foreach (KeyValuePair<JointType, Point> item in jointPositions)
            {
                if (!drophereRectangle.Contains(item.Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            switch (gameState)
            {
                case GameState.overview:
                    if (hovering)
                    {
                        spriteBatch.Draw(gameTextures["drophere"], drophereRectangle, Color.White);
                        spriteBatch.DrawString(font2, "Move both hands here", new Vector2(screenWidth / 2 - (screenWidth / 8), screenHeight / 6), Color.White);
                    }
                    else
                    {
                        if (checkIfLessProjecets())
                        {
                            spriteBatch.Draw(gameTextures["arrow"], arrowLeftRectangle, null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                        }
                        if (checkIfMoreProjecets())
                        {
                            spriteBatch.Draw(gameTextures["arrow"], arrowRightRectangle, Color.White);
                        }
                    }

                    drawAllProjects(spriteBatch);
                    break;

                case GameState.detail:
                    drawSinlgeProject(spriteBatch);
                    spriteBatch.Draw(gameTextures["back"], backbuttonRectangle, Color.White);
                    break;

                default:
                    break;
            }

            particleEngine.Draw(spriteBatch);
            
            spriteBatch.End();

            skeleton.Draw(spriteBatch, kinectCounter);
            
            base.Draw(gameTime);
        }

        private void drawSinlgeProject(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(detailsProject.texture, detailsProject.detailsPlace, Color.White);

            spriteBatch.DrawString(font2, parseText(detailsProject.description, 350), new Vector2(550, 150), Color.White);

            spriteBatch.DrawString(font, "Datum: " + detailsProject.date.Date, new Vector2(125, 250), Color.White);  

            spriteBatch.DrawString(font, "Klas: " + parseText(detailsProject.klas, 200), new Vector2(125, 300), Color.White);

            spriteBatch.DrawString(font, "Tools: " + parseText(detailsProject.tools, 200), new Vector2(125, 350), Color.White);

            spriteBatch.DrawString(font, "Naam: " + parseText(detailsProject.naam, 200), new Vector2(125, 400), Color.White);

            spriteBatch.DrawString(font, "Opleiding: " + parseText(detailsProject.opleiding, 200), new Vector2(125, 450), Color.White);

            spriteBatch.DrawString(font, "Views: " + detailsProject.viewCount.ToString(), new Vector2(125, 500), Color.White);
        }

        private String parseText(String text, Int32 width)
        {
            String line = String.Empty;
            String returnString = String.Empty;
            String[] wordArray = text.Split(' ');

            foreach (String word in wordArray)
            {
                if (font.MeasureString(line + word).Length() > width)
                {
                    returnString = returnString + line + '\n';
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }

        private void drawAllProjects(SpriteBatch spriteBatch)
        {
            // Draw all projects.
            foreach (KeyValuePair<Int32, Project> entry in displayProjects)
            {
                // Needed to compensate the drawing.
                Rectangle location = new Rectangle(entry.Value.locationRectangle.X + (entry.Value.locationRectangle.Width / 2), entry.Value.locationRectangle.Y + (entry.Value.locationRectangle.Height / 2), entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);
                
                if (entry.Value.hover)
                {
                    // Increase the size of the rectangle.
                    Rectangle zoomedInRectangle = new Rectangle(location.X, location.Y, location.Width + 50, location.Height + 50);

                    // Draw it a bit transparant.
                    spriteBatch.Draw(entry.Value.texture, zoomedInRectangle, null, Color.Lerp(Color.White, Color.Transparent, 0.8f), 0f, new Vector2(entry.Value.texture.Width / 2, entry.Value.texture.Height / 2), SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(entry.Value.texture, location, null, Color.White, 0f, new Vector2(entry.Value.texture.Width / 2, entry.Value.texture.Height / 2), SpriteEffects.None, 0f);
                }
            }
        }

        public void LoadInKinect()
        {
            this.skeleton = new SkeletonStreamProcessor(this.Content);

            this.chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution320x240Fps30, skeleton);
            this.Services.AddService(typeof(KinectChooser), this.chooser);

            Int32 width = 160;
            Int32 height = 120;

            this.colorStreamerProcessor = new ColorStreamRenderer(this);
            this.colorStreamerProcessor.Size = new Vector2(width, height);
            this.colorStreamerProcessor.Position = new Vector2(Game1.screenWidth / 2, Game1.screenHeight - height);

            this.deptStreamerProcessor = new DepthStreamRenderer(this);
            this.deptStreamerProcessor.Size = new Vector2(width, height);
            this.deptStreamerProcessor.Position = new Vector2(Game1.screenWidth / 2 - width, Game1.screenHeight - height);

            this.Components.Add(this.chooser);

            // this.Components.Add(this.colorStreamerProcessor);
            // this.Components.Add(this.deptStreamerProcessor);
        }

        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data.
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        /*
         * 
         * 
        //if (entry.Value.hover)
        //{
        //    // Make it follow the mouse.
        //    // entry.Value.locationRectangle = new Rectangle(mouseState[0].X - (entry.Value.locationRectangle.Width / 2), mouseState[0].Y - (entry.Value.locationRectangle.Height / 2), entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);

        //    // If postion of hoverjoint is available.
        //    if (joints.ContainsKey(hoveringWith))
        //    {
        //        // Make it follow the joint that selected it.
        //        entry.Value.locationRectangle = new Rectangle(joints[hoveringWith].X - (entry.Value.locationRectangle.Width / 2), joints[hoveringWith].Y - (entry.Value.locationRectangle.Height / 2), entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);
        //    }
        //}
         * 
       // left button is pressed.
        if (mouseState[0].LeftButton == ButtonState.Pressed)
        {
            // If mouse is on project and is not already hovering, hover that project.
            if (entry.Value.locationRectangle.Contains(mouseLocation) && !hovering)
            {
                // This project is hovering.
                entry.Value.hover = true;
                // Hovering active.
                hovering = true;
            }
        }

        // Button was released.
        if (mouseState[0].LeftButton == ButtonState.Released && mouseState[1].LeftButton == ButtonState.Pressed)
        {
            if (entry.Value.hover)
            {
                // Put it back on it's place.
                entry.Value.locationRectangle = entry.Value.place;
                                
                if (drophereRectangle.Contains(new Point(mouseState[0].X, mouseState[0].Y)))
                {
                    detailsProject = entry.Value;
                                    
                    gameState = GameState.detail;
                }
                                
                // Place it on the last known mouse location.
                // entry.Value.locationRectangle = new Rectangle(mouseState[0].X - (entry.Value.locationRectangle.Width / 2), mouseState[0].Y - (entry.Value.locationRectangle.Height / 2), entry.Value.locationRectangle.Width, entry.Value.locationRectangle.Height);
            }

            // Stop hovering.
            entry.Value.hover = false;
            hovering = false;
        }
        */
    }

    /*
    public static class JsonHelper
    {
        public static T Deserialize<T>(string json)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                T obj = (T)serializer.ReadObject(stream);
                return obj;
            }
        }

        public static string Serialize(object objectToSerialize)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(objectToSerialize.GetType());
                serializer.WriteObject(ms, objectToSerialize);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
    */

    public static class TextureContent
    {
        public static Dictionary<string, T> LoadListContent<T>(this ContentManager contentManager, string contentFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(contentManager.RootDirectory + "/" + contentFolder);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException();
            }

            Dictionary<String, T> result = new Dictionary<String, T>();

            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                String key = Path.GetFileNameWithoutExtension(file.Name);

                result[key] = contentManager.Load<T>(contentFolder + "/" + key);
            }

            return result;
        }
    }
}
