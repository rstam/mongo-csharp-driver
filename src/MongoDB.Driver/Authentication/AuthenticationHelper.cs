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
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Authentication
{
    internal static class AuthenticationHelper
    {
        public static void Authenticate(OperationContext operationContext, IConnection connection, ConnectionDescription description, IAuthenticator authenticator)
        {
            Ensure.IsNotNull(operationContext, nameof(operationContext));
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));

            if (authenticator == null)
            {
                return;
            }

            // authentication is currently broken on arbiters
            if (!description.HelloResult.IsArbiter)
            {
                authenticator.Authenticate(operationContext, connection, description);
            }
        }

        public static async Task AuthenticateAsync(OperationContext operationContext, IConnection connection, ConnectionDescription description, IAuthenticator authenticator)
        {
            Ensure.IsNotNull(operationContext, nameof(operationContext));
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(description, nameof(description));

            if (authenticator == null)
            {
                return;
            }

            // authentication is currently broken on arbiters
            if (!description.HelloResult.IsArbiter)
            {
                    await authenticator.AuthenticateAsync(operationContext, connection, description).ConfigureAwait(false);
            }
        }

        public static string MongoPasswordDigest(string username, SecureString password)
        {
            if (password.Length == 0)
            {
                return MongoPasswordDigest(username, new byte[0]);
            }

            var passwordIntPtr = Marshal.SecureStringToGlobalAllocUnicode(password);
            try
            {
                var passwordChars = new char[password.Length];
                var passwordCharsHandle = GCHandle.Alloc(passwordChars, GCHandleType.Pinned);
                try
                {
                    Marshal.Copy(passwordIntPtr, passwordChars, 0, password.Length);

                    return MongoPasswordDigest(username, passwordChars);
                }
                finally
                {
                    Array.Clear(passwordChars, 0, passwordChars.Length);
                    passwordCharsHandle.Free();
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(passwordIntPtr);
            }
        }

        private static string MongoPasswordDigest(string username, char[] passwordChars)
        {
            var passwordBytes = new byte[Utf8Encodings.Strict.GetByteCount(passwordChars)];
            var passwordBytesHandle = GCHandle.Alloc(passwordBytes, GCHandleType.Pinned);
            try
            {
                Utf8Encodings.Strict.GetBytes(passwordChars, 0, passwordChars.Length, passwordBytes, 0);

                return MongoPasswordDigest(username, passwordBytes);
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                passwordBytesHandle.Free();
            }
        }

        private static string MongoPasswordDigest(string username, byte[] passwordBytes)
        {
            var prefixString = username + ":mongo:";
            var prefixBytes = Utf8Encodings.Strict.GetBytes(prefixString);

            var buffer = new byte[prefixBytes.Length + passwordBytes.Length];
            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Buffer.BlockCopy(prefixBytes, 0, buffer, 0, prefixBytes.Length);
                Buffer.BlockCopy(passwordBytes, 0, buffer, prefixBytes.Length, passwordBytes.Length);

                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(buffer);
                    return BsonUtils.ToHexString(hash);
                }
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
                bufferHandle.Free();
            }
        }
    }
}
