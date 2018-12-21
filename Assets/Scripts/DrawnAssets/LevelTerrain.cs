using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTerrain : BaseShape
{
    [Header("Parameters")]
    [SerializeField]
    private int _numDivisionsX;

    [SerializeField]
    private float _minHeight;

    [SerializeField]
    private float _maxHeight;

    [SerializeField]
    private BaseVertex _startVertex;

    [SerializeField]
    private BaseVertex _endVertex;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _baseVertexPrefab;

    private List<BaseVertex> _terrainVertices;

    // Use this for initialization
    protected override void Start()
    {
        NoiseGenerator perlinNoiseGenerator = new NoiseGenerator();

        _terrainVertices = new List<BaseVertex>();

        // generate the specified number of vertices for the hill on the terrain, and position them
        // evenly
        float pointDistance = _endVertex.transform.position.x - _startVertex.transform.position.x;
        float xOffset = Mathf.Abs(pointDistance) / (_numDivisionsX + 2.0f);
        float yRange = _maxHeight - _minHeight;
        for (int i = 0; i <= _numDivisionsX; i++)
        {
            BaseVertex terrainVertex = Instantiate(_baseVertexPrefab, transform).AddComponent<BaseVertex>();

            float xPosition = _startVertex.transform.position.x + (i + 1) * xOffset;
            float yPosition = perlinNoiseGenerator.GetNoise(i, _numDivisionsX, _minHeight, _maxHeight);
            float radialMask = 1.0f;

            // the further we are from the center of the hill, the more we attenuate the noise.
            // This gives a nice mountain-like profile to it.
            if (i > _numDivisionsX / 2.0f)
            {
                radialMask = (_numDivisionsX - i) / (_numDivisionsX / 2.0f);
            }
            else
            {
                radialMask = i / (_numDivisionsX / 2.0f);
            }

            // apply new y position (noise)
            yPosition += yRange / 2.0f;
            yPosition = transform.position.y + radialMask * yPosition;

            // cap the y position if needed, to ensure the level is physically clearable.
            float maxWorldHeight = transform.position.y + _maxHeight;
            float minWorldHeight = transform.position.y;
            if (yPosition > maxWorldHeight)
            {
                yPosition = maxWorldHeight;
            }
            else if (yPosition < minWorldHeight)
            {
                yPosition = minWorldHeight;
            }

            terrainVertex.transform.position = new Vector3(xPosition, yPosition, 0);

            _terrainVertices.Add(terrainVertex);
        }

        // make edges out of vertices of the terrain. They will be used for collisions
        for (int i = 1; i < _terrainVertices.Count - 1; i++)
        {
            _terrainVertices[i].AddSibling(_terrainVertices[i - 1]);
            _terrainVertices[i].AddSibling(_terrainVertices[i + 1]);
        }

        int verticesCount = _terrainVertices.Count;
        if (verticesCount > 1)
        {
            _terrainVertices[0].AddSibling(_startVertex);
            _terrainVertices[verticesCount - 1].AddSibling(_endVertex);

            _startVertex.AddSibling(_terrainVertices[0]);
            _endVertex.AddSibling(_terrainVertices[verticesCount - 1]);
        }

        AddVertex(_terrainVertices);

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
