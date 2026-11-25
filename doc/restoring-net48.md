# Restoring the net48 target

The library no longer targets .NET Framework 4.8: the target was dropped before 1.0.0, because
nobody had asked for it and it carried its own dependencies and code paths. Adding `net48` back
to `TargetFrameworks` in the library and the test project is not enough. This page lists what
the compiler will not tell you.

The test project used to carry an `app.config` with a binding redirect for
`System.Runtime.CompilerServices.Unsafe`. It is not needed to restore the target: with it
removed, the 270 tests still passed on net48, because the build generates the redirect itself.

The code that was removed is in the git history. Look for the commit whose subject mentions
net48:

```bash
git log --grep="net48"
```

## What the compiler will not tell you

- **RSA must be created with `RSACng`.** On .NET Framework, `RSA.Create()` hands out an
  `RSACryptoServiceProvider`, which cannot do OAEP with SHA-256, the padding used to unwrap the
  key of a JWE. `RsaHelper.Create` must return `new RSACng(...)` on that target, behind
  `#if NETFRAMEWORK`. Nothing fails at build time without it: it fails when a key is
  unwrapped. Bring back with it the tests that create their keys with `RSACng` and the one that
  checks the type returned by `RsaHelper.Create`.

## What would make a return impossible

Do not let a public interface use a default interface method or a static abstract member. The
.NET Framework runtime does not support them, and never will: once one is published, the net48
target cannot come back without a breaking change. The test `InterfacesStayPlainTests` fails the
build if an interface of the library gets a method with a body, a static member or a field.

## What the compiler will tell you

The polyfill packages that .NET 8 and later provide in-box (time provider, hash code,
immutable collections, `System.Text.Json`) have to be referenced again for this target. The
build lists the ones that are missing, so this page does not name versions.

## What to check afterwards

Run the whole test suite on every target, `net48` included. The C# Dev Kit test explorer cannot
run the .NET Framework host: use `dotnet test` from the command line.
