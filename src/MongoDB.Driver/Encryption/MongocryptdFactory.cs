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

namespace MongoDB.Driver.Encryption
{
    internal class MongocryptdFactory
    {
        #region static
        public static MongoClient CreateClient(IReadOnlyDictionary<string, object> extraOptions)
        {
            var helper = new MongocryptdFactory();
            helper._extraOptions = extraOptions ?? new Dictionary<string, object>();
            var client = helper.CreateMongocryptdClient();
            return client;
        }
        #endregion

        private IReadOnlyDictionary<string, object> _extraOptions;

        // private methods
        private MongoClient CreateMongocryptdClient()
        {
            var connectionString = CreateMongocryptdConnectionString();

            if (ShouldMongocryptdBeSpawned(out var path, out var args))
            {
                StartProcess(path, args);
            }

            return new MongoClient(connectionString);
        }

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

        private bool ShouldMongocryptdBeSpawned(out string path, out string args)
        {
            path = null;
            args = null;
            if (!_extraOptions.TryGetValue("mongocryptdBypassSpawn", out var mongoCryptBypassSpawn) || (!bool.Parse(mongoCryptBypassSpawn.ToString())))
            {
                if (!_extraOptions.TryGetValue("mongocryptdSpawnPath", out var objPath))
                {
                    path = string.Empty; // look at the PATH env variable
                }
                else
                {
                    path = objPath.ToString();
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
                        case IDictionary dictionary:
                            foreach (var key in dictionary.Keys)
                            {
                                args += $"--{trimStartHyphens(key.ToString())} {dictionary[key]} ";
                            }
                            break;
                        case IEnumerable enumerable:
                            foreach (var item in enumerable)
                            {
                                args += $"--{trimStartHyphens(item.ToString())} ";
                            }
                            break;
                        default:
                            args = mongocryptdSpawnArgs.ToString();
                            break;
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
                using (Process mongoCryptD = new Process())
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
    }
}
