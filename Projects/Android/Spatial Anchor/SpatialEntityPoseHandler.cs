using RAZR_PointCRep.Spatial_Anchor;
using StereoKit;

public class SpatialEntityPoseHandler
{
    Pose pose;
    public SpatialEntityPoseHandler(Pose pose)
    {
        this.pose = pose;
    }
    public SpatialEntityPoseHandler()
    {
    }
    public void DrawAnchor(Pose pose, Color color)
    {
        Mesh.Cube.Draw(Material.Default, pose.ToMatrix(0.1f), color);
    }

    public Pose PoseInfo()
    {
        return this.pose;
    }
}
