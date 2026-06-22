using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HadesNode
{
    public int id;
    public int depth;
    public NodeType type;
    public int levelIndex;
    public List<int> connections = new List<int>();
}

public class HadesRunGenerator
{
    public List<HadesNode> GenerateRun(int startLevel, int runDepth, float difficultyBias)
    {
        List<HadesNode> nodes = new List<HadesNode>();
        int idCounter = 0;
        List<List<HadesNode>> layers = new List<List<HadesNode>>();

        for (int d = 0; d < runDepth; d++)
        {
            var layer = new List<HadesNode>();
            int width = (d == 0 || d == runDepth - 1) ? 1 : Random.Range(2, 4);

            for (int w = 0; w < width; w++)
            {
                HadesNode node = new HadesNode();
                node.id = idCounter++;
                node.depth = d;
                node.levelIndex = startLevel + d;

                if (d == 0)
                {
                    node.type = NodeType.Fight;
                }
                else if (d == runDepth - 1)
                {
                    node.type = NodeType.Boss;
                }
                else
                {
                    // NodeType distribution scaled by difficulty bias
                    float r = Random.value * difficultyBias;
                    if (r < 0.4f) node.type = NodeType.Fight;
                    else if (r < 0.65f) node.type = NodeType.Elite;
                    else if (r < 0.8f) node.type = NodeType.Mystery;
                    else if (r < 0.9f) node.type = NodeType.Rest;
                    else node.type = NodeType.Shop;
                }

                layer.Add(node);
                nodes.Add(node);
            }
            layers.Add(layer);
        }

        // Connect layers
        for (int d = 0; d < runDepth - 1; d++)
        {
            var curLayer = layers[d];
            var nextLayer = layers[d + 1];

            foreach (var node in curLayer)
            {
                if (nextLayer.Count == 1)
                {
                    node.connections.Add(nextLayer[0].id);
                }
                else
                {
                    int links = Random.Range(1, 3);
                    for (int k = 0; k < links; k++)
                    {
                        int targetId = nextLayer[Random.Range(0, nextLayer.Count)].id;
                        if (!node.connections.Contains(targetId))
                        {
                            node.connections.Add(targetId);
                        }
                    }
                }
            }
        }

        return nodes;
    }
}
