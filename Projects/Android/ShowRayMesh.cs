// SPDX-License-Identifier: MIT
// The authors below grant copyright rights under the MIT license:
// Copyright (c) 2019-2023 Nick Klingensmith
// Copyright (c) 2023 Qualcomm Technologies, Inc.

using RAZR_PointCRep;
using StereoKit;
using static System.Net.Mime.MediaTypeNames;

namespace RAZR_PointCRep
{
    internal class ShowRayMesh : IClass
    {
        string title = "Ray to Mesh";
        string description = "";

        /// :CodeSample: Ray.Intersect Mesh.Intersect
        /// ### Ray Mesh Intersection
        /// Here's an example of casting a Ray at a mesh someplace in world space,
        /// transforming it into model space, calculating the intersection point,
        /// and displaying it back in world space.
        /// 
        /// ![Ray Mesh Intersection]({{site.url}}/img/screenshots/RayMeshIntersect.jpg)
        ///
        Mesh sphereMesh = Default.MeshSphere;
        Mesh boxMesh = Mesh.GenerateRoundedCube(Vec3.One * 0.2f, 0.05f);
        Pose boxPose = ((Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1))) * Matrix.T(0, -0.1f, 0)).Pose;
        Pose castPose = ((Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1))) * Matrix.T(0.25f, 0.11f, 0.2f)).Pose;

        public void StepRayMesh()
        {
            // Draw our setup, and make the visuals grab/moveable!
            UI.Handle("Box", ref boxPose, boxMesh.Bounds);
            UI.Handle("Cast", ref castPose, sphereMesh.Bounds * 0.03f);
            boxMesh.Draw(Default.MaterialUI, boxPose.ToMatrix());
            sphereMesh.Draw(Default.MaterialUI, castPose.ToMatrix(0.03f));
            Lines.Add(castPose.position, boxPose.position, Color.White, 0.005f);

            // Create a ray that's in the Mesh's model space
            Matrix transform = boxPose.ToMatrix();
            Ray ray = transform
                .Inverse
                .Transform(Ray.FromTo(castPose.position, boxPose.position));

            // Draw a sphere at the intersection point, if the ray intersects 
            // with the mesh.
            if (ray.Intersect(boxMesh, out Ray at, out uint index))
            {
                sphereMesh.Draw(Default.Material, Matrix.TS(transform.Transform(at.position), 0.01f));
                if (boxMesh.GetTriangle(index, out Vertex a, out Vertex b, out Vertex c))
                {
                    Vec3 aPt = transform.Transform(a.pos);
                    Vec3 bPt = transform.Transform(b.pos);
                    Vec3 cPt = transform.Transform(c.pos);
                    Lines.Add(aPt, bPt, new Color32(0, 255, 0, 255), 0.005f);
                    Lines.Add(bPt, cPt, new Color32(0, 255, 0, 255), 0.005f);
                    Lines.Add(cPt, aPt, new Color32(0, 255, 0, 255), 0.005f);
                }
            }

        }
        /// :End:

        public void Initialize()
        {
            MenuSort.Screenshot("RayMeshIntersect.jpg", 0, 600, 600, 78,
                (Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1))) * new Vec3(-0.198f, 0.107f, -0.361f),
                (Matrix.TR(0, -0.1f, -0.6f, Quat.LookDir(0, 0, 1))) * new Vec3(-0.046f, -0.200f, 0.578f));
        }

        public void Step()
        {
            StepRayMesh();
        }

        public void Shutdown() { }
    }
}