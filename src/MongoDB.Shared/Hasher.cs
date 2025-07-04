﻿/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MongoDB.Shared
{
    internal class Hasher
    {
        #region static

        // public static methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(T obj) => EqualityComparer<T>.Default.GetHashCode(obj);

        #endregion

        // private fields
        private int _hashCode;

        // constructors
        public Hasher()
        {
            _hashCode = 17;
        }

        public Hasher(int seed)
        {
            _hashCode = seed;
        }

        // public methods
        public override int GetHashCode()
        {
            return _hashCode;
        }

        // this overload added to avoid boxing
        public Hasher Hash(bool obj)
        {
            _hashCode = 37 * _hashCode + obj.GetHashCode();
            return this;
        }

        // this overload added to avoid boxing
        public Hasher Hash(int obj)
        {
            _hashCode = 37 * _hashCode + obj.GetHashCode();
            return this;
        }

        // this overload added to avoid boxing
        public Hasher Hash(long obj)
        {
            _hashCode = 37 * _hashCode + obj.GetHashCode();
            return this;
        }

        // this overload added to avoid boxing
        public Hasher Hash<T>(Nullable<T> obj) where T : struct
        {
            _hashCode = 37 * _hashCode + ((obj == null) ? -1 : obj.Value.GetHashCode());
            return this;
        }

        public Hasher Hash(object obj)
        {
            _hashCode = 37 * _hashCode + ((obj == null) ? -1 : obj.GetHashCode());
            return this;
        }

        public Hasher HashElements<T>(IEnumerable<T> sequence)
        {
            if (sequence == null)
            {
                _hashCode = 37 * _hashCode + -1;
            }
            else
            {
                foreach (var value in sequence)
                {
                    _hashCode = 37 * _hashCode + (value == null ? -1 : GetHashCode(value));
                }
            }
            return this;
        }

        public Hasher HashStructElements<T>(IEnumerable<T> sequence) where T : struct
        {
            foreach (var value in sequence)
            {
                _hashCode = 37 * _hashCode + value.GetHashCode();
            }
            return this;
        }
    }
}
