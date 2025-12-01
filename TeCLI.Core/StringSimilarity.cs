using System;
using System.Collections.Generic;
using System.Linq;

namespace TeCLI;

/// <summary>
/// Provides string similarity calculations for suggesting corrections to user input.
/// </summary>
public static class StringSimilarity
{
    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// The Levenshtein distance is the minimum number of single-character edits
    /// (insertions, deletions, or substitutions) required to change one string into another.
    /// </summary>
    /// <param name="source">The source string</param>
    /// <param name="target">The target string</param>
    /// <returns>The Levenshtein distance between the two strings</returns>
    public static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        int sourceLength = source.Length;
        int targetLength = target.Length;

        // Create a 2D array to store distances
        int[,] distance = new int[sourceLength + 1, targetLength + 1];

        // Initialize first column and row
        for (int i = 0; i <= sourceLength; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= targetLength; j++)
            distance[0, j] = j;

        // Calculate distances
        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(
                        distance[i - 1, j] + 1,      // deletion
                        distance[i, j - 1] + 1),     // insertion
                    distance[i - 1, j - 1] + cost);  // substitution
            }
        }

        return distance[sourceLength, targetLength];
    }

    /// <summary>
    /// Finds the most similar string from a collection of candidates.
    /// </summary>
    /// <param name="input">The input string to compare</param>
    /// <param name="candidates">Collection of candidate strings</param>
    /// <param name="maxDistance">Maximum Levenshtein distance to consider (default: 3)</param>
    /// <returns>The most similar string, or null if no candidates are within maxDistance</returns>
    public static string? FindMostSimilar(string input, IEnumerable<string> candidates, int maxDistance = 3)
    {
        if (string.IsNullOrEmpty(input) || candidates == null || !candidates.Any())
            return null;

        string? bestMatch = null;
        int bestDistance = int.MaxValue;

        foreach (var candidate in candidates)
        {
            // Case-insensitive comparison
            int distance = CalculateLevenshteinDistance(
                input.ToLowerInvariant(),
                candidate.ToLowerInvariant());

            if (distance < bestDistance && distance <= maxDistance)
            {
                bestDistance = distance;
                bestMatch = candidate;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Finds all similar strings from a collection of candidates within a maximum distance.
    /// </summary>
    /// <param name="input">The input string to compare</param>
    /// <param name="candidates">Collection of candidate strings</param>
    /// <param name="maxDistance">Maximum Levenshtein distance to consider (default: 2)</param>
    /// <param name="maxResults">Maximum number of suggestions to return (default: 3)</param>
    /// <returns>List of similar strings sorted by similarity</returns>
    public static List<string> FindSimilar(string input, IEnumerable<string> candidates, int maxDistance = 2, int maxResults = 3)
    {
        if (string.IsNullOrEmpty(input) || candidates == null || !candidates.Any())
            return new List<string>();

        var matches = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Distance = CalculateLevenshteinDistance(
                    input.ToLowerInvariant(),
                    candidate.ToLowerInvariant())
            })
            .Where(x => x.Distance <= maxDistance)
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Candidate)
            .Take(maxResults)
            .Select(x => x.Candidate)
            .ToList();

        return matches;
    }
}
