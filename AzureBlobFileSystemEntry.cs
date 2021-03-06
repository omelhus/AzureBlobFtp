﻿using System;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AdamHurwitz.FtpServer.FileSystem.AzureBlob
{
    public class AzureBlobFileSystemEntry : IUnixFileSystemEntry
    {
        public AzureBlobFileSystemEntry(AzureBlobFileSystem fileSystem, IListBlobItem item)
        {
            FileSystem = fileSystem;
            Item = item;
            if (Item.GetType().Name == "CloudBlobDirectory")
                IsFolder = true;
            else
                IsFolder = false;

            Permissions = new GenericUnixPermissions(
                new GenericAccessMode(true, true, IsFolder),
                new GenericAccessMode(true, true, IsFolder),
                new GenericAccessMode(true, true, IsFolder));
        }

        public bool IsFolder { get; }
        public IListBlobItem Item { get; }
        public IUnixFileSystem FileSystem { get; }
        public string Group => "group";
        public long NumberOfLinks => 1;
        public string Owner => "owner";
        public IUnixPermissions Permissions { get; }

        public DateTimeOffset? CreatedTime
        {
            get
            {
                if (IsFolder)
                    return DateTimeOffset.MinValue;
                return ((CloudBlockBlob) Item).Properties.LastModified;
            }
        }

        public DateTimeOffset? LastWriteTime
        {
            get
            {
                if (IsFolder)
                    return DateTimeOffset.MinValue;
                return ((CloudBlockBlob) Item).Properties.LastModified;
            }
        }

        public string Name
        {
            get
            {
                if (IsFolder)
                {
                    var dir = (CloudBlobDirectory) Item;
                    if (!string.IsNullOrEmpty(dir.Parent?.Prefix))
                        return dir.Prefix.Replace(dir.Parent.Prefix, "").TrimEnd('/');
                    return dir.Prefix.TrimEnd('/');
                }
                var blob = (CloudBlockBlob) Item;
                if (!string.IsNullOrEmpty(blob.Parent?.Prefix))
                    return blob.Name.Replace(blob.Parent.Prefix, "");
                return blob.Name;
            }
        }
    }
}