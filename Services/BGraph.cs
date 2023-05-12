using TSAIdentity.Data;
using TSAIdentity.Models;

namespace TSAIdentity.Services
{
    public class BGraph
    {
        private readonly Dictionary<Vertex, List<Vertex>> _adjacencyList;
        private readonly ApplicationDbContext _context;
        public Dictionary<Guid,Guid> matchings = new Dictionary<Guid, Guid>();
        public BGraph(List<Employee> employees, List<Tasks> tasks, ApplicationDbContext context)
        {
            _context = context;
            _adjacencyList = new Dictionary<Vertex, List<Vertex>>();
            int i = 0;
            // Create nodes for employees and tasks
            foreach (var employee in employees)
            {
                var employeeNode = new Vertex(++i, employee.EmployeeId, VertexType.Employee);
                var employeeSkills = _context.EmployeeSkills
                    .Where(es => es.EmployeeId == employee.EmployeeId)
                    .Select(es => es.SkillId)
                    .ToList();

                if (!_adjacencyList.ContainsKey(employeeNode))
                {
                    _adjacencyList[employeeNode] = new List<Vertex>();
                }

                // Label employee node with skills
                foreach (var skillId in employeeSkills)
                {
                    var skillNode = GetOrCreateSkillNode(skillId);

                    _adjacencyList[employeeNode].Add(skillNode);
                    _adjacencyList[skillNode].Add(employeeNode);
                }
            }

            int j = 0;
            foreach (var task in tasks)
            {
                var taskNode = new Vertex(++j, task.TaskId, VertexType.Task);
                var requiredSkill = task.SkillId;

                if (!_adjacencyList.ContainsKey(taskNode))
                {
                    _adjacencyList[taskNode] = new List<Vertex>();
                }

                // Label task node with required skills
                var skillNode = GetOrCreateSkillNode(requiredSkill);

                _adjacencyList[taskNode].Add(skillNode);
                _adjacencyList[skillNode].Add(taskNode);

                var employeeNodesWithMatchingSkills = _adjacencyList[skillNode]
            .Where(vertex => vertex.Type == VertexType.Employee);

                foreach (var employeeNode in employeeNodesWithMatchingSkills)
                {
                    _adjacencyList[employeeNode].Add(taskNode);
                    _adjacencyList[taskNode].Add(employeeNode);
                }
            }

            //algorithm use space

            var lefts = new HashSet<string> ();
            var rights = new HashSet<string> ();
            var edges = new Dictionary<string,HashSet<string>> ();
            foreach(var vertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Task)){
                lefts.Add(vertex.Id.ToString());
                var empset= new HashSet<string> ();
                foreach(var emp in _adjacencyList[vertex].Where(v=>v.Type == VertexType.Employee))
                {
                    empset.Add(emp.Id.ToString());
                }
                edges[vertex.Id.ToString()] = empset;
            }
            foreach (var vertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Employee))
            {
                rights.Add(vertex.Id.ToString());
            }
            
            var matches= HopcroftKarp(lefts, rights, edges);

