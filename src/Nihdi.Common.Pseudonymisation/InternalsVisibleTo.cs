// <copyright file="InternalsVisibleTo.cs" company="Riziv-Inami">
// Copyright (c) Riziv-Inami. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

// The build server signs every assembly with a strong name, and a signed assembly only accepts a friend assembly
// that names its public key too. The server passes the key file on the command line (one key for test builds,
// another for released packages), and the project defines SIGNED when it does (see the csproj). A local build
// is not signed, so it names the friend assembly alone.
// These are public keys, not secrets. They belong to the build server and must be removed once the build no longer signs.
#if SIGNED
// Key the build server signs test builds with
[assembly: InternalsVisibleTo("Nihdi.Common.Pseudonymisation.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a7edb0cd52b4a4dc35a2062702d9aa45fe758c93438d6fbf6a92b56bd25521f5a5d4e3d6a97d6d72f31edbc8fc1c8de2cca076d48eb87984475101fca6dee540195c2098989ffd19384d47dcc0430148e929743c0cae26d6c767a1628436983b52b79b59b7ea9b4f0e0a11134871d69a9c0916976c3c66c5c197129a00ab3eb5")]

// Key the build server signs released packages with
[assembly: InternalsVisibleTo("Nihdi.Common.Pseudonymisation.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c109ebbb0c44975e35113b16cea7124345e2773fef5d56932338a15d8d763399ac8af57560fe1a546e248c3d249e3da3bf260db383f375293c17f694c4747efee9704c46bdd54896b9c96b1caef3c008d6448bc17b69a1828732a5396dd4bbaa39752f82c7b79628b66cb3b08890f5b8c53f6190ccf1baa1cf30d1a723557ff5")]
#else
[assembly: InternalsVisibleTo("Nihdi.Common.Pseudonymisation.Tests")]
#endif
