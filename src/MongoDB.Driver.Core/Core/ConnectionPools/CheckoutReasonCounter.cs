/* Copyright 2021-present MongoDB Inc.
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
using System.Threading;

namespace MongoDB.Driver.Core.ConnectionPools
{
    internal enum CheckoutReason
    {
        Cursor,
        Transaction
    }

    internal interface ICheckoutReasonTracker
    {
        CheckoutReason? CheckoutReason { get; }
        void SetCheckoutReasonIfNotAlreadySet(CheckoutReason reason);
    }

    internal sealed class CheckoutReasonCounter
    {
        public int _cursorCheckoutsCount = 0;
        public int _transactionCheckoutsCount = 0;

        public int GetCheckoutsCount(CheckoutReason reason) =>
            reason switch
            {
                CheckoutReason.Cursor => _cursorCheckoutsCount,
                CheckoutReason.Transaction => _transactionCheckoutsCount,
                _ => throw new InvalidOperationException($"Invalid checkout reason {reason}.")
            };

        public void Increment(CheckoutReason reason)
        {
            switch (reason)
            {
                case CheckoutReason.Cursor:
                    Interlocked.Increment(ref _cursorCheckoutsCount);
                    break;
                case CheckoutReason.Transaction:
                    Interlocked.Increment(ref _transactionCheckoutsCount);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid checkout reason {reason}.");
            }
        }

        public void Decrement(CheckoutReason? reason)
        {
            switch (reason)
            {
                case null:
                    break;
                case CheckoutReason.Cursor:
                    Interlocked.Decrement(ref _cursorCheckoutsCount);
                    break;
                case CheckoutReason.Transaction:
                    Interlocked.Decrement(ref _transactionCheckoutsCount);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid checkout reason {reason}.");
            }
        }
    }
}
