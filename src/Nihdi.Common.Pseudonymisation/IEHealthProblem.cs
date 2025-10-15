// <copyright file="IEHealthProblem.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation;

/// <summary>
/// Interface representing an eHealth problem.
/// </summary>
public interface IEHealthProblem
{
    /// <summary>
    /// Gets the detail of the problem.
    /// </summary>
    /// <value>The detail of the problem.</value>
    string Detail
    {
        get;
    }

    /// <summary>
    /// Gets the status code associated with the problem.
    /// </summary>
    /// <value>The status code associated with the problem.</value>
    string Status
    {
        get;
    }

    /// <summary>
    /// Gets the title of the problem.
    /// </summary>
    /// <value>The title of the problem.</value>
    string Title
    {
        get;
    }

    /// <summary>
    /// Gets the type of the problem.
    /// </summary>
    /// <value>The type of the problem.</value>
    string Type
    {
        get;
    }
}