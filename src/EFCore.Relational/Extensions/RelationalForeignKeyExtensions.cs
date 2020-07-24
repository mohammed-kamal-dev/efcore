// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IForeignKey" /> for relational database metadata.
    /// </summary>
    public static class RelationalForeignKeyExtensions
    {
        /// <summary>
        ///     Returns the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The foreign key constraint name. </returns>
        public static string GetConstraintName([NotNull] this IForeignKey foreignKey)
            => foreignKey.GetConstraintName(
                StoreObjectIdentifier.Table(foreignKey.DeclaringEntityType.GetTableName(), foreignKey.DeclaringEntityType.GetSchema()),
                StoreObjectIdentifier.Table(foreignKey.PrincipalEntityType.GetTableName(), foreignKey.PrincipalEntityType.GetSchema()));

        /// <summary>
        ///     Returns the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <param name="principalStoreObject"> The identifier of the principal store object. </param>
        /// <returns> The foreign key constraint name. </returns>
        public static string GetConstraintName(
            [NotNull] this IForeignKey foreignKey, StoreObjectIdentifier storeObject, StoreObjectIdentifier principalStoreObject)
        {
            var annotation = foreignKey.FindAnnotation(RelationalAnnotationNames.Name);
            return annotation != null
                ? (string)annotation.Value
                : foreignKey.GetDefaultName(storeObject, principalStoreObject);
        }

        /// <summary>
        ///     Returns the default constraint name that would be used for this foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The default constraint name that would be used for this foreign key. </returns>
        public static string GetDefaultName([NotNull] this IForeignKey foreignKey)
        {
            var tableName = foreignKey.DeclaringEntityType.GetTableName();
            var schema = foreignKey.DeclaringEntityType.GetSchema();
            var principalTableName = foreignKey.PrincipalEntityType.GetTableName();

            var name = new StringBuilder()
                .Append("FK_")
                .Append(tableName)
                .Append("_")
                .Append(principalTableName)
                .Append("_")
                .AppendJoin(foreignKey.Properties.Select(p => p.GetColumnName()), "_")
                .ToString();

            return Uniquifier.Truncate(name, foreignKey.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Returns the default constraint name that would be used for this foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <param name="principalStoreObject"> The identifier of the principal store object. </param>
        /// <returns> The default constraint name that would be used for this foreign key. </returns>
        public static string GetDefaultName(
            [NotNull] this IForeignKey foreignKey,
            StoreObjectIdentifier storeObject,
            StoreObjectIdentifier principalStoreObject)
        {
            var propertyNames = foreignKey.Properties.Select(p => p.GetColumnName(storeObject)).ToList();
            var principalPropertyNames = foreignKey.PrincipalKey.Properties.Select(p => p.GetColumnName(storeObject)).ToList();
            var rootForeignKey = foreignKey;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                var linkedForeignKey = rootForeignKey.DeclaringEntityType
                    .FindRowInternalForeignKeys(storeObject)
                    .SelectMany(fk => fk.PrincipalEntityType.GetForeignKeys())
                    .FirstOrDefault(k => principalStoreObject.Name == k.PrincipalEntityType.GetTableName()
                        && principalStoreObject.Schema == k.PrincipalEntityType.GetSchema()
                        && propertyNames.SequenceEqual(k.Properties.Select(p => p.GetColumnName(storeObject)))
                        && principalPropertyNames.SequenceEqual(k.PrincipalKey.Properties.Select(p => p.GetColumnName(storeObject))));
                if (linkedForeignKey == null)
                {
                    break;
                }

                rootForeignKey = linkedForeignKey;
            }

            if (rootForeignKey != foreignKey)
            {
                return rootForeignKey.GetConstraintName(storeObject, principalStoreObject);
            }

            var baseName = new StringBuilder()
                .Append("FK_")
                .Append(storeObject.Name)
                .Append("_")
                .Append(principalStoreObject.Name)
                .Append("_")
                .AppendJoin(foreignKey.Properties.Select(p => p.GetColumnName(storeObject)), "_")
                .ToString();

            return Uniquifier.Truncate(baseName, foreignKey.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetConstraintName([NotNull] this IMutableForeignKey foreignKey, [CanBeNull] string value)
            => foreignKey.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the foreign key constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured name. </returns>
        public static string SetConstraintName(
            [NotNull] this IConventionForeignKey foreignKey, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            foreignKey.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Name,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the constraint name.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the constraint name. </returns>
        public static ConfigurationSource? GetConstraintNameConfigurationSource([NotNull] this IConventionForeignKey foreignKey)
            => foreignKey.FindAnnotation(RelationalAnnotationNames.Name)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Gets the foreign key constraints to which the foreign key is mapped.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <returns> The foreign key constraints to which the foreign key is mapped. </returns>
        public static IEnumerable<IForeignKeyConstraint> GetMappedConstraints([NotNull] this IForeignKey foreignKey) =>
            (IEnumerable<IForeignKeyConstraint>)foreignKey[RelationalAnnotationNames.ForeignKeyMappings]
                ?? Enumerable.Empty<IForeignKeyConstraint>();

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IForeignKey" /> that is mapped to the same constraint in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The foreign key if found, or <see langword="null" /> if none was found.</returns>
        public static IForeignKey FindSharedObjectRootForeignKey([NotNull] this IForeignKey foreignKey, StoreObjectIdentifier storeObject)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var foreignKeyName = foreignKey.GetConstraintName(storeObject,
                StoreObjectIdentifier.Table(foreignKey.PrincipalEntityType.GetTableName(), foreignKey.PrincipalEntityType.GetSchema()));
            var rootForeignKey = foreignKey;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                var linkedKey = rootForeignKey.DeclaringEntityType
                    .FindRowInternalForeignKeys(storeObject)
                    .SelectMany(fk => fk.PrincipalEntityType.GetForeignKeys())
                    .FirstOrDefault(k => k.GetConstraintName(storeObject,
                        StoreObjectIdentifier.Table(k.PrincipalEntityType.GetTableName(), k.PrincipalEntityType.GetSchema()))
                        == foreignKeyName);
                if (linkedKey == null)
                {
                    break;
                }

                rootForeignKey = linkedKey;
            }

            return rootForeignKey == foreignKey ? null : rootForeignKey;
        }

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IMutableForeignKey" /> that is mapped to the same constraint in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The foreign key if found, or <see langword="null" /> if none was found.</returns>
        public static IMutableForeignKey FindSharedObjectRootForeignKey(
            [NotNull] this IMutableForeignKey foreignKey, StoreObjectIdentifier storeObject)
            => (IMutableForeignKey)((IForeignKey)foreignKey).FindSharedObjectRootForeignKey(storeObject);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IConventionForeignKey" /> that is mapped to the same constraint in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="storeObject"> The identifier of the containing store object. </param>
        /// <returns> The foreign key if found, or <see langword="null" /> if none was found.</returns>
        public static IConventionForeignKey FindSharedObjectRootForeignKey(
            [NotNull] this IConventionForeignKey foreignKey, StoreObjectIdentifier storeObject)
            => (IConventionForeignKey)((IForeignKey)foreignKey).FindSharedObjectRootForeignKey(storeObject);
    }
}
