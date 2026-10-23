// <copyright file="InternalsVisibleTo.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

// The build server signs every assembly with a strong name, and a signed assembly only accepts a friend assembly
// that names its public key too. The server passes the key file on the command line, and the project defines SIGNED
// when it does (see the csproj). A local build is not signed, so it names the friend assembly alone.
// This is a public key, not a secret. It belongs to the build server and must be removed once the build no longer signs.
#if SIGNED
[assembly: InternalsVisibleTo("Nihdi.Common.Pseudonymisation.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ede1640985b3181bc3bb8e0fe90da780ace8037668ab0d194ef60da713ed20329932045e0c1dfd5ff0450c06150197079e1a6cc569ff4bc39e66f98633e035e9fbe8f51d83ae37e9f32d015e7e21fc19bda52091a016403826722164b3720d01b0b63a75ca36441477df252de8d5f07137def42f35cfb4c8d347575e72283ccf")]
#else
[assembly: InternalsVisibleTo("Nihdi.Common.Pseudonymisation.Tests")]
#endif
