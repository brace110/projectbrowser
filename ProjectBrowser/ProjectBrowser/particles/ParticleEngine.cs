using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace ProjectBrowser
{
    public class ParticleEngine
    {
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particle> particles;
        private List<Texture2D> textures;

        private Color[] colors = new Color[6];
        Random r = new Random();

        public ParticleEngine(Microsoft.Xna.Framework.Content.ContentManager content)
        {            
            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(content.Load<Texture2D>("Particles/circle"));
            textures.Add(content.Load<Texture2D>("Particles/star"));
            textures.Add(content.Load<Texture2D>("Particles/diamond"));

            // Accuracy.
            colors[0] = new Color(52, 181, 99);
            colors[1] = new Color(0, 147, 68);
            // Snelheid.
            colors[2] = new Color(51, 146, 208);
            colors[3] = new Color(27, 117, 187);
            // Balans.
            colors[4] = new Color(228, 96, 163);
            colors[5] = new Color(219, 66, 143);

            EmitterLocation = new Vector2(400, 240);
            this.textures = textures;
            this.particles = new List<Particle>();
            random = new Random();
        }

        private Particle GenerateNewParticle()
        {
            Texture2D texture = textures[random.Next(textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity = new Vector2(
                    1f * (float)(random.NextDouble() * 2 - 1),
                    1f * (float)(random.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = colors[r.Next(0, 6)];
            //Color color = new Color(
            //        (float)random.NextDouble(),
            //        (float)random.NextDouble(),
            //        (float)random.NextDouble());
            float size = (float)random.NextDouble();
            int ttl = 40 + random.Next(40);

            return new Particle(texture, position, velocity, angle, angularVelocity, color, size, ttl);
        }

        public void Update(bool addParticle)
        {
            int total = 4;

            if (addParticle)
            {
                for (int i = 0; i < total; i++)
                {
                    particles.Add(GenerateNewParticle());
                }
            }

            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // spriteBatch.Begin();

            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }

            // spriteBatch.End();
        }
    }
}
