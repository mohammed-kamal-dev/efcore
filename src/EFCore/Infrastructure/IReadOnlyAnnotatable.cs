// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A class that supports annotations. Annotations allow for arbitrary metadata to be stored on an object.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the value of the annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The value of the existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        object? this[[NotNull] string name] { get; }

        /// <summary>
        ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        IAnnotation? FindAnnotation([NotNull] string name);

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        IEnumerable<IAnnotation> GetAnnotations();

        /// <summary>
        ///     Gets the annotation with the given name, throwing if it does not exist.
        /// </summary>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns> The annotation with the specified name. </returns>
        IAnnotation GetAnnotation([NotNull] string annotationName)
            => Annotatable.GetAnnotation(this, annotationName);

        /// <summary>
        ///     Gets the debug string for all annotations declared on the object.
        /// </summary>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> Debug string representation of all annotations. </returns>
        string AnnotationsToDebugString(int indent = 0)
        {
            var annotations = GetAnnotations().ToList();
            if (annotations.Count == 0)
            {
                return "";
            }

            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.AppendLine().Append(indentString).Append("Annotations: ");
            foreach (var annotation in annotations)
            {
                builder
                    .AppendLine()
                    .Append(indentString)
                    .Append("  ")
                    .Append(annotation.Name)
                    .Append(": ")
                    .Append(annotation.Value);
            }

            return builder.ToString();
        }
    }
}
