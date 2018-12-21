using UnityEngine;
using System.Collections.Generic;

// Put this script on a Camera
public class DrawLines : MonoBehaviour
{
    public Material lineMat;

    public List<BaseShape> BaseShapes;

    public void AddShape(BaseShape shape)
    {
        if (BaseShapes == null)
        {
            BaseShapes = new List<BaseShape>();
        }

        BaseShapes.Add(shape);
    }

    // Connect all of the `points` to the `mainPoint`
    void DrawConnectingLines()
    {
        if (BaseShapes.Count > 0)
        {
            // Loop through each point to connect to the mainPoint
            foreach (BaseShape shape in BaseShapes)
            {
                foreach (Edge edge in shape.Edges)
                {
                    Vector3 initPointPosition = edge.FirstVertex.transform.position;
                    Vector3 endPointPosition = edge.SecondVertex.transform.position;

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

    // To show the lines in the game window whne it is running
    void OnPostRender()
    {
        DrawConnectingLines();
    }

    // To show the lines in the editor
    void OnDrawGizmos()
    {
        DrawConnectingLines();
    }
}