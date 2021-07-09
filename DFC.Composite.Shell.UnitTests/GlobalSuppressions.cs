// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Reliability",
    "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "This isn't a library project",
    Scope = "namespaceanddescendants",
    Target = "~N:DFC.Composite.Shell.Test")]
[assembly: SuppressMessage(
    "Reliability",
    "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "This isn't a library project",
    Scope = "namespaceanddescendants",
    Target = "~N:DFC.Composite.Shell.UnitTests.ClientHandlers")]
[assembly: SuppressMessage(
    "Reliability",
    "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "This isn't a library project",
    Scope = "namespaceanddescendants",
    Target = "~N:DFC.Composite.Shell.UnitTests.ServicesTests")]
[assembly: SuppressMessage(
    "Reliability",
    "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "This isn't a library project",
    Scope = "namespaceanddescendants",
    Target = "~N:DFC.Composite.Shell.UnitTests.Controllers")]