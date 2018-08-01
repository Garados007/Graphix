using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes.Math
{
    public class If : IValueWrapper
    {
        public object Value { get => bufcalc(); set { } }
        public IValueWrapper RemoteSource { get => null; set { } }
        public string Name { get; set; }
        public bool Exists { get; set; }
        
        public bool Precompile { get; set; }
        
        public IValueWrapper Condition { get; set; }

        public IValueWrapper True { get; set; }

        public IValueWrapper False { get; set; }


        object buffer; bool hasbuffer = false;
        private object bufcalc()
        {
            if (Precompile)
            {
                if (hasbuffer) return buffer;
                hasbuffer = true;
                return buffer = calc();
            }
            else return calc();
        }

        private object calc()
        {
            var cond = Condition.Value;
            bool success;
            if (cond is bool) success = (bool)cond;
            else if (cond is double) success = (double)cond != 0;
            else if (cond is int) success = (int)cond != 0;
            else if (cond is string) success = !string.IsNullOrEmpty((string)cond);
            else success = cond != null;
            if (success) return True.Value;
            else return False.Value;
        }

        public IValueWrapper Clone()
        {
            var val = new If();
            val.Condition = Condition.Clone();
            val.Exists = Exists;
            val.False = False.Clone();
            val.Name = Name;
            val.Precompile = Precompile;
            val.True = True.Clone();
            return val;
        }

        public void MoveTargets(PrototypeFlattenerHelper helper)
        {
            if (Condition != null)
            {
                if (helper.Conversion.ContainsKey(Condition))
                    Condition = helper.Conversion[Condition];
                Condition.MoveTargets(helper);
            }
            if (True != null)
            {
                if (helper.Conversion.ContainsKey(True))
                    True = helper.Conversion[True];
                True.MoveTargets(helper);
            }
            if (False != null)
            {
                if (helper.Conversion.ContainsKey(False))
                    False = helper.Conversion[False];
                False.MoveTargets(helper);
            }
        }
    }
}