            foreach (var match in matches)
            {
                matchings[Guid.Parse(match.Key)] = Guid.Parse(match.Value);
            }

        }
        private Vertex GetOrCreateSkillNode(Guid skillId)
        {
            int k = 0;
            foreach (var vertex in _adjacencyList.Keys)
            {
                if (vertex.Type == VertexType.Skill && vertex.Id == skillId)
                {
                    return vertex; // Return existing skill node if found
                }
            }

            // Create a new skill node if not found
            var skillNode = new Vertex(++k, skillId, VertexType.Skill);
            _adjacencyList[skillNode] = new List<Vertex>();
            return skillNode;
        }

        public List<Vertex> GetAdjacentVertices(Vertex vertex)
        {
            return _adjacencyList[vertex];
        }

        public void DisplayVertexLinkages()
        {
            foreach (var vertex in _adjacencyList.Keys)
            {
                Console.WriteLine($"Vertex: {vertex.Id} ({vertex.Type})");
                Console.WriteLine("Adjacent Vertices:");

                foreach (var adjacentVertex in _adjacencyList[vertex])
                {
                    Console.WriteLine($"Vertex: {adjacentVertex.Id} ({adjacentVertex.Type})");
                }

                Console.WriteLine();
            }
        }
        static bool HasAugmentingPath(IEnumerable<string> lefts,
                              IReadOnlyDictionary<string, HashSet<string>> edges,
                              IReadOnlyDictionary<string, string> toMatchedRight,
                              IReadOnlyDictionary<string, string> toMatchedLeft,
                              IDictionary<string, long> distances,
                              Queue<string> q)
        {
            foreach (var left in lefts)
            {
                if (toMatchedRight[left] == "")
                {
                    distances[left] = 0;
                    q.Enqueue(left);
                }
                else
                {
                    distances[left] = long.MaxValue;
                }
            }

            distances[""] = long.MaxValue;

            while (0 < q.Count)
            {
                var left = q.Dequeue();

                if (distances[left] < distances[""])
                {
                    foreach (var right in edges[left])
                    {
                        var nextLeft = toMatchedLeft[right];
                        if (distances[nextLeft] == long.MaxValue)
                        {
                            // The nextLeft has not been visited and is being visited.
                            distances[nextLeft] = distances[left] + 1;
                            q.Enqueue(nextLeft);
                        }
                    }
                }
            }

            return distances[""] != long.MaxValue;
        }

        // DFS
        static bool TryMatching(string left,
                                IReadOnlyDictionary<string, HashSet<string>> edges,
                                IDictionary<string, string> toMatchedRight,
                                IDictionary<string, string> toMatchedLeft,
                                IDictionary<string, long> distances)
        {
            if (left == "")
            {
                return true;
            }

            foreach (var right in edges[left])
            {
                var nextLeft = toMatchedLeft[right];
                if (distances[nextLeft] == distances[left] + 1)
                {
                    if (TryMatching(nextLeft, edges, toMatchedRight, toMatchedLeft, distances))
                    {
                        toMatchedLeft[right] = left;
                        toMatchedRight[left] = right;
                        return true;
                    }
                }
            }

            // The left could not match any right.
            distances[left] = long.MaxValue;

            return false;
        }

        static Dictionary<string, string> HopcroftKarp(HashSet<string> lefts,
                                                       IEnumerable<string> rights,
                                                       IReadOnlyDictionary<string, HashSet<string>> edges)
        {
            // "distance" is from a starting left to another left when zig-zaging left, right, left, right, left in DFS.

            // Take the following for example:
            // left1 -> (unmatched edge) -> right1 -> (matched edge) -> left2 -> (unmatched edge) -> right2 -> (matched edge) -> left3
            // distance can be as follows.
            // distances[left1] = 0 (Starting left is distance 0.)
            // distances[left2] = distances[left1] + 1 = 1
            // distances[left3] = distances[left2] + 1 = 2

            // Note
            // Both a starting left and an ending left are unmatched with right.
            // Moving from left to right uses a unmatched edge.
            // Moving from right to left uses a matched edge.

            var distances = new Dictionary<string, long>();

            var q = new Queue<string>();

            // All lefts start as being unmatched with any right.
            var toMatchedRight = lefts.ToDictionary(s => s, s => "");

            // All rights start as being unmatched with any left.
            var toMatchedLeft = rights.ToDictionary(s => s, s => "");

            // Note
            // toMatchedRight and toMatchedLeft are the same thing but inverse to each other.
            // Using either of them is enough but inefficient
            // because a dictionary cannot be straightforwardly looked up bi-directionally.

            while (HasAugmentingPath(lefts, edges, toMatchedRight, toMatchedLeft, distances, q))
            {
                foreach (var unmatchedLeft in lefts.Where(left => toMatchedRight[left] == ""))
                {
                    TryMatching(unmatchedLeft, edges, toMatchedRight, toMatchedLeft, distances);
                }
            }

            // Remove unmatches
            RemoveItems(toMatchedRight, kvp => kvp.Value == "");

            // Return matches
            return toMatchedRight;
        }

        static void RemoveItems<T1, T2>(IDictionary<T1, T2> d, Func<KeyValuePair<T1, T2>, bool> isRemovable)
        {
            foreach (var kvp in d.Where(isRemovable).ToList())
            {
                d.Remove(kvp.Key);
            }
        }

    }
    public enum VertexType
    {
        Employee,
        Task,
        Skill,
        Null
    }
    public class Vertex
    {
        public Vertex(int label, Guid id, VertexType type)
        {
            Id = id;
            Type = type;
        }
        public Guid Id { get; }
        public VertexType Type { get; }
    }
}
