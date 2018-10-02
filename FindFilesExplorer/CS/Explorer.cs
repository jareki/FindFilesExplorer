using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FindFilesExplorer.CS
{
    public class Explorer
    {
        string _initdir, _mask, _search;
        Task t;
        object lockobj;
        bool paused;
        CancellationTokenSource tokensource;
        CancellationToken token;
        
        public event EventHandler<ExplorerEventArgs> FileExploring;
        public event EventHandler<ExplorerEventArgs> FileFound;
        public event EventHandler SearchEnded;

        public Explorer(string initdir, string mask, string search)
        {
            _initdir = initdir;
            _mask = mask;
            _search = search;

            lockobj = new object();
            paused = false;
        }

        public void Start()
        {
            tokensource = new CancellationTokenSource();
            token = tokensource.Token;  
            t = Task.Run(() => { FolderLookup(_initdir, token); OnSearchEnded(); }, token);
        }

        public void Pause()
        {
            if (!paused)
            {
                Monitor.Enter(lockobj);
                paused = true;
            }
        }

        public void Resume()
        {
            if (paused)
            {
                Monitor.Exit(lockobj);
                paused = false;
            }
        }

        public void Stop()
        {
            if (paused)
            {
                Monitor.Exit(lockobj);
                paused = false;
            }
            tokensource.Cancel();
        }

        void OnFileFound(string filename)
        {
            Application.Current.Dispatcher.Invoke
                (
                    () =>FileFound?.Invoke(this, new ExplorerEventArgs { FileName = filename })
                );
        }

        void OnFileLookup(string filename)
        {
            Application.Current.Dispatcher.Invoke
                (
                    () => FileExploring?.Invoke(this, new ExplorerEventArgs { FileName = filename })
                );
        }

        void OnSearchEnded()
        {
            Application.Current.Dispatcher.Invoke
                (
                    () => SearchEnded?.Invoke(this, null)
                );
        }

        void FolderLookup(string folder, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            if (Directory.Exists(folder))
            {
                try
                {
                    string[] subdirs;
                    lock (lockobj)
                        subdirs = Directory.GetDirectories(folder);
                    Array.ForEach(subdirs, dir => FolderLookup(dir, token));

                    string[] files;
                    lock (lockobj)
                        files = Directory.GetFiles(folder, _mask, SearchOption.TopDirectoryOnly);
                    Array.ForEach(files, file => FileLookup(file, token));
                }
                catch { }
            }            
        }

        void FileLookup(string filename, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            OnFileLookup(filename);
            try
            {
                lock (lockobj)
                    foreach (string line in File.ReadLines(filename))
                    {
                        if (line.Contains(_search))
                        {
                            OnFileFound(filename);
                            return;
                        }
                    }
            }
            catch { }
        }
    }
}
