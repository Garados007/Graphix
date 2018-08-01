using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix.Physic
{
    public interface ISoundPlayer : IDisposable
    {
        void PlaySound(string file, double volume);
    }
}
