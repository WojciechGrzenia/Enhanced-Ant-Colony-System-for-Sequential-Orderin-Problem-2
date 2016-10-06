using System;

namespace DTO
{
    /// <summary>
    ///     Represents options for eating pheromones during local search.
    /// </summary>
    [Serializable]
    public enum EatPheromoneDuringLocalSearch
    {
        DontEatPheromone,
        EatOnlyOnChangedRoutes,
        EatOnWholePath
    }
}