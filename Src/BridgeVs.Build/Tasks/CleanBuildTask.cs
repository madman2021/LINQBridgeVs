﻿#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.IO;
using BridgeVs.Build.Util;
using BridgeVs.Shared.Common;
using BridgeVs.Shared.Logging;
using BridgeVs.Shared.Options;
using Microsoft.Build.Framework;
using FS = BridgeVs.Shared.FileSystem.FileSystemFactory;


namespace BridgeVs.Build.Tasks
{
    public class CleanBuildTask : ITask
    {
        [Required]
        public string VisualStudioVer { private get; set; }

        [Required]
        public string Assembly { private get; set; }

        [Required]
        public string SolutionName { get; set; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            Log.VisualStudioVersion = VisualStudioVer;

            if (!CommonRegistryConfigurations.IsSolutionEnabled(SolutionName, VisualStudioVer))
            {
                return true;
            }

            try
            {
                string visualizerAssemblyName = VisualizerAssemblyNameFormat.GetTargetVisualizerAssemblyName(VisualStudioVer, Assembly);
                string targetInstallationPath = VisualStudioOption.GetVisualizerDestinationFolder(VisualStudioVer);

                string visualizerFullPath = Path.Combine(targetInstallationPath, visualizerAssemblyName);

                if (FS.FileSystem.File.Exists(visualizerFullPath))
                    FS.FileSystem.File.Delete(visualizerFullPath);

                //check if pdb also exists and delete it
                string visualizerPdbFullPath = Path.ChangeExtension(visualizerFullPath, "pdb");

                if (FS.FileSystem.File.Exists(visualizerPdbFullPath))
                    FS.FileSystem.File.Delete(visualizerPdbFullPath);
            }
            catch (System.Exception exception)
            {
                Log.Write(exception, "Error During cleanup");
                BuildWarningEventArgs errorEvent = new BuildWarningEventArgs("Debugger Visualizer Cleanup", "", "CleanBuildTask", 0, 0, 0, 0, $"There was an error cleaning custom debugger visualizers", "", "LINQBridgeVs");
                BuildEngine.LogWarningEvent(errorEvent);
                exception.Capture(VisualStudioVer, message: "Error during project cleaning");
            }

            return true;
        }
    }
}