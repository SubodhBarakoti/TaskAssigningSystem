using Humanizer;
using Microsoft.Data.SqlClient.DataClassification;
using TSAIdentity.Data;
using TSAIdentity.Models;

namespace TSAIdentity.Services
{
    public class Bipartite
    {
        private readonly Dictionary<Vertexs, List<Vertexs>> _adjacencyList;
        private readonly ApplicationDbContext _context;
        public Dictionary<Vertexs, Vertexs> matching = new Dictionary<Vertexs, Vertexs>();
        public Bipartite(List<Employee> employees, List<Tasks> tasks, ApplicationDbContext context)
        {
            _context = context;
            _adjacencyList = new Dictionary<Vertexs, List<Vertexs>>();
            int i = 0;
            // Create nodes for employees and tasks
            foreach (var employee in employees)
            {
                var employeeNode = new Vertexs(++i,employee.EmployeeId, VertexType.Employee);
                var employeeSkills = _context.EmployeeSkills
                    .Where(es => es.EmployeeId == employee.EmployeeId)
                    .Select(es => es.SkillId)
                    .ToList();

                if (!_adjacencyList.ContainsKey(employeeNode))
                {
                    _adjacencyList[employeeNode] = new List<Vertexs>();
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
                var taskNode = new Vertexs(++j, task.TaskId, VertexType.Task);
                var requiredSkill = task.SkillId;

                if (!_adjacencyList.ContainsKey(taskNode))
                {
                    _adjacencyList[taskNode] = new List<Vertexs>();
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

            int m = _adjacencyList.Keys.Count(vertex => vertex.Type == VertexType.Employee);
            int n = _adjacencyList.Keys.Count(vertex => vertex.Type == VertexType.Task);


            BipGraph g = new BipGraph(m, n);

            foreach (var employeeVertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Employee))
            {
                var adjacentTaskVertices = _adjacencyList[employeeVertex].Where(v => v.Type == VertexType.Task);
                foreach (var taskVertex in adjacentTaskVertices)
                {
                    g.addEdge(employeeVertex.Label,taskVertex.Label);
                }
            }


            List<Tuple<int, int>> matchingSet = g.hopcroftKarp();

            Console.WriteLine("Matching Set:");
            foreach (var pair in matchingSet)
            {
                var employeeLabel = pair.Item1;
                var taskLabel = pair.Item2;

                var employee = _adjacencyList.Keys.Where(v => v.Type == VertexType.Employee && v.Label==employeeLabel).FirstOrDefault();
                var task = _adjacencyList.Keys.Where(v => v.Type == VertexType.Task && v.Label == taskLabel).FirstOrDefault();
                if(employee != null && task != null)
                {
                    matching[employee] = task;
                }
                
            }


        }
        private Vertexs GetOrCreateSkillNode(Guid skillId)
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
            var skillNode = new Vertexs(++k,skillId, VertexType.Skill);
            _adjacencyList[skillNode] = new List<Vertexs>();
            return skillNode;
        }

        public List<Vertexs> GetAdjacentVertices(Vertexs vertex)
        {
            return _adjacencyList[vertex];
        }

        public void DisplayVertexLinkages()
        {
            foreach (var vertex in _adjacencyList.Keys)
            {
                Console.WriteLine($"Vertex:{vertex.Label} {vertex.Id} ({vertex.Type})");
                Console.WriteLine("Adjacent Vertices:");

                foreach (var adjacentVertex in _adjacencyList[vertex])
                {
                    Console.WriteLine($" {adjacentVertex.Label} {adjacentVertex.Id} ({adjacentVertex.Type})");
                }

                Console.WriteLine();
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

    public class Vertexs
    {
        public Vertexs(int label, Guid id, VertexType type)
        {
            Label = label;
            Id = id;
            Type = type;
        }
        public int Label { get; }
        public Guid Id { get; }
        public VertexType Type { get; }
    }

    class BipGraph
    {
        private readonly int m;
        private readonly int n;
        private readonly List<int>[] adj;
        private int[] pairU;
        private int[] pairV;
        private int[] dist;
        const int NIL = 0;
        const int INF = int.MaxValue;
        public BipGraph(int m, int n)
        {
            this.m = m;
            this.n = n;
            adj = new List<int>[m + 1];
            for (int i = 0; i <= m; i++)
            {
                adj[i] = new List<int>();
            }
        }

        public void addEdge(int u, int v)
        {
            adj[u].Add(v);
        }

        public List<Tuple<int, int>> hopcroftKarp()
        {
            pairU = Enumerable.Repeat(NIL, m + 1).ToArray();
            pairV = Enumerable.Repeat(NIL, n + 1).ToArray();
            dist = Enumerable.Repeat(0, m + 1).ToArray();

            List<Tuple<int, int>> matchingSet = new List<Tuple<int, int>>();
            while (bfs())
            {
                for (int u = 1; u <= m; u++)
                {
                    if (pairU[u] == NIL && dfs(u))
                    {
                        matchingSet.Add(new Tuple<int, int>(u, pairU[u]));
                    }
                }
            }
            return matchingSet;
        }

        private bool bfs()
        {
            var Q = new Queue<int>();

            for (int u = 1; u <= m; u++)
            {
                if (pairU[u] == NIL)
                {
                    dist[u] = 0;
                    Q.Enqueue(u);
                }
                else
                {
                    dist[u] = INF;
                }
            }
            dist[NIL] = INF;

            while (Q.Count > 0)
            {
                int u = Q.Dequeue();
                if (dist[u] < dist[NIL])
                {
                    foreach (int v in adj[u])
                    {
                        if (dist[pairV[v]] == INF)
                        {
                            dist[pairV[v]] = dist[u] + 1;
                            Q.Enqueue(pairV[v]);
                        }
                    }
                }
            }
            return dist[NIL] != INF;
        }

        private bool dfs(int u)
        {
            if (u != NIL)
            {
                foreach (int v in adj[u])
                {
                    if (dist[pairV[v]] == dist[u] + 1)
                    {
                        if (dfs(pairV[v]))
                        {
                            pairV[v] = u;
                            pairU[u] = v;
                            return true;
                        }
                    }
                }
                dist[u] = INF;
                return false;
            }
            return true;
        }
    }
}
