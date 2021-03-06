// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IReadOnlyNavigation" />.
    /// </summary>
    [Obsolete("Use IReadOnlyNavigation")]
    public static class NavigationExtensions
    {
        /// <summary>
        ///     Gets a value indicating whether the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity.
        /// </summary>
        /// <param name="navigation"> The navigation property to check. </param>
        /// <returns>
        ///     <see langword="true" /> if the given navigation property is the navigation property on the dependent entity
        ///     type that points to the principal entity, otherwise <see langword="false" />.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.IsOnDependent")]
        public static bool IsDependentToPrincipal([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).IsOnDependent;

        /// <summary>
        ///     Gets a value indicating whether the given navigation property is a collection property.
        /// </summary>
        /// <param name="navigation"> The navigation property to check. </param>
        /// <returns>
        ///     <see langword="true" /> if this is a collection property, false if it is a reference property.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.IsCollection")]
        public static bool IsCollection([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).IsCollection;

        /// <summary>
        ///     Gets the navigation property on the other end of the relationship. Returns null if
        ///     there is no navigation property defined on the other end of the relationship.
        /// </summary>
        /// <param name="navigation"> The navigation property to find the inverse of. </param>
        /// <returns>
        ///     The inverse navigation, or <see langword="null" /> if none is defined.
        /// </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.Inverse")]
        public static INavigation? FindInverse([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).Inverse;

        /// <summary>
        ///     Gets the entity type that a given navigation property will hold an instance of
        ///     (or hold instances of if it is a collection navigation).
        /// </summary>
        /// <param name="navigation"> The navigation property to find the target entity type of. </param>
        /// <returns> The target entity type. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use IReadOnlyNavigation.TargetEntityType")]
        public static IEntityType GetTargetType([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).TargetEntityType;

        /// <summary>
        ///     Gets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="navigation"> The navigation property to find whether it should be eager loaded. </param>
        /// <returns> A value indicating whether this navigation should be eager loaded by default. </returns>
        [Obsolete("Use IReadOnlyNavigation.IsEagerLoaded")]
        public static bool IsEagerLoaded([NotNull] this INavigation navigation)
            => Check.NotNull(navigation, nameof(navigation)).IsEagerLoaded;
    }
}
