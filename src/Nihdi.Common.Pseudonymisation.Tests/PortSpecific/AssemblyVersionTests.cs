// <copyright file="AssemblyVersionTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

/// <summary>
/// The library must carry a version number. With no version attribute compiled in, the assembly reports
/// 0.0.0.0, and an integrator who reads the file properties or a log line cannot tell which release runs.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class AssemblyVersionTests
{
    [TestMethod]
    public void TheLibraryAssembly_HasAVersion()
    {
        var version = typeof(IDomain).Assembly.GetName().Version;

        Assert.IsNotNull(version);
        Assert.AreNotEqual(new Version(0, 0, 0, 0), version, "No version attribute is compiled into the library (GenerateAssemblyInfo is off?).");
    }
}
