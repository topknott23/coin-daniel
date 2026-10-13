using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace IslandCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputPath = File.Exists("../coins_flattened.png")
                ? "../coins_flattened.png"
                : "coins_flattened.png";

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Error: Input file '{inputPath}' not found.");
                return;
            }

            Console.WriteLine("Loading image...");
            using (Bitmap bitmap = new Bitmap(inputPath))
            {
                Color bgColor = bitmap.GetPixel(0, 0);
                Console.WriteLine($"Background base color: C({bgColor.R}, {bgColor.G}, {bgColor.B})");
                Console.WriteLine("Finding islands using BFS...");

                List<int> coinAreas = FindCoinAreas(bitmap, bgColor, minCoinArea: 1000);

                Console.WriteLine($"\nFound {coinAreas.Count} total coins.");

                if (coinAreas.Count == 0)
                {
                    Console.WriteLine("No valid coins found to group.");
                    return;
                }

                var groups = GroupCoinsByGaps(coinAreas, numGroups: 5);

                Console.WriteLine($"Grouped into {groups.Count} coin denominations:");
                string[] coinNames = { "5 Cent", "10 Cent", "25 Cent", "1 Peso", "5 Peso" };

                for (int i = 0; i < groups.Count; i++)
                {
                    double avgArea = groups[i].Average();
                    string name = i < coinNames.Length ? coinNames[i] : $"Group {i + 1}";
                    Console.WriteLine($"{name} (~{avgArea:F0} pixels): {groups[i].Count} coins");
                }
            }
        }

        /// <summary>
        /// Detects connected coin components using Breadth-First Search (BFS) flood fill.
        /// </summary>
        static List<int> FindCoinAreas(Bitmap bitmap, Color bgColor, int minCoinArea)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            bool[,] visited = new bool[width, height];
            bool[,] isCoin = new bool[width, height];

            // Single pass to classify pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color p = bitmap.GetPixel(x, y);
                    // Foreground coin pixels differ from the detected background
                    isCoin[x, y] = (p.R != bgColor.R || p.G != bgColor.G || p.B != bgColor.B);
                }
            }

            List<int> coinAreas = new List<int>();

            // 8-directional neighbor offsets
            int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
            int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (visited[x, y] || !isCoin[x, y]) continue;

                    // BFS Flood Fill to measure island area
                    int area = 0;
                    Queue<Point> queue = new Queue<Point>();

                    visited[x, y] = true;
                    queue.Enqueue(new Point(x, y));

                    while (queue.Count > 0)
                    {
                        Point current = queue.Dequeue();
                        area++;

                        for (int i = 0; i < 8; i++)
                        {
                            int nx = current.X + dx[i];
                            int ny = current.Y + dy[i];

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny] && isCoin[nx, ny])
                            {
                                visited[nx, ny] = true;
                                queue.Enqueue(new Point(nx, ny));
                            }
                        }
                    }

                    if (area >= minCoinArea)
                    {
                        coinAreas.Add(area);
                    }
                }
            }

            return coinAreas;
        }

        /// <summary>
        /// Groups coins into denominations by finding the largest natural breaks (gaps) in sorted areas.
        /// </summary>
        static List<List<int>> GroupCoinsByGaps(List<int> coinAreas, int numGroups)
        {
            coinAreas.Sort();

            if (coinAreas.Count < numGroups)
            {
                return new List<List<int>> { new List<int>(coinAreas) };
            }

            // Calculate gaps between adjacent sorted coin areas
            var gaps = new List<(int gapSize, int splitIndex)>();
            for (int i = 1; i < coinAreas.Count; i++)
            {
                gaps.Add((coinAreas[i] - coinAreas[i - 1], i));
            }

            // Identify split points at the (numGroups - 1) largest size gaps
            var splitIndices = gaps.OrderByDescending(g => g.gapSize)
                                   .Take(numGroups - 1)
                                   .Select(g => g.splitIndex)
                                   .OrderBy(idx => idx)
                                   .ToList();

            List<List<int>> groups = new List<List<int>>();
            int startIndex = 0;

            foreach (int splitIndex in splitIndices)
            {
                groups.Add(coinAreas.GetRange(startIndex, splitIndex - startIndex));
                startIndex = splitIndex;
            }
            groups.Add(coinAreas.GetRange(startIndex, coinAreas.Count - startIndex));

            return groups;
        }
    }
}
