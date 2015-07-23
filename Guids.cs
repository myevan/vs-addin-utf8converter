// Guids.cs
// MUST match guids.h
using System;

namespace PbTheCat.UTF8Converter
{
    static class GuidList
    {
        public const string guidUTF8ConverterPkgString = "bfd0931c-5865-436c-bc5e-a56bd771604c";
        public const string guidUTF8ConverterCmdSetString = "2251b923-7b9e-4e97-9a63-c8fe607fba9b";

        public static readonly Guid guidUTF8ConverterCmdSet = new Guid(guidUTF8ConverterCmdSetString);
    };
}