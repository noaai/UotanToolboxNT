﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UotanToolbox.Common.PatchHelper
{
    internal class KernelSUPatch
    {
        private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
        public async static Task KernelSU_Patch(ZipInfo zipInfo, BootInfo bootInfo)
        {
            if (bootInfo.HaveKernel == false)
            {
                throw new Exception(GetTranslation("Basicflash_BootWong"));
            }
            File.Copy(Path.Combine(zipInfo.TempPath, "Image"), Path.Combine(bootInfo.TempPath, "kernel"));
            CleanBoot(bootInfo.TempPath);
            (string mb_output, int exitcode) = await CallExternalProgram.MagiskBoot($"repack \"{bootInfo.Path}\"", bootInfo.TempPath);
            if (exitcode != 0)
            {
                throw new Exception("ksu_pack_1 failed: " + mb_output);
            }
            string allowedChars = "abcdef0123456789";
            Random random = new Random();
            string randomStr = new string(Enumerable.Repeat(allowedChars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            File.Copy(Path.Combine(bootInfo.TempPath, "new-boot.img"), Path.Combine(Path.GetDirectoryName(bootInfo.Path), "boot_patched_" + randomStr + ".img"), true);
        }
        private static void CleanBoot(string path)
        {
            string[] filesToDelete =
                {
                "magisk64.xz",
                "magisk32.xz",
                "magiskinit",
                "stub.xz",
                "ramdisk.cpio.orig",
                "config",
                "stock_boot.img",
                "cpio",
                "init",
                "init.xz",
                ".magisk",
                ".rmlist"
                };
            try
            {
                foreach (string file in filesToDelete)

                {
                    string filePath = Path.Combine(path, file);
                    if (File.Exists(filePath))
                    {
                        FileHelper.WipeFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("bootclean failed: " + ex.Message);
            }
        }
    }
}