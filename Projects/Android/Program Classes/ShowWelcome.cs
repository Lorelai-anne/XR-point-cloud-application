using Android.Util;
using RAZR_PointCRep.Tools;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Media.TV.TvContract.Channels;

namespace RAZR_PointCRep.Show
{
    internal class Welcome : IClass
    {
        string message = "Welcome to Resonant Sciences point cloud representation program!";
        string message2 = "Take this time to place some spatial nodes!";
        public void Initialize()
        {
        }

        public void Step()
        {
            Hierarchy.Push(Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1)));
            Text.Add(message, Matrix.S(1.25f), V.XY(.6f, 0), TextFit.Wrap, TextAlign.TopCenter, TextAlign.TopLeft);
            Text.Add(message2, Matrix.S(1.15f), V.XY(.5f, 0), TextFit.Wrap, TextAlign.TopCenter, TextAlign.TopLeft);
            Hierarchy.Pop();
        }

        public void Shutdown()
        {
        }
    }
}
