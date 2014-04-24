using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectBrowser
{
    class MouseInput
    {
        MouseState[] mouseStates = new MouseState[2];

        public MouseInput()
        {
            mouseStates[0] = Mouse.GetState();
            mouseStates[1] = mouseStates[0];
        }

        public MouseState[] refresh()
        {
            mouseStates[1] = mouseStates[0];
            mouseStates[0] = Mouse.GetState();

            return mouseStates;
        }
    }
}
