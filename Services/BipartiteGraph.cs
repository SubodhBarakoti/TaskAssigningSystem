using TSAIdentity.Data;
using TSAIdentity.Models;

namespace TSAIdentity.Services
{
    public class BipartiteGraph
    {
        private readonly Dictionary<Vertex, List<Vertex>> _adjacencyList;
        private readonly ApplicationDbContext _context;
        public Dictionary<Vertex, Vertex> matching = new Dictionary<Vertex, Vertex>();
        public Dictionary<Vertex, int> distance =new Dictionary<Vertex, int>();
        public static int MaxMatchingSize = 0;
        public readonly Vertex _nullVertex;
        public BipartiteGraph(List<Employee> employees, List<Tasks> tasks, ApplicationDbContext context)
        {
            _context = context;
            _adjacencyList = new Dictionary<Vertex, List<Vertex>>();

            // Create nodes for employees and tasks
            foreach (var employee in employees)
            {
                var employeeNode = new Vertex(employee.EmployeeId, VertexType.Employee);
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


            foreach (var task in tasks)
            {
                var taskNode = new Vertex(task.TaskId, VertexType.Task);
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



            //adding null vertex
            _nullVertex = new Vertex(Guid.Empty, VertexType.Null);
            distance[_nullVertex] = int.MaxValue;

        }
        private Vertex GetOrCreateSkillNode(Guid skillId)
        {
            foreach (var vertex in _adjacencyList.Keys)
            {
                if (vertex.Type == VertexType.Skill && vertex.Id == skillId)
                {
                    return vertex; // Return existing skill node if found
                }
            }

            // Create a new skill node if not found
            var skillNode = new Vertex(skillId, VertexType.Skill);
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
                    Console.WriteLine($"  {adjacentVertex.Id} ({adjacentVertex.Type})");
                }

                Console.WriteLine();
            }
        }
        public void HopcroftKarp()
        {
            foreach (var employeeVertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Employee))
            {
                distance[employeeVertex] = 0;
                matching[employeeVertex] = null;
            }

            foreach (var vertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Task))
            {
                matching[vertex] = null;
            }
            // Run the Hopcroft-Karp algorithm
            while (BFS())
            {
                Console.WriteLine("BFS run finally");
                foreach (var employeeVertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Employee))
                {
                    
                    if (matching[employeeVertex] == null && DFS(employeeVertex))
                    {
                        MaxMatchingSize++;
                    }
                }
                
            }
        }

        private bool BFS()
        {
            var queue = new Queue<Vertex>();

            //here
            int count = 0;
            //to here



            // Initialize distances and enqueue unmatched employees
            foreach (var employeeVertex in _adjacencyList.Keys.Where(v => v.Type == VertexType.Employee))
            {
                if (matching[employeeVertex] == null)
                {
                    distance[employeeVertex] = 0;
                    queue.Enqueue(employeeVertex);
                }
                else
                {
                    distance[employeeVertex] = int.MaxValue;
                }
            }
            
            // Run BFS
            while (queue.Count > 0)
            {
                var currentVertex = queue.Dequeue();
                if (distance[currentVertex] < distance[_nullVertex])
                {
                    foreach (var taskVertex in GetAdjacentVertices(currentVertex).Where(v => v.Type == VertexType.Task))
                    {
                        ////here
                        //if (matching[taskVertex] == null && count==0)
                        //{
                        //    matching[taskVertex] = _nullVertex;
                        //    count++;
                        //}
                        ////to here


                        if (matching[taskVertex] != null && distance[matching[taskVertex]] == int.MaxValue)
                        {
                            Console.WriteLine("Ok");
                            distance[matching[taskVertex]] = distance[currentVertex] + 1;

                            ////here
                            //distance[_nullVertex] = distance[matching[taskVertex]];
                            ////to here

                            queue.Enqueue(matching[taskVertex]);
                        }
                    }
                }
            }

            return distance[_nullVertex] != int.MaxValue;
        }

        private bool DFS(Vertex employeeVertex)
        {
            if (employeeVertex != null)
            {
                foreach (var taskVertex in _adjacencyList[employeeVertex].Where(v => v.Type == VertexType.Task))
                {
                    if (distance[matching[taskVertex]] == distance[employeeVertex] + 1 && DFS(matching[taskVertex]))
                    {
                        matching[employeeVertex] = taskVertex;
                        matching[taskVertex] = employeeVertex;
                        return true;
                    }
                }

                distance[employeeVertex] = int.MaxValue;
                return false;
            }

            return true;
        }


    }

    

    public class Vertex
    {
        public Vertex(Guid id, VertexType type)
        {
            Id = id;
            Type = type;
        }

        public Guid Id { get; }
        public VertexType Type { get; }
    }
}
