using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Prototypes.Math
{
    public class MathValue<T> : ValueWrapper<T>
    {
        public override T Value { get => (T)ValueSource.Value; set { } }
        
        public IValueWrapper ValueSource { get; set; }

        public string ValueType { get; set; }

        public override IValueWrapper Clone()
        {
            var math = new MathValue<T>();
            math.Exists = Exists;
            math.Name = Name;
            math.ValueSource = ValueSource.Clone();
            math.ValueType = ValueType;
            return math;
        }

        public override void MoveTargets(PrototypeFlattenerHelper helper)
        {
            if (ValueSource != null)
            {
                if (helper.Conversion.ContainsKey(ValueSource))
                    ValueSource = helper.Conversion[ValueSource];
                ValueSource.MoveTargets(helper);
            }
        }
    }
}
