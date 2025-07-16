using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAZR_PointCRep.Tools
{
    internal interface IClass
    {
        void Initialize();
        void Step();
        void Shutdown();
    }
}
