using System;
using Microsoft.CodeAnalysis;

namespace VarReplacer
{
    internal static class ITypeSymbolExtensions
    {
        public static bool HasAnonymousType(this ITypeSymbol realType)
        {
            if (realType == null)
                return false;

            if (realType.IsAnonymousType)
                return true;

            IArrayTypeSymbol arrayType = realType as IArrayTypeSymbol;
            if (arrayType != null && HasAnonymousType(arrayType.ElementType))
                return true;

            INamedTypeSymbol namedType = realType as INamedTypeSymbol;
            if (namedType != null)
            {
                if (namedType.IsGenericType)
                {
                    foreach (ITypeSymbol argument in namedType.TypeArguments)
                    {
                        if (HasAnonymousType(argument as INamedTypeSymbol))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
