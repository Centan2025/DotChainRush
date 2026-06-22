using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Fight,
    Elite,
    Boss,
    Mystery,
    Rest,
    Shop
}

[System.Serializable]
public class RunNode
{
    public int id;
    public int depth;
    public NodeType type;
    public int levelNumber;
    public List<int> connections = new List<int>(); // connected node indices in next depth
}

[System.Serializable]
public class RunDirector
{
    public List<RunNode> activeRun = new List<RunNode>();
    public int currentDepth = 0;
    public int selectedNodeId = -1;

    public void GenerateNewRun(int startLevel, int depth = 10)
    {
        activeRun.Clear();
        currentDepth = 0;
        selectedNodeId = -1;

        int nodeCounter = 0;
        List<List<RunNode>> nodesByDepth = new List<List<RunNode>>();

        // Generate layers of nodes
        for (int d = 0; d < depth; d++)
        {
            var depthList = new List<RunNode>();
            int nodeCountInLayer = (d == 0 || d == depth - 1) ? 1 : Random.Range(2, 4);

            for (int n = 0; n < nodeCountInLayer; n++)
            {
                var node = new RunNode();
                node.id = nodeCounter++;
                node.depth = d;
                node.levelNumber = startLevel + d;

                if (d == 0)
                {
                    node.type = NodeType.Fight;
                }
                else if (d == depth - 1)
                {
                    node.type = NodeType.Boss;
                }
                else
                {
                    // Random node types
                    float rand = Random.value;
                    if (rand < 0.5f) node.type = NodeType.Fight;
                    else if (rand < 0.7f) node.type = NodeType.Elite;
                    else if (rand < 0.85f) node.type = NodeType.Mystery;
                    else if (rand < 0.93f) node.type = NodeType.Rest;
                    else node.type = NodeType.Shop;
                }

                depthList.Add(node);
                activeRun.Add(node);
            }
            nodesByDepth.Add(depthList);
        }

        // Connect nodes to next depth layer
        for (int d = 0; d < depth - 1; d++)
        {
            var currentLayer = nodesByDepth[d];
            var nextLayer = nodesByDepth[d + 1];

            foreach (var node in currentLayer)
            {
                // Each node connects to at least one node in the next layer
                if (nextLayer.Count == 1)
                {
                    node.connections.Add(nextLayer[0].id);
                }
                else
                {
                    // Branching connection
                    int count = Random.Range(1, 3);
                    for (int k = 0; k < count; k++)
                    {
                        int targetIdx = Random.Range(0, nextLayer.Count);
                        int targetId = nextLayer[targetIdx].id;
                        if (!node.connections.Contains(targetId))
                        {
                            node.connections.Add(targetId);
                        }
                    }
                }
            }
        }
    }

    public List<RunNode> GetAvailableChoices()
    {
        if (selectedNodeId == -1)
        {
            // First step
            List<RunNode> firstDepth = new List<RunNode>();
            foreach (var n in activeRun)
            {
                if (n.depth == 0) firstDepth.Add(n);
            }
            return firstDepth;
        }

        var currentNode = activeRun.Find(x => x.id == selectedNodeId);
        if (currentNode == null) return new List<RunNode>();

        List<RunNode> choices = new List<RunNode>();
        foreach (int targetId in currentNode.connections)
        {
            var node = activeRun.Find(x => x.id == targetId);
            if (node != null) choices.Add(node);
        }
        return choices;
    }

    public void SelectNode(int nodeId)
    {
        var node = activeRun.Find(x => x.id == nodeId);
        if (node != null)
        {
            selectedNodeId = nodeId;
            currentDepth = node.depth;
        }
    }
}
