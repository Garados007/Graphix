using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix
{
    public struct ScreenPos
    {
        public double Value { get; set; }

        public PosType PosType { get; set; }

        public ScreenPos(double value, PosType posType)
        {
            Value = value;
            PosType = posType;
        }

        public override string ToString()
        {
            switch (PosType)
            {
                case PosType.Relative: return Value.ToString() + "%";
                case PosType.RelativeWidth: return Value.ToString() + "%w";
                case PosType.RelativeHeight: return Value.ToString() + "%h";
                case PosType.Screen: return Value.ToString() + "v";
                case PosType.ScreenWidth: return Value.ToString() + "vw";
                case PosType.ScreenHeight: return Value.ToString() + "vh";
                default: return Value.ToString();
            }
        }
    }

    public enum PosType
    {
        Absolute,
        Relative,
        RelativeWidth,
        RelativeHeight,
        Screen,
        ScreenWidth,
        ScreenHeight
    }
}
