/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Locate an object (Needle) within a list and its children (Haystack) based on GUID match.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T DeepFindById<T>(this IEnumerable<T> lstHaystack, string strGuid) where T : IHasChildren<T>, IHasInternalId
        {
            if (lstHaystack == null || string.IsNullOrWhiteSpace(strGuid) || strGuid.IsEmptyGuid())
            {
                return default;
            }

            return lstHaystack.DeepFirstOrDefault(x => x.Children, x => x.InternalId == strGuid);
        }

        /// <summary>
        /// Locate an object (Needle) within a list (Haystack) based on GUID match.
        /// </summary>
        /// <param name="strGuid">InternalId of the Needle to Find.</param>
        /// <param name="lstHaystack">Haystack to search.</param>
        public static T FindById<T>(this IEnumerable<T> lstHaystack, string strGuid) where T : IHasInternalId
        {
            if (lstHaystack == null || string.IsNullOrWhiteSpace(strGuid) || strGuid.IsEmptyGuid())
            {
                return default;
            }

            return lstHaystack.FirstOrDefault(x => x.InternalId == strGuid);
        }

        /// <summary>
        /// Get a HashCode representing the contents of an enumerable (instead of just of the pointer to the location where the enumerable would start)
        /// </summary>
        /// <typeparam name="T">The type for which GetHashCode() will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static int GetEnsembleHashCode<T>(this IEnumerable<T> lstItems)
        {
            return lstItems?.Aggregate(19, (current, objItem) => current * 31 + objItem.GetHashCode()) ?? 0;
        }

        /// <summary>
        /// Syntactic sugar to wraps this object instance into an IEnumerable consisting of a single item.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="objItem">The instance that will be wrapped. </param>
        /// <returns>An IEnumerable consisting of just <paramref name="objItem"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> Yield<T>(this T objItem)
        {
            return ToEnumerable(objItem); // stealth array allocation through params is still faster than yield return
        }

        /// <summary>
        /// Making use of params for syntactic sugar, wraps a list of objects into an IEnumerable consisting of them.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="lstItems">The list of objects that will be wrapped. </param>
        /// <returns>An IEnumerable consisting of <paramref name="lstItems"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerable<T>(params T[] lstItems)
        {
            return lstItems; // faster and lighter on memory than yield return
        }
    }
}
