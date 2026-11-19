// <copyright file="InternalNamespaceVisibilityTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

using Nihdi.Common.Pseudonymisation.Internal;

/// <summary>
/// The <c>Internal</c> namespace holds the implementation classes. None of them may be visible to the
/// users of the package, who work through the interfaces.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class InternalNamespaceVisibilityTests
{
    [TestMethod]
    public void NoTypeOfTheInternalNamespace_IsPublic()
    {
        var publicTypes = typeof(Domain).Assembly
            .GetTypes()
            .Where(t => t.Namespace == typeof(Domain).Namespace && (t.IsPublic || t.IsNestedPublic))
            .Select(t => t.FullName)
            .ToList();

        Assert.AreEqual(
            0,
            publicTypes.Count,
            "These types of the Internal namespace are public: " + string.Join(", ", publicTypes));
    }
}
