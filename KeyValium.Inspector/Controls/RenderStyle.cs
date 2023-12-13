using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector.Controls
{
    internal class RenderStyle
    {
        public RenderStyle() 
        { 
        }    

        public RenderStyle(Color? backcolor, Color? forecolor)
        {
            BackColor = backcolor;
            ForeColor = forecolor;
        }

        public Color? BackColor
        {
            get;
            set;
        }

        public Color? ForeColor
        {
            get;
            set;
        }
    }
}
