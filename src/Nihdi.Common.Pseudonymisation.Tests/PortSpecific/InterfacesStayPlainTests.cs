// <copyright file="InterfacesStayPlainTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

using System.Reflection;

/// <summary>
/// An interface of the library may only declare members that the classes implement: no method with a
/// body, no static member, no field.
/// <para>
/// The Java original does have methods with a body in its interfaces (<c>default</c>), so a fix from
/// upstream can bring one. The .NET Framework runtime cannot load such an interface, and never will:
/// once the library publishes one, the net48 target cannot come back without a breaking change.
/// </para>
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class InterfacesStayPlainTests
{
    private const BindingFlags _allDeclaredMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    [TestMethod]
    public void NoInterfaceOfTheLibrary_HasAMethodWithABody_OrAStaticMember_OrAField()
    {
        var offenders = new List<string>();

        foreach (var type in typeof(IDomain).Assembly.GetTypes().Where(t => t.IsInterface))
        {
            offenders.AddRange(type.GetMethods(_allDeclaredMembers)
                .Where(m => !m.IsAbstract || m.IsStatic)
                .Select(m => $"{type.FullName}.{m.Name} (method with a body or static)"));

            offenders.AddRange(type.GetFields(_allDeclaredMembers)
                .Select(f => $"{type.FullName}.{f.Name} (field)"));
        }

        var message = "The .NET Framework runtime cannot load an interface with a method body or a static member, "
            + "which would close the door on restoring the net48 target (see doc/restoring-net48.md). "
            + "Implement the member in the classes instead, or write it as an extension method. Offenders: "
            + string.Join("; ", offenders);

        Assert.AreEqual(0, offenders.Count, message);
    }
}
