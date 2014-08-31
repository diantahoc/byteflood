﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTorrent.Common;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ByteFlood
{
    public class FileInfo : INotifyPropertyChanged
    {
        public string Name
        {
            get
            {
                return App.Settings.ShowRelativePaths ? this.File.Path : this.File.FullPath;
            }
        }

        public string FileName 
        {
            get { return this.File.Path.Split(System.IO.Path.DirectorySeparatorChar).Last(); }
        }

        public double Progress { get { return this.File.BitField.PercentComplete; } }

        public string Priority
        {
            get { return this.File.Priority.ToString(); }
        }

        public void ChangePriority(Priority pr)
        {
            if (this.File.Priority != pr)
            {
                this.File.Priority = pr;
                UpdateList("Priority");
            }
            if (this.Owner != null)
            {
                this.Owner.SetSavedFilePriority(this, pr);
            }
        }

        public bool DownloadFile
        {
            get { return this.File.Priority != MonoTorrent.Common.Priority.Skip; }
            set
            {
                if (value)
                {
                    this.File.Priority = MonoTorrent.Common.Priority.Normal;
                }
                else
                {
                    this.File.Priority = MonoTorrent.Common.Priority.Skip;
                }
            }
        }

        public string Size { get { return Utility.PrettifyAmount(this.File.Length); } }

        public TorrentFile File { get; private set; }

        public long RawSize { get { return this.File.Length; } }

        public FileInfo() { }

        public TorrentInfo Owner { get; private set; }

        public FileInfo(TorrentInfo owner, TorrentFile file)
        {
            this.File = file;
            this.Owner = owner;
            if (this.Owner != null)
            {
                this.Owner.FileInfoList.Add(this);
                var saved_pr = this.Owner.GetSavedFilePriority(this);

                this.ChangePriority(saved_pr);
            }
        }

        public void Update()
        {
            UpdateList("Progress");
        }

        #region INotifyPropertyChanged implementation

        public void UpdateList(params string[] columns)
        {
            if (PropertyChanged == null)
                return;
            foreach (string str in columns)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class DirectoryKey : System.Collections.Hashtable
    {
        //public const string FILE_MARKER = "<files>";

        public DirectoryKey(string name)
        {
            //base.Add(FILE_MARKER, new FileList());
            this.Name = name;
        }


        public string Name { get; private set; }

        //public List<FileInfo> Files 
        //{
        //    get 
        //    {
        //        return (List<FileInfo>)base[FILE_MARKER];
        //    }
        //}

        /// <summary>
        /// No use outside of TorrentInfo.PopulateFileList()
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="trunk"></param>
        public static void ProcessFile(string branch, DirectoryKey trunk, TorrentInfo owner, TorrentFile f)
        {
            string[] parts = branch.Split('\\');
            if (parts.Length == 1)
            {
                //((FileList)trunk[DirectoryKey.FILE_MARKER]).Add(new FileInfo(owner, f));
                trunk.Add(f.FullPath, new FileInfo(owner, f));
            }
            else
            {
                string node = parts[0];
                string other = branch.Substring(node.Length + 1);

                if (!trunk.ContainsKey(node))
                {
                    trunk[node] = new DirectoryKey(node);
                }
                ProcessFile(other, (DirectoryKey)trunk[node], owner, f);
            }
        }


    }

    [Serializable]
    [XmlType(TypeName = "FilePriority")]
    public struct FilePriority
    {
        public string Key
        { get; set; }

        public MonoTorrent.Common.Priority Value
        { get; set; }

    }

    //public class FileList 
    //{
    //    private List<FileInfo> a = new List<FileInfo>();

    //    public void Add(FileInfo f) { a.Add(f); }

    //    public FileInfo this[int index] 
    //    {
    //        get 
    //        {
    //            return a[index];
    //        }
    //        set 
    //        {
    //            a[index] = value;
    //        }
    //    }

    //    public FileInfo[] Files 
    //    {
    //        get { return a.ToArray(); }
    //    }
    //}
}
