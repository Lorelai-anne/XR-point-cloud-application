using Android.Accounts;
using RAZR_PointCRep.Spatial_Anchor;
using RAZR_PointCRep.Tools;
using StereoKit;
using System;
using System.Collections.Generic;
using System.Linq;
using static Android.Provider.CalendarContract;
using static RAZR_PointCRep.Spatial_Anchor.SpatialEntityFBExt;

public class PointCloud
{
    Material mat;
    Vertex[] verts;
    Mesh mesh;

    bool distanceIndependant;
    float pointSize;

    public bool DistanceIndependantSize { get => distanceIndependant; set { mat["screen_size"] = value ? 1.0f : 0.0f; distanceIndependant = value; } }
    public float PointSize { get => pointSize; set { mat["point_size"] = value; pointSize = value; } }

    public PointCloud(float pointSizeMeters = 0.01f)
    {
        mat = new Material(Shader.FromFile("point_cloud.hlsl"));
        PointSize = pointSizeMeters;
    }
    public PointCloud(float pointSizeMeters, Mesh fromMesh)
        : this(pointSizeMeters) => SetPoints(fromMesh);
    public PointCloud(float pointSizeMeters, Vertex[] fromVerts)
        : this(pointSizeMeters) => SetPoints(fromVerts);
    public PointCloud(float pointSizeMeters, Model fromModel)
        : this(pointSizeMeters) => SetPoints(fromModel);

    public void Draw(Matrix transform) => mesh?.Draw(mat, transform);

    public void SetPoints(Vertex[] points)
    {
        if (verts == null)
            verts = new Vertex[points.Length * 4];
        if (verts.Length != points.Length * 4)
        {
            Log.Err("You can re-use a point cloud, but the number of points should stay the same!");
            return;
        }

        for (int i = 0; i < points.Length; i++)
        {
            Vertex p = points[i];
            int idx = i * 4;
            verts[idx] = new Vertex(p.pos, p.norm, V.XY(-.5f, .5f), p.col);
            verts[idx + 1] = new Vertex(p.pos, p.norm, V.XY(.5f, .5f), p.col);
            verts[idx + 2] = new Vertex(p.pos, p.norm, V.XY(.5f, -.5f), p.col);
            verts[idx + 3] = new Vertex(p.pos, p.norm, V.XY(-.5f, -.5f), p.col);
        }
        if (mesh == null)
        {
            uint[] inds = new uint[points.Length * 6];
            for (uint i = 0; i < points.Length; i++)
            {
                uint ind = i * 6;
                uint vert = i * 4;
                inds[ind] = vert + 2;
                inds[ind + 1] = vert + 1;
                inds[ind + 2] = vert + 0;

                inds[ind + 3] = vert + 3;
                inds[ind + 4] = vert + 2;
                inds[ind + 5] = vert + 0;
            }
            mesh = new Mesh();
            mesh.SetInds(inds);
        }
        mesh.SetVerts(verts);
    }
    public void SetPoints(Mesh fromMeshVerts)
        => SetPoints(fromMeshVerts.GetVerts());
    public void SetPoints(Model fromModelVerts)
        => SetPoints(fromModelVerts.Visuals
            .SelectMany(v => TransformVerts(v.ModelTransform, v.Mesh.GetVerts()))
            .ToArray());

    private static Vertex[] TransformVerts(Matrix mat, Vertex[] verts)
    {
        for (int i = 0; i < verts.Length; i++)
            verts[i].pos = mat.Transform(verts[i].pos);
        return verts;
    }
}

namespace RAZR_PointCRep.Show
{
    internal class ShowPointCloud : IClass
    {
        Model model2 = Model.FromFile("DamagedHelmet.gltf");
        Model model3 = Model.FromFile("Cosmonaut.glb");
        Model model6 = Model.FromFile("suzanne_bin.stl");


        SpatialEntityPoseHandler handler = Program.handler;

