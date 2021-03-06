/***** BEGIN LICENSE BLOCK *****
 * Version: MPL 2.0/GPL 2.0/LGPL 2.1
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is HashTableHashing.SuperFastHash.
 *
 * The Initial Developer of the Original MurmurHash2 Code is
 * Davy Landman.
 * Portions created by the Initial Developer are Copyright (C) 2009
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 * Thomas Kejser
 *    Turning this code into SQL Server CLR version
 *    and adding MurmurHash3 implementation based on C++ source.
 * Rune Skovbo Johansen
 *    Removing all SQL dependencies (take byte array instead of SqlBinary).
 *    Adding overload that takes an int array (less code and executes faster).
 *    Adding methods for obtaining random integers or floats in given ranges.
 *    Adding overload optimized for single int input with no loop.
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

namespace Hash
{
	public static class MurmurHash
	{
		private const uint C1 = 0xcc9e2d51;
		private const uint C2 = 0x1b873593;

		public static uint GetHash(byte[] data, uint seed)
		{
			var curLength = data.Length; // Current position in byte array
			var length = curLength; // The const length we need to fix tail
			var h1 = seed;
			uint k1;

			// Body, eat stream a 32-bit int at a time
			var currentIndex = 0;
			while (curLength >= 4)
			{
				// Get four bytes from the input into an uint
				k1 = (uint) (data[currentIndex++]
				             | (data[currentIndex++] << 8)
				             | (data[currentIndex++] << 16)
				             | (data[currentIndex++] << 24));

				// Bitmagic hash
				k1 *= C1;
				k1 = RotL32(k1, 15);
				k1 *= C2;

				h1 ^= k1;
				h1 = RotL32(h1, 13);
				h1 = h1 * 5 + 0xe6546b64;
				curLength -= 4;
			}

			// Tail, the reminder bytes that did not make it to a full int.
			// (This switch is slightly more ugly than the C++ implementation
			// because we can't fall through.)
			switch (curLength)
			{
				case 3:
					k1 = (uint) (data[currentIndex++]
					             | (data[currentIndex++] << 8)
					             | (data[currentIndex] << 16));
					k1 *= C1;
					k1 = RotL32(k1, 15);
					k1 *= C2;
					h1 ^= k1;
					break;
				case 2:
					k1 = (uint) (data[currentIndex++]
					             | (data[currentIndex] << 8));
					k1 *= C1;
					k1 = RotL32(k1, 15);
					k1 *= C2;
					h1 ^= k1;
					break;
				case 1:
					k1 = data[currentIndex];
					k1 *= C1;
					k1 = RotL32(k1, 15);
					k1 *= C2;
					h1 ^= k1;
					break;
			}

			// Finalization, magic chants to wrap it all up
			h1 ^= (uint) length;
			h1 = Mix(h1);

			return h1;
		}

		// Overload optimized for int input.
		public static uint GetHash(int[] data, uint seed)
		{
			var h1 = seed;

			// Body, eat stream a 32-bit int at a time
			var length = data.Length;
			for (var i = 0; i < length; i++)
			{
				uint k1;
				unchecked
				{
					k1 = (uint) data[i];
				}

				// Bitmagic hash
				k1 *= C1;
				k1 = RotL32(k1, 15);
				k1 *= C2;

				h1 ^= k1;
				h1 = RotL32(h1, 13);
				h1 = h1 * 5 + 0xe6546b64;
			}

			// Finalization, magic chants to wrap it all up
			h1 ^= (uint) (length * 4);
			h1 = Mix(h1);

			return h1;
		}

		// Overload optimized for single int input.
		public static uint GetHash(int data, uint seed)
		{
			var h1 = seed;
			uint k1;

			unchecked
			{
				k1 = (uint) data;
			}

			// Bitmagic hash
			k1 *= C1;
			k1 = RotL32(k1, 15);
			k1 *= C2;

			h1 ^= k1;
			h1 = RotL32(h1, 13);
			h1 = h1 * 5 + 0xe6546b64;

			// Finalization, magic chants to wrap it all up
			h1 ^= 4U;
			h1 = Mix(h1);

			return h1;
		}

		private static uint RotL32(uint x, byte r)
		{
			return (x << r) | (x >> (32 - r));
		}

		private static uint Mix(uint h)
		{
			h ^= h >> 16;
			h *= 0x85ebca6b;
			h ^= h >> 13;
			h *= 0xc2b2ae35;
			h ^= h >> 16;
			return h;
		}
	}
}
