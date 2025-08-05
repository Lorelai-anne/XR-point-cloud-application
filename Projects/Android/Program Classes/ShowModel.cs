using RAZR_PointCRep.Tools;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAZR_PointCRep.Show
{
    internal class ShowModel : IClass
    {
        List<IAsset> filteredAssets = new List<IAsset>();
        Type filterType = typeof(IAsset);
        float filterScroll = 0;
        const int filterScrollCt = 12;
        bool winEn = false;
        Pose simpleWinPose = Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1)).Pose;
        static bool HandFacingHead(Handed handed)
        {
            Hand hand = Input.Hand(handed);
            if (!hand.IsTracked)
                return false;

            Vec3 palmDirection = hand.palm.Forward.Normalized;
            Vec3 directionToHead = (Input.Head.position - hand.palm.position).Normalized;

            return Vec3.Dot(palmDirection, directionToHead) > 0.5f;
        }
        void VisualizeModel(Model item)
        {
            UI.Model(item, V.XX(UI.LineHeight));
            UI.SameLine();
        }
        void VisualizeMesh(Mesh item)
        {
            Bounds meshSize = item.Bounds;
            Bounds b = UI.LayoutReserve(V.XX(UI.LineHeight), false, UI.LineHeight);
            float scale = 1.0f / meshSize.dimensions.Length;
            item.Draw(Material.Default, Matrix.TS(b.center + meshSize.center * scale, b.dimensions * scale));

            UI.SameLine();
        }
        void VisualizeSprite(Sprite item)
        {
            UI.Image(item, V.XX(UI.LineHeight));
            UI.SameLine();
        }

        void UpdateFilter(Type type)
        {
            filterType = type;
            filterScroll = 0.0f;
            filteredAssets.Clear();

            // Here's where the magic happens! `Assets.Type` can take a Type, or a
            // generic <T>, and will give a list of all assets that match that
            // type!
            filteredAssets.AddRange(Assets.Type(filterType));
        }
        Model _model;
        public void AssetWindow()
        {
            UISettings settings = UI.Settings;
            float height = filterScrollCt * (UI.LineHeight + settings.gutter) + settings.margin * 2;

            UI.WindowBegin("Menu", ref simpleWinPose, V.XY(0.5f, height));
            UI.LayoutPushCut(UICut.Left, 0.08f);
            UI.PanelAt(UI.LayoutAt, UI.LayoutRemaining);

            UI.Label("Filter");

            UI.HSeparator();

            Vec2 size1 = new Vec2(0.08f, 0);

            // A radio button selection for what to filter by

            if (UI.Radio("Model", filterType == typeof(Model), size1)) UpdateFilter(typeof(Model));
            UI.SameLine();
            if (UI.Radio("Mesh", filterType == typeof(Mesh), size1)) UpdateFilter(typeof(Mesh));
            UI.SameLine();
            if (UI.Radio("Sprite", filterType == typeof(Sprite), size1)) UpdateFilter(typeof(Sprite));
            UI.SameLine();
            if (UI.Radio("All", filterType == typeof(IAsset), size1)) UpdateFilter(typeof(IAsset));

            //if (UI.Radio("All", filterType == typeof(IAsset), size1)) UpdateFilter(typeof(IAsset));

            UI.LayoutPop();

            UI.LayoutPushCut(UICut.Right, UI.LineHeight);
            UI.VSlider("scroll", ref filterScroll, 0, Math.Max(0, filteredAssets.Count - 3), 1, 0, UIConfirm.Pinch);
            UI.LayoutPop();


            // Used to visual the models in the button, as well as create a button with the asset visualization and name
            for (int i = (int)filterScroll; i < Math.Min(filteredAssets.Count, (int)filterScroll + filterScrollCt); i++)
            {
                IAsset asset = filteredAssets[i];
                UI.PushId(i);
                switch (asset)
                {
                    case Mesh item: VisualizeMesh(item); break;
                    case Sprite item: VisualizeSprite(item); break;
                    case Model item: VisualizeModel(item); break;
                }
                UI.PopId();
                if (UI.Button(string.IsNullOrEmpty(asset.Id) ? "(null)" : asset.Id, V.XY(UI.LayoutRemaining.x, 0))) // When pressed, will create point cloud of model based of of vertices in model
                {
                    Model model = Model.FromFile(string.IsNullOrEmpty(asset.Id) ? "(null)" : asset.Id);
                    _model = model; //gives _model value value of model associated with button so _model can be printed in Step()
                }
            }
            UI.WindowEnd();
        }
        Model model2 = Model.FromFile("DamagedHelmet.gltf");
        Model model3 = Model.FromFile("Cosmonaut.glb");
        Model model6 = Model.FromFile("suzanne_bin.stl");
        public void Initialize()
        {
            _model = model2; //Initializes _model default variable to Damaged Helement file as default when program starts
            //can change which model initializes at, but must initialize, otherwise throws null pointer exception and crashes
        }

        Pose modelPose = Matrix.T(0.5f, 1, -.25f).Pose;
        public void Step()
        {
            UI.Handle("Cube", ref modelPose, _model.Bounds);
            _model.Draw(modelPose.ToMatrix()); //must draw model outside of any window
            bool secWin = winEn;
            Handed handed = Handed.Left;

            if (!HandFacingHead(handed)) //if palm not facing head, skip window
                return;

            // Decide the size and offset of the menu
            Vec2 size = new Vec2(4, 16);
            float offset = handed == Handed.Left ? -2 - size.x : 2 + size.x;

            // Position the menu relative to the side of the hand
            Hand hand = Input.Hand(handed);
            Vec3 at = hand[FingerId.Little, JointId.KnuckleMajor].position;
            Vec3 down = hand[FingerId.Little, JointId.Root].position;
            Vec3 across = hand[FingerId.Index, JointId.KnuckleMajor].position;

            Pose menuPose = new Pose(
                at,
                Quat.LookAt(at, across, at - down) * Quat.FromAngles(0, handed == Handed.Left ? 90 : -90, 0));
            menuPose.position += menuPose.Right * offset * 0.03f;
            menuPose.position += menuPose.Up * (size.y / 2) * U.cm;

            UI.WindowBegin("Point Cloud", ref menuPose);
            {
                if (UI.Toggle("Load Models", ref secWin))
                {
                    winEn = !winEn; //used to initialize and uninitialize asset file window
                }
            }
            UI.WindowEnd();

            if (winEn)
            {
                AssetWindow(); //if bool is true, enable window. Will automatically uninitialize window if false since it checks per frame
            }
        }

        public void Shutdown(){}
    }
}
