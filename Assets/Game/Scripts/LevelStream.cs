using System.Collections.Generic;

public class LevelStream
{
    private Dictionary<int, LevelConfig> cache = new Dictionary<int, LevelConfig>();
    private LevelGenerator generator = new LevelGenerator();

    public LevelConfig Get(int id)
    {
        if (!cache.ContainsKey(id))
        {
            if (WorldDirector.Instance != null)
            {
                cache[id] = WorldDirector.Instance.GenerateLevel(id);
            }
            else
            {
                cache[id] = generator.GenerateLevel(id);
            }
        }
        return cache[id];
    }

    public void ClearCache()
    {
        cache.Clear();
    }
}
