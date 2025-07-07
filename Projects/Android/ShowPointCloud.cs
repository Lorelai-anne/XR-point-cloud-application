using StereoKit;
using System;
using System.Linq;

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

namespace RAZR_PointCRep
{
    internal class ShowPointCloud : IClass
    {
        Pose cloudPose = (Matrix.T(0.2f, -0.1f, 0) * (Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1)))).Pose;
        float cloudScale = 1;
        PointCloud cloud;

        Pose settingsPose = (Matrix.T(-0.2f, 0, 0) * (Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1)))).Pose;
        float pointSize = 0.01f;

        public void Initialize() // currently drawing points for sphere, if file picker not working, change this
        {
            Model model = Model.FromFile("DamagedHelmet.gltf");
            cloud = new PointCloud(pointSize, model);
            cloudScale = 0.5f / model.Bounds.dimensions.Length;
        }

        public void Shutdown()
        {
        }

        public void Step()
        {
            Handed handed = Handed.Left;

            cloud.Draw(cloudPose.ToMatrix(cloudScale));

            if (!HandFacingHead(handed))
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
                if (UI.Button("Load Model"))
                {
                    Platform.FilePicker(PickerMode.Open, (file) =>
                    {
                        Model model = Model.FromFile(file);
                        cloud = new PointCloud(pointSize, model);
                        cloudScale = 0.5f / model.Bounds.dimensions.Length;
                    }, null, Assets.ModelFormats);
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

                UI.Label("Size:", V.XY(.04f, 0));
                UI.SameLine();
                if (UI.HSlider("Point Size", ref pointSize, 0.001f, 0.1f, 0))
                    cloud.PointSize = pointSize;
                UI.PanelEnd();
            }
            UI.WindowEnd();
        }

        static bool HandFacingHead(Handed handed)
        {
            Hand hand = Input.Hand(handed);
            if (!hand.IsTracked)
                return false;

            Vec3 palmDirection = (hand.palm.Forward).Normalized;
            Vec3 directionToHead = (Input.Head.position - hand.palm.position).Normalized;

            return Vec3.Dot(palmDirection, directionToHead) > 0.5f;
        }
    }
}