        Pose cloudPose = (Matrix.T(0.2f, -0.1f, 0) * Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1))).Pose;

        float cloudScale;
        PointCloud cloud;
        float pointSize = 0.003f;

        public void ASCIIParse(string fileName)
        {
            string[] lines = Platform.ReadFileText(fileName).Split("\n"); // Reads file and splits into array at every new line
            Vertex[] pointss = new Vertex[lines.Length * 4]; //Creating Vertex Array with the size of lines[], So you can reuse the CreatePointcloud(Vertex[]) instead of creating new one
            bool readingPoints = false;
            int i = 0;

            foreach (string line in lines)
            {
                //Log.Info($"{line}");
                if (readingPoints)
                {
                    string[] splitLine = line.Split(' ');
                    if (splitLine.Length >= 3)
                    {
                        float x = float.Parse(splitLine[0]);
                        float y = float.Parse(splitLine[1]);
                        float z = float.Parse(splitLine[2]);

                        pointss[i].pos = V.XYZ(x, y, z);
                        pointss[i].col = Color.HSV(x, y, 1).ToLinear();
                        i++;
                    }
                }
                else
                {
                    Log.Info($"{line}"); // Printing out header data in debug log
                }
                if (line.Contains("DATA ascii")) // Checks if lines contain point cloud data not headers
                {
                    readingPoints = true;
                }
            }

            // INITIALIZE MUST CONTAIN new PointCloud() OR IT WILLL CRASH & THROW NULL POINTER EXCEPTION
            cloud = new PointCloud(pointSize, pointss); // Creates new PointCloud
            cloudScale = 1;
        }
        float parseProgress = 0;
        bool isParsing = false;

        public void BinaryParse(string fileName) //works technically but needs optimization
        {
            byte[] fileData = Platform.ReadFileBytes(fileName);

            int headerEnd = 0;
            string headerText = "";
            for (int i = 0; i < fileData.Length - 4; i++)
            {
                if (fileData[i] == 'D' && fileData[i + 1] == 'A' && fileData[i + 2] == 'T' && fileData[i + 3] == 'A')
                {
                    while (i < fileData.Length && fileData[i] != '\n') i++;
                    headerEnd = i + 1;
                    break;
                }
            }

            headerText = System.Text.Encoding.ASCII.GetString(fileData, 0, headerEnd);
            Log.Info(headerText);

            int pointStride = sizeof(float) * 7; // x y z normal_x normal_y normal_z rgb
            int numPoints = (fileData.Length - headerEnd) / pointStride;

            Vertex[] points = new Vertex[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                int offset = headerEnd + i * pointStride;

                float x = BitConverter.ToSingle(fileData, offset + 0);
                float y = BitConverter.ToSingle(fileData, offset + 4);
                float z = BitConverter.ToSingle(fileData, offset + 8);

                float rgbFloat = BitConverter.ToSingle(fileData, offset + 24);
                uint rgb = BitConverter.ToUInt32(BitConverter.GetBytes(rgbFloat), 0);
                byte r = (byte)((rgb >> 16) & 0xFF);
                byte g = (byte)((rgb >> 8) & 0xFF);
                byte b = (byte)(rgb & 0xFF);

                points[i].pos = V.XYZ(x, z, -y);
                points[i].col = Color.HSV(r, g, b).ToLinear();
            }

            cloud = new PointCloud(pointSize, points);
            cloudScale = 1;
        }
        public void Initialize()
        {
            Log.Info($"INITIAL POINTCLOUD POSE POSITION : {cloudPose.position}");
            cloudPose = handler.PoseInfo();
            Log.Info($"ANCHOR POINTCLOUD POSE POSITION : {cloudPose.position}");

            ASCIIParse("Cat.pcd"); //Reads ASCII pcd files
            //BinaryParse("Vineyard_2024-03-13-trimmed.pcd"); //Reads binary pcd files
        }
        public void Shutdown()
        {
        }

        
        
        List<IAsset> filteredAssets = new List<IAsset>();
        Type filterType = typeof(IAsset);
        float filterScroll = 0;
        const int filterScrollCt = 12;
        void VisualizeModel(Model item)
        {
            UI.Model(item, V.XX(UI.LineHeight));
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
        // Shows Models availible to use for point clouds
        //originally going to use file picker, according to research it doesn't run natively on stereokit
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
                    case Model item: VisualizeModel(item); break;
                }
                UI.PopId();
                if (UI.Button(string.IsNullOrEmpty(asset.Id) ? "(null)" : asset.Id, V.XY(UI.LayoutRemaining.x, 0))) // When pressed, will create point cloud of model based of of vertices in model
                {
                    Model model = Model.FromFile(string.IsNullOrEmpty(asset.Id) ? "(null)" : asset.Id);
                    cloud = new PointCloud(pointSize, model);
                    cloudScale = 0.5f / model.Bounds.dimensions.Length;
                }
            }
            UI.WindowEnd();
        }
        // Think of this almost as the 'main' method of the class
        bool winEn = false;
        Pose simpleWinPose = Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1)).Pose;
        Pose simpleWinPose2 = Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1)).Pose;
        public void Step()
        {
            bool secWin = winEn;
            Handed handed = Handed.Left;

            cloud.Draw(cloudPose.ToMatrix(cloudScale));

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
                if (UI.Toggle("Load Model",ref secWin))
                {
                    winEn = !winEn; //used to initialize and uninitialize asset file window
                }
                UI.HSlider("Cloud Scale", ref cloudScale, 0.001f, 2, 0);

                UI.PanelBegin();
                UI.Label("Point Cloud Settings");
                UI.HSeparator();
                UI.Label("Mode:", V.XY(.04f, 0));
                UI.SameLine();
                if (UI.Radio("Fixed", cloud.DistanceIndependantSize)) cloud.DistanceIndependantSize = true;
                UI.SameLine();
                if (UI.Radio("Perspective", !cloud.DistanceIndependantSize)) cloud.DistanceIndependantSize = false;

                UI.Label("Size:", V.XY(.04f, 0)); //Adjust size of points
                UI.SameLine();
                if (UI.HSlider("Point Size", ref pointSize, 0.001f, 0.005f, 0))
                    cloud.PointSize = pointSize;
                UI.PanelEnd();
                if (UI.Button("ASCII"))
                {
                    ASCIIParse("Cat.pcd"); //Reads ASCII pcd files
                }
                if (UI.Button("binary"))
                {
                    BinaryParse("Vineyard_2024-03-13-trimmed.pcd"); //Reads binary pcd files
                }
            }
            UI.WindowEnd();

            if (winEn)
            {
                AssetWindow(); //if bool is true, enable window. WIll automatically uninitialize window if false since it checks per frame
            }
        }
        static bool HandFacingHead(Handed handed)
        {
            Hand hand = Input.Hand(handed);
            if (!hand.IsTracked)
                return false;

            Vec3 palmDirection = hand.palm.Forward.Normalized;
            Vec3 directionToHead = (Input.Head.position - hand.palm.position).Normalized;

            return Vec3.Dot(palmDirection, directionToHead) > 0.5f;
        }
    }
}
