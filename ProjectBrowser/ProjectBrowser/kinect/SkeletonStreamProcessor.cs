using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using ProjectBrowser;

namespace ProjectBrowser
{
    public class SkeletonStreamProcessor
    {
        Skeleton[] skeletonData;
        Skeleton skeleton;

        private Texture2D jointTexture;
        private Texture2D cursor;
        private Texture2D box;

        public SkeletonStreamProcessor(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            jointTexture = Content.Load<Texture2D>("kinect/Skeleton/joint");
            cursor = Content.Load<Texture2D>("kinect/cursor");
            box = Content.Load<Texture2D>("kinect/box");
        }
        public void Draw(SpriteBatch spriteBatch, Int32 kinectCounter)
        {
            spriteBatch.Begin();
            DrawSkeleton(spriteBatch, kinectCounter);
            spriteBatch.End();

            // Old kinect stuff.
            // spriteBatch.Draw(colorVideo, new Rectangle(0, 0, colorVideo.Width, colorVideo.Height), Color.White);
            // spriteBatch.Draw(depthVideo, new Rectangle(640, 0, depthVideo.Width, depthVideo.Height), Color.White);
        }

        public void kinect_AllFramesReady(object sender, SkeletonFrameReadyEventArgs imageFrames)
        {  
            using (SkeletonFrame skeletonFrame = imageFrames.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((skeletonData == null) || (skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    // Copy the skeleton data to our array.
                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                }
            }

            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeleton = skel;
                    }
                }
            }
        }

        private void DrawSkeletonWhole(SpriteBatch spriteBatch)
        {
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked || skel.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        foreach (Joint joint in skel.Joints)
                        {
                            Vector2 position = mapToScreen(joint.Position);

                            spriteBatch.Draw(jointTexture, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Black);
                        }
                    }
                }
            }
        }

        private void DrawSkeleton(SpriteBatch spriteBatch, Int32 kinectCounter)
        {
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked || skel.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        foreach (Joint joint in skel.Joints)
                        {
                            if (joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight)
                            {
                                Vector2 position = mapToScreen(joint.Position);

                                spriteBatch.Draw(cursor, position, Color.White);

                                spriteBatch.Draw(Game1.pixel, new Rectangle(Game1.screenWidth / 2 - 75, 30, kinectCounter, 10), Color.Green);
                                spriteBatch.Draw(box, new Rectangle(Game1.screenWidth / 2 - 75, 30, 150, 10), Color.White);
                            }
                        }
                    }
                }
            }
        }

        private void DrawBone(JointCollection joints, JointType startJoint, JointType endJoint)
        {
            Vector2 start = mapToScreen(joints[startJoint].Position);
            
            Vector2 end = mapToScreen(joints[endJoint].Position);

            Vector2 diff = end - start;
            // Vector2 scale = new Vector2(1.0f, diff.Length() / this.boneTexture.Height);

            // float angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;

            // this.SharedSpriteBatch.Draw(this.boneTexture, start, null, Color.White, angle, this.boneOrigin, scale, SpriteEffects.None, 1.0f);
        }

        public Dictionary<JointType, Point> getSkeletonLocations(List<JointType> allowedJoints)
        {
            Dictionary<JointType, Point> locations = new Dictionary<JointType, Point>();

            if (skeletonData != null)
            {
                for (int i = 0; i < skeletonData.Length; i++)
                {
                    if (skeletonData[i] != null)
                    {
                        if (skeletonData[i].TrackingState != SkeletonTrackingState.NotTracked)
                        {
                            foreach (Joint joint in skeletonData[i].Joints)
                            {
                                foreach (JointType type in allowedJoints)
                                {
                                    if (joint.JointType == type)
                                    {
                                        Vector2 position = mapToScreen(joint.Position);

                                        locations[joint.JointType] = new Point(Convert.ToInt32(position.X), Convert.ToInt32(position.Y));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return locations;
        }

        private Vector2 mapToScreen(SkeletonPoint jointPosition)
        {
            return new Vector2((((0.5f * jointPosition.X) + 0.5f) * (Game1.screenWidth)), (((-0.5f * jointPosition.Y) + 0.5f) * (Game1.screenHeight)));
        }
    }
}
