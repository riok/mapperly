//HintName: MapperlyInternal.PreserveReferenceHandler.g.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Riok.Mapperly.Abstractions.ReferenceHandling;

#nullable enable

namespace Riok.Mapperly.Internal.Tests
{
    /// <summary>
    /// A <see cref="IReferenceHandler"/> implementation
    /// which returns the same target object instance if encountered the same source object instance.
    /// Do not use directly. Should only be used by Mapperly generated code.
    /// API surface is not subject to semantic releases and may break in any release.
    /// </summary>
    internal sealed class PreserveReferenceHandler : IReferenceHandler
    {
        private readonly Dictionary<(Type, Type), ReferenceHolder> _referenceHolders = new();

        // disable nullability since older target frameworks
        // may not support the NotNullWhenAttribute
#nullable disable
        /// <inheritdoc cref="IReferenceHandler.TryGetReference{TSource,TTarget}"/>
        public bool TryGetReference<TSource, TTarget>(TSource source, out TTarget target)
            where TSource : notnull
            where TTarget : notnull
        {
            var refHolder = GetReferenceHolder<TSource, TTarget>();
            return refHolder.TryGetRef(source, out target);
        }

#nullable enable

        /// <inheritdoc cref="IReferenceHandler.SetReference{TSource,TTarget}"/>
        public void SetReference<TSource, TTarget>(TSource source, TTarget target)
            where TSource : notnull
            where TTarget : notnull => GetReferenceHolder<TSource, TTarget>().SetRef(source, target);

        private ReferenceHolder GetReferenceHolder<TSource, TTarget>()
        {
            var mapping = (typeof(TSource), typeof(TTarget));
            if (_referenceHolders.TryGetValue(mapping, out var refHolder))
                return refHolder;

            return _referenceHolders[mapping] = new();
        }

        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        {
            // cannot use System.Collections.Generic.ReferenceEqualityComparer since it is not available in netstandard2.0

            public static readonly IEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

            private ReferenceEqualityComparer() { }

            bool IEqualityComparer<T>.Equals(T? x, T? y) => ReferenceEquals(x, y);

            int IEqualityComparer<T>.GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private class ReferenceHolder
        {
            private readonly Dictionary<object, object> _references = new(ReferenceEqualityComparer<object>.Instance);

            // disable nullability since older target frameworks
            // may not support the NotNullWhenAttribute
#nullable disable
            public bool TryGetRef<TSource, TTarget>(TSource source, out TTarget target)
                where TSource : notnull
                where TTarget : notnull
            {
                if (_references.TryGetValue(source, out var targetObj))
                {
                    target = (TTarget)targetObj;
                    return true;
                }

                target = default;
                return false;
            }

#nullable enable

            public void SetRef<TSource, TTarget>(TSource source, TTarget target)
                where TSource : notnull
                where TTarget : notnull
            {
                _references[source] = target;
            }
        }
    }
}
