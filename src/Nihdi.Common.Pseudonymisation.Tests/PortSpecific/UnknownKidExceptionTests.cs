// <copyright file="UnknownKidExceptionTests.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Tests.PortSpecific;

using Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Tests for <see cref="UnknownKidException"/> as seen by an application that uses the library.
/// <para>
/// Tests in this folder exist only in the C# port.
/// </para>
/// </summary>
[TestClass]
public class UnknownKidExceptionTests
{
    [TestMethod]
    public void UnknownKidException_IsPublic()
    {
        // An application has to be able to catch this exception by its type, to tell an
        // unknown key apart from the other reasons a transit info is refused.
        Assert.IsTrue(typeof(UnknownKidException).IsPublic);
    }

    [TestMethod]
    public void UnknownKidException_CaughtByItsType_GivesTheKid()
    {
        try
        {
            throw new UnknownKidException("kid-1");
        }
        catch (UnknownKidException e)
        {
            Assert.AreEqual("kid-1", e.Kid);
            Assert.IsInstanceOfType(e, typeof(InvalidTransitInfoException));
        }
    }
}
