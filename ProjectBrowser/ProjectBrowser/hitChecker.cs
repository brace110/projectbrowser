using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectBrowser
{
    class hitChecker
    {
        MouseInput mouseInput;

        public Int32 counter;

        private Int32 kinectCounter = 0;

        public Boolean mouseHover = false;
        public Boolean kinectHover = false;

        private ParticleEngine particleEngine;
        
        public hitChecker(ParticleEngine _particleEngine)
        {
            mouseInput = new MouseInput();

            particleEngine = _particleEngine;
        }

        public Boolean check(Rectangle location, MouseState[] mouseStates, List<Point> handLocations)
        {
            Point mouseLocation;
            
            if (mouseStates != null)
            {
                mouseLocation = new Point(mouseStates[0].X, mouseStates[0].Y);
            }
            else
            {
                mouseLocation = new Point(-5000, -5000);
            }

            // Check mouse.
            if (location.Contains(mouseLocation))
            {
                // Add Particles on this button
                mouseHover = true;

                if (mouseStates[0].LeftButton == ButtonState.Pressed && mouseStates[1].LeftButton == ButtonState.Released)
                {
                    // Click.
                    return true;
                }
            }

            //if (kinectChecker(location, handLocations))
            //{
            //    kinectHover = true;

            //    counter++;
            //}

            // If either the mouse or a kinect hand hovers over an object, add particles.
            if (mouseHover || kinectHover)
            {
                // This is set true so only one image get particles per update.
                if (!Game1.particlesWereAdded)
                {
                    Game1.particlesWereAdded = true;

                    // Set location of particles.
                    particleEngine.EmitterLocation = new Vector2(location.Center.X, location.Center.Y);
                }

                // Reset booleans because one or more has become true.
                mouseHover = false;
                kinectHover = false;

                return true;
            }

            return false;
        }

        public JointType? kinectChecker(Rectangle location, Dictionary<JointType, Point> jointLocations)
        {
            foreach (KeyValuePair<JointType, Point> jointPos in jointLocations)
            {
                if (location.Contains(jointPos.Value))
                {
                    return jointPos.Key;
                }
            }

            return null;
        }

        //public Boolean checkKinect(Rectangle location, List<Point> jointLocations)
        //{
        //    // If joint is on location.
        //    if (kinectChecker(location, jointLocations))
        //    {
        //        kinectCounter++;

        //        Console.WriteLine(kinectCounter);

        //        if (kinectCounter > 180)
        //        {
        //            kinectCounter = 0;
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        // None of the joints was on the target, reset the counter.
        //        // kinectCounter = 0;
        //    }

        //    return false;
        //}
    }
}
