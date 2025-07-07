using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAZR_PointCRep
{
    internal interface IClass
    {
        void Initialize();
        void Step();
        void Shutdown();
    }
}
