using System.Reflection;

namespace GrainLibrary.Services;

public record Router
{
    public Type Type { get; set; }
    public object Target { get; set; }
    public MethodInfo MethodInfo { get; set; }
}