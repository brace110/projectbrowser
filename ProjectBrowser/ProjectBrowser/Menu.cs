using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FencingWithMeXNA
{
    class Menu
    {
        private MouseInput mouseInput;
        private hitChecker hitchecker;

        public Menu(hitChecker _hitChecker)
        {
            textures = new List<ContentManager.TextureNames>();

            textures.Add(ContentManager.TextureNames.button_accuracy);
            textures.Add(ContentManager.TextureNames.button_balans);
            textures.Add(ContentManager.TextureNames.button_snelheid);

            mouseInput = new MouseInput();

            hitchecker = _hitChecker;
        }

        public void update()
        {
            MouseState[] mouseStates = mouseInput.refresh();

            foreach (ContentManager.TextureNames textureName in textures)
            {
                if (hitchecker.check(ContentManager.rectangles[textureName], mouseStates))
                {
                    // Mouse click or kinect click.
                    if (hitchecker.mouseHover || hitchecker.counter > 180)
                    {
                        // Reset the kinect counter.
                        hitchecker.counter = 0;

                        // Texture was clicked on.
                        switch (textureName)
                        {
                            case ContentManager.TextureNames.button_accuracy:
                                FencingWithMeXNA.Game1.gameState = Game1.GameState.level_accuracy;
                                break;

                            case ContentManager.TextureNames.button_balans:
                                FencingWithMeXNA.Game1.gameState = Game1.GameState.level_balans;
                                break;

                            case ContentManager.TextureNames.button_snelheid:
                                FencingWithMeXNA.Game1.gameState = Game1.GameState.level_speed;
                                break;

                            default:
                                // Button was clicked on but has no response.
                                break;
                        }

                        break;
                    }
                }
            }
        }
    }
}
