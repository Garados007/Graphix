using Graphix.Physic;
using Graphix.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Graphix.Internal
{
    public static class PrototypeLoaderAccess
    {
        public static Dictionary<string, Tuple<Type, Delegate>> Effects => PrototypeLoader.effects;

        public static Dictionary<string, Tuple<Type, Delegate>> Activators => PrototypeLoader.activators;

        public static Dictionary<string, Type> MathParameterTypes => PrototypeLoader.mathParameterTypes;

        public static Dictionary<string, Delegate> ParameterConverter => PrototypeLoader.parameterConverter;

        public static Dictionary<string, Type> ParameterTypes => PrototypeLoader.parameterTypes;

        public static List<Type> DotnetPrototypes => PrototypeLoader.dotnetPrototypes;

        public static void SetParameter(PrototypeLoader pl, string value, IValueWrapper target, PrototypeBase current, bool forceref = false, Delegate customConverter = null)
        {
            pl.SetParameter(value, target, current, forceref, customConverter);
        }

        public static void EffectBase(PrototypeLoader pl, PrototypeBase pb, AnimationEffect e, XmlNode node)
        {
            PrototypeLoader.EffectBase(pl, pb, e, node);
        }
    }
}
