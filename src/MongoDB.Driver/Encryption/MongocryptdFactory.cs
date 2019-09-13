/* Copyright 2019-present MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MongoDB.Driver.Encryption
{
    internal class MongocryptdFactory
    {
        #region static
        private static string[] __supportedExtraOptionKeys=
        {
            "mongocryptdURI",
            "mongocryptdBypassSpawn",
            "mongocryptdSpawnPath",
            "mongocryptdSpawnArgs"
        };
        #endregion

        private readonly IReadOnlyDictionary<string, object> _extraOptions;

        public MongocryptdFactory(IReadOnlyDictionary<string, object> extraOptions)
        {
            _extraOptions = extraOptions ?? new Dictionary<string, object>();
            EnsureThatExtraOptionsAreValid(extraOptions);
        }

        // public methods
        public MongoClient CreateMongocryptdClient()
        {
            var connectionString = CreateMongocryptdConnectionString();
            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(1000);
            return new MongoClient(clientSettings);
        }

        public void SpawnMongocryptdProcessIfRequired()
        {
            if (ShouldMongocryptdBeSpawned(out var path, out var args))
            {
                StartProcess(path, args);
            }
        }

        // private methods
        private string CreateMongocryptdConnectionString()
        {
            if (_extraOptions.TryGetValue("mongocryptdURI", out var connectionString))
            {
                return (string)connectionString;
            }
            else
            {
                return "mongodb://localhost:27020";
            }
        }

        private void EnsureThatExtraOptionsAreValid(IReadOnlyDictionary<string, object> extraOptions)
        {
            foreach (var extraOption in extraOptions)
            {
                if (!__supportedExtraOptionKeys.Contains(extraOption.Key))
                {
                    throw new ArgumentException($"Invalid extra option key: {extraOption.Key}.");
                }
            }
        }

        private bool ShouldMongocryptdBeSpawned(out string path, out string args)
        {
            path = null;
            args = null;
            if (!_extraOptions.TryGetValue("mongocryptdBypassSpawn", out var mongoCryptBypassSpawn)
                || !IsMongoCryptBypassSpawnValid(mongoCryptBypassSpawn))
            {
                if (_extraOptions.TryGetValue("mongocryptdSpawnPath", out var objPath))
                {
                    path = (string)objPath;
                }
                else
                {
                    path = string.Empty; // look at the PATH env variable
                }

                if (!Path.HasExtension(path))
                {
                    string fileName = "mongocryptd.exe";
                    path = Path.Combine(path, fileName);
                }

                args = string.Empty;
                if (_extraOptions.TryGetValue("mongocryptdSpawnArgs", out var mongocryptdSpawnArgs))
                {
                    string trimStartHyphens(string str) => str.TrimStart('-').TrimStart('-');
                    switch (mongocryptdSpawnArgs)
                    {
                        case string str:
                            args += str;
                            break;
                        case IEnumerable enumerable:
                            foreach (var item in enumerable)
                            {
                                args += $"--{trimStartHyphens(item.ToString())} ";
                            }
                            break;
                        default:
                            throw new InvalidCastException($"Invalid type: {mongocryptdSpawnArgs.GetType().Name} of mongocryptdSpawnArgs option.");
                    }
                }

                args = args.Trim();
                if (!args.Contains("idleShutdownTimeoutSecs"))
                {
                    args += " --idleShutdownTimeoutSecs 60";
                }
                args = args.Trim();

                return true;
            }

            return false;
        }

        private void StartProcess(string path, string args)
        {
            try
            {
                using (var mongoCryptD = new Process())
                {
                    mongoCryptD.StartInfo.Arguments = args;
                    mongoCryptD.StartInfo.FileName = path;
                    mongoCryptD.StartInfo.CreateNoWindow = true;
                    mongoCryptD.StartInfo.UseShellExecute = false;

                    if (!mongoCryptD.Start())
                    {
                        // skip it. This case can happen if no new process resource is started
                        // (for example, if an existing process is reused)
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MongoClientException("Exception starting mongocryptd process. Is mongocryptd on the system path?", ex);
            }
        }

        private bool IsMongoCryptBypassSpawnValid(object objectValue)
        {
            if (objectValue is bool value)
            {
                return value;
            }
            else
            {
                throw new InvalidCastException($"Invalid type: {objectValue.GetType().Name} of mongocryptdBypassSpawn option.");
            }
        }
    }
}
