// <copyright file="EHealthProblemException.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

namespace Nihdi.Common.Pseudonymisation.Exceptions;

/// <summary>
/// Exception thrown when the result of a "multiple" operation contains a problem.
/// </summary>
public class EHealthProblemException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EHealthProblemException"/> class.
    /// </summary>
    /// <param name="problem">The eHealth problem.</param>
    public EHealthProblemException(IEHealthProblem problem)
      : base(problem.Detail)
    {
        Problem = problem;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EHealthProblemException"/> class.
    /// </summary>
    /// <param name="problem">The eHealth problem.</param>
    /// <param name="cause">The cause of the exception.</param>
    public EHealthProblemException(IEHealthProblem problem, Exception cause)
      : base(problem.Detail, cause)
    {
        Problem = problem;
    }

    /// <summary>
    /// Gets the eHealth problem.
    /// </summary>
    /// <value>The eHealth problem.</value>
    public IEHealthProblem Problem
    {
        get; private set;
    }
}
