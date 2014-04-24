using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectBrowser
{
    class Project
    {
        public DateTime date { get; set; }
        public String description { get; set; }
        public String tools { get; set; }
        public String name { get; set; }
        public Int32 id { get; set; }
        public String textureName { get; set; }
        public Texture2D texture { get; set; }
        public Rectangle locationRectangle { get; set; }
        public Rectangle detailsPlace { get; set; }
        public Rectangle place { get; set; }
        public Boolean hover { get; set; }
        public Int32 viewCount { get; set; }
        public String naam { get; set; }
        public String klas { get; set; }
        public String opleiding { get; set; }

        public Project(Int32 _id, String _name, String _textureName, DateTime _date, String _description, String _tools, Int32 _viewCount, String _naam, String _klas, String _opleiding)
        {
            name = _name;
            id = _id;
            textureName = _textureName;
            hover = false;

            date = _date;
            description = _description;
            tools = _tools;

            viewCount = _viewCount;

            naam = _naam;
            klas = _klas;
            opleiding = _opleiding;

            locationRectangle = new Rectangle(0, 0, 150, 100);
            detailsPlace = new Rectangle(250, 120, 150, 100);
        }
    }
}
