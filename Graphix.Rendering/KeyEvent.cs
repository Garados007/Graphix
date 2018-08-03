using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Rendering
{
    public class KeyEvent
    {
        public enum Mode
        {
            Down,
            Up,
            Press
        }

        public Mode KeyMode { get; set; }

        public Physic.Keys Key { get; set; }

        public string Char { get; set; }
    }
}
