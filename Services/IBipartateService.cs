using TSAIdentity.Data;
using TSAIdentity.Models;

namespace TSAIdentity.Services
{
    public interface IBipartateService
    {
        Dictionary<Guid, Guid> ConfigureBipartateGraph(List<Employee> employees, List<Tasks> tasks, ApplicationDbContext context);
        void DisplayVertexLinkages();
        List<Vertex> GetAdjacentVertices(Vertex vertex);
    }
}