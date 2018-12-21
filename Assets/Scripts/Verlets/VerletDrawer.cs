using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class renders verlet shapes. It overlaps with DrawLines. Ideally there would only be one
// class to handle both cases.
public class VerletDrawer : MonoBehaviour
{

    public Material lineMat;

    public List<VerletShape> Shapes;

    private void Render()
    {
        // render each shape one stick at a time, unless the stick is marked as a constraint
        foreach(VerletShape shape in Shapes)
        {
            foreach (Stick stick in shape.Sticks)
            {
                if(!stick.IsConstraint)
                {
                    Vector3 initPointPosition = new Vector3(stick.P0.CurrentX, stick.P0.CurrentY, 0);
                    Vector3 endPointPosition = new Vector3(stick.P1.CurrentX, stick.P1.CurrentY, 0);
                    GL.Begin(GL.LINES);
                    lineMat.SetPass(0);
                    GL.Color(new Color(lineMat.color.r, lineMat.color.g, lineMat.color.b, lineMat.color.a));
                    GL.Vertex3(initPointPosition.x, initPointPosition.y, initPointPosition.z);
                    GL.Vertex3(endPointPosition.x, endPointPosition.y, endPointPosition.z);
                    GL.End();
                }
            }
        }
    }

    // render
    void OnPostRender()
    {
        Render();
    }
}
