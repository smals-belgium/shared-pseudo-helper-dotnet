// <copyright file="InternalsVisibleTo.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

// This assembly is not signed with a strong name, on purpose. The friend assembly is therefore named without a public key.
// If the build ever signs it again, the compiler refuses this line (CS1726): a signed assembly only accepts a friend
// assembly that names its public key too (see doc/restoring-net48.md, the build server signs projects that target net48).
[assembly: InternalsVisibleTo("Nihdi.Common.Pseudonymisation.Tests")]
