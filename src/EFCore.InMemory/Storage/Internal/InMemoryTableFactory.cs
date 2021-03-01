// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InMemoryTableFactory : IInMemoryTableFactory
    {
        private readonly bool _sensitiveLoggingEnabled;
        private readonly bool _nullabilityCheckEnabled;

        private readonly ConcurrentDictionary<(IEntityType EntityType, IInMemoryTable? BaseTable), Func<IInMemoryTable>> _factories = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryTableFactory(
            [NotNull] ILoggingOptions loggingOptions,
            [NotNull] IInMemorySingletonOptions options)
        {
            Check.NotNull(loggingOptions, nameof(loggingOptions));
            Check.NotNull(options, nameof(options));

            _sensitiveLoggingEnabled = loggingOptions.IsSensitiveDataLoggingEnabled;
            _nullabilityCheckEnabled = options.IsNullabilityCheckEnabled;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IInMemoryTable Create(IEntityType entityType, IInMemoryTable? baseTable)
            => _factories.GetOrAdd((entityType, baseTable), e => CreateTable(e.EntityType, e.BaseTable))();

        private Func<IInMemoryTable> CreateTable([NotNull] IEntityType entityType, IInMemoryTable? baseTable)
            => (Func<IInMemoryTable>)typeof(InMemoryTableFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))!
                .MakeGenericMethod(entityType.FindPrimaryKey()!.GetKeyType())
                .Invoke(null, new object?[] { entityType, baseTable, _sensitiveLoggingEnabled, _nullabilityCheckEnabled })!;

        [UsedImplicitly]
        private static Func<IInMemoryTable> CreateFactory<TKey>(
            IEntityType entityType,
            IInMemoryTable baseTable,
            bool sensitiveLoggingEnabled,
            bool nullabilityCheckEnabled)
            where TKey : notnull
            => () => new InMemoryTable<TKey>(entityType, baseTable, sensitiveLoggingEnabled, nullabilityCheckEnabled);
    }
}
