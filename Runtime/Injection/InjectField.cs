using System;
using JetBrains.Annotations;

namespace Ninito.MinJect.Injection
{
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse]
    public sealed class InjectField : Attribute
    {
    }
}